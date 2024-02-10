using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{

    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float spawnDelay = 0.01f;
    public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();
    public List<SpawnData> SpawnDatas { get => spawnDatas; set => spawnDatas = value; }
    private Transform cubesParent;

    [Header("Arena Generation Properties")]
    public int minHeight = 1;
    public int maxHeight = 5;

    // cube size determined by the scale of the cube prefab and the spacing between cubes.
    public float CubeSize = 2f;
    public float floatProbability = 0.2f; // Probability of a cube floating
    private int attempts = 0;
    private const int maxAttempts = 3;

    // Define the offsets for adjacent cubes in a 3D grid
    private Vector3[] offsets = new Vector3[]
    {
        // Cardinal directions along the axes
        Vector3.forward, Vector3.back, Vector3.up,
        Vector3.down, Vector3.right, Vector3.left,

        // Diagonal directions
        new Vector3(1, 1, 1),    // Upper-front-right diagonal
        new Vector3(1, 1, -1),   // Upper-front-left diagonal
        new Vector3(1, -1, 1),   // Upper-back-right diagonal
        new Vector3(1, -1, -1),  // Upper-back-left diagonal
        new Vector3(-1, 1, 1),   // Lower-front-right diagonal
        new Vector3(-1, 1, -1),  // Lower-front-left diagonal
        new Vector3(-1, -1, 1),  // Lower-back-right diagonal
        new Vector3(-1, -1, -1), // Lower-back-left diagonal

        // Additional offsets
        new Vector3(0, 1, 1),    // Upper-middle-front
        new Vector3(0, 1, -1),   // Upper-middle-back
        new Vector3(0, -1, 1),   // Lower-middle-front
        new Vector3(0, -1, -1),  // Lower-middle-back
        new Vector3(1, 1, 0),    // Middle-front-right
        new Vector3(1, -1, 0),   // Middle-back-right
        new Vector3(-1, 1, 0),   // Middle-front-left
        new Vector3(-1, -1, 0),  // Middle-back-left
        new Vector3(1, 0, 1),    // Middle-upper-right
        new Vector3(1, 0, -1),   // Middle-upper-left
        new Vector3(-1, 0, 1),   // Middle-lower-right
        new Vector3(-1, 0, -1),  // Middle-lower-left
        new Vector3(0, 1, 0),    // Upper-middle
        new Vector3(0, -1, 0)    // Lower-middle
    };

    [HideInInspector]
    public List<ColorOption> colorsUsed = new List<ColorOption>(); // Track colors used in the current arena generation

    static public Action OnFloorComplete { get; set; }

    private void Awake()
    {
        Init();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateArena(gridSizeX, gridSizeZ);
        }
    }

    private void Init()
    {
        if (cubesParent == null)
        {
            cubesParent = new GameObject("Cubes").transform;
            cubesParent.transform.position = Vector3.zero;
        }
        OnFloorComplete += DisplayAllSpawnDatas;
        GenerateArena(gridSizeX, gridSizeZ);
        Debug.Log("CubeManager initialized");
    }

    public void GenerateArena(int gridSizex = 6, int gridSizeZ = 6)
    {
        DestoryAllCubes();
        for (int x = 0; x < gridSizex; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                int randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                // check if random height value is an even number, otherwise redo assignment
                while (randomHeight % 2 != 0)
                {
                    //Debug.Log($"randomHieght is odd - {randomHeight}");
                    randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                }

                int id = SpawnDatas.Count;
                Vector3 cubePosition = new Vector3(x * CubeSize, randomHeight, z * CubeSize);
                ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);

                if (color != ColorOption.Neutral)
                {
                    while ((CheckIfColorIsNearby(id, cubePosition, color) || colorsUsed.Contains(color)) && attempts < maxAttempts)
                    {
                        color = (ColorOption)UnityEngine.Random.Range(0, 4);
                        attempts++;
                        if (attempts == maxAttempts)
                        {
                            color = ColorOption.Neutral;
                            attempts = 0;
                            break;
                        }
                    }
                }

                colorsUsed.Clear();

                // Check if the cube should float
                if (UnityEngine.Random.value < floatProbability)
                {
                    float floatingHeight = maxHeight + UnityEngine.Random.Range(1f, 5f); // Round to the nearest integer
                    cubePosition.y = floatingHeight;
                }

                //Debug.Log($"Adding new SpawnData:\n\tid: {id}" +
                //    $"\n\tcubePosition: {cubePosition}" +
                //    $"\n\tcolor: {color}");

                SpawnData spawnData = new SpawnData { id = id, position = cubePosition, color = color };
                SpawnCube(spawnData);

                colorsUsed.Add(color);

                // traverses down the y axis and adds a cube at each position until it reaches the ground or y = 0
                if (cubePosition.y > 0)
                {
                    for (int i = (int)cubePosition.y - (int)CubeSize; i < cubePosition.y; i = i - (int)CubeSize)
                    {
                        id = SpawnDatas.Count;
                        Vector3 groundPos = new Vector3(cubePosition.x, i, cubePosition.z);

                        //Debug.Log($"Adding new SpawnData:\n\tid: {id}" +
                        //                  $"\n\tcubePosition: {cubePosition}" +
                        //                  $"\n\tcolor: {color}");

                        SpawnData groundSpawnData = new SpawnData
                        {
                            id = id,
                            position = groundPos,
                            color = ColorOption.Neutral
                        };

                        SpawnCube(groundSpawnData);

                        if (i == 0)
                            break;
                    }
                }
            }
        }
        OnFloorComplete?.Invoke();
    }

    public float minimumDistance = 4f;
    public bool CheckIfColorIsNearby(int id, Vector3 position, ColorOption color)
    {
        if (SpawnDatas.Count == 0)
            return false;
        Debug.Log($"checking if cube({id}) has a similar color({color}) near its position: /n/t{position} ");

        // Check each adjacent cube
        foreach (Vector3 offset in offsets)
        {
            Vector3 adjacentPos = position + (offset * CubeSize); // Multiply by cubesize to get the adjacent cube
            Debug.Log($"checking cube({id} - {color})'s position({position}) adjacentPos: {adjacentPos}");
            // Check if a cube exists at the adjacent position
            SpawnData adjacentCube = SpawnDatas.Find(spawnData => spawnData.position == adjacentPos);

            if (adjacentCube.color == color)
            {
                Debug.Log($"\n\t\tchecking cube({id} is same color as adjacentCube({adjacentCube.id})\n\t\t{color} == {adjacentCube.color}");
                // Calculate the distance between the cubes
                float distance = Vector3.Distance(position, adjacentPos);

                // If the distance is less than the minimumDistance, disallow spawning

                if (distance < minimumDistance)
                {
                    Debug.Log($"Failed to spawn - Cube {adjacentCube.id} and {id} have the same Color {color} and are too close (distance: {distance})");
                    return true;
                }
            }
        }
        return false;
    }

    void DisplayAllSpawnDatas()
    {
        foreach (SpawnData data in spawnDatas)
        {
            Debug.Log($"id: {data.id}, position: {data.position}, color: {data.color}");
        }
    }
    private void OnDestroy()
    {
        OnFloorComplete -= DisplayAllSpawnDatas;
    }

    public void SpawnCube(SpawnData data)
    {
        spawnDatas.Add(data);
        var cube = Instantiate(cubePrefab, data.position, Quaternion.identity, cubesParent);
        cube.GetComponent<CubeBehavior>().InitializeCube(data.id, data.color); // this should allow some color colored cubes at some point
        cubes.Add(cube);
    }

    // This adds a delay between spawning cubes, which is nice for debugging and looks cool.
    private IEnumerator SpawnCubesWithDelay()
    {
        foreach (SpawnData data in spawnDatas)
        {
            yield return new WaitForSeconds(spawnDelay);
            var cube = Instantiate(cubePrefab, data.position, Quaternion.identity, cubesParent);
            cube.GetComponent<CubeBehavior>().InitializeCube(data.id, data.color); // this should allow some color colored cubes at some point
            cubes.Add(cube);
        }

        // organize cubes list by id
        cubes.Sort((x, y) => x.GetComponent<CubeBehavior>().id.CompareTo(y.GetComponent<CubeBehavior>().id));
        Debug.Log($"{GetTotalCubeCount()} cubes spawned");
    }

    private int GetTotalCubeCount()
    {
        return cubes.Count;
    }

    public void DestoryAllCubes()
    {
        if (cubes.Count == 0)
        {
            Debug.Log("No cubes to destroy");
            return;
        }
        Debug.Log("Destroying cubes!");
        foreach (GameObject cube in cubes)
            Destroy(cube);

        cubes.Clear();
        spawnDatas.Clear();
        //cubes = new List<GameObject>();
        Debug.Log("post destory - cubes.Count: " + cubes.Count);
    }
}

// Enum to represent different colors
public enum ColorOption
{
    Neutral,
    Red,
    Green,
    Blue
}

public struct SpawnData
{
    public int id;
    public Vector3 position;
    public ColorOption color;
}
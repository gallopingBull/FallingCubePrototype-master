using System;
using System.Collections.Generic;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public int minHeight = 1;
    public int maxHeight = 5;
    public float spacing = 2f;
    public float floatProbability = 0.2f; // Probability of a cube floating

    const int maxAttempts = 10;
    int attempts = 0;

    // Define the offsets for adjacent cubes in a 3D grid
    Vector3[] offsets = new Vector3[]
    {
        Vector3.forward, Vector3.back, Vector3.up,
        Vector3.down, Vector3.right, Vector3.left
    };

    [SerializeField] List<GameObject> cubes;
    [SerializeField] List<SpawnData> spawnDatas;
    [SerializeField] List<ColorOption> colorsUsed;

    static public Action OnFloorComplete { get; set; }

    void Start()
    {
        SpawnTerrainWithFloatingCubes();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DestoryAllCubes();
            SpawnTerrainWithFloatingCubes();
        }
    }

    void SpawnTerrainWithFloatingCubes()
    {
        cubes = new List<GameObject>();
        spawnDatas = new List<SpawnData>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Debug.Log($"x: {x} z: {z}");
                int randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                // check if random height value is an even number, otherwise redo assignment
                while (randomHeight % 2 != 0)
                {
                    //Debug.Log($"randomHieght is odd - {randomHeight}");
                    randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                }

                Debug.Log($"*** cubes.count: {cubes.Count} ***");
                int id = cubes.Count;
                Vector3 cubePosition = new Vector3(x * spacing, randomHeight, z * spacing);
                ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);

                Debug.Log($"Attempt new spawn with:\n\tid: {id}" +
                    $"\n\tcubePosition: {cubePosition}" +
                    $"\n\tcolor: {color}");

                if (color != ColorOption.Neutral)
                {
                    while ((CheckIfColorIsNearby(id, cubePosition, color) || colorsUsed.Contains(color)) && attempts < maxAttempts)
                    {
                        color = (ColorOption)UnityEngine.Random.Range(0, 4);
                        //Debug.Log($"in while loop - color is {color} now - attempt: {attempts}");
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


                SpawnData spawnData = new SpawnData { id = id, position = cubePosition, color = color };

                // Spawn cube.
                var cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);
                cube.GetComponent<BlockBehavior>().InitializeCube(id, color);
                cubes.Add(cube);
                spawnDatas.Add(spawnData);

                colorsUsed.Add(color);

                // traverses down the y axis and adds a cube at each position until it reaches the ground or y = 0
                if (cubePosition.y > 0)
                {
                    for (int i = (int)cubePosition.y - (int)spacing; i < cubePosition.y; i = i - (int)spacing)
                    {
                        id = cubes.Count;
                        Vector3 groundPos = new Vector3(cubePosition.x, i, cubePosition.z);

                        SpawnData groundSpawnData =
                        new SpawnData { id = id, position = groundPos, color = ColorOption.Neutral };

                        Debug.Log($"\t\ttraversing down {id} y position!");
                        Debug.Log($"\t\tcurrent cube count: {cubes.Count}!");

                        var groundCube = Instantiate(cubePrefab, groundPos, Quaternion.identity);
                        groundCube.GetComponent<BlockBehavior>().InitializeCube(id, ColorOption.Neutral); // this should allow some color colored cubes at some point
                        
                        Debug.Log($"\t\tnew spawn with:" +
                            $"\n\t\t\t\tid: {id}" +
                            $"\n\t\t\t\tcubePosition: {cubePosition}" +
                            $"\n\t\t\t\tcolor: {color}");
                        
                        cubes.Add(groundCube);
                        spawnDatas.Add(groundSpawnData);

                        if (i == 0)
                            break;
                    }
                }

            }
        }
        // organize cubes list by id
        cubes.Sort((x, y) => x.GetComponent<BlockBehavior>().Id.CompareTo(y.GetComponent<BlockBehavior>().Id));
        Debug.Log($"{GetTotalCubeCount()} cubes spawned");
        OnFloorComplete?.Invoke();
    }

    private bool CheckIfColorIsNearby(int id, Vector3 position, ColorOption color)
    {
        if (spawnDatas.Count == 0)
            return false;
        Debug.Log($"checking if cube({id}) has a similar color({color}) near its position: /n/t{position} ");

        // Check each adjacent cube
        foreach (Vector3 offset in offsets)
        {
            Vector3 adjacentPos = position + (offset * 2); // Multiply by 2 to get the adjacent cube
            //Debug.Log($"adjacentPos: {adjacentPos}");
            // Check if a cube exists at the adjacent position
            SpawnData adjacentCube = spawnDatas.Find(spawnData => spawnData.position == adjacentPos);
            //Debug.Log($"adjacentCube.id: {adjacentCube.id}");
            if (adjacentCube.color == color)
            {
                Debug.Log($"fail - {adjacentCube.id} and {id} the same Color {color}");
                return true;
            }
        }
        return false;
    }

    private int GetTotalCubeCount()
    {
        return cubes.Count;
    }


    private void DestoryAllCubes()
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

struct SpawnData
{
    public int id;
    public Vector3 position;
    public ColorOption color;
}
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

    [SerializeField] List<GameObject> cubes;
    [SerializeField] List<SpawnData> spawnDatas;
    [SerializeField] List<ColorOption> colorsUsed;
    List<bool> spawnedLocs;

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
                int randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                // check if random height value is an even number, otherwise redo assignment
                while (randomHeight % 2 != 0)
                {
                    //Debug.Log($"randomHieght is odd - {randomHeight}");
                    randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                }

                Vector3 cubePosition = new Vector3(x * spacing, randomHeight, z * spacing);
                ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);
                if (color != ColorOption.Neutral)
                {
                    while ((CheckIfColorIsNearby(cubePosition, color) || colorsUsed.Contains(color)) && attempts < maxAttempts)
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

                int id = cubes.Count + 1;
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
                        Vector3 groundPos = new Vector3(cubePosition.x, i, cubePosition.z);
                        var groundCube = Instantiate(cubePrefab, groundPos, Quaternion.identity);

                        cube.GetComponent<BlockBehavior>().InitializeCube(id, ColorOption.Neutral); // this should allow some color colored cubes at some point
                        cubes.Add(groundCube);

                        SpawnData groundSpawnData = new SpawnData { position = groundPos, color = ColorOption.Neutral };
                        spawnDatas.Add(groundSpawnData);

                        if (i == 0)
                            break;
                    }
                }

            }
        }

        Debug.Log($"{GetTotalCubeCount()} cubes spawned");
        OnFloorComplete?.Invoke();
    }

    private bool CheckNearbyIDsForColor(int id, ColorOption color)
    {
        if (spawnDatas.Count == 0)
            return false;

        // check if color is already assigned to a cube nearby (based on x and z positions).
        foreach (SpawnData spawnData in spawnDatas)
        {
            if (spawnData.color == color)
            {
                // Compare only the x and z positions
                Vector2 currentPos = new Vector2(spawnDatas[id].position.x, spawnDatas[id].position.z);
                Vector2 spawnPos = new Vector2(spawnData.position.x, spawnData.position.z);

                if (Vector2.Distance(currentPos, spawnPos) < 2)
                    return true;
            }
        }
        return false;
    }

    private bool CheckIfColorIsNearby(Vector3 position, ColorOption color)
    {
        // Define the offsets for adjacent cubes in a 3D grid
        Vector3[] offsets = new Vector3[]
        {
        new Vector3(-1, -1, -1), new Vector3(-1, -1, 0), new Vector3(-1, -1, 1),
        new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1),
        new Vector3(-1, 1, -1), new Vector3(-1, 1, 0), new Vector3(-1, 1, 1),
        new Vector3(0, -1, -1), new Vector3(0, -1, 0), new Vector3(0, -1, 1),
        new Vector3(0, 0, -1), /* skip (0, 0, 0) */ new Vector3(0, 0, 1),
        new Vector3(0, 1, -1), new Vector3(0, 1, 0), new Vector3(0, 1, 1),
        new Vector3(1, -1, -1), new Vector3(1, -1, 0), new Vector3(1, -1, 1),
        new Vector3(1, 0, -1), new Vector3(1, 0, 0), new Vector3(1, 0, 1),
        new Vector3(1, 1, -1), new Vector3(1, 1, 0), new Vector3(1, 1, 1)
        };

        // Check each adjacent cube
        foreach (Vector3 offset in offsets)
        {
            Vector3 adjacentPos = position + offset;

            // Check if a cube exists at the adjacent position
            SpawnData adjacentCube = spawnDatas.Find(spawnData => spawnData.position == adjacentPos);
            Debug.Log($"{adjacentCube.id}");
            if (adjacentCube.color == color)
            {
                Debug.Log($"fail - {adjacentCube.id}Color {color} is nearby");
                return true;
            }
        }

        return false;
    }

    private bool CheckIfColorIsAboveOrBelow(Vector3 position, ColorOption color)
    {
        if (spawnDatas.Count == 0)
            return false;

        // check if color is already assigned to a cube above or below (based on x and z positions).
        foreach (SpawnData spawnData in spawnDatas)
        {
            if (spawnData.color == color)
            {
                // Compare only the x and z positions
                Vector2 currentPos = new Vector2(position.x, position.z);
                Vector2 spawnPos = new Vector2(spawnData.position.x, spawnData.position.z);

                float distance = Vector2.Distance(currentPos, spawnPos);
                Debug.Log($"{spawnData.id} distance :{distance}");
                if (distance < 2 /*&& Mathf.Abs(position.y - spawnData.position.y) <= 1*/)
                {
                    Debug.Log($"fail same {spawnData.id}Color  {color} is nearby");
                    return true;
                }
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

        Debug.Log("Destroying cubes");
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
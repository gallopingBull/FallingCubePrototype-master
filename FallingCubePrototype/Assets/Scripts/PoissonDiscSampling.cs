using System.Collections.Generic;
using UnityEngine;

public class PoissonDiscSampling : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public int minHeight = 1;
    public int maxHeight = 5;
    public float spacing = 2f;
    public float floatProbability = 0.2f; // Probability of a cube floating

    [SerializeField] List<GameObject> cubes;
    [SerializeField] List<SpawnData> spawnDatas;
    List<bool> spawnedLocs;

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
                int randomHeight = Random.Range(minHeight, maxHeight + 1);
                // check if random height value is an even number, otherwise redo assignment
                while (randomHeight % 2 != 0)
                {
                    Debug.Log($"randomHieght is odd - {randomHeight}");
                    randomHeight = Random.Range(minHeight, maxHeight + 1);
                }

                Vector3 cubePosition = new Vector3(x * spacing, randomHeight, z * spacing);
                ColorOption color = (ColorOption)Random.Range(0, 4); // 0, 1, 2

                // check if color is already assigned to a cube nearby.

                // Check if the cube should float
                if (Random.value < floatProbability)
                {
                    float floatingHeight = maxHeight + Random.Range(1f, 5f); // Round to the nearest integer
                    cubePosition.y = floatingHeight;
                }

                // Spawn cube.
                var cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);  
                cube.GetComponent<BlockBehavior>().InitializeCube(color);
                cubes.Add(cube);    

                SpawnData spawnData = new SpawnData{ position = cubePosition, color = color };
                spawnDatas.Add(spawnData);

                // traverses down the y axis and adds a cube at each position until it reaches the ground or y = 0
                if(cubePosition.y > 0)
                {
                    for (int i = (int)cubePosition.y - 2; i < cubePosition.y; i = i - 2)
                    {
                        Vector3 groundPos = new Vector3(cubePosition.x, i, cubePosition.z); 

                        var groundCube = Instantiate(cubePrefab, groundPos, Quaternion.identity);
                        cube.GetComponent<BlockBehavior>().InitializeCube(ColorOption.Neutral); // this should allow some color colored cubes at some point
                        cubes.Add(groundCube);

                        SpawnData groundSpawnData = new SpawnData { position = groundPos, color = ColorOption.Neutral };
                        spawnDatas.Add(groundSpawnData);
                        if (i == 0)
                            break;
                    }
                }
            }
        }
    }

    private void DestoryAllCubes()
    {
        if (cubes.Count > 0)
        {
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }
        }
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
    public Vector3 position;
    public ColorOption color;
}
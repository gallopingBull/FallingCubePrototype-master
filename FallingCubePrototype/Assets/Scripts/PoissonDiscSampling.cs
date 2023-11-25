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

                // Add to CubeSpawnPosition list
                // Spawn cube.
                cubes.Add(Instantiate(cubePrefab, cubePosition, Quaternion.identity));
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

struct CubeSpawnPosition
{
    Vector3 position;
    ColorOption color;
}
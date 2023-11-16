using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PoissonDiscSampling : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public int minHeight = 1;
    public int maxHeight = 5;
    public float spacing = 2f;

    void Start()
    {
        SpawnCubesInGrid();
    }

    void SpawnCubesInGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                int randomHeight = Random.Range(minHeight, maxHeight + 1); // Adding 1 to include the upper bound
                Vector3 cubePosition = new Vector3(x * spacing, randomHeight, z * spacing);
                Instantiate(cubePrefab, cubePosition, Quaternion.identity);
            }
        }
    }
}
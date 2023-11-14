using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PoissonDiscSampling : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject tmpCube;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float spacing = 2f;
    public float minHeight = 1f;
    public float maxHeight = 5f;

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
                float randomHeight = Random.Range(minHeight, maxHeight);
                Vector3 cubePosition = new Vector3(x * spacing, randomHeight, z * spacing);
                Instantiate(cubePrefab, cubePosition, Quaternion.identity);
                //tmpCube.GetComponent<BlockBehavior>().SetGround();
            }
        }
    }
}
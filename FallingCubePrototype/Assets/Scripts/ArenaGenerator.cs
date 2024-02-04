using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public int minHeight = 0;
    public int maxHeight = 5;

    // cube size determined by the scale of the cube prefab and the spacing between cubes.
    public float CubeSize = 2f;
    public float floatProbability = 0.2f; // Probability of a cube floating

    private const int maxAttempts = 10;
    private int attempts = 0;

    // Define the offsets for adjacent cubes in a 3D grid
    private Vector3[] offsets = new Vector3[]
    {
        Vector3.forward, Vector3.back, Vector3.up,
        Vector3.down, Vector3.right, Vector3.left
    };

    [SerializeField] List<ColorOption> colorsUsed;
    private CubeManager cubeManager;

    static public Action OnFloorComplete { get; set; } // maybe this should be in CubeManager?

    void Awake()
    {
        cubeManager = GetComponent<CubeManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateArena();
        }
    }

    public void GenerateArena(int gridSizex = 6, int gridSizeZ = 6)
    {
        cubeManager.DestoryAllCubes();
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

                int id = cubeManager.SpawnDatas.Count;
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
                cubeManager.SpawnCube(spawnData);

                colorsUsed.Add(color);

                // traverses down the y axis and adds a cube at each position until it reaches the ground or y = 0
                if (cubePosition.y > 0)
                {
                    for (int i = (int)cubePosition.y - (int)CubeSize; i < cubePosition.y; i = i - (int)CubeSize)
                    {
                        id = cubeManager.SpawnDatas.Count;
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

                        cubeManager.SpawnCube(groundSpawnData);

                        if (i == 0)
                            break;
                    }
                }
            }
        }
        OnFloorComplete?.Invoke();
    }

    private bool CheckIfColorIsNearby(int id, Vector3 position, ColorOption color)
    {
        if (cubeManager.SpawnDatas.Count == 0)
            return false;
        //Debug.Log($"checking if cube({id}) has a similar color({color}) near its position: /n/t{position} ");

        // Check each adjacent cube
        foreach (Vector3 offset in offsets)
        {
            Vector3 adjacentPos = position + (offset * CubeSize); // Multiply by 2(spacing) to get the adjacent cube

            // Check if a cube exists at the adjacent position
            SpawnData adjacentCube = cubeManager.SpawnDatas.Find(spawnData => spawnData.position == adjacentPos);

            if (adjacentCube.color == color)
            {
                //Debug.Log($"fail - {adjacentCube.id} and {id} the same Color {color}");
                return true;
            }
        }
        return false;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public GameObject cubePrefab;

    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    Vector2Int maxGridSize = new Vector2Int(10, 10); // Maximum grid size of the map

    public int minHeight = 1;
    public int maxHeight = 5;

    public List<GridPoint> gridPoints = new List<GridPoint>();





    // cube size determined by the scale of the cube prefab and the spacing between cubes.
    public float CubeSize = 2f;
    
    private Transform cubesParent;
    public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();

    // Start is called before the first frame update
    void Start()
    {
        cubesParent = new GameObject("Cubes").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        // 
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public GameObject gridPointPrefab; 

    // List of cube types to select and create in level editor. 
    public List<GameObject> cubeTypePrefab;

    public int maxGridSizeX = 10;
    public int maxGridSizeY = 10;
    public int maxGridSizeZ = 10;

    //Vector3 maxGridSize = new Vecto3(10, 10); // Maximum grid size of the map

    public int minHeight = 1;
    public int maxHeight = 5;

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    private float h, z;

    public List<GameObject> gridPoints = new List<GameObject>();
    private Transform gridMapParent;
    //public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();

    // Start is called before the first frame update
    void Start()
    {
        gridMapParent = new GameObject("SelectionGridMap").transform;
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //InputHandler();
    }

    private void Init()
    {
        // SelectGridPoint(0, 0)
        InputHandler();
        GenerateSelectionGrid();
    }

    private void GenerateSelectionGrid()
    {
        for (int i = 0; i < maxGridSizeX; i++) 
        {
            for(int j = 0; j < maxGridSizeY; j++)
            {
                for (int k = 0; k < maxGridSizeZ + 1; k++) 
                {
                    Vector3 pos = new Vector3(i, j, k);
                    var gridPoint = Instantiate(gridPointPrefab, pos, transform.rotation); // assume rotation is (0,0,0).
                    //GridPoint p = new GridPoint();      
                    gridPoint.transform.parent = gridMapParent;
                    gridPoints.Add(gridPoint);
                }
            }   
        }
    }

    private void InputHandler() 
    {

        if (Input.GetAxis("LeftAnalogHorizontal") != 0 || Input.GetAxis("LeftAnalogVertical") != 0)
        {
            h = Input.GetAxis("LeftAnalogHorizontal");
            z = Input.GetAxis("LeftAnalogVertical");

            // Check if gamepad stick input is within dead zone
            if (Mathf.Abs(h) < gamepadDeadzone)
            {
                h = 0f;
            }
            if (Mathf.Abs(z) < gamepadDeadzone)
            {
                z = 0f;
            }
        }
        else
        {
            h = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

    }

    private void SelectGridPoint() { }

    private void SelectCubeType() { }

    private void GenerateCube() { }

    private void DeleteCube() { }

    private void DeleteAllCubes() { }
}

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
    private GameObject currentGridPoint;
    //public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<Material> selectionMats = new List<Material>();
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
        InputHandler();
    }

    private void Init()
    {
        GenerateSelectionGrid();
        SelectGridPoint(Vector3.zero);
    }

    private void GenerateSelectionGrid()
    {
        if (gridPoints != null)
            DeleteAllGridPoints();

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
        if (Input.GetAxis("LeftAnalogHorizontal") == 0 || Input.GetAxis("LeftAnalogVertical") == 0)
            return;
        if (Input.GetAxis("LeftAnalogHorizontal") != 0 || Input.GetAxis("LeftAnalogVertical") != 0)
        {
            h = Input.GetAxis("LeftAnalogHorizontal");
            z = Input.GetAxis("LeftAnalogVertical");

            // Check if gamepad stick input is within dead zone
            //if (Mathf.Abs(h) < gamepadDeadzone)
            //{
            //    h = 0f;
            //}
            //if (Mathf.Abs(z) < gamepadDeadzone)
            //{
            //    z = 0f;
            //}
        }
        //else
        //{
        //    h = Input.GetAxis("Horizontal");
        //    z = Input.GetAxis("Vertical");
        //}

        int y = 0;
        //if (Input.GetButtonDown("Up"))
        //{
        //    y = 1;  
        //}
        //
        //if (Input.GetButtonDown("Down"))
        //{
        //    y = -1;
        //}

        Vector3 position = new Vector3(h, y, z);
        // Get this grid point, if em
        SelectGridPoint(position);  
    }

    private void SelectGridPoint(Vector3 position) 
    {
        Debug.Log($"position: {position}");
        if (currentGridPoint == null)
        {
            currentGridPoint = GetGridPointByPosition(position);
            currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];

        }
        else
        {
            // same grid point! 
            if (currentGridPoint.transform.position == (position * 2) + currentGridPoint.transform.position)
                return;

            //DeSelectGridPoint();

            // Select grid point here
            currentGridPoint = GetGridPointByPosition((position * 2) + currentGridPoint.transform.position);
            currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];
        }
    }

    private void DeSelectGridPoint() 
    {
        if (currentGridPoint == null)
            return;

        // change material is the
        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[1];
        currentGridPoint = null;
    }

    private void SelectCubeType() { }

    private void GenerateCube() { }

    private void DeleteCube() { }

    private void DeleteAllCubes() 
    {
        // I should be able to do this.
        //ArenaGenerator.DestroyAllCubes();
    }

    public GameObject GetGridPointByPosition(Vector3 position)
    {
        GameObject gridPoint = gridPoints.Find(gp => gp.transform.position == position);
        if (gridPoint == null)
            return null;
        return gridPoint;
    }

    private void DeleteAllGridPoints() 
    {
        foreach (GameObject gridPoint in gridPoints)
            Destroy(gridPoint);
        gridPoints.Clear();
    }

    // Serialize SpawnData to json file
    private void SaveSpawnData() { }
    
    // Load serialize SpawnData from json file
    private void LoadSpawnData() { }
}

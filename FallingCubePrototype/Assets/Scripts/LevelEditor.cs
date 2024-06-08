using Invector.vCamera;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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
    private int cubeScale = 2; 

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    private float h, z;

    public List<GameObject> gridPoints = new List<GameObject>();
    private Transform gridMapParent;
    private GameObject currentGridPoint;
    //public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<Material> selectionMats = new List<Material>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();

    /// //// //// 
    // Allows you to hold down a key for movement.
    [SerializeField] private bool isRepeatedMovement = false;
    // Time in seconds to move between one grid position and the next.
    [SerializeField] private float moveDuration = 0.1f;
    // The size of the grid
    [SerializeField] private float gridSize = 2f;
    private bool isMoving = false;

    /// //// //// 


    // Start is called before the first frame update
    void Start()
    {
        gridMapParent = new GameObject("SelectionGridMap").transform;
        Init();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Only process on move at a time.
        if (!isMoving)
        {
            InputHandler();

            // Accomodate two different types of moving.
            //Func<KeyCode, bool> inputFunction;
            //if (isRepeatedMovement)
            //{
            //    // GetKey repeatedly fires.
            //    inputFunction = Input.GetKey;
            //}
            //else
            //{
            //    // GetKeyDown fires once per keypress
            //    inputFunction = Input.GetKeyDown;
            //}
            //
            //// If the input function is active, move in the appropriate direction.
            //if (inputFunction(KeyCode.UpArrow))
            //{
            //    StartCoroutine(Move(Vector2.up));
            //}
            //else if (inputFunction(KeyCode.DownArrow))
            //{
            //    StartCoroutine(Move(Vector2.down));
            //}
            //else if (inputFunction(KeyCode.LeftArrow))
            //{
            //    StartCoroutine(Move(Vector2.left));
            //}
            //else if (inputFunction(KeyCode.RightArrow))
            //{
            //    StartCoroutine(Move(Vector2.right));
            //}
        }
    }

    private void Init()
    {
        GenerateSelectionGrid();
        currentGridPoint = GetGridPointByPosition(Vector3.zero);
        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];
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
                    var gridPoint = Instantiate(gridPointPrefab, pos * cubeScale, transform.rotation); // assume rotation is (0,0,0).
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
        {
            return;
        }
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

        Vector3 direction = new Vector3(Mathf.RoundToInt(h), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
        // Clamp target position within the boundaries of the map
        direction.x = Mathf.Clamp(direction.x, y, maxGridSizeX);
        direction.z = Mathf.Clamp(direction.z, y, maxGridSizeY);
        // Get this grid point, if em
        StartCoroutine(MoveToSelection(direction));  
    }

    private IEnumerator MoveToSelection(Vector3 dir) 
    {
        isMoving = true;

        Vector3 targetPosition = currentGridPoint.transform.position + (dir * cubeScale);
        Debug.Log($"targetPosition: {targetPosition}");
        Debug.Log($"dir: {dir}");

        yield return new WaitForSeconds(.1f);

        SelectGridPoint(targetPosition);

        isMoving = false;
    }

    private void SelectGridPoint(Vector3 targetPosition)
    {
        var prevGP = currentGridPoint;

        // Select grid point here
        if (targetPosition.x >= maxGridSizeX || targetPosition.z >= maxGridSizeZ)
        {
            Debug.Log("returning null");
            currentGridPoint = prevGP;
            currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];
        }
        else
        {
            currentGridPoint = GetGridPointByPosition(targetPosition);
            DeSelectGridPoint(prevGP);
            if (currentGridPoint == null)
                currentGridPoint = prevGP;
        }

        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];
    }


    private void DeSelectGridPoint(GameObject gp) 
    {
        if (gp == null)
            return;

        // change material is the
        gp.GetComponentInChildren<Renderer>().material = selectionMats[1];
    }

    private void SelectCubeType() { }

    private void GenerateCube() 
    {
        // I should be able to do this.
        //ArenaGenerator.GenerateCube();
    }

    private void DeleteCube() 
    {
        // I should be able to do this.
        //ArenaGenerator.DeleteCube();
    }

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

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
        StartCoroutine(SelectGridPoint(Vector3.zero));
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

        Vector3 targetPosition = new Vector3(Mathf.RoundToInt(h), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(targetPosition.x * cubeScale, 0, maxGridSizeX * cubeScale);
        targetPosition.z = Mathf.Clamp(targetPosition.z * cubeScale, 0, maxGridSizeY * cubeScale);
        // Get this grid point, if em
        StartCoroutine(SelectGridPoint(targetPosition));
        //SelectGridPoint(targetPosition);  
    }

    private IEnumerator SelectGridPoint(Vector3 position) 
    {
        isMoving = true;
        Debug.Log($"position: {position}");
        yield return new WaitForSeconds(.1f);

        if (currentGridPoint == null)
        {
            currentGridPoint = GetGridPointByPosition(position);
            Debug.Log($"currentGridPoint.name: {currentGridPoint.name}");
            currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];

        }
        else
        {
            var prevGP = currentGridPoint;

            if (position.x == maxGridSizeX)
            {
                //isMoving = false;

                yield return null;
            }

            if (position.z == maxGridSizeZ)
            {
                //isMoving = false;

                yield return null;
            }

            currentGridPoint = GetGridPointByPosition(position + currentGridPoint.transform.position);
            if (currentGridPoint)
            {
                DeSelectGridPoint(prevGP);
                currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];

            }
            else
            { currentGridPoint = prevGP; }

            // Select grid point here
        }
        isMoving = false;

    }


    private void DeSelectGridPoint(GameObject gp) 
    {
        if (gp == null)
            return;

        // change material is the
        gp.GetComponentInChildren<Renderer>().material = selectionMats[1];
    }

    // Smooth movement between grid positions.
    private IEnumerator Move(Vector2 direction)
    {
        // Record that we're moving so we don't accept more input.
        isMoving = true;

        // Make a note of where we are and where we are going.
        Vector2 startPosition = currentGridPoint.transform.position;
        Vector2 endPosition = startPosition + (direction * gridSize);

        // Smoothly move in the desired direction taking the required time.
        float elapsedTime = 0;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / moveDuration;
            var test = Vector2.Lerp(startPosition, endPosition, percent);
            yield return null;
        }


        // same grid point! 
        //if (currentGridPoint.transform.position == endPosition)
        //    yield return null;

       // DeSelectGridPoint(prevGP);

        // Select grid point here
        currentGridPoint = GetGridPointByPosition(endPosition);
        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];


        // Make sure we end up exactly where we want.
        //transform.position = endPosition;
     
        // We're no longer moving so we can accept another move input.
        isMoving = false;
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

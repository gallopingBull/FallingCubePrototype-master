using Invector.vCamera;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public GameObject gridPointPrefab;

    // List of cube types to select and create in level editor.
    public List<GameObject> cubeTypePrefab;

    public int maxGridSizeX = 10;
    public int maxGridSizeY = 10;
    public int maxGridSizeZ = 10;

    public int minHeight = 1;
    public int maxHeight = 5;
    private int cubeScale = 2;

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    private float h, z;

    public List<GameObject> gridPoints = new List<GameObject>();
    private Transform gridMapParent;
    private GameObject currentGridPoint;
    [SerializeField] List<Material> selectionMats = new List<Material>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();

    [SerializeField] private bool isRepeatedMovement = false;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float gridSize = 2f;
    private bool isMoving = false;

    void Start()
    {
        gridMapParent = new GameObject("SelectionGridMap").transform;
        Init();
    }

    void FixedUpdate()
    {
        if (!isMoving)
        {
            InputHandler();
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
            for (int j = 0; j < maxGridSizeY; j++)
            {
                for (int k = 0; k < maxGridSizeZ + 1; k++)
                {
                    Vector3 pos = new Vector3(i, j, k);
                    var gridPoint = Instantiate(gridPointPrefab, pos * cubeScale, transform.rotation); // assume rotation is (0,0,0).
                    gridPoint.transform.parent = gridMapParent;
                    gridPoints.Add(gridPoint);
                }
            }
        }
    }

    private void InputHandler()
    {
        h = Input.GetAxis("LeftAnalogHorizontal");
        z = Input.GetAxis("LeftAnalogVertical");

        if (Mathf.Abs(h) < gamepadDeadzone)
        {
            h = 0f;
        }
        if (Mathf.Abs(z) < gamepadDeadzone)
        {
            z = 0f;
        }

        if (h == 0 && z == 0)
        {
            return;
        }

        int y = 0;

        Vector3 direction = new Vector3(Mathf.RoundToInt(h), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
        StartCoroutine(MoveToSelection(direction));
    }

    private IEnumerator MoveToSelection(Vector3 dir)
    {
        isMoving = true;

        Vector3 targetPosition = currentGridPoint.transform.position + (dir * cubeScale);

        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, (maxGridSizeX - 1) * cubeScale);
        targetPosition.z = Mathf.Clamp(targetPosition.z, 0, (maxGridSizeZ - 1) * cubeScale);

        yield return new WaitForSeconds(moveDuration);

        SelectGridPoint(targetPosition);

        isMoving = false;
    }

    private void SelectGridPoint(Vector3 targetPosition)
    {
        var prevGP = currentGridPoint;

        // Select grid point here
        currentGridPoint = GetGridPointByPosition(targetPosition);
        if (currentGridPoint == null)
        {
            currentGridPoint = prevGP;
        }
        else
        {
            DeSelectGridPoint(prevGP);
        }

        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];
    }

    private void DeSelectGridPoint(GameObject gp)
    {
        if (gp == null)
            return;

        gp.GetComponentInChildren<Renderer>().material = selectionMats[1];
    }

    private void DeleteAllGridPoints()
    {
        foreach (GameObject gridPoint in gridPoints)
            Destroy(gridPoint);
        gridPoints.Clear();
    }

    public GameObject GetGridPointByPosition(Vector3 position)
    {
        return gridPoints.Find(gp => gp.transform.position == position);
    }

    // Serialize SpawnData to json file
    private void SaveSpawnData() { }

    // Load serialize SpawnData from json file
    private void LoadSpawnData() { }
}

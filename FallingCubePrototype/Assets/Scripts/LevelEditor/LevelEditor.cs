using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LevelEditor : MonoBehaviour
{
    public GameObject gridPointPrefab;

    // List of cube types to select and create in level editor.
    public List<GameObject> cubeTypePrefab;

    public int maxGridSizeX = 10;
    public int maxGridSizeY = 10;
    public int maxGridSizeZ = 10;

    private int gridSize = 2; // grid size is based off cube scale

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    private float h, z;

    public List<GameObject> gridPoints = new List<GameObject>();
    private Transform gridMapParent;
    private GameObject currentGridPoint;
    [SerializeField] List<Material> selectionMats = new List<Material>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();

    [SerializeField] private float moveDuration = 0.1f;
    private bool isMoving = false;

    // Camera Related Fields
    private Camera cam;
    public Transform cameraTarget; // The object to focus on
    public float rotationSpeed = 100.0f; // Speed of camera rotation
    public float zoomSpeed = 10.0f; // Speed of zooming
    public float minZoomDistance = 5.0f; // Minimum distance to the target
    public float maxZoomDistance = 20.0f; // Maximum distance from the target
    public float transitionSpeed = 2.0f; // Speed of smooth transition to the new target
    private float currentZoomDistance; // Current distance from the target
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isTransitioning = false;

    void Start() => Init(); 

    void Update()
    {
        if (!isMoving)
            InputHandler();

        if (cameraTarget)
        {
            if (isTransitioning)
            {
                SmoothTransition();
            }
            else
            {
                // Rotate the camera around the target object based on input
                RotateAroundCameraTarget();

                // Zoom the camera in and out based on input
                ZoomOnCameraTarget();
            }

        }
    }

    private void Init()
    {
        gridMapParent = new GameObject("SelectionGridMap").transform;
        cam = Camera.main;
        
        GenerateSelectionGrid();
        currentGridPoint = GetGridPointByPosition(Vector3.zero);
        currentGridPoint.GetComponentInChildren<Renderer>().material = selectionMats[0];

        cameraTarget = currentGridPoint.transform;
        //cameraTarget.position = transform.position;
        //cameraTarget.rotation = transform.rotation;
        currentZoomDistance = Vector3.Distance(cam.transform.position, cameraTarget.position);
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
                    var gridPoint = Instantiate(gridPointPrefab, pos * gridSize, transform.rotation); // assume rotation is (0,0,0).
                    gridPoint.transform.parent = gridMapParent;
                    gridPoints.Add(gridPoint);
                }
            }
        }
    }

    private void InputHandler()
    {
        // Get horizontal and vertical analog stick input
        h = Input.GetAxis("LeftAnalogHorizontal");
        z = Input.GetAxis("LeftAnalogVertical");

        // Get the forward and right vectors of the camera without vertical component   
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 direction = cameraForward * z + cameraRight * h;

        // Apply deadzone filtering
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            direction.z = 0f;
        else
            direction.x = 0f;
        direction.Normalize();


        int y = 0;
        if (Input.GetButton("RB"))
            y = 1;
        else if (Input.GetButton("LB"))
            y = -1;
        direction.y = y;    

        // Only proceed if there's any movement input
        if (h != 0 || z != 0 || y != 0)
            StartCoroutine(MoveToSelection(direction));

    }

    private IEnumerator MoveToSelection(Vector3 dir)
    {
        isMoving = true;

        Vector3 targetPosition = currentGridPoint.transform.position + (dir * gridSize);

        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(Mathf.RoundToInt(targetPosition.x), 0, (maxGridSizeX - 1) * gridSize);
        targetPosition.y = Mathf.Clamp(Mathf.RoundToInt(targetPosition.y), 0, (maxGridSizeY - 1) * gridSize);
        targetPosition.z = Mathf.Clamp(Mathf.RoundToInt(targetPosition.z), 0, (maxGridSizeZ - 1) * gridSize);

        yield return new WaitForSeconds(moveDuration);

        SelectGridPoint(targetPosition);

        isMoving = false;
    }

    private void SelectGridPoint(Vector3 targetPosition)
    {
        var prevGP = currentGridPoint;

        // Select grid point here
        currentGridPoint = GetGridPointByPosition(targetPosition);
        cameraTarget = currentGridPoint.transform;
        DeSelectGridPoint(prevGP);

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

    private void RotateAroundCameraTarget()
    {
        float horizontalInput = Input.GetAxis("RightAnalogHorizontal"); // Keyboard or joystick horizontal input
        float verticalInput = Input.GetAxis("RightAnalogVertical"); // Keyboard or joystick vertical input

        // Calculate rotation around the target object
        Vector3 direction = (cam.transform.position - cameraTarget.position).normalized;
        Quaternion rotation = Quaternion.Euler(verticalInput * rotationSpeed * Time.deltaTime, horizontalInput * rotationSpeed * Time.deltaTime, 0);
        direction = rotation * direction;

        // Update camera position
        cam.transform.position = cameraTarget.position + direction * currentZoomDistance;

        // Keep the camera looking at the target
        cam.transform.LookAt(cameraTarget);
    }

    private void ZoomOnCameraTarget()
    {
        // Change these controls
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // Mouse scroll wheel input
        //float controllerInput = Input.GetAxis("RightAnalogVertical") * 0.1f; // Optional joystick zoom control

        // Calculate the zoom amount
        float zoomAmount = scrollInput; //+ controllerInput;

        // Adjust the current zoom distance
        currentZoomDistance -= zoomAmount * zoomSpeed;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

        // Update the camera position to reflect the new zoom distance
        cam.transform.position = cameraTarget.position - cam.transform.forward * currentZoomDistance;
    }

    public void SetTarget(Transform newTarget)
    {
        cameraTarget = newTarget;
        currentZoomDistance = Vector3.Distance(cam.transform.position, cameraTarget.position);
    }
    private void SmoothTransition()
    {
        // Interpolate the camera's position and rotation smoothly towards the target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);

        // Check if the transition is complete
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f && Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            isTransitioning = false;
        }
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

    // Serialize SpawnData to json file
    private void SaveSpawnData() { }

    // Load serialize SpawnData from json file
    private void LoadSpawnData() { }
}

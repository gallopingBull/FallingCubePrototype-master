using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCubeTest : MonoBehaviour
{

    public bool enableMovement;

    public Transform target;
    private GameObject _camera;
    public float moveDistance = 1f; // Distance the cube moves with each step
    float h, z;
    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    private Vector3 lastPosition;
    public float snapThreshold = 0.1f; // Distance threshold for snapping to whole numbers

    void Start()
    {
        _camera = GameObject.Find("Camera");
        // Initialize last position to current position
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //if (target == null)
        //    return;

        // Move the cube only if input 
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

        MoveCardinally(h, z);
        //Movecube();
        //clamp x and z position to prevent cube from falling off grid/map
        //transform.position += new Vector3(transform.position.x, transform.position.y, transform.position.z);

        //pushPoint.transform.position = new Vector3(Mathf.Clamp(pushPoint.position.x, -100, 100),
        //    pushPoint.position.y,
        //    Mathf.Clamp(pushPoint.position.z, -100, 100));

    }

    private void Movecube()
    {
        //multiply input value by .60f to make stick less sensitive
        //when moving cubes, otherwise cube movement glitches out

        // Stop moving cube if camera is rotating
        if (Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0)
            return;

        Vector3 camF = _camera.transform.forward;
        Vector3 camR = _camera.transform.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = camF * z + camR * h;

        // Ensure movement only along the X or Z axis, not diagonally
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z))
            moveDirection.z = 0f;
        else
            moveDirection.x = 0f;

        moveDirection.Normalize();

        // Calculate target position based on the current position and move distance
        Vector3 targetPosition = new Vector3(
            Mathf.RoundToInt(moveDirection.x),
            0f,
            Mathf.RoundToInt(moveDirection.z)
        ) * moveDistance * .05f;

    }

    void MoveCardinally(float horizontalInput, float verticalInput)
    {
        // Stop moving cube if camera is rotating
        if (Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0)
            return;

        // Get the forward and right vectors of the camera without vertical component
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = cameraForward * verticalInput + cameraRight * horizontalInput;

        // Ensure movement only along the X or Z axis, not diagonally
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z))
        {
            moveDirection.z = 0f;
        }
        else
        {
            moveDirection.x = 0f;
        }

        moveDirection.Normalize();

        // Calculate target position based on the current position and move distance
        Vector3 targetPosition = transform.position + new Vector3(
            Mathf.RoundToInt(moveDirection.x),
            0f,
            Mathf.RoundToInt(moveDirection.z)
        ) * moveDistance;

        // Check if both X and Z positions are whole values
        if (Mathf.Approximately(targetPosition.x, Mathf.Round(targetPosition.x)) ||
            Mathf.Approximately(targetPosition.z, Mathf.Round(targetPosition.z)))
        {
            // Move the cube to the target position
            transform.position = targetPosition;
            lastPosition = targetPosition;
        }
        else
        {
            // Snap to whole numbers if close enough
            SnapToWholeNumbers(targetPosition);
        }
    }

    void SnapToWholeNumbers(Vector3 targetPosition)
    {
        // Calculate the distance to the nearest whole numbers in X and Z directions
        float distanceX = Mathf.Abs(targetPosition.x - Mathf.Round(targetPosition.x));
        float distanceZ = Mathf.Abs(targetPosition.z - Mathf.Round(targetPosition.z));

        // If the distance is within the snap threshold, snap to whole numbers
        if (distanceX < snapThreshold)
        {
            targetPosition.x = Mathf.Round(targetPosition.x);
        }
        if (distanceZ < snapThreshold)
        {
            targetPosition.z = Mathf.Round(targetPosition.z);
        }

        // Move the cube to the snapped position
        transform.position = targetPosition;
        lastPosition = targetPosition;
    }
}

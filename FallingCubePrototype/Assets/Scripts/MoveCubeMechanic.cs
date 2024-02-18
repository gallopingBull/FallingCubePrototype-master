using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

public class MoveCubeMechanic : vPushActionController
{
    #region variables
    [HideInInspector]
    public bool enableMovement;

    private float h, z;
    private float cubeScale = 2f; // Default cube scale
    private Vector3 lastPosition;
    private Transform target;
    private GameObject camera;
    public Transform pushPointOld;

    bool isPaused = false;

    // TODO: this value should be assigned using exponential smoothing to make the movement less sensitive
    public float moveDistance = 1f; // Distance the cube moves with each step
    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    public float snapThreshold = 0.1f; // Distance threshold for snapping to whole numbers
    public Vector2Int maxGridSize = new Vector2Int(10, 10); // Maximum grid size of the map
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        camera = GameObject.Find("Camera");
        //pushPointOld = GameObject.Find("PushPoint").GetComponent<Transform>();

        // Initialize last position to current position
        lastPosition = transform.position;

        // n - 1 to account for 0-based index
        maxGridSize.x--;
        maxGridSize.y--;

        //GrabMechanic.OnGrab += InitGrab;
        //GrabMechanic.OnRelease += DeParentToPushPoint;
        //GrabMechanic.OnRelease += EnableBoxMovement;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetButtonDown("Start"))
    //    {
    //        if (isPaused)
    //        {
    //            Time.timeScale = 1;
    //            isPaused = false;
    //        }
    //        else
    //        {
    //            Time.timeScale = 0;
    //            isPaused = true;
    //        }
    //    }
    //
    //    if (enableMovement)
    //    {
    //        //InputHandler();
    //    }
    //}

    //void LateUpdate()
    //{
    //MoveCardinally();
    //}

    // equivalent to vPushActionController.MoveInput
    private void InputHandler()
    {
        // Stop moving cube if camera is rotating
        if (Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0) // TODO: move this to the InputHandler method
            return;

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

    // TODO: delete this method after completing vPushActionController converion
    private void InitGrab()
    {
        EnableBoxMovement();
        SetPushPointPosition();
        ParentToPushPoint();
    }

    protected override void MoveInput()
    {
        //base.MoveInput();
        // Stop moving cube if camera is rotating
        if (Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0) // TODO: move this to the InputHandler method
            return;

        if (tpInput.enabled || !isPushingPulling || !pushPoint || isStoping)
            return;

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
        /// Get the forward and right vectors of the camera without vertical component
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = cameraForward * z + cameraRight * h;

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
        Vector3 targetPosition = pushPoint.targetBody.position + new Vector3(
            Mathf.RoundToInt(moveDirection.x),
            0f,
            Mathf.RoundToInt(moveDirection.z)
        ) * moveDistance * cubeScale;


        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, maxGridSize.x * cubeScale);
        targetPosition.z = Mathf.Clamp(targetPosition.z, 0, maxGridSize.y * cubeScale);


        // Check if both X and Z positions are whole values
        if (Mathf.Approximately(targetPosition.x, Mathf.Round(targetPosition.x)) ||
            Mathf.Approximately(targetPosition.z, Mathf.Round(targetPosition.z)))
        {
            // Move the cube to the target position
            pushPoint.targetBody.position = targetPosition;
            lastPosition = targetPosition;
        }
        else
        {
            // Snap target position to multiples of cube scale if close enough
            SnapToMultipleOfCubeScale(targetPosition);
        }
    }

    // equivalent to vPushActionController.MoveObject
    private void MoveCardinally()
    {
        // Get the forward and right vectors of the camera without vertical component
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = cameraForward * z + cameraRight * h;

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
        Vector3 targetPosition = pushPointOld.position + new Vector3(
            Mathf.RoundToInt(moveDirection.x),
            0f,
            Mathf.RoundToInt(moveDirection.z)
        ) * moveDistance * cubeScale;


        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, maxGridSize.x * cubeScale);
        targetPosition.z = Mathf.Clamp(targetPosition.z, 0, maxGridSize.y * cubeScale);


        // Check if both X and Z positions are whole values
        if (Mathf.Approximately(targetPosition.x, Mathf.Round(targetPosition.x)) ||
            Mathf.Approximately(targetPosition.z, Mathf.Round(targetPosition.z)))
        {
            // Move the cube to the target position
            pushPointOld.position = targetPosition;
            lastPosition = targetPosition;
        }
        else
        {
            // Snap target position to multiples of cube scale if close enough
            SnapToMultipleOfCubeScale(targetPosition);
        }
    }


    // TODO: change this method so instead of a snap its more of
    // a discrete "push" to the nearest whole number
    private void SnapToMultipleOfCubeScale(Vector3 targetPosition)
    {
        // Snap X and Z positions to multiples of cube scale
        float snappedX = Mathf.Round(targetPosition.x / cubeScale) * cubeScale;
        float snappedZ = Mathf.Round(targetPosition.z / cubeScale) * cubeScale;

        // Snap to whole numbers if close enough
        if (Mathf.Abs(targetPosition.x - snappedX) < snapThreshold)
        {
            targetPosition.x = snappedX;
        }
        if (Mathf.Abs(targetPosition.z - snappedZ) < snapThreshold)
        {
            targetPosition.z = snappedZ;
        }

        // Move the cube to the snapped position
        pushPoint.targetBody.position = targetPosition;
        lastPosition = targetPosition;
    }

    // use this only to assign initial push point position
    public void SetPushPointPosition()
    {
        Vector3 _tmpPos;
        _tmpPos = new Vector3(target.position.x, target.position.y + 2, target.position.z);
        pushPointOld.position = _tmpPos;
    }

    public void ParentToPushPoint()
    {
        transform.SetParent(pushPointOld, true);
        target.SetParent(pushPointOld, true);
    }

    public void DeParentToPushPoint()
    {
        transform.parent = null;
        target.parent = null;
    }

    // small delay before player movement is enabled again
    private IEnumerator EnableMovement()
    {
        yield return new WaitForSeconds(1.5f);
        if (!enableMovement)
            enableMovement = true;
        StopCoroutine("EnableMovement");
    }

    public void EnableBoxMovement()
    {
        if (!enableMovement)
            target = GetComponent<GrabMechanic>().targetCube.transform.parent.parent;
        else
            target = null;

        enableMovement = !(enableMovement);
    }

    private void OnDestroy()
    {
        //GrabMechanic.OnGrab -= InitGrab;
        //GrabMechanic.OnRelease -= DeParentToPushPoint;
        //GrabMechanic.OnRelease -= EnableBoxMovement;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.IK;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using System.Collections;
using UnityEngine;

public class MoveCubeMechanic : vPushActionController
{
    #region variables

    [HideInInspector]
    public bool enableMovement;

    private float h, z;
    private float cubeScale = .25f; // Default cube scale
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
    }

    protected override void MoveObject()
    {
        var strengthFactor = Mathf.Clamp(strength / pushPoint.targetBody.mass, 0, 1);
        var direction = ClampDirection(pushPoint.transform.TransformDirection(inputDirection));
        movementDirection = direction;

        // Calculate target position with grid constraints and apply a discrete push
        Vector3 targetPosition = CalculateTargetPositionWithGrid(pushPoint.targetBody.position, direction, strengthFactor);
        pushPoint.targetBody.position = targetPosition; // Directly setting position, consider using Rigidbody methods for physics-based movement

        // Check movement state and invoke events accordingly
        bool _isMoving = (pushPoint.targetBody.position - lastBodyPosition).magnitude > 0.001f;
        UpdateMovementState(_isMoving);
    }

    private Vector3 CalculateTargetPositionWithGrid(Vector3 currentPosition, Vector3 direction, float strengthFactor)
    {
        // Transform direction to discrete steps and apply strength factor
        Vector3 moveStep = new Vector3(Mathf.RoundToInt(direction.x), 0f, Mathf.RoundToInt(direction.z));
        Vector3 targetPosition = currentPosition + moveStep * moveDistance * cubeScale * strengthFactor;

        // Clamp target position within the boundaries of the map
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, maxGridSize.x * cubeScale);
        targetPosition.z = Mathf.Clamp(targetPosition.z, 0, maxGridSize.y * cubeScale);

        // Apply a discrete "push" to the nearest whole number, replacing the SnapToMultipleOfCubeScale functionality
        targetPosition = DiscretePushToWholeNumber(targetPosition);

        return targetPosition;
    }

    private Vector3 DiscretePushToWholeNumber(Vector3 position)
    {
        position.x = Mathf.Round(position.x / cubeScale) * cubeScale;
        position.z = Mathf.Round(position.z / cubeScale) * cubeScale;
        return position;
    }

    private void UpdateMovementState(bool _isMoving)
    {
        if (_isMoving != isMoving)
        {
            isMoving = _isMoving;

            if (isMoving)
            {
                pushPoint.pushableObject.onStartMove.Invoke();
            }
            else
            {
                pushPoint.pushableObject.onMovimentSpeedChanged.Invoke(0);
                pushPoint.pushableObject.onStopMove.Invoke();
            }
        }
        if (isMoving)
        {
            float movementSpeed = Mathf.Clamp((pushPoint.targetBody.position - lastBodyPosition).magnitude / Time.fixedDeltaTime, 0, 1f);
            pushPoint.pushableObject.onMovimentSpeedChanged.Invoke(movementSpeed);
            lastBodyPosition = pushPoint.targetBody.position;
        }
    }
}

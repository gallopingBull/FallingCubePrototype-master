﻿using System;
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
        //StartCoroutine(EnsureCubeScaleAlignment());
        // n - 1 to account for 0-based index
        maxGridSize.x--;
        maxGridSize.y--;
    }
    protected override void MoveObject()
    {
        var strengthFactor = Mathf.Clamp(strength / pushPoint.targetBody.mass, 0, 1);
        var intendedDirection = ClampDirection(pushPoint.transform.TransformDirection(inputDirection));
        if (intendedDirection != Vector3.zero && CanChangeDirection())
        {
            Vector3 intendedPosition = pushPoint.targetBody.position + intendedDirection * 15 * cubeScale * vTime.fixedDeltaTime;
            intendedPosition = ApplyStepConstraints(intendedPosition);

            if (!PositionIsLocked(intendedPosition))
            {
                // Only apply the movement if the new position is different from the current position
                pushPoint.targetBody.position = intendedPosition;
                UpdateMovementState(intendedPosition);
            }
        }
    }

    private bool CanChangeDirection()
    {
        // Ensure we can change direction if the cube is aligned properly.
        return IsPositionAligned(pushPoint.targetBody.position);
    }

    private Vector3 ApplyStepConstraints(Vector3 targetPosition)
    {
        // Adjust the target position to be a multiple of the cube's scale
        targetPosition.x = Mathf.Round(targetPosition.x / cubeScale) * cubeScale;
        targetPosition.z = Mathf.Round(targetPosition.z / cubeScale) * cubeScale;
        return targetPosition;
    }

    private bool PositionIsLocked(Vector3 targetPosition)
    {
        // Check if moving to the target position would effectively lock the cube's position
        return pushPoint.targetBody.position == targetPosition;
    }

    private bool IsPositionAligned(Vector3 position)
    {
        // Check if both X and Z positions are whole numbers considering the cube's scale
        return Mathf.Approximately(position.x, Mathf.Round(position.x)) ||
               Mathf.Approximately(position.z, Mathf.Round(position.z));
    }

    private void UpdateMovementState(Vector3 newPosition)
    {
        // Update movement state and potentially trigger events
        bool _isMoving = (newPosition - lastBodyPosition).magnitude > 0.001f;
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
            pushPoint.pushableObject.onMovimentSpeedChanged.Invoke(Mathf.Clamp(pushPoint.targetBody.velocity.magnitude, 0, 1f));
        }

        lastBodyPosition = newPosition;
    }
}

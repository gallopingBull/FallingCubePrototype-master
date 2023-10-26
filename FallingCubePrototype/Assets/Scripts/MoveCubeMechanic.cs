using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Generateclass that lets the player move cube orthonogally only. The player may not rotate the cube and the cube cannot be moved diagonally. 
// The cube may only be set on x and y coordinates with whole numbers.


public class MoveCubeMechanic : MonoBehaviour
{
    public float moveDistance = 1f; // Distance the cube moves with each step

    void Update()
    {
        // if camera moves, stop moving cube and wait for
        // camera to stop moving before moving cube again with new camera position.

        // Get input for movement
        float horizontalInput = Input.GetAxis("LeftAnalogHorizontal");
        float verticalInput = Input.GetAxis("LeftAnalogVertical");

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
        ) * moveDistance *.025f;

        // Move the cube to the target position
        transform.position = targetPosition;
    }
}


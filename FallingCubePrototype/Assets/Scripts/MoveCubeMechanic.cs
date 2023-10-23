using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Generateclass that lets the player move cube orthonogally only. The player may not rotate the cube and the cube cannot be moved diagonally. 
// The cube may only be set on x and y coordinates with whole numbers.


public class MoveCubeMechanic : MonoBehaviour
{

    public Camera camera;
    private Vector3 lastCameraForward;


    void Update()
    {
        lastCameraForward = camera.transform.forward;
        lastCameraForward.y = 0; // Ensure no vertical movement
        lastCameraForward.Normalize();
        Debug.Log($"lastCameraForward{lastCameraForward}");
        // Get the horizontal and vertical input axis
        float horizontalInput = Input.GetAxis("LeftAnalogHorizontal");
        float verticalInput = Input.GetAxis("LeftAnalogVertical");

        // Calculate the movement direction based on the camera's forward vector
        Vector3 movement = (horizontalInput * Vector3.right + verticalInput * lastCameraForward).normalized;
        // Check if the movement is diagonal and restrict it to orthogonal movement
        if (Mathf.Abs(movement.x) > 0.1f && Mathf.Abs(movement.z) > 0.1f)
        {
            // Diagonal movement is not allowed, restrict to either horizontal or vertical movement
            movement.x = Mathf.Round(movement.x);
            movement.z = 0;
        }
        else
        {
            // Round the movement values to ensure whole number coordinates
            movement.x = Mathf.Round(movement.x);
            movement.z = Mathf.Round(movement.z);
        }

        // Move the cube based on the calculated movement
        transform.Translate(movement * .1f);
    }
}

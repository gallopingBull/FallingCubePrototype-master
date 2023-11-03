using UnityEngine;

// Generateclass that lets the player move cube orthonogally only. The player may not rotate the cube and the cube cannot be moved diagonally. 
// The cube may only be set on x and y coordinates with whole numbers.


public class MoveCubeMechanic : vPushActionController
{
    public float moveDistance = 1f; // Distance the cube moves with each step

    protected override void MoveInput()
    {
        MoveCube();
        if (tpInput.enabled || !isPushingPulling || !pushPoint || isStoping)
        {
            return;
        }
        tpInput.CameraInput();
        
        inputHorizontal = tpInput.horizontalInput.GetAxis();
        inputVertical = tpInput.verticallInput.GetAxis();
        if (Mathf.Abs(inputHorizontal) > 0.5f)
        {
            inputVertical = 0;
        }
        else if (Mathf.Abs(inputVertical) > 0.5f)
        {
            inputHorizontal = 0;
        }
        
        if (Mathf.Abs(inputHorizontal) < 0.8f)
        {
            inputHorizontal = 0;
        }
        
        if (Mathf.Abs(inputVertical) < 0.8f)
        {
            inputVertical = 0;
        }
        
        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();


        inputDirection = cameraForward * inputVertical + cameraRight * inputHorizontal;
        inputDirection = pushPoint.transform.InverseTransformDirection(inputDirection);
        
        if (inputDirection.z > 0 && !pushPoint.canPushForward)
        {
            inputDirection.z = 0;
        }
        else if (inputDirection.z < 0 && (!pushPoint.canPushBack || isCollidingBack))
        {
            inputDirection.z = 0;
        }
        
        if (inputDirection.x > 0 && (!pushPoint.canPushRight || isCollidingRight))
        {
            inputDirection.x = 0;
        }
        else if (inputDirection.x < 0 && (!pushPoint.canPushLeft || isCollidingLeft))
        {
            inputDirection.x = 0;
        }
        
        inputDirection.y = 0;
        
        if (inputDirection.magnitude > 0.1f)
        {
            inputWeight = Mathf.Lerp(inputWeight, 1, Time.deltaTime * animAcceleration);
        }
        else
        {
            inputWeight = Mathf.Lerp(inputWeight, 0, Time.deltaTime * animAcceleration);
        }
    }


    private void MoveCube()
    {
        // Get input for movement
        float horizontalInput = Input.GetAxis("LeftAnalogHorizontal");
        float verticalInput = Input.GetAxis("LeftAnalogVertical");

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
        ) * moveDistance * .05f;

        // Move the cube to the target position
        transform.position = targetPosition;
    }
}


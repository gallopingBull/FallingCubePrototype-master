using System;
using Invector;
using Invector.vCharacterController;
using UnityEngine;

public class MoveCubeMechanic : vPushActionController
{
    private float cubeScale = 2f; // Default cube scale

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    public float snapThreshold = 0.1f; // Distance threshold for snapping to whole numbers
    public Vector2Int maxGridSize = new Vector2Int(10, 10); // Maximum grid size of the map

    public float checkDistance = 6f; // Distance to check adjacent positions
    public float downwardCheckDistance = 10f; // Distance to check downward from adjacent 
    private Vector3 detectionOffSets = new Vector3(0, 2, 0);

    public float minCollisionDistance = 0;

    public GameObject currentCubeFloor;
    public static Action<GameObject> OnNewCubePosition;
    public static Action OnExitCubePosition;

    public float sphereSize = 1f;

    private Vector3 targetPos = new Vector3();
    private int layerMask;
    public float maxDetectionDistance = 4; //2.3f;

    public float Distance = 0;

    public float minGrabDistance = .85f;

    RaycastHit[] sphereHitsBlock;

    /// <summary>
    /// this is just an idea. this code is not implemented yet. 
    /// </summary>
    //CubeDetectionData[] orthogonalCubeHitDetectionData = new orthogonalCubeHitDetectionData[3]; 
    //CubeDetectionData[] vertCubeHitDetectionData = new vertCubeHitDetectionData[3]; 

    RaycastHit[] orthogonalHits = new RaycastHit[3];
    bool[] OrthoHitCubes = new bool[3];
    float[] curDirectionDistances = new float[3];
    Vector3 curRelativeDirection;

    RaycastHit[] verticalHits = new RaycastHit[3];
    bool[] verticalHitCubes = new bool[3];
    float[] downwardRayDistances = new float[3];
    Vector3[] downwardPosition = new Vector3[3];

    Vector3[] directions;

    private bool isDetectingBack = false;
    private bool isDetectingBackLeft = false;
    private bool isDetectingBackRight = false;

    float cooldownTime = 2f; // input delay
    float grabStartTime = -Mathf.Infinity;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // n - 1 to account for 0-based index
        maxGridSize.x--;
        maxGridSize.y--;

        layerMask = LayerMask.GetMask("Cube");

        OnNewCubePosition += SetNewFloorCube;
        OnExitCubePosition += RemoveNewFloorCube;
    }

    // I changed GetButtonDown() to GetButton() to remove toggle input. I also check if the button is being held
    // down in the final stop condition. 
    protected override void EnterExitInput()
    {
        if (tpInput.enabled || !isStarted || !pushPoint)
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

            if (Physics.Raycast(ray, out hit, minGrabDistance, pushpullLayer))
            {
                var _object = hit.collider.gameObject.GetComponent<vPushObjectPoint>();
                if (_object && pushPoint != _object && _object.canUse && 
                    _object.gameObject.GetComponentInParent<CubeBehavior>().state == CubeBehavior.States.grounded)
                {
                    pushPoint = _object;
                    onFindObject.Invoke();
                }
                else if (_object == null && pushPoint)
                {
                    pushPoint = null;
                    onLostObject.Invoke();
                }
            }
            else if (pushPoint)
            {
                pushPoint = null;
                onLostObject.Invoke();
            }

            if (pushPoint && pushPoint.canUse && startPushPullInput.GetButton() && CanUseInput() && 
                pushPoint.gameObject.GetComponentInParent<CubeBehavior>().state == CubeBehavior.States.grounded)
            {
                StartCoroutine(StartPushAndPull());
                grabStartTime = Time.time;
            }

        }
        else if (isPushingPulling && !startPushPullInput.GetButton())
        {
            StartCoroutine(StopPushAndPull());
        }
    }

    bool CanUseInput()
    {
        return Time.time - grabStartTime >= cooldownTime;
    }

    protected override void MoveInput()
    {
        if (!tpInput ||
            !tpInput.cc ||
            !tpInput.cc._capsuleCollider ||
            tpInput.enabled ||
            !isPushingPulling ||
            !pushPoint ||
            isStoping) return;

        tpInput.CameraInput();

        bool _isDetectingLeft = false;
        bool _isDetectingRight = false;
        bool _isDetectingBack = false;

        inputHorizontal = tpInput.horizontalInput.GetAxis();
        inputVertical = tpInput.verticallInput.GetAxis();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        Vector3 cameraForward = Quaternion.AngleAxis(-90, Vector3.up) * cameraRight;

        // Calculate the forward direction of the player (or game object) on the XZ plane
        Vector3 playerForward = transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        inputDirection = cameraForward * inputVertical + cameraRight * inputHorizontal;

        // Ensure movement only along the X or Z axis, not diagonally
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.z))
        {
            inputDirection.z = 0f;
        }
        else
        {
            inputDirection.x = 0f;
        }

        inputDirection.Normalize();
        inputDirection = pushPoint.transform.InverseTransformDirection(inputDirection);

        // Perform a spherecast to check for game objects with a "Block" tag
        sphereHitsBlock = Physics.SphereCastAll(transform.position, sphereSize, Vector3.down, 0, layerMask);

        foreach (RaycastHit sphereHit in sphereHitsBlock)
        {
            if (sphereHit.collider.CompareTag("Block"))
            {
                if (sphereHitsBlock.Length == 1)
                {
                    if (currentCubeFloor == null || currentCubeFloor != sphereHit.transform.gameObject)
                    {
                        //Debug.Log($"asigning new targetPos using {sphereHit.transform.name}");
                        //Debug.Log($"targetPos: {sphereHit.transform.position}");
                        currentCubeFloor = sphereHit.transform.gameObject;
                        targetPos = currentCubeFloor.transform.position;
                        targetPos += detectionOffSets;
                        break; // Exit the loop after drawing the first hit
                    }
                }
                else if (sphereHitsBlock.Length > 1)
                {

                }
            }
        }

        if (isPushingPulling)
        {
            // Check each adjacent direction relative to the player's forward direction
            directions = new Vector3[] { -transform.forward, -transform.right, transform.right };
            for (int i = 0; i < directions.Length; i++)
            {

                // Shoot a ray in the current direction
                curRelativeDirection = directions[i];
                OrthoHitCubes[i] = Physics.Raycast(targetPos, curRelativeDirection, out orthogonalHits[i], checkDistance, layerMask);
                curDirectionDistances[i] = OrthoHitCubes[i] ? orthogonalHits[i].distance : checkDistance;

                if (OrthoHitCubes[i])
                {
                    var tmpPos = new Vector3();
                    if (i == 0)
                        maxDetectionDistance = 4f;
                    else
                        maxDetectionDistance = 2.83f;

                    tmpPos = new Vector3(
                        orthogonalHits[i].transform.position.x,
                        pushPoint.pushableObject.transform.position.y,
                        orthogonalHits[i].transform.position.z
                        );

                    Distance = Vector3.Distance(pushPoint.pushableObject.transform.position, tmpPos);

                    //Debug.Log($"distance: {Distance}");

                    if (Distance <= maxDetectionDistance)
                    {
                        switch (i)
                        {
                            // behind player
                            case 0:
                                //Debug.Log("Colliding from the back!");
                                if (inputDirection.z < 0)
                                {
                                    inputDirection.z = 0;
                                    _isDetectingBack = true;
                                }

                                break;

                            // behind player - left-side
                            case 1:
                                //Debug.Log("Colliding from the left!");
                                if (inputDirection.x < 0)
                                {
                                    inputDirection.x = 0;
                                    _isDetectingLeft = true;
                                }

                                break;

                            // behind player - right-side
                            case 2:
                                //Debug.Log("Colliding from the right!");
                                if (inputDirection.x > 0)
                                {
                                    inputDirection.x = 0;
                                    _isDetectingRight = true;
                                }

                                break;

                            default:
                                break;
                        }
                        pushPoint.targetBody.position = pushPoint.targetBody.position;
                        //continue;
                    }
                    else
                    {
                        // TODO: is there a better way of freezing the cube in place if its near another cube?
                        //Debug.Log("wont move push body position");
                        pushPoint.targetBody.position = pushPoint.targetBody.position;
                    }
                }

                downwardPosition[i] = targetPos + curRelativeDirection * curDirectionDistances[i];

                // Shoot a ray downward from the end point of the previous ray
                verticalHitCubes[i] = Physics.Raycast(downwardPosition[i], Vector3.down, out verticalHits[i], downwardCheckDistance, layerMask);
                downwardRayDistances[i] = verticalHitCubes[i] ? verticalHits[i].distance : downwardCheckDistance;

                // Perform a spherecast at the downward position
                RaycastHit[] sphereHits = Physics.SphereCastAll(downwardPosition[i], 0.1f, Vector3.down, downwardCheckDistance, layerMask);

                if (verticalHitCubes[i])
                {
                    // Check if any cube is found underneath
                    foreach (RaycastHit sphereHit in sphereHits)
                    {
                        if (sphereHit.collider.CompareTag("Block"))
                            break; // Exit the loop after drawing the ray to the first cube found
                    }
                }
                else if (!verticalHitCubes[i] && !OrthoHitCubes[i])
                {
                    maxDetectionDistance = 2f;
                    // instead of tracking currentCubeFloor.transform.position, cache its position into a lastKnownPos to checka against.
                    // i think this is causing the ccurrentCubeFloor not assigned error if this is checked while player is betweem
                    var tmpPos = new Vector3(
                       currentCubeFloor.transform.position.x,
                       pushPoint.pushableObject.transform.position.y,
                       currentCubeFloor.transform.position.z);

                    Distance = Vector3.Distance(pushPoint.pushableObject.transform.position, tmpPos);
                    //Debug.Log($"distance: {Distance}");

                    if (Distance <= maxDetectionDistance)
                    {
                        switch (i)
                        {
                            // behind player
                            case 0:
                                //Debug.Log("no cube underneath you from behind!");
                                if (inputDirection.z < 0)
                                {
                                    inputDirection.z = 0;
                                    //_isDetectingBack = true;    
                                }

                                break;

                            // behind player - left-side
                            case 1:
                                //Debug.Log("no cube underneath you from left-side!");
                                if (inputDirection.x < 0)
                                {
                                    inputDirection.x = 0;
                                    //_isDetectingLeft = true;
                                }

                                break;

                            // behind player - right-side
                            case 2:
                                //Debug.Log("no cube underneath you from right-side!");
                                if (inputDirection.x > 0)
                                {
                                    inputDirection.x = 0;
                                    //_isDetectingRight = true;
                                }

                                break;

                            default:
                                break;
                        }
                    }
                }

                //Debug.Log($"inputDirection.magnitude: {inputDirection.magnitude}");
                if (inputDirection.magnitude > 0.1f)
                    inputWeight = Mathf.Lerp(inputWeight, 1, Time.deltaTime * animAcceleration);
                else
                    inputWeight = Mathf.Lerp(inputWeight, 0, Time.deltaTime * animAcceleration);
            }
        }
        isDetectingBack = _isDetectingBack;
        isDetectingBackRight = _isDetectingRight;
        isDetectingBackLeft = _isDetectingLeft;
    }

    protected override void MoveObject()
    {
        // Stop moving cube if camera is rotating
        //if ((Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0) || (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        //    return;

        var strengthFactor = Mathf.Clamp(strength / pushPoint.targetBody.mass, 0, 1);
        var intendedDirection = ClampDirection(pushPoint.transform.TransformDirection(inputDirection));
        movementDirection = intendedDirection;

        if (intendedDirection != Vector3.zero)
        {
            Vector3 intendedPosition = pushPoint.targetBody.position + new Vector3(
                Mathf.RoundToInt(intendedDirection.x),
                0,
                Mathf.RoundToInt(intendedDirection.z)
                ) * strengthFactor * cubeScale * vTime.fixedDeltaTime;

            // Clamp target position within the boundaries of the map
            intendedPosition.x = Mathf.Clamp(intendedPosition.x, 0, maxGridSize.x * cubeScale);
            intendedPosition.z = Mathf.Clamp(intendedPosition.z, 0, maxGridSize.y * cubeScale);

            //intendedPosition = ApplyStepConstraints(intendedPosition);
            pushPoint.targetBody.velocity = intendedDirection * inputWeight;
            //Debug.Log("calling PositionIsLocked...");
            if (IsPositionAligned(intendedPosition))
            {
                // Only apply the movement if the new position is different from the current position
                pushPoint.targetBody.position = intendedPosition;
                UpdateMovementState(intendedPosition);
            }
            else
            {
                DiscretePushToNearestWholeNumber(intendedPosition);
                //Debug.Log("Position is locked...\n\tThis would be where SnapToMultipleOfCubeScale() would be called...");
            }
        }
    }

    protected override void CheckBreakActionConditions()
    {
        // radius of the SphereCast
        float radius = tpInput.cc._capsuleCollider.radius * 0.9f;
        var dist = 10f;

        // ray for RayCast
        Ray ray2 = new Ray(transform.position + new Vector3(0, tpInput.cc.colliderHeight / 2, 0), Vector3.down);

        // raycast for check the ground distance
        // hacky fix: check for climable_surfaces to resolve issue with moving cubes on top of other cubes.
        if (Physics.Raycast(ray2, out tpInput.cc.groundHit, (tpInput.cc.colliderHeight / 2) + dist, tpInput.cc.groundLayer) && (!tpInput.cc.groundHit.collider.isTrigger || tpInput.cc.groundHit.collider.transform.name == "climbable_surface"))
        {
            dist = transform.position.y - tpInput.cc.groundHit.point.y;
        }

        // sphere cast around the base of the capsule to check the ground distance
        if (tpInput.cc.groundCheckMethod == vThirdPersonMotor.GroundCheckMethod.High && dist >= tpInput.cc.groundMinDistance)
        {
            Vector3 pos = transform.position + Vector3.up * (tpInput.cc._capsuleCollider.radius);
            Ray ray = new Ray(pos, -Vector3.up);
            if (Physics.SphereCast(ray, radius, out tpInput.cc.groundHit, tpInput.cc._capsuleCollider.radius + tpInput.cc.groundMaxDistance, tpInput.cc.groundLayer) && !tpInput.cc.groundHit.collider.isTrigger)
            {
                Physics.Linecast(tpInput.cc.groundHit.point + (Vector3.up * 0.1f), tpInput.cc.groundHit.point + Vector3.down * 0.15f, out tpInput.cc.groundHit, tpInput.cc.groundLayer);
                float newDist = transform.position.y - tpInput.cc.groundHit.point.y;
                if (dist > newDist)
                {
                    dist = newDist;
                }
            }
        }

        if (dist > tpInput.cc.groundMaxDistance || Vector3.Distance(transform.position, pushPoint.transform.TransformPoint(startLocalPosition)) > (breakActionDistance))
        {
            bool falling = dist > tpInput.cc.groundMaxDistance;
            if (falling)
            {
                tpInput.cc.isGrounded = false;
                tpInput.cc.animator.SetBool(vAnimatorParameters.IsGrounded, false);
                tpInput.cc.animator.PlayInFixedTime("Falling");
            }
            StartCoroutine(StopPushAndPull(!falling));
        }
    }

    public void StopPushAndPullCaller()
    {
        // I'd like to stop player movement as soon as the cube starts falling.
        // Right now there's a lag between player 
        //weight = 0;
        //tpInput.cc.ResetInputAnimatorParameters();
        //tpInput.cc.inputSmooth = Vector3.zero;
        //tpInput.cc.input = Vector3.zero;
        //tpInput.cc.inputMagnitude = 0;
        //tpInput.cc.StopCharacter();
        //tpInput.cc._rigidbody.velocity = Vector3.zero;
        //tpInput.cc.moveDirection = Vector3.zero;
        StartCoroutine(StopPushAndPull(true));
    }

    //protected override IEnumerator StopPushAndPull(bool playAnimation = true)
    //{
    //
    //}

    // TODO: change this method so instead of a snap its more of
    // a discrete "push" to the nearest whole number
    private void DiscretePushToNearestWholeNumber(Vector3 targetPosition)
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
        lastBodyPosition = targetPosition;
    }

    private bool IsPositionAligned(Vector3 position)
    {
        // Check if both X and Z positions are whole numbers considering the cube's scale
        //Debug.Log($"IsPositionAligned: {position.x}, {position.z}");
        //Debug.Log($"\n\tTarget Positions: {Mathf.Round(position.x)}, {Mathf.Round(position.z)}");
        bool result = Mathf.Approximately(position.x, Mathf.Round(position.x)) ||
                      Mathf.Approximately(position.z, Mathf.Round(position.z));
        //Debug.Log($"\n\tresult: {result}");
        return result;
    }

    private void UpdateMovementState(Vector3 newPosition)
    {
        // Update movement state and potentially trigger events
        bool _isMoving = (newPosition - lastBodyPosition).magnitude > 0.001f && inputWeight > 0f;
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

    private void SetNewFloorCube(GameObject cube) => currentCubeFloor = cube;

    private void RemoveNewFloorCube() => currentCubeFloor = null;

    public bool CheckDistance()
    {
        //if (currentCubeFloor == null)
        //    return false;

        Debug.Log($"currentCubeFloor: {currentCubeFloor}");
        //Debug.Log($"pushPoint.pushableObject: {pushPoint.pushableObject}");

        var tmpPos = new Vector3();
        tmpPos = new Vector3(
           currentCubeFloor.transform.position.x,
           transform.position.y,
           currentCubeFloor.transform.position.z);
        Distance = Vector3.Distance(transform.position, tmpPos);
        return Distance <= .25f; // .45-.65f seems to work the best but the former causes issues when cubes fall to early
    }
    
    private void OnDestroy()
    {
        OnNewCubePosition -= SetNewFloorCube;
    }

    public class CubeDetectionData
    {
        RaycastHit raycastHit;
        bool raycastHitCubeDetected;
        float raycastDirDistance;
        //Vector3 curRelativeDirection;
    }

    private void OnDrawGizmos()
    {   
        // I think this is checking underneath the player to detwemine if theyre under more than
        // one cube. this helps resolve any issues that may comeform walking over two cubes...
        Gizmos.color = sphereHitsBlock?.Length > 1 ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, sphereSize); // Add a small offset

        if (isPushingPulling && directions != null)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Gizmos.color = OrthoHitCubes[i] ? Color.yellow : Color.white;
                Gizmos.DrawLine(targetPos, targetPos + directions[i] * curDirectionDistances[i]);

                if (OrthoHitCubes[i])
                {
                    //Gizmos.color = OrthoHitCubes[i] ? Color.yellow : Color.white;
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(downwardPosition[i], .1f);
                    continue;
                }

                Gizmos.color = Color.white;
                Gizmos.DrawSphere(downwardPosition[i], .1f);

                if (verticalHitCubes[i])
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(downwardPosition[i], (downwardPosition[i] + Vector3.down) * downwardRayDistances[i]);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere((downwardPosition[i] + Vector3.down) * downwardRayDistances[i], .1f);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(downwardPosition[i], downwardPosition[i] + (Vector3.down * downwardCheckDistance));
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(downwardPosition[i] + (Vector3.down * downwardCheckDistance), .1f);
                }
            }
        }
    }
}

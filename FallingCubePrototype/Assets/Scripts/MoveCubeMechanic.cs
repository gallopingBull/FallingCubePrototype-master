using System;
using Invector;
using Invector.vCharacterController;
using UnityEngine;

// TODO: Only check for adjacent cubes for the current cube the player is standing when they's push/pullling cube.
// When the player moves to another cube, cube detection should be intialized to ready for the player to start moving a cube.
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

    private GameObject currentCubeFloor;
    public static Action<GameObject> OnNewCubePosition;
    public static Action OnExitCubePosition;

    public float sphereSize = 1f;
    

    private bool canPullBack, canPullBackLeft, canPullBackRight;    


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // n - 1 to account for 0-based index
        maxGridSize.x--;
        maxGridSize.y--;

        OnNewCubePosition += SetNewFloorCube;
        OnExitCubePosition += RemoveNewFloorCube;
    }

    protected override void MoveObject()
    {
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

    private bool CheckLocationBehindPlayer(Vector3 loc)
    {
        return false;
    }

    private void OnDestroy()
    {
        OnNewCubePosition -= SetNewFloorCube;
    }

    private void OnDrawGizmos()
    {
        if (!tpInput || !tpInput.cc || !tpInput.cc._capsuleCollider) return;
        bool _canPullBack = false;
        bool _canPullBackLeft = false;
        bool _canPullBackRight = false;

        
        Vector3 targetPos = new Vector3();
        int layer_mask = LayerMask.GetMask("BlockDetection");

        // Perform a spherecast to check for game objects with a "Block" tag
        RaycastHit[] sphereHitsBlock = Physics.SphereCastAll(transform.position, sphereSize, Vector3.down, downwardCheckDistance,layer_mask);
        Gizmos.color = sphereHitsBlock.Length > 1 ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, sphereSize); // Add a small offset
        Debug.Log($"sphereHitsBlock.Length: {sphereHitsBlock.Length}");
        foreach (var GO in sphereHitsBlock)
        {
            Debug.Log($"this GO is in the current sphereHitBlock: {GO.transform.gameObject}");
        }
        foreach (RaycastHit sphereHit in sphereHitsBlock)
        {
            if (sphereHit.collider.CompareTag("Player"))
            {
                //Debug.Log("skipping player!!");
                continue;
            }
            if (sphereHit.collider.CompareTag("Block") /*   && currentCubeFloor != sphereHit.transform.gameObject*/)
            {
                Debug.Log($"asigning new targetPos using {sphereHit.transform.name}");
                if (currentCubeFloor != sphereHit.transform.gameObject) {
                    //currentCubeFloor = sphereHit.transform.gameObject;
                }

                  targetPos = currentCubeFloor.transform.position;
                targetPos += detectionOffSets;
                //Debug.Log($"new targetPos: {targetPos}");
                break; // Exit the loop after drawing the first hit
            }
        }
        
        if (isPushingPulling)
        {
            //Debug.Log($"targetPos while isPushingPulling: {targetPos}");
            // Check each adjacent direction relative to the player's forward direction
            Vector3[] directions = { -transform.forward, -transform.right, transform.right };
            for (int i = 0; i < directions.Length; i++)
            {
                //Debug.Log("!!!!!!!!");
                // Shoot a ray in the current direction
                RaycastHit hit1;
                bool hitCubeInCurrentDirection = Physics.Raycast(targetPos, directions[i], out hit1, checkDistance);
                float currentDirectionDistance = hitCubeInCurrentDirection ? hit1.distance : checkDistance;
                Gizmos.color = hitCubeInCurrentDirection ? Color.yellow : Color.white;
                //Debug.Log($"hitCubeInCurrentDirection[{i}]: {hitCubeInCurrentDirection}");
                if (hitCubeInCurrentDirection)
                {
                    //Debug.Log($"ray[{i}] hitting {hit1.transform.name} @ {hit1.transform.position}");
                    //Debug.Log($"player postion: {transform.position}");
                    if (hit1.collider.CompareTag("Player"))
                    {
                        continue;
                    }

                   // Debug.Log($"is player in position: {transform.position == hit1.transform.position}\n\t");
                    if (hit1.transform.CompareTag("Block") && transform.position == hit1.transform.position)
                    {        
                        switch (i)
                        {
                            // behind player
                            case 0:
                                Debug.Log("Colliding from the back!");

                                _canPullBack = false;
                                if (inputDirection.z > 0)
                                {
                                    //inputDirection.z = 0;
                                }
                                else if (inputDirection.z < 0 && !canPullBack)
                                {
                                    inputDirection.z = 0;
                                }

                                //inputWeight = 0;
                                break;

                            // behind player - left-side
                            case 1:
                                _canPullBackLeft = false;

                                if (inputDirection.x < 0 && !canPullBackLeft)
                                {
                                    inputDirection.x = 0;
                                }

                                //inputWeight = 0;
                                break;

                            // behind player - right-side
                            case 2:
                                Debug.Log("Colliding from the right!");

                                _canPullBackRight = false;

                                if (inputDirection.x > 0 && !canPullBackRight)
                                {
                                    inputDirection.x = 0;
                                }

                                //inputWeight = 0;
                                break;
                            default: break;
                        }
                    
                        inputWeight = 0f;   
                    }
                    else
                    {
                        switch (i)
                        {
                            // behind player
                            case 0:
                                _canPullBack = true;
                                Debug.Log("Not Colliding from the back!");
                                break;
                            // behind player - left-side
                            case 1:
                                _canPullBackLeft = true;
                                Debug.Log("Not Colliding from the left!");
                                break;
                            // behind player - right-side
                            case 2:
                                _canPullBackRight = true;
                                Debug.Log("Not Colliding from the right!");
                                break;
                            default: break;

                        }
                    }
                }
        
                Gizmos.DrawLine(targetPos, targetPos + directions[i] * currentDirectionDistance);
                Gizmos.color = hitCubeInCurrentDirection ? Color.yellow : Color.white;
        
                Vector3 downwardPosition = targetPos + directions[i] * currentDirectionDistance;
                Gizmos.DrawSphere(downwardPosition, .1f);
        
                // Shoot a ray downward from the end point of the previous ray
                bool hitCubeUnderneath = Physics.Raycast(downwardPosition, Vector3.down, out hit1, downwardCheckDistance);
                float downwardRayDistance = hitCubeUnderneath ? hit1.distance : downwardCheckDistance;
        
                // Perform a spherecast at the downward position
                RaycastHit[] sphereHits = Physics.SphereCastAll(downwardPosition, 0.1f, Vector3.down, downwardCheckDistance);
                Gizmos.DrawSphere(downwardPosition, .1f);
        
                if (hitCubeUnderneath)
                {
                    // Check if any cube is found underneath
                    foreach (RaycastHit sphereHit in sphereHits)
                    {
                        if (sphereHit.collider.CompareTag("Block"))
                        {
                            // Draw a yellow ray to the cube underneath
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawRay(downwardPosition, Vector3.down * (sphereHit.distance + 0.1f)); // Add a small offset
                            Gizmos.DrawSphere(downwardPosition + Vector3.down * (sphereHit.distance + 0.1f), .1f);
                            break; // Exit the loop after drawing the ray to the first cube found
                        }
                    }
                }
                else
                {
                    Gizmos.DrawSphere(downwardPosition + Vector3.down * downwardCheckDistance, .1f);
                }
        
                // Draw a line downward
                Gizmos.color = hitCubeUnderneath ? Color.yellow : Color.white;
                Gizmos.DrawLine(downwardPosition, downwardPosition + Vector3.down * downwardRayDistance);
            }
        }
        
        canPullBack = _canPullBack;
        canPullBackLeft = _canPullBackLeft; 
        canPullBackRight = _canPullBackRight;   
    }
}

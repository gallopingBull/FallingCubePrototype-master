using Invector;
using Invector.vCharacterController;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// TODO: Only check for adjacent cubes for the current cube the player is standing when they's push/pullling cube.
// When the player moves to another cube, cube detection should be intialized to ready for the player to start moving a cube.
public class MoveCubeMechanic : vPushActionController
{
    private float cubeScale = 2f; // Default cube scale

    public float gamepadDeadzone = 0.1f; // Dead zone for gamepad stick input
    public float snapThreshold = 0.1f; // Distance threshold for snapping to whole numbers
    public Vector2Int maxGridSize = new Vector2Int(10, 10); // Maximum grid size of the map

    private Vector3[] adjPlayerPositions = { /*Vector3.forward,*/ Vector3.back, Vector3.left, Vector3.right, Vector3.down};

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // n - 1 to account for 0-based index
        maxGridSize.x--;
        maxGridSize.y--;
    }

    protected override void OnCollisionStay(Collision collision)
    {
        base.OnCollisionStay(collision);
        // Check player position for obsructions
        if (isMoving)
        {
            foreach (var direction in adjPlayerPositions)
            {
                if (CheckAdjacentFloorSpace(direction))
                {
                   //Debug.Log("Correct object found in direction: " + direction);
                }
                else
                {
                    //Debug.Log("No correct object directly adjacent in direction: " + direction);
                }

            }
        }
    }

    public float checkDistance = 2f; // Distance to check adjacent positions
    public float downwardCheckDistance = 2f; // Distance to check downward from adjacent positions

    private bool CheckAdjacentFloorSpace(Vector3 direction)
    {
        RaycastHit hit;
        // this might be reduntant 
        // Check directly adjacent
        //if (Physics.Raycast(transform.position, direction, out hit, checkDistance))
        //{
        //    if (hit.collider.CompareTag("Cube"))
        //    {
        //        return true; // Directly adjacent object has the correct tag
        //    }
        //}
        //else
        //{
        //    // nothing was hit directly next to the player
        //    return false;
        //}

        // Check below the adjacent position if direct check fails or no hit
        Vector3 downwardPosition = transform.position + direction * checkDistance;

        Debug.Log($"downwardPosition : {downwardPosition}");
        if (Physics.Raycast(downwardPosition, Vector3.down, out hit, downwardCheckDistance))
        {
            if (hit.collider.CompareTag("Block"))
            {
                Debug.Log($"detected cube {hit.transform.name} underneat the player's adjacent grid space.");
                return true; // Object below the adjacent position has the correct tag
            }
        }
        else
        {
            Debug.Log("no cube was detected underneat the player's adjacent grid space.");
            return false;
        }

        return false; // No correct object found in this direction
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
                Debug.Log("Position is locked...\n\tThis would be where SnapToMultipleOfCubeScale() would be called...");
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
    void DiscretePushToNearestWholeNumber(Vector3 targetPosition)
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

    void OnDrawGizmos()
    {
        // Check each adjacent direction
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (Vector3 direction in directions)
        {
            Vector3 downwardPosition = transform.position + direction * checkDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(downwardPosition, Vector3.down * downwardCheckDistance);
        }
        //if (!tpInput || !tpInput.cc || !tpInput.cc._capsuleCollider) return;
        //
        //// Visualize the raycast for ground check
        //float raycastHeightOffset = tpInput.cc.colliderHeight / 2;
        //Vector3 raycastStartPosition = transform.position + new Vector3(0, raycastHeightOffset, 0);
        //Vector3 raycastDirection = Vector3.down;
        //float raycastDistance = raycastHeightOffset + 10f; // Assuming 10f is the checking distance
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(raycastStartPosition, raycastStartPosition + raycastDirection * raycastDistance);
        //
        //// SphereCast visualization for ground checking
        //if (tpInput.cc.groundCheckMethod == vThirdPersonMotor.GroundCheckMethod.High)
        //{
        //    float radius = tpInput.cc._capsuleCollider.radius * 0.9f;
        //    Vector3 sphereCastStartPosition = transform.position + Vector3.up * (tpInput.cc._capsuleCollider.radius);
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(sphereCastStartPosition, radius); // Starting sphere
        //
        //    Vector3 sphereCastEndPosition = sphereCastStartPosition + Vector3.down * (tpInput.cc._capsuleCollider.radius + tpInput.cc.groundMaxDistance);
        //    Gizmos.DrawWireSphere(sphereCastEndPosition, radius); // Ending sphere
        //
        //    // Draw connecting lines for clarity
        //    Gizmos.DrawLine(new Vector3(sphereCastStartPosition.x + radius, sphereCastStartPosition.y, sphereCastStartPosition.z),
        //                    new Vector3(sphereCastEndPosition.x +    radius, sphereCastEndPosition.y, sphereCastEndPosition.z));
        //    Gizmos.DrawLine(new Vector3(sphereCastStartPosition.x - radius, sphereCastStartPosition.y, sphereCastStartPosition.z),
        //                    new Vector3(sphereCastEndPosition.x - radius, sphereCastEndPosition.y, sphereCastEndPosition.z));
        //    Gizmos.DrawLine(new Vector3(sphereCastStartPosition.x, sphereCastStartPosition.y, sphereCastStartPosition.z + radius),
        //                    new Vector3(sphereCastEndPosition.x, sphereCastEndPosition.y, sphereCastEndPosition.z + radius));
        //    Gizmos.DrawLine(new Vector3(sphereCastStartPosition.x, sphereCastStartPosition.y, sphereCastStartPosition.z - radius),
        //                    new Vector3(sphereCastEndPosition.x, sphereCastEndPosition.y, sphereCastEndPosition.z - radius));
        //}
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class TestClimbMechanic : MonoBehaviour
{
    public ClimbState ClimbState = ClimbState.disabled;
    private int yOffset = 1;
    [Header("Forward Raycast")]
    public float rayDistance = 3f;
    public LayerMask hitMask = ~0;


    private Vector3 playerPosition;
    private Vector3 playerDirection;

    [Header("Validation Sphere")]
    public float validationSphereRadius = 0.25f;
    public LayerMask cubeLayerMask;
    [Tooltip("Tags that count as valid cubes (leave empty to accept any collider)")]
    public string[] validTags = new string[] { "Cube" };

    [Header("Gizmos")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float gizmoSphereSize = 0.15f;

    // Results
    private bool hasHit = false;
    private bool validCubeHit = false;
    private Vector3 hitPoint;
    private Collider hitCube;

    private void Start()
    {
        ClimbState = ClimbState.preCheck;
    }

    void Update()
    {
        switch (ClimbState)
        {
            case ClimbState.disabled:
                break;
            case ClimbState.preCheck:
                PreClimbCheck();
                break;
            case ClimbState.climbing:
                break;
            default:
                break;
        }
    }

    // This occurs when user is not climbing and not pushing cubes or attacking. Only
    // occurs when moving and falling.
    private void PreClimbCheck()
    {
        hasHit = false;
        validCubeHit = false;
        hitCube = null;
        hitPoint = Vector3.zero;

        playerPosition = transform.position;
        playerPosition.y = playerPosition.y + yOffset;
        playerDirection = transform.forward;

        if (Physics.Raycast(playerPosition, playerDirection, out RaycastHit hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            hasHit = true;
            hitPoint = hit.point;

           // Check if the hit point overlaps any valid cubes
            Collider[] overlaps = Physics.OverlapSphere(hitPoint, validationSphereRadius, cubeLayerMask, QueryTriggerInteraction.Ignore);
            foreach (var c in overlaps)
            {
                if (validTags == null || validTags.Length == 0 || validTags.Contains(c.tag))
                {
                    validCubeHit = true;
                    hitCube = c;
                    break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        switch (ClimbState)
        {
            case ClimbState.disabled:
                break;
            case ClimbState.preCheck:
                Gizmos.color = Color.white;
                if (hasHit)
                {
                    Gizmos.color = validCubeHit ? validColor : invalidColor;
                    Gizmos.DrawSphere(hitPoint, gizmoSphereSize);

                    // Draw validation sphere (the overlap check)
                    Gizmos.DrawWireSphere(hitPoint, validationSphereRadius);

                    Gizmos.DrawLine(playerPosition, hitPoint);

                    if (hitCube != null)
                    {
                        // outline cube bounds
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(hitCube.bounds.center, hitCube.bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawLine(playerPosition, playerPosition + playerDirection * rayDistance);
                    Gizmos.DrawWireSphere(playerPosition + playerDirection * rayDistance, validationSphereRadius);
                }
                break;
            case ClimbState.climbing:
                break;
            default:
                break;
        } 
    }
}

public enum ClimbState
{
    disabled, preCheck, climbing
}

using System;
using Invector.vCharacterController;
using UnityEngine;

public class GrabMechanic : MonoBehaviour
{
    #region variables
    private bool isGrabbing;
    private bool EnableGrab;

    [SerializeField] float m_MaxDistance = 10;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 1.5f;
    public float HeightOffset = 0;
    private Collider collider;
    private RaycastHit m_Hit;
    [SerializeField] LayerMask layerMask;

    [HideInInspector]
    public GameObject targetCube;

    private RigidbodyConstraints rbConstraints;

    public static Action OnGrab;
    public static Action OnRelease;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>(); // get player's collider
        rbConstraints = GetComponent<Rigidbody>().constraints;
    }

    // Update is called once per frame
    void Update()
    {
        CubeDetection();
        //InputHandler();
    }

    private void InputHandler()
    {
        if ((Input.GetAxis("RB") == 1 || Input.GetMouseButton(0)) && EnableGrab)
        {
            if (!isGrabbing)
            {
                Debug.Log("Grabbing");
                Grab();
            }
        }
        if ((Input.GetAxis("RB") == 0 || Input.GetMouseButtonUp(0)) || !EnableGrab)
        {
            if (isGrabbing)
            {
                Debug.Log("Releasing");
                Release();
            }
        }
    }

    private void Grab()
    {
        if (targetCube != null &&
            !targetCube.transform.parent.parent.GetComponent<CubeBehavior>().isDestroying)
        {
            OnGrab?.Invoke();

            // change state 
            // reset movement/rb variables

            // disable player rotation and camera rotation doesnt affect the player's rotation
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = true;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = true;

            // get forward axis and set player position and rotation to directly face cube at set distance
            //SetPlayerPositionAndRotation();

            // this is supposed to turn on the push animation. 
            GetComponent<vThirdPersonInput>().SetDragMovement();

            //print(targetCube.name);
            //print(targetCube.transform.parent.parent.name);

            // set the cube's state to dragging 
            targetCube.transform.parent.GetComponentInParent<CubeBehavior>().SetDragging();

            isGrabbing = true;
        }
    }

    private void Release()
    {
        // TODO: consider player only able to release cube if it's reach a whole number (-1, 0, 1) on the X/Z axis.
        if (targetCube != null &&
            !targetCube.transform.parent.parent.GetComponent<CubeBehavior>().isDestroying)
        {

            // change state 
            // reset movement/rb variables

            //targetCube.transform.parent.GetComponentInParent<CubeBehavior>().RoundCubeLocation();
            targetCube.transform.parent.parent.eulerAngles = Vector3.zero;
            targetCube.transform.parent.GetComponentInParent<CubeBehavior>().SetGround(); // TODO: find another less coupled way of changing the cube's state

            // ***need to clean this up*** \\
            // these two functions reset drag settings and stop push animation
            GetComponent<vThirdPersonInput>().SetDragMovement();
            GetComponent<vThirdPersonInput>().StopDragMovement(true);

            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = false;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = false;
            // ***need to clean this up*** \\

            isGrabbing = false;
            OnRelease?.Invoke();
        }
    }

    // check if player is in proximity of a cube
    private void CubeDetection()
    {

        m_HitDetect = Physics.BoxCast(collider.bounds.center,
            transform.localScale * (BoxColliderSize * .15f),
            transform.forward, out m_Hit,
            transform.rotation, m_MaxDistance, layerMask);

        if (m_HitDetect)
        {
            // Output the name of the Collider your Box hit
            //Debug.Log($"{gameObject.name} Hit : {m_Hit.collider.name}");
            if (m_Hit.collider.tag == "CubeHandle")
            {
                //print("hittting block");
                //print("m_Hit.distance: " + m_Hit.distance);

                targetCube = m_Hit.collider.gameObject;

                if (m_Hit.distance <= 0)
                    print("making contact with cube");

                if (m_Hit.distance < 1f || m_Hit.distance > .75f)
                {
                    if (!EnableGrab)
                        EnableGrab = true;
                }
                else
                {
                    if (isGrabbing)
                        Release();
                    else
                        EnableGrab = false;
                }
            }
        }

        // condition in case a cube underneath is destroyed or the
        // player has moved away from cube before releasing it
        else
        {
            if (isGrabbing)
                Release();
            targetCube = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = new Vector3(transform.position.x,
            transform.position.y + HeightOffset,
            transform.position.z);

        //Check if there has been a hit yet
        if (m_HitDetect)
        {

            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(pos, (transform.forward) * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(pos + (transform.forward) * m_Hit.distance, transform.localScale * BoxColliderSize);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(pos, (transform.forward) * m_MaxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(pos + (transform.forward) * m_MaxDistance, transform.localScale * BoxColliderSize);
        }
    }
}

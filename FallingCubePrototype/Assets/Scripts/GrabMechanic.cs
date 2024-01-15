using Invector.vCharacterController;
using UnityEngine;

public class GrabMechanic : MonoBehaviour
{
    #region variables
    private bool isGrabbing;
    private bool EnableGrab;

    private GameObject player;

    [SerializeField] private float m_MaxDistance = 10;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 1.5f;
    public float HeightOffset = 0;
    private Collider m_Collider;
    private RaycastHit m_Hit;
    [SerializeField] private LayerMask layerMask;

    [HideInInspector]
    public GameObject targetCube;

    private RigidbodyConstraints rbConstraints;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<Collider>(); // get player's collider
        rbConstraints = GetComponent<Rigidbody>().constraints;
    }

    // Update is called once per frame
    void Update()
    {
        CubeDetection();
        InputHandler();
    }

    private void InputHandler()
    {
        if (Input.GetAxis("RB") == 1 && EnableGrab)
        {
            if (!isGrabbing)
            {
                //Debug.Log("Grabbing");
                Grab();
            }
        }
        if (Input.GetAxis("RB") == 0 || !EnableGrab)
        {
            if (isGrabbing)
            {
                //Debug.Log("Releasing");
                Release();
            }
        }
    }

    private void Grab()
    {
        if (targetCube != null &&
            !targetCube.transform.parent.parent.GetComponent<CubeBehavior>().isDestroying)
        {
            // change state 
            // reset movement/rb variables

            GetComponent<MoveCubeMechanic>().EnableBoxMovement();

            // get forward axis and set player position and rotation to directly face cube at set distance
            //SetPlayerPositionAndRotation();

            // disable player rotation and camera rotation doesnt affect the player's rotation
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = true;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = true;

            GetComponent<vThirdPersonInput>().SetDragMovement();
            GetComponent<vThirdPersonInput>().cc.Strafe();

            //print(targetCube.name);
            //print(targetCube.transform.parent.parent.name);
            targetCube.transform.parent.GetComponentInParent<CubeBehavior>().SetDragging();

            GetComponent<MoveCubeMechanic>().SetPushPointPosition();
            GetComponent<MoveCubeMechanic>().ParentToPushPoint();

            isGrabbing = true;
        }
    }

    private void Release()
    {
        //player can ONLY release cube if it's reach 
        //a whole number (-1, 0, 1) on the  X/Z axis 
        if (targetCube != null &&
            !targetCube.transform.parent.parent.GetComponent<CubeBehavior>().isDestroying)
        {

            // change state 
            // reset movement/rb variables

            GetComponent<MoveCubeMechanic>().DeParentToPushPoint();

            targetCube.transform.parent.GetComponentInParent<CubeBehavior>().RoundCubeLocation();
            targetCube.transform.parent.parent.eulerAngles = Vector3.zero;
            targetCube.transform.parent.GetComponentInParent<CubeBehavior>().SetGround();

            // ***need to clean this up*** \\
            // these two functions reset drag settings and stop push animation
            GetComponent<vThirdPersonInput>().SetDragMovement();
            GetComponent<vThirdPersonInput>().StopDragMovement(true);

            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = false;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = false;
            GetComponent<vThirdPersonInput>().cc.Strafe();
            // ***need to clean this up*** \\

            isGrabbing = false;
            GetComponent<MoveCubeMechanic>().EnableBoxMovement();
        }
    }

    // check if player is in proximity of a cube
    private void CubeDetection()
    {

        m_HitDetect = Physics.BoxCast(m_Collider.bounds.center,
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

                if (m_Hit.distance < 1f ||
                    m_Hit.distance > .75f)
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

    private void SetPlayerPositionAndRotation()
    {
        Vector3 directionToTarget = transform.position - targetCube.transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        float distance = directionToTarget.magnitude;

        // set player position further from cube if they're too close.
        if (distance < 1.5f)
            transform.position = transform.position + (transform.forward * -.4f);
    }

    // returns player position
    private Vector3 GetPlayerPosition()
    {
        Vector3 pos = new Vector3(transform.position.x,
            transform.position.y + HeightOffset,
            transform.position.z);
        return pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 tmpPos = GetPlayerPosition();
        //Check if there has been a hit yet
        if (m_HitDetect)
        {

            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(tmpPos, (transform.forward) * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(tmpPos + (transform.forward) * m_Hit.distance, transform.localScale * BoxColliderSize);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(tmpPos, (transform.forward) * m_MaxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(tmpPos + (transform.forward) * m_MaxDistance, transform.localScale * BoxColliderSize);
        }
    }
}

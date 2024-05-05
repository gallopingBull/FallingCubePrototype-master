using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{
    public enum States
    {
        falling, grounded, dragging, init
    }

    //Cube Member Variables
    #region variables
    public int id;
    public States state;
    private Rigidbody rb;
    public Vector3 velocity;
    private Vector3 prevVel;

    public float colorPingPongTime = .35f;

    //paritcle and trail FXs
    public GameObject LandingIndicator;
    public GameObject[] ExhaustTrail;
    public GameObject[] DraggingDust;
    public GameObject GroundedDust;
    public GameObject ExplosionParticle;

    private AudioSource audioSource;
    public AudioClip fallingSFX,
        landingSFX,
        grabSFX,
        dragSFX,
        contactSFX,
        explosionSFX;

    public ColorOption color = ColorOption.Neutral; // this should be private.

    public bool isDestroying;
    public int ScoreValue = 1;

    private bool enableAlarm;

    [SerializeField] private Renderer rend;

    private bool markerEnabled;

    [SerializeField] private bool isSpawned;
    [SerializeField] private float m_MaxDistance = 10;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 1.5f;
    private Collider cubeCollider;
    public Collider ClimbingCollider;
    private RaycastHit m_Hit;
    private Collider cubeKillZone;
    private float fallDrag;

    #endregion

    #region functions 


    // Start is called before the first frame update
    void Start()
    {
        EnterState(States.init);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state != States.init)
        {
            // custom velocity calculation since blocbehavior doesn't use rb at times.
            float tmpYVel = Mathf.Round(rb.velocity.y);
            velocity = (transform.position - prevVel) / Time.deltaTime;
            prevVel = transform.position;

            m_HitDetect = Physics.BoxCast(cubeCollider.bounds.center,
                transform.localScale * BoxColliderSize,
                -transform.up, out m_Hit,
                transform.rotation, m_MaxDistance);

            StateManager(tmpYVel);

            if (enableAlarm)
                ExplosionAlert();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnCollisionExit(Collision collision)
    {
        
    }

    void StateManager(float _tmpYVel)
    {
        float tmpYVel = _tmpYVel;
        switch (state)
        {
            case States.falling:
                if (m_HitDetect)
                {
                    // Debug.Log(gameObject.name+ " Hit : " + m_Hit.collider.name);
                    EnableLandingMarker();

                    if (m_Hit.distance < 1 && !cubeCollider.enabled)
                        cubeCollider.enabled = true;

                    //Landed on other cube
                    if (tmpYVel == 0 && cubeCollider.enabled)
                    {
                        float tmpYloc = Mathf.Round(transform.position.y);
                        transform.position = new Vector3(transform.position.x, tmpYloc, transform.position.z);
                        EnterState(States.grounded);
                    }
                }

                else
                {
                    LandingIndicator.transform.position =
                        new Vector3(transform.position.x, 0, transform.position.z);
                }

                // block has reached first row
                if (transform.position.y < .5f)
                {
                    transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                    EnterState(States.grounded);

                    cubeCollider.enabled = true;
                }
                break;

            case States.grounded:

                // cube underneath this one is gone, this cube should fall
                // TODO: explore not allowing a cube to fall until its reaches the center of a grid space.
                if ((!m_HitDetect || m_Hit.distance > .75) && transform.position.y != 0)
                {
                    EnterState(States.falling);
                    //Debug.Log(gameObject.name + " should start falling");
                }
                break;

            case States.dragging:
                //Debug.Log("In Dragging State"

                if (!audioSource.isPlaying)
                {
                    PlaySFX(dragSFX);
                    audioSource.loop = true;

                }

                break;
            case States.init:
                break;
            default:
                break;
        }
    }

    public void EnterState(States _state)
    {
        ExitState(state);
        switch (_state)
        {
            case States.falling:
                //Debug.Log("set state to falling");
                if (ExhaustTrail != null)
                {
                    foreach (GameObject go in ExhaustTrail)
                    {
                        go.GetComponentInChildren<ParticleSystem>().Play();
                    }
                }
                PlaySFX(fallingSFX);

                cubeCollider.enabled = false;
                EnableRB();
                state = _state;

                break;

            case States.grounded:
                if (ExhaustTrail != null)
                {
                    foreach (GameObject go in ExhaustTrail)
                    {
                        go.GetComponentInChildren<ParticleSystem>().Stop();
                    }
                }

                if (GroundedDust != null)
                {
                    //Debug.Log("play landing paritcle");
                    GroundedDust.GetComponent<ParticleSystem>().Play();
                }
                if (state == States.falling)
                {
                    PlaySFX(landingSFX);
                }

                //RoundCubeLocation();
                state = _state;
                cubeCollider.enabled = true;
                DisableRB();

                break;

            case States.dragging:
                PlaySFX(grabSFX);
                if (DraggingDust != null)
                {
                    foreach (GameObject _go in DraggingDust)
                    {
                        _go.GetComponent<ParticleSystem>().Play();
                    }
                }
                rb.mass = 1;
                rb.isKinematic = false;
                rb.useGravity = true;
                cubeKillZone.gameObject.SetActive(false);
                ClimbingCollider.enabled = false;
                state = _state;

                break;

            case States.init:
                state = _state;
                Init();

                if (isSpawned)
                    EnterState(States.falling);
                else
                    EnterState(States.grounded);
                EnterState(States.grounded);

                break;

            default:
                break;
        }
    }

    private void ExitState(States _state)
    {
        switch (_state)
        {
            case States.falling:
                DisableLandingMarker();
                break;
            case States.grounded:
                break;
            case States.dragging:
                if (DraggingDust != null)
                {
                    foreach (GameObject _go in DraggingDust)
                    {
                        if (_go.GetComponentInChildren<ParticleSystem>().isPlaying)
                        {
                            _go.GetComponentInChildren<ParticleSystem>().Stop();
                        }
                    }
                }
                if (audioSource.isPlaying || audioSource.loop)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                }
                cubeKillZone.gameObject.SetActive(true);
                ClimbingCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    private void Init()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        // this is only for older versions of the cube prefab
        if (!rend)
            rend = GetComponentInChildren<Renderer>();

        SetMaterialColor();
        //cubeCollider = GetComponent<Collider>();
        cubeCollider = gameObject.transform.Find("CubeMesh").GetComponent<Collider>();
        cubeKillZone = gameObject.transform.Find("CubeKillZone").GetComponent<Collider>();
    }

    private void SetMaterialColor()
    {
        // Select a random color for the cube
        //color = (ColorOption)Random.Range(0, 4); // Do I need this?
        //Debug.Log($"{id} is being initialized with color: {color} in SetMaterialColor()...");
        switch (color)
        {
            case ColorOption.Neutral:
                //Debug.Log($"{gameObject.name} is a Neutral color.");
                rend.material.color = Color.white;
                break;
            case ColorOption.Red:
                //Debug.Log($"{gameObject.name} is a Red color.");
                rend.material.color = Color.red;
                break;
            case ColorOption.Green:
                //Debug.Log($"{gameObject.name} is a Green color.");
                rend.material.color = Color.green;
                break;
            case ColorOption.Blue:
                //Debug.Log($"{gameObject.name} is a Blue color.");
                rend.material.color = Color.blue;
                break;
            default:
                rend.material.color = Color.white;
                //Debug.Log($"{gameObject.name} is a Neutral color.");
                break;
        }
    }


    private void EnableRB()
    {
        //rb.mass = /*.01f*/10000000;
        fallDrag = GetNewSpeed();
        rb.drag = fallDrag/*2*/;
        // 2 is the fastest
        // 4 is the slowest
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void DisableRB()
    {
        rb.velocity = Vector3.zero;
        //rb.constraints =  RigidbodyConstraints.FreezeAll
        rb.drag = 100;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    float cubeScale = 2;
    public float snapThreshold = 0.1f; // Distance threshold for snapping to whole numbers
    public void RoundCubeLocation()
    {
        Vector3 targetPosition = new Vector3(Mathf.Round(transform.position.x),
         Mathf.Round(transform.position.y),
         Mathf.Round(transform.position.z));

        // Snap X and Z positions to multiples of cube scale
        float snappedX = (targetPosition.x / cubeScale) * cubeScale;
        float snappedZ = (targetPosition.z / cubeScale) * cubeScale;

        RigidbodyConstraints tmpConst;
        tmpConst = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezePosition;


        // Snap to whole numbers if close enough
        if (Mathf.Abs(targetPosition.x - snappedX) < snapThreshold)
        {
            Debug.Log("snapping x!");
            targetPosition.x = snappedX;
        }
        if (Mathf.Abs(targetPosition.z - snappedZ) < snapThreshold)
        {
            Debug.Log("snapping z!");
            targetPosition.z = snappedZ;
        }

        transform.position = targetPosition;
        rb.constraints = tmpConst;
    }

    // Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Check if there has been a hit yet
        if (m_HitDetect)
        {
            // Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, (-transform.up) * m_Hit.distance);

            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position + (-transform.up) * m_Hit.distance, transform.localScale * BoxColliderSize);
        }
        // If there hasn't been a hit yet, draw the ray at the maximum distancepull push
        else
        {
            // Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, (-transform.up) * m_MaxDistance);

            // Draw a cube at the maximum distance
            Gizmos.DrawWireCube(transform.position + (-transform.up) * m_MaxDistance, transform.localScale * BoxColliderSize);
        }
    }

    private void EnableLandingMarker()
    {
        markerEnabled = true;
        LandingIndicator.SetActive(true);
        Vector3 tmpLoc = new Vector3(m_Hit.transform.position.x,
                       m_Hit.transform.position.y,
                       m_Hit.transform.position.z);
        //Debug.log(m_Hit.transform.name);

        if (m_Hit.transform.tag != "Player")
        {
            LandingIndicator.transform.position = new Vector3(Mathf.Round(tmpLoc.x),
                     tmpLoc.y + 1.5f,
                     Mathf.Round(tmpLoc.z));
        }
        else
        {
            // TODO: i think i might be able to remove this else statement
            //Debug.log(m_Hit.transform.tag);
            LandingIndicator.transform.position = new Vector3(transform.position.x,
            Mathf.Round(m_Hit.transform.position.y + .75f),
            transform.position.z);
        }

        if (LandingIndicator.GetComponent<ParticleSystem>().isPaused)
            LandingIndicator.GetComponent<ParticleSystem>().Play();

    }

    private void DisableLandingMarker()
    {
        markerEnabled = false;
        LandingIndicator.GetComponent<ParticleSystem>().Stop();
        LandingIndicator.SetActive(false);
    }

    public void EnableExplosion()
    {
        enableAlarm = true;
    }

    public void ExplosionAlert()
    {
        //Debug.Log($"{gameObject.name}(rend: {rend.gameObject.name}) stepping into Explosion Alert() for color: {color}");
        Color curColor = rend.material.color;
        switch (color)
        {
            //nuetral
            case ColorOption.Neutral:
                Debug.Log($"{gameObject.name} is nuetral. this cube should not be exploding lol.");
                break;
            //red
            case ColorOption.Red:
                rend.material.color = Color.Lerp(curColor, Color.magenta * 5, Mathf.PingPong(Time.time, colorPingPongTime));
                break;

            //green
            case ColorOption.Green:
                rend.material.color = Color.Lerp(curColor, Color.yellow * 5, Mathf.PingPong(Time.time, colorPingPongTime));
                break;

            //blue
            case ColorOption.Blue:
                rend.material.color = Color.Lerp(curColor, Color.cyan * 5, Mathf.PingPong(Time.time, colorPingPongTime));
                break;
            default:
                break;
        }
    }

    public void DestroyCube()
    {
        StartCoroutine("DestoryCube");
    }

    private IEnumerator DestoryCube()
    {
        EnableExplosion(); // activates explosion warning fx

        yield return new WaitForSeconds(1.5f);

        AudioManager._instance.PlaySFX(explosionSFX);
        Instantiate(ExplosionParticle, transform.position, transform.rotation);

        Destroy(gameObject);

        StopCoroutine("DestoryCube");
    }

    // this float is assigned to rb.mass when
    // cube is falling 
    private float GetNewSpeed()
    {
        if (!GameManager.gm)
            return 0;
        float tmpSpeed;
        if (GameManager.gm.CountdownTime > 67.5)
        {
            tmpSpeed = 4f;
        }
        else if (GameManager.gm.CountdownTime > 45)
        {
            tmpSpeed = 3.5f;
        }
        else if (GameManager.gm.CountdownTime > 45)
        {
            tmpSpeed = 3.0f;
        }
        else if (GameManager.gm.CountdownTime > 22.5)
        {
            tmpSpeed = 2.5f;
        }
        else
        {
            tmpSpeed = 2.0f;
        }
        return tmpSpeed;
    }

    public void PlaySFX(AudioClip sfx)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(sfx);
    }

    #region public state callers
    public void InitializeCube(int _id, ColorOption _color)
    {
        //Debug.Log($"cube.id({id}) is being initialized with color: {color}");
        id = _id;
        color = _color;
        gameObject.name = $"Cube-{id}-{color}".ToUpper();
        EnterState(States.init);
    }
    public void SetFalling()
    {
        EnterState(States.falling);
    }
    public void SetGround()
    {
        EnterState(States.grounded);
    }
    public void SetDragging()
    {
        EnterState(States.dragging);
    }

    #endregion

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    public enum States
    {
        falling, grounded, dragging, init
    }
    ///
    //Cube Member Variables
    #region variables
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

    public AudioClip fallingSFX, 
        landingSFX, 
        grabSFX,
        dragSFX,
        contactSFX,
        explosionSFX;


    public Color[] IgnoreCubeColors;
    public Color curColor;
    [SerializeField] private int colorIndex;


    [HideInInspector] public bool isDestroying;
    public int ScoreValue = 1;

    private bool enableAlarm;
    private Renderer rend;

    private bool markerEnabled;

    [SerializeField] private bool isSpawned;
    [SerializeField] private float m_MaxDistance = 10;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 1.5f;
    private Collider m_Collider;
    private RaycastHit m_Hit;

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
            //custom velocity calculation since blocbehavior doesn't
            //use rb at times.
            float tmpYVel = Mathf.Round(rb.velocity.y);
            velocity = (transform.position - prevVel) / Time.deltaTime;
            prevVel = transform.position;

            m_HitDetect = Physics.BoxCast(m_Collider.bounds.center,
                transform.localScale * BoxColliderSize,
                -transform.up, out m_Hit,
                transform.rotation, m_MaxDistance);

            StateManager(tmpYVel);

            if (enableAlarm)
                ExplosionAlert();
        }
    }

    void StateManager(float _tmpYVel)
    {
        float tmpYVel = _tmpYVel;
        switch (state)
        {
            case States.falling:
                if (m_HitDetect)
                {
                    //Debug.Log(gameObject.name+ " Hit : " + m_Hit.collider.name);
                    EnableLandingMarker();

                    if (m_Hit.distance < 1 && !GetComponent<BoxCollider>().enabled)
                        GetComponent<BoxCollider>().enabled = true;

                    //Landed on other cube
                    if (tmpYVel == 0 && GetComponent<BoxCollider>().enabled)
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

                //block has reached first row
                if (transform.position.y < .5f)
                {
                    transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                    EnterState(States.grounded);

                    GetComponent<BoxCollider>().enabled = true;
                }
                break;

            case States.grounded:

                //cube underneath this one is gone, this cube should fall
                if ((!m_HitDetect || m_Hit.distance > .75) && transform.position.y != 0)
                {
                    EnterState(States.falling);
                    //print(gameObject.name + " should start falling");
                }
                break;

            case States.dragging:
                //print("In Dragging State"
                
                if (!GetComponent<AudioSource>().isPlaying)
                {
                    PlaySFX(dragSFX);
                    GetComponent<AudioSource>().loop = true;

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
                //print("set state to falling");
                if (ExhaustTrail != null)
                {
                    foreach (GameObject go in ExhaustTrail)
                    {
                        go.GetComponentInChildren<ParticleSystem>().Play();
                    }
                }
                PlaySFX(fallingSFX);

                GetComponent<BoxCollider>().enabled = false;
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
                    //print("play landing paritcle");
                    GroundedDust.GetComponent<ParticleSystem>().Play();
                }
                if (state == States.falling)
                {
                    PlaySFX(landingSFX);
                }

                rb.mass = 1000;
                state = _state;
                GetComponent<BoxCollider>().enabled = true;
                DisableRB();

                break;

            case States.dragging:
                PlaySFX(grabSFX);
                if (DraggingDust != null)
                {
                    foreach (GameObject _go in DraggingDust)
                    {
                        print("play particle");
                        _go.GetComponent<ParticleSystem>().Play();
                    }
                }
                rb.mass = 1;
                rb.isKinematic =false ;
                rb.useGravity = true;

                //GetComponent<BoxCollider>().enabled = true;
                state = _state;

                break;

            case States.init:
                state = _state;
                Init();

                if (isSpawned)
                    EnterState(States.falling);
                else
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
                if (GetComponent<AudioSource>().isPlaying ||
                    GetComponent<AudioSource>().loop)
                {
                    GetComponent<AudioSource>().Stop();
                    GetComponent<AudioSource>().loop = false;

                }
                break;
            default:
                break;
        }
    }

    private void Init()
    {   
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        curColor = rend.material.color;
        m_Collider = GetComponent<Collider>();
    }

    private void EnableRB()
    {
        rb.mass = /*.01f*/10000000;
        fallDrag = GetNewSpeed();
        rb.drag = fallDrag/*2*/;
        //2 is the fastest
        //4 is the slowest
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

    public void RoundCubeLocation()
    {
        RigidbodyConstraints tmpConst;
        tmpConst = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezePosition;

        Vector3 pos = new Vector3(Mathf.Round(transform.position.x),
          Mathf.Round(transform.position.y),
          Mathf.Round(transform.position.z));

        transform.position = pos;
        rb.constraints = tmpConst;
    
    }

    //Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (m_HitDetect)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, (-transform.up) * m_Hit.distance);

            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position + (-transform.up) * m_Hit.distance, transform.localScale * BoxColliderSize);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distancepull push
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, (-transform.up) * m_MaxDistance);

            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(transform.position + (-transform.up) * m_MaxDistance, transform.localScale * BoxColliderSize);
        }
    }

    private void EnableLandingMarker()
    {
        markerEnabled = true;

        Vector3 tmpLoc = new Vector3(m_Hit.transform.position.x,
                       m_Hit.transform.position.y,
                       m_Hit.transform.position.z);
        //print(m_Hit.transform.name);

        if (m_Hit.transform.tag != "Player")
        {
            LandingIndicator.transform.position = new Vector3(Mathf.Round(tmpLoc.x),
                     tmpLoc.y + 1.5f,
                     Mathf.Round(tmpLoc.z));
        }

        else
        {
           print(m_Hit.transform.tag);
           LandingIndicator.transform.position = new Vector3(transform.position.x,
           Mathf.Round(m_Hit.transform.position.y + .75f),
           transform.position.z);
        }

        if (LandingIndicator.GetComponent<ParticleSystem>().isPaused)
        {
            LandingIndicator.GetComponent<ParticleSystem>().Play();
        }
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
        //GetComponent<BoxCollider>().enabled = false;
    }

    public void ExplosionAlert() 
    { 
        switch(colorIndex) {
            //red
            case 0:
                rend.material.color = Color.Lerp(curColor, Color.magenta * 5, Mathf.PingPong(Time.time, colorPingPongTime));
                break;

            //green
            case 1:
                rend.material.color = Color.Lerp(curColor, Color.yellow * 5, Mathf.PingPong(Time.time, colorPingPongTime));
                break;

            //blue
            case 2:
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
        //instantiate explosion dust for cube

        AudioManager._instance.PlaySFX(explosionSFX);
        Instantiate(GetComponent<BlockBehavior>().ExplosionParticle,
            GetComponent<Transform>().position,
            GetComponent<Transform>().rotation);

        Destroy(gameObject);

        StopCoroutine("DestoryCube");

        #region testing

        /*foreach(GameObject target in tmpTargets)
          {
              print("destroying cube");
              GameManager.gm.AddPoints(target.GetComponent<BlockBehavior>().ScoreValue);
              Destroy(target);
          }
          tmpTargets.Clear();

          //print("destroying cube");
          GameManager.gm.AddPoints(transform.parent.parent.gameObject.GetComponent<BlockBehavior>().ScoreValue);
          Destroy(transform.parent.parent.gameObject);
          */
        #endregion
    }


    //this float is assigned to rb.mass when
    //cube is falling
    private float GetNewSpeed()
    {
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
        if (GetComponent<AudioSource>().isPlaying){
            GetComponent<AudioSource>().Stop();
        }

        GetComponent<AudioSource>().PlayOneShot(sfx);
    }

    #region public state callers
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

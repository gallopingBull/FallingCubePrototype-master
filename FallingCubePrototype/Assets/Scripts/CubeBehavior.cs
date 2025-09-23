﻿using System;
using System.Collections;
//using System.Threading; 
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class CubeBehavior : MonoBehaviour
{ 
    public enum States
    {
        falling, grounded, dragging, init
    }

    // Cube Member Variables
    #region variables
    public Guid id; // I think it might be better to use a GUID instead.
    
    public States state;
    private Rigidbody rb;
    public Vector3 velocity;
    private Vector3 prevVel;

    public float colorPingPongTime = .35f;

    // paritcle and trail FXs
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
    private Collider cubeCollider; // used for general mesh detecting.
    private Collider cubePushZoneCollider; // used for when to push player while cube falls.

    public Collider ClimbingCollider;
    private RaycastHit m_Hit;
    private Collider cubeKillZone;
    private float fallDrag;
    LayerMask layerMask;
    private MoveCubeMechanic player;
    Collider[] colliders;

    // cube infor panel text game objects
    private bool disablingCubeInfo = false;
    [SerializeField] GameObject cubeInfoPanel;
    [SerializeField] TextMeshProUGUI cubeIDText;
    [SerializeField] TextMeshProUGUI cubeStateText;
    [SerializeField] TextMeshProUGUI cubeColorText;
    [SerializeField] TextMeshProUGUI cubePosText;
    private Coroutine lastRoutine = null;

    #endregion

    #region functions 
    // Start is called before the first frame update
    void Awake()
    {
        // Temporarily ignore collisions with the same game object
        colliders = GetComponentsInChildren<Collider>();
        cubePushZoneCollider = transform.Find("PushZone").GetComponent<Collider>();
        EnterState(States.init);
        layerMask = LayerMask.NameToLayer("Cube");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state != States.init)
        {
            // Custom velocity calculation since cube behavior doesn't use rb at times.
            float tmpYVel = Mathf.Round(rb.velocity.y);
            velocity = (transform.position - prevVel) / Time.deltaTime;
            prevVel = transform.position;

            m_HitDetect = Physics.BoxCast(cubeCollider.bounds.center,
                transform.localScale * BoxColliderSize,
                -transform.up, out m_Hit,
                transform.rotation, m_MaxDistance);

            // Update cubeInfoPanel text objects here.
            if (cubeInfoPanel && cubeInfoPanel.activeInHierarchy)
            {
                // Truncate the string to a desired length
                int desiredLength = 8; // Example: take the first 8 characters
                string truncatedGuidString = id.ToString().Substring(0, Math.Min(id.ToString().Length, desiredLength));
                cubeIDText.text = truncatedGuidString;
                cubeStateText.text = state.ToString();
                cubePosText.text =
                    $"x: {Mathf.Floor(transform.position.x * 10) / 10}, y: {Mathf.Floor(transform.position.y * 10) / 10}, z: {Mathf.Floor(transform.position.z * 10) / 10}";
            }

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
                // TODO: Automatically release cube if space undearneath is empty

                if ((!m_HitDetect || m_Hit.distance > .75) && transform.position.y != 0)
                {
                    EnterState(States.falling);
                    //Debug.Log(gameObject.name + " should start falling");
                }
                break;

            case States.dragging:
                //Debug.Log("In Dragging State");
                if (!audioSource.isPlaying)
                {
                    PlaySFX(dragSFX);
                    audioSource.loop = true;
                }
                //Debug.Log($"m_Hit.name: {m_Hit.transform.gameObject.name} - m_HitDetect: {m_HitDetect} - m_Hit.distance: {m_Hit.distance}");
                if ((!m_HitDetect || m_Hit.distance > .75) && transform.position.y != 0)
                {
                    // release cube here 
                    // TODO: check if player is dragging to avoid unecessary call StopPush... also if user has ability to move cubes over gaps.
                    player = GameObject.Find("Player").GetComponent<MoveCubeMechanic>();

                    player.StopPushAndPullCaller();
                    EnterState(States.falling);
                    //Debug.Log(gameObject.name + " should start falling");
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
                cubePushZoneCollider.enabled = true;   
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

                //if (state == States.falling)
                //{
                //    PlaySFX(landingSFX);
                //    GameManager.gm.CubeManager.FinalizeCubePosition(gameObject, state);
                //}

                if (GameManager.gm && GameManager.gm.CubeManager.init)
                {
                    GameManager.gm.CubeManager.FinalizeCubePosition(gameObject, state);
                }

                // CHECK FOR STACKED CUBES AND FINALIZE THEIR POSITION
                var stackedCubes = transform.FindObjectsWithTag("Block");
                if (stackedCubes != null && stackedCubes.Count > 0)
                {
                    foreach (var cube in stackedCubes)
                    {
                        // Weird condition that will ensure only nested stacked
                        // cubes are reset.
                        if (cube.layer == LayerMask.NameToLayer("Default"))
                        {
                            GameManager.gm.CubeManager.FinalizeCubePosition(cube, cube.GetComponent<CubeBehavior>().state);
                        }
                    }
                }

                state = _state;
                cubeCollider.enabled = true;
                DisableRB();

                break;

            case States.dragging:

                GameManager.gm.CubeManager.AddStackedCubes(gameObject);

                PlaySFX(grabSFX);
                if (DraggingDust != null)
                {
                    foreach (GameObject _go in DraggingDust)
                    {
                        _go.GetComponent<ParticleSystem>().Play();
                    }
                }

                cubeInfoPanel.SetActive(true);
                if (disablingCubeInfo)
                {
                    StopCoroutine(lastRoutine);
                    disablingCubeInfo = false;
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
                cubePushZoneCollider.enabled = false;
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
                if (!disablingCubeInfo)
                    lastRoutine = StartCoroutine(HideCubeInfo());
                cubeKillZone.gameObject.SetActive(true);
                ClimbingCollider.enabled = true;
                
                //GameManager.gm.CubeManager.RemoveStackedCubes(gameObject);
                //if (GameManager.gm && GameManager.gm.CubeManager.init)
                //{
                //    GameManager.gm.CubeManager.FinalizeCubePosition(gameObject, _state);
                //}

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
        
        cubeIDText.text = id.ToString();
        cubeColorText.text = color.ToString();

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

    private IEnumerator HideCubeInfo()
    {
        disablingCubeInfo = true;
        yield return new WaitForSeconds(3f);
        cubeInfoPanel.SetActive(false);
        disablingCubeInfo = false;
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
        if (GameManager.gm.currentCountdownTime > 67.5)
        {
            tmpSpeed = 4f;
        }
        else if (GameManager.gm.currentCountdownTime > 45)
        {
            tmpSpeed = 3.5f;
        }
        else if (GameManager.gm.currentCountdownTime > 45)
        {
            tmpSpeed = 3.0f;
        }
        else if (GameManager.gm.currentCountdownTime > 22.5)
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
    public void InitializeCube(Guid _id, ColorOption _color)
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

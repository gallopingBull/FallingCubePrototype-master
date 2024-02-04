using ProBuilder2.Common;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

//TODO: Change class name to CeilingSpawner
public class AerialCubeSpawner : MonoBehaviour
{
    #region variables
    public bool EnableSpawner = false;
    [SerializeField]
    private bool isSpawning = false;
    [SerializeField]
    private bool init = true;
    public float yOffset = 15f; // Spawn height above the player 
    private List<Vector2> gridPositions;
    [HideInInspector] public bool spawnPlayer;

    public Transform CurSpawnLoc;
    public List<Transform> SpawnLocs;

    private Transform parent;
    private float randFloat;

    [SerializeField]
    private float maxDistance = 30;

    private bool hitDetect;

    public float BoxColliderSize = 30;
    private Collider collider;
    private RaycastHit hit;

    public GameObject targetCube;
    public GameObject CubeHit;

    [SerializeField]
    private GameObject player;

    public int randLoc;
    public Queue<int> lastSpawnLocs;
    public bool CheckColor = false;

    private int MAXCubeSpawnAmmount;
    private int CubeSpawnAmmount;

    private float timer;

    private int playerSpawnPoint;
    private CubeManager cubeManager;

    #endregion

    #region functions
    // Start is called before the first frame update
    void Start()
    {
        cubeManager = GameObject.Find("CubeManager").GetComponent<CubeManager>();
        ArenaGenerator.OnFloorComplete += EnableCubeSpawner;
        lastSpawnLocs = new Queue<int>();
        collider = GetComponent<Collider>();
        if (parent == null)
        {
            parent = new GameObject("SpawnLocs").transform;
            parent.transform.position = Vector3.zero;
        }

        gridPositions = GenerateGridPositions();
        GenerateCeilingSpawnLocations();

        Debug.Log("AerialCubeSpawner initialized");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Spawn();

        if (CurSpawnLoc != null)
        {
            if (CheckColor && isSpawning)
            {
                hitDetect = Physics.BoxCast(CurSpawnLoc.transform.position,
                   transform.localScale * BoxColliderSize,
                   -transform.up, out hit,
                   transform.rotation, maxDistance);
            }

            if (spawnPlayer && !CheckColor && !isSpawning)
            {
                hitDetect = Physics.BoxCast(CurSpawnLoc.transform.position,
                   transform.localScale * BoxColliderSize,
                   -transform.up, out hit,
                   transform.rotation, maxDistance);
            }
        }
    }

    public void EnableCubeSpawner()
    {
        EnableSpawner = true;
        StartCoroutine("AutoSpawner");
    }

    IEnumerator AutoSpawner()
    {
        if (init)
            MAXCubeSpawnAmmount = Random.Range(10, 15);

        while (EnableSpawner)
        {
            timer = GameManager.gm._time;

            if (init)
            {
                if (CubeSpawnAmmount == MAXCubeSpawnAmmount)
                {
                    EnableSpawner = false;

                    StopCoroutine(AutoSpawner());
                }
                CubeSpawnAmmount++;
                randFloat = Random.Range(.25f, .35f);
            }

            else
            {
                randFloat = GetNewSpawnRate();
            }

            yield return new WaitForSeconds(randFloat);
            SpawnManager();
            CheckColor = true;
        }
    }

    // change rate to spawn based on current time from Gm.time
    // as time progresses more cubes spawn
    private float GetNewSpawnRate()
    {
        float tmpRate;
        if (GameManager.gm.CountdownTime > 67.5)
        {
            tmpRate = Random.Range(8, 10);
        }
        else if (GameManager.gm.CountdownTime > 45)
        {
            tmpRate = Random.Range(6.5f, 8);
        }
        else if (GameManager.gm.CountdownTime > 45)
        {
            tmpRate = Random.Range(4, 6.5f);
        }
        else if (GameManager.gm.CountdownTime > 22.5)
        {
            tmpRate = Random.Range(2.5f, 4f);
        }
        else
        {
            tmpRate = Random.Range(2, 2.5f);
        }
        return tmpRate;
    }

    private int GetRandomSpawnPosition()
    {
        int tmpLoc = Random.Range(0, SpawnLocs.Count);
        // TODO: this is a hacky fix. find better solutions for this null check.
        //Debug.Log($"is lastSpawnLocs null: {lastSpawnLocs}");
        if (lastSpawnLocs == null)
            return 0;
        if (lastSpawnLocs.Contains(tmpLoc) || (tmpLoc == playerSpawnPoint && init))
        {
            return GetRandomSpawnPosition();
        }

        else
        {
            if (lastSpawnLocs.Count > 0)
                lastSpawnLocs.Dequeue();
            lastSpawnLocs.Enqueue(tmpLoc);
            return tmpLoc;
        }
    }

    // spawns cubes
    private void SpawnManager()
    {
        isSpawning = true;

        randLoc = GetRandomSpawnPosition();
        CurSpawnLoc = SpawnLocs[randLoc];

        ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);
        int id = cubeManager.SpawnDatas.Count;
        SpawnData spawnData = new SpawnData { id = id, position = CurSpawnLoc.position, color = color };
        cubeManager.SpawnCube(spawnData);

        targetCube = cubeManager.cubes.Last();

        targetCube.SetActive(false);
        Invoke("CheckSpawnPosition", .5f);
    }

    public void SpawnPlayerCaller()
    {
        //Debug.Log("spawn player caller");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        spawnPlayer = true;
        randLoc = GetRandomSpawnPosition();
        CurSpawnLoc = SpawnLocs[randLoc];

        Invoke("CheckPlayerSpawnPosition", .1f);
    }

    private void CheckPlayerSpawnPosition()
    {
        if (spawnPlayer)
        {
            if (hitDetect &&
            hit.transform.gameObject.tag == "Block")
            {
                if (!player.activeInHierarchy)
                {
                    spawnPlayer = false;
                    init = false;
                    player.transform.position = hit.transform.position;

                    playerSpawnPoint = randLoc;

                    player.SetActive(true);
                    //GameManager.gm.StartGame();
                }
            }
            else
            {
                SpawnPlayer();
                return;
            }
        }
    }

    private void CheckSpawnPosition()
    {
        // just in case cube is destoryed before it is landed on
        if (!hit.collider)
        {
            if (!init)
            {
                GetNewCube();
                return;
            }
            else
            {
                if (targetCube != null)
                    targetCube.SetActive(true);
            }
        }
        else
        {
            CubeHit = null;
            CubeHit = hit.transform.gameObject;
            if (hitDetect && CubeHit.tag == "Player" && init)
            {
                GetNewCube();
                return;
            }

            if (hitDetect && CubeHit.tag == "Block")
            {
                #region debug logs
                //Debug.Log("****----hitting block----****");
                //Debug.Log("m_Hit object = " + CubeHit);
                //Debug.Log("m_Hit color: " + CubeHit.GetComponent<BlockBehavior>().curColor);

                //Debug.Log("tmpCube name = " + tmpCube.gameObject.name);
                //Debug.Log("tmpCube color: " + tmpCube.GetComponent<Renderer>().material.color);
                //Debug.Log("****------****");
                #endregion

                // spawned cube can't land on or by similar color
                // delete cube and spawn another 
                if (targetCube.GetComponent<CubeBehavior>().color ==
                    CubeHit.GetComponent<CubeBehavior>().color)
                {
                    GetNewCube();
                    return;
                }
                else
                    targetCube.SetActive(true);
            }

            else
                targetCube.SetActive(true);

        }

        isSpawning = false;
        CheckColor = false;
    }

    public void Spawn()
    {
        SpawnManager();
        CheckColor = true;
    }

    private void GetNewCube()
    {
        GameObject _tmpCube;
        _tmpCube = targetCube;
        targetCube = null;
        Destroy(_tmpCube);
        SpawnManager();
    }

    public List<Vector2> GenerateGridPositions()
    {
        int gridSizeX = cubeManager.gridSizeX;
        int gridSizeZ = cubeManager.gridSizeZ;
        gridPositions = new List<Vector2>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector2 newPos = new Vector2(x, z);
                gridPositions.Add(newPos);
            }
        }

        return gridPositions;
    }

    public void GenerateCeilingSpawnLocations()
    {
        SpawnLocs = new List<Transform>();

        for (int x = 0; x < gridPositions.Count; x++)
        {

            GameObject loc = new GameObject();

            loc.name = $"SpawnLoc {x}";
            loc.transform.parent = parent.transform;
            loc.transform.position = new Vector3(gridPositions[x].x * 2,
            yOffset,
            gridPositions[x].y * 2);

            SpawnLocs.Add(loc.transform);
        }
    }

    void OnDisable()
    {
        ArenaGenerator.OnFloorComplete -= EnableCubeSpawner;
    }

    private void OnDrawGizmos()
    {
        if (isSpawning)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }


        Vector3 tmpPos;
        if (CurSpawnLoc != null)
        {
            tmpPos = new Vector3(CurSpawnLoc.transform.position.x,
            CurSpawnLoc.transform.position.y - 3,
            CurSpawnLoc.transform.position.z);


            //Check if there has been a hit yet
            if (hitDetect)
            {
                //Draw a Ray forward from GameObject toward the hit
                Gizmos.DrawRay(tmpPos,
                    (-transform.up) * hit.distance);

                //Draw a cube that extends to where the hit exists
                Gizmos.DrawWireCube(tmpPos + (-transform.up) * hit.distance,
                    transform.localScale * BoxColliderSize);
            }
            //If there hasn't been a hit yet, draw the ray at the maximum distancepull push
            else
            {
                //Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.DrawRay(tmpPos, (-transform.up) * maxDistance);

                //Draw a cube at the maximum distance
                Gizmos.DrawWireCube(tmpPos + (-transform.up) * maxDistance, transform.localScale * BoxColliderSize);
            }
        }
    }
    #endregion
}

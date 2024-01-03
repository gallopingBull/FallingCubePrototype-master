using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    #region variables
    public bool EnableSpawner = false;
    [SerializeField]
    private bool isSpawning = false;
    [SerializeField]
    private bool init = true;

    [HideInInspector] public bool spawnPlayer;

    public GameObject[] Blocks;

    public GameObject CurSpawnLoc;
    public GameObject[] SpawnLocs;

    private float randFloat;

    [SerializeField]
    private float m_MaxDistance = 30;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 30;
    private Collider m_Collider;
    private RaycastHit m_Hit;

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

    #endregion

    #region functions
    // Start is called before the first frame update
    void Start()
    {
        lastSpawnLocs = new Queue<int>();
        m_Collider = GetComponent<Collider>();
        if (EnableSpawner)
            StartCoroutine(AutoSpawner());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Spawn();

        if (CheckColor && isSpawning)
        {
            m_HitDetect = Physics.BoxCast(CurSpawnLoc.transform.position,
               transform.localScale * BoxColliderSize,
               -transform.up, out m_Hit,
               transform.rotation, m_MaxDistance);
        }

        if (spawnPlayer && !CheckColor && !isSpawning)
        {
            m_HitDetect = Physics.BoxCast(CurSpawnLoc.transform.position,
               transform.localScale * BoxColliderSize,
               -transform.up, out m_Hit,
               transform.rotation, m_MaxDistance);
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

    //change rate to spawn based on current time from Gm.time
    //as time progresses more cubes spawn
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

    private int GetRandomCubeIndex()
    {
        //2 out of 3 cubes should be white
        int randNum = Random.Range(0, 2);
        if (randNum % 2 == 0)
        {
            //white block index
            return 3;
        }
        else
        {
            return Random.Range(0, Blocks.Length);
        }
    }

    private int GetRandomSpawnPosition()
    {
        int tmpLoc = Random.Range(0, SpawnLocs.Length);
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

    //spawns cubes
    private void SpawnManager()
    {
        randLoc = GetRandomSpawnPosition();
        int randBlock = GetRandomCubeIndex(); // TODO: This will probably be changed to a random number color instead of using the prefab variants id.
        isSpawning = true;

        CurSpawnLoc = SpawnLocs[randLoc];

        targetCube = Instantiate(Blocks[0], CurSpawnLoc.transform.position, transform.rotation);
        targetCube.SetActive(false);
        Invoke("CheckSpawnPosition", .5f);
    }

    public void SpawnPlayerCaller()
    {
        //print("spawn player caller");
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
            if (m_HitDetect &&
            m_Hit.transform.gameObject.tag == "Block")
            {
                if (!player.activeInHierarchy)
                {
                    spawnPlayer = false;
                    init = false;
                    player.transform.position = m_Hit.transform.position;

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
        //just in case cube is destoryed before it is landed on
        if (!m_Hit.collider)
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
            CubeHit = m_Hit.transform.gameObject;
            if (m_HitDetect && CubeHit.tag == "Player" && init)
            {
                GetNewCube();
                return;
            }

            if (m_HitDetect && CubeHit.tag == "Block")
            {
                #region debugging prints
                //print("****----hitting block----****");
                //print("m_Hit object = " + CubeHit);
                //print("m_Hit color: " + CubeHit.GetComponent<BlockBehavior>().curColor);
                //
                //print("tmpCube name = " + tmpCube.gameObject.name);
                //print("tmpCube color: " + tmpCube.GetComponent<Renderer>().material.color);
                //print("****------****");
                #endregion

                // spawned cube can't land on or by similar color
                // delete cube and spawn another 
                if (targetCube.GetComponent<BlockBehavior>().color ==
                    CubeHit.GetComponent<BlockBehavior>().color)
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

    public void Spawn(/*int spawnAmmount, int blockIndex, Vector3 spawnLoc*/)
    {
        SpawnManager();
        CheckColor = true;

        #region old
        /*randLoc = GetRandomSpawnPosition();
        int randBlock = GetRandomCubeIndex();

        for (int i = 0; i < spawnAmmount; i++)
        {
            Instantiate(Blocks[randBlock], SpawnLocs[randLoc].gameObject.transform.position, transform.rotation);
        }*/
        //CheckColor = false;
        #endregion 
    }

    private void GetNewCube()
    {
        GameObject _tmpCube;
        _tmpCube = targetCube;
        targetCube = null;
        Destroy(_tmpCube);
        SpawnManager();
    }

    public void SpawnSingleCube(int blockIndex, Vector3 spawnLoc)
    {
        targetCube = Instantiate(Blocks[blockIndex], spawnLoc, transform.rotation);
        targetCube.SetActive(false);
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
            if (m_HitDetect)
            {
                //Draw a Ray forward from GameObject toward the hit
                Gizmos.DrawRay(tmpPos,
                    (-transform.up) * m_Hit.distance);

                //Draw a cube that extends to where the hit exists
                Gizmos.DrawWireCube(tmpPos + (-transform.up) * m_Hit.distance,
                    transform.localScale * BoxColliderSize);
            }
            //If there hasn't been a hit yet, draw the ray at the maximum distancepull push
            else
            {
                //Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.DrawRay(tmpPos, (-transform.up) * m_MaxDistance);

                //Draw a cube at the maximum distance
                Gizmos.DrawWireCube(tmpPos + (-transform.up) * m_MaxDistance, transform.localScale * BoxColliderSize);
            }
        }
    }
    #endregion
}

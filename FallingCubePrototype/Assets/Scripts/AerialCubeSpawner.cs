using ProBuilder2.Common;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public int randLoc;
    public Queue<int> lastSpawnLocs;
    public bool CheckColor = false;

    private int MAXCubeSpawnAmmount;
    private int CubeSpawnAmmount;

    private float timer;

    private CubeManager cubeManager;

    private int attempts = 0;
    private const int maxAttempts = 10;
    [SerializeField] float minimumDistance = 4f; // TODO: This value should also scale by cube size

    #endregion

    #region functions
    // Start is called before the first frame update

    void Start()
    {
        cubeManager = GameObject.Find("CubeManager").GetComponent<CubeManager>();
        lastSpawnLocs = new Queue<int>();
        collider = GetComponent<Collider>();
        GameManager.OnGameBegin += EnableCubeSpawner;

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

            if (!CheckColor && !isSpawning)
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
        Debug.Log("AerialCubeSpawner enabled");
        EnableSpawner = true;
        StartCoroutine(AutoSpawner());
    }

    private IEnumerator AutoSpawner()
    {
        // TODO: maybe add some delay here before spawning begins??? 

        if (init)
            MAXCubeSpawnAmmount = UnityEngine.Random.Range(10, 15);

        while (EnableSpawner)
        {
            timer = GameManager.gm.initialCountdownTime;

            if (init)
            {
                if (CubeSpawnAmmount == MAXCubeSpawnAmmount)
                {
                    EnableSpawner = false;

                    StopCoroutine(AutoSpawner());
                }
                CubeSpawnAmmount++;
                randFloat = UnityEngine.Random.Range(.25f, .35f);
            }

            else
            {
                randFloat = GetNewSpawnRate();
            }
            //Debug.Log($"{randFloat} seconds till next spawn...");
            yield return new WaitForSeconds(5f);
            Spawn();
            CheckColor = true;
        }
    }

    // spawns cubes
    public void Spawn()
    {
        if (!enabled)
        {
            //Debug.Log("aerial spawn component is currently disabled, returning...");
            return;
        }

        CheckColor = true;
        isSpawning = true;

        randLoc = GetRandomSpawnPosition();
        CurSpawnLoc = SpawnLocs[randLoc];

        Guid id = Guid.NewGuid();
        Vector3 cubePosition = new Vector3(CurSpawnLoc.position.x, 0, CurSpawnLoc.position.z);

        float cubebeneath =
        cubeManager.CubeSize + cubeManager.SpawnDatas.Find(spawnData => spawnData.position.x == CurSpawnLoc.position.x && spawnData.position.z == CurSpawnLoc.position.z).position.y;
        //Debug.Log($"cubebeneath: {cubebeneath} for cube at {CurSpawnLoc.position}");
        cubePosition.y = cubebeneath;
        ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);

        if (color != ColorOption.Neutral)
        {
            while ((cubeManager.CheckIfColorIsNearByDistance(id, cubePosition, color, minimumDistance) || cubeManager.colorsUsed.Contains(color)) && attempts < maxAttempts)
            {
                //Debug.Log($"Color {color} is nearby or already used, trying again...");
                color = (ColorOption)UnityEngine.Random.Range(0, 4);
                attempts++;
                if (attempts == maxAttempts)
                {
                    color = ColorOption.Neutral;
                    attempts = 0;
                    break;
                }
            }
        }

        cubeManager.colorsUsed.Clear();
        SpawnData spawnData = new SpawnData { id = id, position = CurSpawnLoc.position, color = color };
        cubeManager.SpawnCube(spawnData);
        isSpawning = false;
        CheckColor = false;
    }

    // change rate to spawn based on current time from Gm.time
    // as time progresses more cubes spawn
    private float GetNewSpawnRate()
    {
        float tmpRate;
        if (GameManager.gm.currentCountdownTime > 67.5)
        {
            tmpRate = UnityEngine.Random.Range(8, 10);
        }
        else if (GameManager.gm.currentCountdownTime > 45)
        {
            tmpRate = UnityEngine.Random.Range(6.5f, 8);
        }
        else if (GameManager.gm.currentCountdownTime > 45)
        {
            tmpRate = UnityEngine.Random.Range(4, 6.5f);
        }
        else if (GameManager.gm.currentCountdownTime > 22.5)
        {
            tmpRate = UnityEngine.Random.Range(2.5f, 4f);
        }
        else
        {
            tmpRate = UnityEngine.Random.Range(2, 2.5f);
        }
        return tmpRate;
    }

    private int GetRandomSpawnPosition()
    {
        int tmpLoc = UnityEngine.Random.Range(0, SpawnLocs.Count);
        if (lastSpawnLocs == null)
            return 0;

        if (lastSpawnLocs.Contains(tmpLoc))
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
        Debug.Log("Generating spawn locations...");
        SpawnLocs = new List<Transform>();

        for (int x = 0; x < gridPositions.Count; x++)
        {

            GameObject loc = new GameObject();

            //loc.name = $"SpawnLoc {x}";
            loc.transform.parent = parent.transform;
            loc.transform.position = new Vector3(gridPositions[x].x * cubeManager.CubeSize,
            yOffset,
            gridPositions[x].y * cubeManager.CubeSize);

            SpawnLocs.Add(loc.transform);
            //Debug.Log($"new Aerial SpawnLoc {x} at {loc.transform.position}");
        }
    }

    void OnDisable()
    {
        GameManager.OnGameBegin -= EnableCubeSpawner;
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

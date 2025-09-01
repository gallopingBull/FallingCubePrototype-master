using ProBuilder2.Common;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float spawnDelay = 0.01f;

    // this is best so far between .125f and .2f. Closes to furthest.
    public float tolerance = 0.0001f;

    public bool init;
    public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();
    public List<SpawnData> SpawnDatas { get => spawnDatas; set => spawnDatas = value; }
    [SerializeField] Transform cubesParent;

    [Header("Arena Generation Properties")]
    public int minHeight = 1;
    public int maxHeight = 5;

    // cube size determined by the scale of the cube prefab and the spacing between cubes.
    public float CubeSize = 2f;
    public float floatProbability = 0.2f; // Probability of a cube floating
    private int attempts = 0;
    private const int maxAttempts = 3;
    // TODO: the mimunum distance should be passed throuigh the method call. 
    [SerializeField] float minimumDistance = 4f;

    public bool arenaGenerated = false; 

    [HideInInspector]
    public List<ColorOption> colorsUsed = new List<ColorOption>(); // Track colors used in the current arena generation

    static public Action OnFloorComplete { get; set; }

    static private List<Vector3> directions = new List<Vector3>
    {
        Vector3.forward, Vector3.back,
        Vector3.right,  Vector3.left,
        Vector3.up, Vector3.down
    }; 

    private void Start()
    {
        Init();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateArena(gridSizeX, gridSizeZ);
        }
    }

    private void Init()
    {
        init = false;
       
        if (!cubesParent)
        {
            cubesParent = new GameObject("Cubes").transform;
            cubesParent.transform.position = Vector3.zero;
        }
        //OnFloorComplete += DisplayAllSpawnDatas;

        if (GameManager.gm) 
        {
            if (!GameManager.gm.isDebug)
                GenerateArena(gridSizeX, gridSizeZ);
            else
            {
                // this is used when debugging randomly generated arena
                if (cubesParent.childCount == 0)
                    GenerateArena(gridSizeX, gridSizeZ);
                else
                    CheckForCubesAnyway();  // this is used when debugging manaully created arena
            }
        }

        init = true;
        Debug.Log("CubeManager Initialized");
    }

    private void CheckForCubesAnyway()
    {
        Debug.Log("Stepping into CheckForCubesAnyway()");

        var currentCubes = GameObject.FindGameObjectsWithTag("Block").GetComponents<CubeBehavior>();
        Debug.Log($"currentCubes.Count = {currentCubes.Count()}");

        if (currentCubes != null && currentCubes.Length > 0)
        {
            for(int i = 0; i < currentCubes.Length; i++)
            {
                // I might not this. When this method is called, it's called
                // in scenes with custom arenas.
                //currentCubes[i].transform.parent = cubesParent;

                SpawnData spawnData = new SpawnData { id = i, position = currentCubes[i].transform.position, color = currentCubes[i].color };
                spawnDatas.Add(spawnData);
                cubes.Add(currentCubes[i].gameObject);
            }
        }

        Debug.Log("Stepping out of CheckForCubesAnyway()");
    }

    public void GenerateArena(int gridSizex = 6, int gridSizeZ = 6)
    {
        DestoryAllCubes();
        arenaGenerated = false; 
        for (int x = 0; x < gridSizex; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                int randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                // check if random height value is an even number, otherwise redo assignment
                while (randomHeight % 2 != 0)
                {
                    //Debug.Log($"randomHieght is odd - {randomHeight}");
                    randomHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
                }

                int id = SpawnDatas.Count;
                Vector3 cubePosition = new Vector3(x * CubeSize, randomHeight, z * CubeSize);
                ColorOption color = (ColorOption)UnityEngine.Random.Range(0, 4);

                if (color != ColorOption.Neutral)
                {
                    while ((CheckIfColorIsNearByDistance(id, cubePosition, color, minimumDistance) || colorsUsed.Contains(color)) && attempts < maxAttempts)
                    {
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

                colorsUsed.Clear();

                // Check if the cube should float
                if (UnityEngine.Random.value < floatProbability)
                {
                    float floatingHeight = maxHeight + UnityEngine.Random.Range(1f, 5f); // Round to the nearest integer
                    cubePosition.y = floatingHeight;
                }

                //Debug.Log($"Adding new SpawnData:\n\tid: {id}" +
                //    $"\n\tcubePosition: {cubePosition}" +
                //    $"\n\tcolor: {color}");

                SpawnData spawnData = new SpawnData { id = id, position = cubePosition, color = color };
                SpawnCube(spawnData);

                colorsUsed.Add(color);

                // traverses down the y axis and adds a cube at each position until it reaches the ground or y = 0
                if (cubePosition.y > 0)
                {
                    for (int i = (int)cubePosition.y - (int)CubeSize; i < cubePosition.y; i = i - (int)CubeSize)
                    {
                        id = SpawnDatas.Count;
                        Vector3 groundPos = new Vector3(cubePosition.x, i, cubePosition.z);

                        //Debug.Log($"Adding new SpawnData:\n\tid: {id}" +
                        //                  $"\n\tcubePosition: {cubePosition}" +
                        //                  $"\n\tcolor: {color}");

                        SpawnData groundSpawnData = new SpawnData
                        {
                            id = id,
                            position = groundPos,
                            color = ColorOption.Neutral
                        };

                        SpawnCube(groundSpawnData);

                        if (i == 0)
                            break;
                    }
                }
            }
        }
        arenaGenerated = true;
        OnFloorComplete?.Invoke();
    }

    public void SpawnCube(SpawnData data)
    {
        spawnDatas.Add(data);
        var cube = Instantiate(cubePrefab, data.position, Quaternion.identity, cubesParent);
        cube.GetComponent<CubeBehavior>().InitializeCube(data.id, data.color); // this should allow some color colored cubes at some point
        cubes.Add(cube);
    }

    // This adds a delay between spawning cubes, which is nice for debugging and looks cool.
    private IEnumerator SpawnCubesWithDelay()
    {
        foreach (SpawnData data in spawnDatas)
        {
            yield return new WaitForSeconds(spawnDelay);
            var cube = Instantiate(cubePrefab, data.position, Quaternion.identity, cubesParent);
            cube.GetComponent<CubeBehavior>().InitializeCube(data.id, data.color); // this should allow some color colored cubes at some point
            cubes.Add(cube);
        }

        // organize cubes list by id
        cubes.Sort((x, y) => x.GetComponent<CubeBehavior>().id.CompareTo(y.GetComponent<CubeBehavior>().id));
        Debug.Log($"{GetTotalCubeCount()} cubes spawned");
    }

    public void FinalizeCubePosition(GameObject cube) 
    {
        if (!cube)
        {
            Debug.LogWarning("cube is null!");
            return;
        }

        StartCoroutine(AdjustCubePosition(cube, () => {
            // ADD CHECK FOR STACKED CUBES
            CheckAdjacentCubesForMatchingColor(cube);
        }));
    }

    float cubeScale = 2;
    private IEnumerator AdjustCubePosition(GameObject cube, Action onComplete = null)
    {
        //Debug.Log($"Stepping into CubeManager.AdjustCubePosition({cube})");
        if (cubes.Count > 0 && !cube)
        {
            Debug.LogWarning($"Cube ({cube.GetComponent<CubeBehavior>().id}) is not registered or is cube is null!");
            yield break;
        }

        Vector3 currentPos = cube.transform.position;

        // Calculate the nearest snapped grid point
        float snappedX = Mathf.Round(currentPos.x / cubeScale) * cubeScale;
        float snappedZ = Mathf.Round(currentPos.z / cubeScale) * cubeScale;

        Vector3 snappedPosition = new Vector3(snappedX, Mathf.Round(currentPos.y), snappedZ);
        #region migrated rb code
        // TODO: Determine if this RigidbodyConstraints code
        // from CubeBehavior is needed or does anything...
        // **edit: look at invector pushpoint classes.
        // there seems to be some rb updates that may make
        // this code redundant.

        //RigidbodyConstraints tmpConst;
        //tmpConst = rb.constraints;
        //rb.constraints = RigidbodyConstraints.FreezePosition;
        #endregion

        // Only snap if cube is *close enough* to the snapped position
        if (Vector3.Distance(currentPos, snappedPosition) < tolerance)
        {
            // Smoothly move into place
            Debug.Log("Close enough to begin lerping.");
            yield return StartCoroutine(LerpCubePosition(cube, snappedPosition, 0.1f));
        }
        else
        {
            // Don’t snap yet, just keep current position
            Debug.Log("Cube not close enough to snap yet.");
        }

        onComplete?.Invoke();
        #region migrated rb code
        //rb.constraints = tmpConst;// TODO: this rb statement too...
        #endregion

    }

    private void CheckStackedCubes(GameObject cube)
    {

    }

    private IEnumerator LerpCubePosition(GameObject cube, Vector3 targetPosition, float duration = 0.2f)
    {
        Vector3 startPos = cube.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cube.transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cube.transform.position = targetPosition; // Final snap
    }

    private void AdjustAllCubePositions()
    {
        foreach (var cube in cubes)
        {
            for (int i = 0; i < 2; i++)
            {
                // Check if cube's position values are whole
                if ((cube.transform.position[i] % 1) > float.Epsilon)
                {
                    Debug.Log($"cube.pos.({cube.GetComponent<CubeBehavior>().id}) transform position values already whole!");
                    continue;
                }

                Mathf.Round(cube.transform.position[i]);
            }
        }
    }

    // this method should only be used after a cube has moved or fell.
    private void CheckAdjacentCubesForMatchingColor(GameObject cube)
    {
        var cb = cube.GetComponent<CubeBehavior>();
        //Debug.Log($"Stepping into CheckAdjacentCubesForColor({cb.id})");

        if (cb.color == ColorOption.Neutral)
            return;

        // 2 is cube scale on cube transform.scale
        var adjCubePositions = directions.Select(dir => cb.transform.position + (dir*2)).ToList();


        var matchingColors = cubes.Where(target =>
            target && 
            adjCubePositions.Contains(target.transform.position) &&
            target.GetComponent<CubeBehavior>().color == cb.color && 
            !cb.isDestroying).ToList();

            //Debug.Log($"matchingColors {matchingColors.Count}");

        if (matchingColors.Count > 0)
        {
            Debug.Log($"is {cube.name} in matchingColor: {matchingColors.Contains(cube)}");

            if (!matchingColors.Contains(cube))
            {
                Debug.Log($"now adding {cube.name} to matchingColor");
                matchingColors.Add(cube);
                Debug.Log($"new matchingColors {matchingColors.Count}");
            }

            foreach (var c in matchingColors)
            {
                Debug.Log($"{c.GetComponent<CubeBehavior>().id} in matchingColors ");

                if (!GameManager.gm)
                    return;
                GameManager.gm.AddCubeTarget(c);
            }
        }
       
        //Debug.Log($"Stepping out of CheckAdjacentCubesForColor({cb.id})");
    }

    // this mostly used to prevent spawning similar colors next to each other in arenas.
    public bool CheckIfColorIsNearByDistance(int id, Vector3 position, ColorOption color, float minDistance)
    {
        if (SpawnDatas.Count == 0)
            return false;
        //Debug.Log($"checking if cube({id}) has a similar color({color}) near its position: /n/t{position} ");

        foreach (var cube in cubes)
        {
            if (cube == null)
                continue;

            float distance = Vector3.Distance(position, cube.transform.position);

            if (distance < minDistance && cube.GetComponent<CubeBehavior>().color == color)
                return true;

        }

        return false;
    }   

    void DisplayAllSpawnDatas()
    {
        foreach (SpawnData data in spawnDatas)
        {
            Debug.Log($"id: {data.id}, position: {data.position}, color: {data.color}");
        }
    }

    private int GetTotalCubeCount()
    {
        return cubes.Count;
    }

    // this was moved over from GetAdjacentCubes.cs
    public void DestoryAdjacentCubes(CubeBehavior targetCube, GameObject adjCube, List<CubeBehavior> targetCubes)
    {
        // if other block has been destoryed so null, exit
        if (!adjCube)
            return;

        targetCube.isDestroying = true;
        if (adjCube.tag == "Block")
        {
            if (adjCube.GetComponentInParent<CubeBehavior>().state == CubeBehavior.States.grounded)
            {
                //Debug.Log("\tDestroying " + tmp.name + " from " + transform.parent.parent.gameObject.name);

                // check if this cube and the other cuber (tmp) are in game manager's target list
                // if not add them in
                if (!GameManager.gm)
                    return;
                GameManager.gm.AddCubeTarget(adjCube);
                GameManager.gm.AddCubeTarget(transform.parent.parent.gameObject);
            }
        }
    }

    public void DestoryAllCubes()
    {
        if (cubes.Count == 0)
        {
            Debug.Log("No cubes to destroy");
            return;
        }
        Debug.Log("Destroying cubes!");
        foreach (GameObject cube in cubes)
            Destroy(cube);

        cubes.Clear();
        spawnDatas.Clear();
        //cubes = new List<GameObject>();
        Debug.Log("post destory - cubes.Count: " + cubes.Count);
    }

    public void ResetCubeParent(GameObject cube)
    {
        cube.transform.parent = cubesParent;
    }

    private void OnDestroy()
    {
        //OnFloorComplete -= DisplayAllSpawnDatas;
    }
}

// Enum to represent different colors
public enum ColorOption
{
    Neutral,
    Red,
    Green,
    Blue
}

public struct SpawnData
{
    public int id;
    public Vector3 position;
    public ColorOption color;
}
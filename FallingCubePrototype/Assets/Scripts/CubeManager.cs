using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public float spawnDelay = 0.01f;
    public GameObject cubePrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;

    [SerializeField] List<GameObject> cubes = new List<GameObject>();
    [SerializeField] List<SpawnData> spawnDatas = new List<SpawnData>();
    private Transform cubesParent;

    public List<GameObject> Cubes { get => cubes; set => cubes = value; }

    public List<SpawnData> SpawnDatas { get => spawnDatas; set => spawnDatas = value; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (cubesParent == null)
        {
            cubesParent = new GameObject("Cubes").transform;
            cubesParent.transform.position = Vector3.zero;
        }
        gridSizeX = GetComponent<ArenaGenerator>().gridSizeX;
        gridSizeZ = GetComponent<ArenaGenerator>().gridSizeZ;

        Debug.Log("CubeManager initialized");
        StartCoroutine(DestoryAllCubes());

        GetComponent<ArenaGenerator>().GenerateArena();
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

    public void CallSpawnCubesWithDelay() { StartCoroutine(SpawnCubesWithDelay()); }

    private int GetTotalCubeCount()
    {
        return Cubes.Count;
    }

    public IEnumerator DestoryAllCubes()
    {
        if (cubes.Count == 0)
        {
            Debug.Log("No cubes to destroy");

            cubes.Clear();
            spawnDatas.Clear();
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Destroying cubes!");
            foreach (GameObject cube in cubes)
                Destroy(cube);

            cubes.Clear();
            spawnDatas.Clear();
            //cubes = new List<GameObject>();
            Debug.Log("post destory - cubes.Count: " + cubes.Count);

            //SpawnDatas.Clear();
            spawnDatas = new List<SpawnData>();
        }
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
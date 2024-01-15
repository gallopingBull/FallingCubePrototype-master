using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public float spawnDelay = 0.01f;
    public GameObject cubePrefab;

    [SerializeField] static List<GameObject> cubes;
    [SerializeField] static List<SpawnData> spawnDatas;
    private Transform cubesParent;

    public static List<GameObject> Cubes { get => cubes; set => cubes = value; }

    static List<SpawnData> SpawnDatas { get => spawnDatas; set => spawnDatas = value; }
    private void Init()
    {
        if (cubesParent == null)
        {
            cubesParent = new GameObject("Cubes").transform;
            cubesParent.transform.position = Vector3.zero;
        }
    }

    public void CallSpawnCubes() { StartCoroutine(SpawnCubes()); }
    private IEnumerator SpawnCubes()
    {
        // organize cubes list by id
        cubes.Sort((x, y) => x.GetComponent<CubeBehavior>().id.CompareTo(y.GetComponent<CubeBehavior>().id));

        foreach (SpawnData data in spawnDatas)
        {
            yield return new WaitForSeconds(spawnDelay);
            var cube = Instantiate(cubePrefab, data.position, Quaternion.identity, cubesParent);
            cube.GetComponent<CubeBehavior>().InitializeCube(data.id, data.color); // this should allow some color colored cubes at some point
            cubes.Add(cube);
        }
        Debug.Log($"{GetTotalCubeCount()} cubes spawned");
    }

    private int GetTotalCubeCount()
    {
        return Cubes.Count;
    }

    private void DestoryAllCubes()
    {
        if (Cubes.Count == 0)
        {
            Debug.Log("No cubes to destroy");
            return;
        }

        Debug.Log("Destroying cubes!");
        foreach (GameObject cube in Cubes)
            Destroy(cube);

        Cubes.Clear();
        SpawnDatas.Clear();
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

struct SpawnData
{
    public int id;
    public Vector3 position;
    public ColorOption color;
}
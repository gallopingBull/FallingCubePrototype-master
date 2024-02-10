using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    // Define the offsets for adjacent cubes in a 3D grid
    private Vector3[] offsets = new Vector3[]
    {
    // Cardinal directions along the axes
    Vector3.forward, Vector3.back, Vector3.up,
    Vector3.down, Vector3.right, Vector3.left,

    // Diagonal directions
    new Vector3(1, 1, 1),    // Upper-front-right diagonal
    new Vector3(1, 1, -1),   // Upper-front-left diagonal
    new Vector3(1, -1, 1),   // Upper-back-right diagonal
    new Vector3(1, -1, -1),  // Upper-back-left diagonal
    new Vector3(-1, 1, 1),   // Lower-front-right diagonal
    new Vector3(-1, 1, -1),  // Lower-front-left diagonal
    new Vector3(-1, -1, 1),  // Lower-back-right diagonal
    new Vector3(-1, -1, -1), // Lower-back-left diagonal

    // Additional offsets
    new Vector3(0, 1, 1),    // Upper-middle-front
    new Vector3(0, 1, -1),   // Upper-middle-back
    new Vector3(0, -1, 1),   // Lower-middle-front
    new Vector3(0, -1, -1),  // Lower-middle-back
    new Vector3(1, 1, 0),    // Middle-front-right
    new Vector3(1, -1, 0),   // Middle-back-right
    new Vector3(-1, 1, 0),   // Middle-front-left
    new Vector3(-1, -1, 0),  // Middle-back-left
    new Vector3(1, 0, 1),    // Middle-upper-right
    new Vector3(1, 0, -1),   // Middle-upper-left
    new Vector3(-1, 0, 1),   // Middle-lower-right
    new Vector3(-1, 0, -1),  // Middle-lower-left
    new Vector3(0, 1, 0),    // Upper-middle
    new Vector3(0, -1, 0)    // Lower-middle
    };

    // Start is called before the first frame update
    void Start()
    {
        foreach (var offset in offsets)
        {
            Vector3 adjacentPos = transform.position + (offset);
            Debug.Log($"offset: {offset}");
            var temp = Instantiate(new GameObject(), adjacentPos, Quaternion.identity);
            temp.AddComponent<MeshRenderer>();
            temp.GetComponent<MeshRenderer>().material.color = Color.red;


        }
    }
}

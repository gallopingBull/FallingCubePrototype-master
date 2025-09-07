using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTransfom : MonoBehaviour
{
    Vector3 lastPos;
    // Update is called once per frame
    void Update()
    {
        if (lastPos != null && gameObject.transform.position != lastPos)
        {
            Debug.Log($"{gameObject.name}.pos: {gameObject.transform.position}");
            lastPos = gameObject.transform.position;    
        }
    }
}

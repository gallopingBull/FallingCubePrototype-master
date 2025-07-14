using System.Collections.Generic;
using UnityEngine;

public class GetAdjacentCubes : MonoBehaviour
{
    private GameObject parent;
    public bool canDetect;

    private CubeBehavior cubeBehavior;
    private List<GameObject> tmpTargets;
    public GameObject tmp;

    private void Awake()
    {
        cubeBehavior = GetComponentInParent<CubeBehavior>();
        parent = transform.parent.parent.gameObject;
    }



    // check if this cubes position are zero'd out
    // so it can be flagged as detectable
    private bool CheckPosition()
    {
        if (transform.parent.parent.gameObject.transform.position.x % 2 == 0 &&
            tmp.transform.position.x % 2 == 0)
        {
            if (transform.parent.parent.gameObject.transform.position.z % 2 == 0 &&
                tmp.transform.position.z % 2 == 0)
            {
                if (!canDetect)
                    canDetect = true;
            }
        }
        else
        {
            if (canDetect)
                canDetect = false;
        }

        return canDetect;
    }
}

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

    private void OnTriggerStay(Collider other)
    {
        if (cubeBehavior.color == ColorOption.Neutral)
            return;

        if (other.tag == "Block" &&
            other.GetComponentInParent<CubeBehavior>().color == cubeBehavior.color &&
            !cubeBehavior.isDestroying)
        {
            Debug.Log($"this object ({transform.parent.parent.name}) || other.transform.name: {other.transform.parent.name}");
            tmp = other.transform.parent.gameObject;
            if (cubeBehavior.state == CubeBehavior.States.grounded && CheckPosition())
            {
                DestoryAdjacentCubes();
            }
        }
    }

    private bool CheckPosition()
    {

        if (transform.parent.parent.gameObject.transform.position.x % 2 == 0 &&
       tmp.transform.position.x % 2 == 0)
        {
            if (transform.parent.parent.gameObject.transform.position.z % 2 == 0 &&
            tmp.transform.position.z % 2 == 0)
            {
                if (!canDetect)
                {
                    canDetect = true;
                }
            }
        }
        else
        {
            if (canDetect)
            {
                canDetect = false;
            }
        }

        return canDetect;
    }

    private void DestoryAdjacentCubes()
    {
        Debug.Log("Stepping into DestoryAdjacentCubes()");
        #region testing shit
        /*
        print("*************");
        print(transform.parent.parent.gameObject.name +" pos ="+ CheckPosition());
        print("x = " + transform.parent.parent.gameObject.transform.position.x);
        print("z = " + transform.parent.parent.gameObject.transform.position.z);
        print("*************");
        */
        #endregion

        //if other block hasnt been destoryed yet
        if (tmp == null) 
        {
            Debug.Log("\tDestoryAdjacentCubes.tmp == null");
            return;
        }

        cubeBehavior.isDestroying = true;           
        if (tmp.tag == "Block" /*&& cubeBehavior.isDestroying*/)
        {
            if (tmp.GetComponentInParent<CubeBehavior>().state == CubeBehavior.States.grounded)
            {
                Debug.Log("\tDestroying " + tmp.name + " from " + transform.parent.parent.gameObject.name);

                // check if this cube and the other cuber (tmp) are in game manager's target list
                // if not add them in
                if (!GameManager.gm)
                    return;
                GameManager.gm.AddCubeTarget(tmp);
                GameManager.gm.AddCubeTarget(transform.parent.parent.gameObject);

            }
        }
        Debug.Log("Stepping out of DestoryAdjacentCubes()");
    }
}

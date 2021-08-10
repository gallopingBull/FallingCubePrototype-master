using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAdjacentBlocks : MonoBehaviour
{
    private GameObject parent;
    public bool canDetect;
    public bool isDestorying; //move this to cube base "BlockBehavior"

    private List<GameObject> tmpTargets;
    public GameObject tmp;

    private void Awake()
    {
        parent = transform.parent.parent.gameObject;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Block" &&
            other.GetComponentInParent<BlockBehavior>().curColor ==
                    GetComponentInParent<BlockBehavior>().curColor
            && !GetComponentInParent<BlockBehavior>().isDestroying)
        {
            foreach (Color color in other.GetComponent<BlockBehavior>().IgnoreCubeColors)
            {
                if (other.GetComponentInParent<BlockBehavior>().curColor == color &&
                    GetComponentInParent<BlockBehavior>().curColor == color)
                {
                    return;
                }
            }

            tmp = other.gameObject;
            if (GetComponentInParent<BlockBehavior>().state == BlockBehavior.States.grounded && CheckPosition())
            {
                DestoryAdjacentCubes();
            }
        }
    }

    private void DestoryAdjacentCubes()
    {
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
        if (tmp == null) { return; }

        GetComponentInParent<BlockBehavior>().isDestroying = true;
        if (tmp.tag == "Block" && 
            GetComponentInParent<BlockBehavior>().isDestroying)
        {
            if (tmp.GetComponentInParent<BlockBehavior>().state ==
            BlockBehavior.States.grounded)
            {
                //print("Destroying " + tmp.name + " from " + transform.parent.parent.gameObject.name);

                //check if this cube and the other cuber (tmp) are in game manager's target list
                // if not add them in

                GameManager.gm.AddCubeTarget(tmp);
                GameManager.gm.AddCubeTarget(transform.parent.parent.gameObject);

            }
        } 
    }



    private void EnableTrigger()
    {
        canDetect = true;
    }
}

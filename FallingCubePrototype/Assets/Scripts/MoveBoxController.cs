using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

public class MoveBoxController : MonoBehaviour
{
    #region variables
    [HideInInspector]
    public bool enableMovement;

    private GameObject target;
    private GameObject _camera; 
    private Vector3 _direction;

    public GameObject pushPoint;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.Find("vThirdPersonCamera_LITE");
        pushPoint = GameObject.Find("PushPoint");
    }

    // Update is called once per frame
    void Update()
    {
        if (enableMovement)
        {
            Movecube();

            //clamp x and z position to prevent cube from falling off grid/map
            target.transform.transform.position = new Vector3(Mathf.Clamp(target.transform.position.x, -2, 2), 
                target.transform.position.y,
                Mathf.Clamp(target.transform.position.z, -2f, 2f)); 
            
            pushPoint.transform.transform.position = new Vector3(Mathf.Clamp(pushPoint.transform.position.x, -2f, 2f),
                pushPoint.transform.position.y,
                Mathf.Clamp(pushPoint.transform.position.z, -2f, 2f));
        }  
    }

    // use this only to assign initial push point position
    public void SetPushPointPosition()
    {
        Vector3 _tmpPos;
        _tmpPos = new Vector3(target.transform.position.x, target.transform.position.y + 2, target.transform.position.z);
        pushPoint.transform.position = _tmpPos;
    }


    public void ParentToPushPoint()
    {
        transform.SetParent(pushPoint.transform, true);
        target.transform.SetParent (pushPoint.transform, true);
    }
    public void DeParentToPushPoint()
    {
        transform.parent = null;
        target.transform.parent = null;
    }

    private void Movecube()
    {
        //multiply input value by .60f to make stick less sensitive
        //when moving cubes, otherwise cube movement glitches out
        var h = GetComponent<vThirdPersonInput>().cc.input.x * 1f/*.60f*/; 
        var z = GetComponent<vThirdPersonInput>().cc.input.z * 1f/*.60f*/;

        Vector3 camF = _camera.transform.forward;
        Vector3 camR = _camera.transform.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;
  
        GetDirection(camF, camR, z, h);
        ///
        // 2-5 for final float (strength factor)
        pushPoint.GetComponent<Rigidbody>().MovePosition(pushPoint.transform.position + _direction * Time.deltaTime * 6f); 
    }


    private void GetDirection(Vector3 _camForward, Vector3 _camRight, float _z, float _h)
    {
        Vector3 tmpDir;
        Vector3 finalDir;

        #region
        if (pushPoint.transform.position.x % 2 == 0)
        {
            //moving on Z
            if (Mathf.Round(pushPoint.transform.position.z) % 2 == 0)
            {
                print("1");
                print("can move up on z axis");
                tmpDir = (_camForward * _z + _camRight * _h);
            }
            else
            {
                print("2");
                tmpDir = (_camForward * 0 + _camRight * _h);
            }
        }

        else if (pushPoint.transform.position.z % 2 == 0)
        {
            //moving on x
            if (Mathf.Round(pushPoint.transform.position.x) % 2 == 0)
            {
                print("can move up on x axis");
                tmpDir = (_camForward * _z + _camRight * _h);
            }
            else
            {
                print("3");
                tmpDir = (_camForward * _z + _camRight * 0);
            }
        }

        else
        {
            print("4");
            return;
        }

        #endregion  

        finalDir = new Vector3(-(Mathf.Round(tmpDir.x)), tmpDir.y, -(Mathf.Round(tmpDir.z)));
        _direction = finalDir;
    }           

    // small delay before player movement is enabled again
    private IEnumerator EnableMovement()
    {
        yield return new WaitForSeconds(1.5f);
        if (!GetComponent<MoveBoxController>().enableMovement)
            GetComponent<MoveBoxController>().enableMovement = true;
        StopCoroutine("EnableMovement");
    }

    public void EnableBoxMovement()
    {
        if (!enableMovement)
            target = GetComponent<GrabMechanic>().targetCube.transform.parent.parent.gameObject;
        else
            target = null;

        enableMovement = !(enableMovement);
    }
}

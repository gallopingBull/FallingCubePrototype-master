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
    public float moveDistance = 1f; // Distance the cube moves with each step
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

    private void Movecube()
    {
        //multiply input value by .60f to make stick less sensitive
        //when moving cubes, otherwise cube movement glitches out
        var h = GetComponent<vThirdPersonInput>().cc.input.x;
        var z = GetComponent<vThirdPersonInput>().cc.input.z;

        // Stop moving cube if camera is rotating
        if (Input.GetAxis("RightAnalogHorizontal") != 0 || Input.GetAxis("RightAnalogVertical") != 0)
            return;

        Vector3 camF = _camera.transform.forward;
        Vector3 camR = _camera.transform.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = camF * z + camR * h;

        // Ensure movement only along the X or Z axis, not diagonally
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z))
        {
            moveDirection.z = 0f;
        }
        else
        {
            moveDirection.x = 0f;
        }

        moveDirection.Normalize();

        // Calculate target position based on the current position and move distance
        Vector3 targetPosition = pushPoint.transform.position + new Vector3(
            Mathf.RoundToInt(moveDirection.x),
            0f,
            Mathf.RoundToInt(moveDirection.z)
        ) * moveDistance * .05f;

        pushPoint.transform.position = targetPosition;
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
        target.transform.SetParent(pushPoint.transform, true);
    }

    public void DeParentToPushPoint()
    {
        transform.parent = null;
        target.transform.parent = null;
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

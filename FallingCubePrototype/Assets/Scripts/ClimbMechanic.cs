using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbMechanic : vFreeClimb
{
    protected override void Update()
    {
        //Debug.Log($"inDrag: {dragInfo.inDrag}");    
        //Debug.Log($"checkConditions: {CheckClimbCondictions()}");
        if (Input.GetAxis("RT") == 1 && CheckClimbCondictions())
        {
            input = new Vector3(TP_Input.horizontalInput.GetAxis(), 0, TP_Input.verticallInput.GetAxis());
            ClimbHandle();
            ClimbJumpHandle();
            ClimbUpHandle();
        }
        else if (Input.GetAxis("RT") == 0 && dragInfo.inDrag) 
        { 
            ExitClimb();
        }
        else
        {
            input = Vector2.zero;
            TP_Input.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0);
            TP_Input.cc.animator.SetFloat(vAnimatorParameters.InputVertical, 0);
        }
    }

    protected override void EnterClimb()
    {
        Debug.Log("EnterClimb");
        base.EnterClimb();
    }
}

using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClimbMechanic : vFreeClimb
{
    public Text text;
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
    private string L_TriggerName = "LT_Linux";
    private string R_TriggerName = "RT_Linux";

#else
    private string L_TriggerName = "LT";
    private string R_TriggerName = "RT";
#endif

    protected override void Update()
    {
        //Debug.Log($"inDrag: {dragInfo.inDrag}");    
        //Debug.Log($"checkConditions: {CheckClimbCondictions()}");

        if (GetComponent<MoveCubeMechanic>().IsMoving)
            return;

        if (text != null)
            text.text = $"{R_TriggerName} = {Input.GetAxis(R_TriggerName)}\n{L_TriggerName} = {Input.GetAxis(L_TriggerName)}";
        
        if (((Input.GetAxis(R_TriggerName) == 1 && Input.GetAxis(L_TriggerName) == 1) || Input.GetMouseButton(1)) && CheckClimbCondictions())
        {
            input = new Vector3(TP_Input.horizontalInput.GetAxis(), 0, TP_Input.verticallInput.GetAxis());
            ClimbHandle();
            ClimbJumpHandle();
            ClimbUpHandle();
        }
        else if (((Input.GetAxis(R_TriggerName) == 0 && Input.GetAxis(L_TriggerName) == 0) || Input.GetMouseButtonUp(1)) && dragInfo.inDrag)
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

using UnityEngine;

public class JoystickDebugger : MonoBehaviour
{
    void Update()
    {
        float val = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(val) > 0.1f)
            Debug.Log("verticalAxis "  + val);

        float val1 = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(val1) > 0.1f)
            Debug.Log("HorizontalAxis " + val1);

        //// Check joystick buttons
        //for (int j = 0; j < 20; j++)
        //{
        //    if (Input.GetKey("joystick button " + j))
        //        Debug.Log("Button " + j + " pressed");
        //}
    }
}
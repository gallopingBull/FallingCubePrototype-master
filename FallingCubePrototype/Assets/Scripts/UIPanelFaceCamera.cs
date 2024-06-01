using UnityEngine;

public class UIPanelFaceCamera : MonoBehaviour
{
    void Start()
    {
        GetComponent<Canvas>().worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (gameObject.activeInHierarchy)
            transform.LookAt(transform.position + Camera.main.transform.rotation * -Vector3.right, 
                Camera.main.transform.rotation * Vector3.up);
    }
}

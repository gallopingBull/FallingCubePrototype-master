using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Invector.vCharacterController;
using UnityEngine;

public class CubeDeathCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (CanDestroyPlayer(other.gameObject))
        {
            other.gameObject.GetComponent<vThirdPersonController>().AddHealth(-1);
            //other.gameObject.GetComponent<vThirdPersonController>().lockMovement = true;
        }
    }

    private bool CanDestroyPlayer(GameObject go)
    {
        return (go.gameObject.tag == "Player" &&
            GetComponentInParent<BlockBehavior>().state ==
            BlockBehavior.States.falling &&
            !GetComponentInParent<BlockBehavior>().isDestroying &&
            !GameManager.gm.gameCompleted);
    }
}

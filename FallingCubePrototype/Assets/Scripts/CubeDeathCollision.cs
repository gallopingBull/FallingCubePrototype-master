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
        return (go.tag == "Player" &&
            GetComponentInParent<CubeBehavior>().state ==
            CubeBehavior.States.falling &&
            !GetComponentInParent<CubeBehavior>().isDestroying &&
            !GameManager.gm.gameCompleted);
    }
}

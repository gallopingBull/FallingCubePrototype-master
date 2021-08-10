using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Invector.vCharacterController;
using UnityEngine;

public class KillZ : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" &&
            !GameManager.gm.gameCompleted) {
            other.gameObject.GetComponent<vThirdPersonController>().AddHealth(-1);
        }
    }
}

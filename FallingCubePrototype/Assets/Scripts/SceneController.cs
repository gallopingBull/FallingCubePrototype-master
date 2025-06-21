using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  playing with this idea...
/// </summary>
public class SceneController : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("No button gameobject found for ReturnToMainMenu!");
            return;
        }

        button.onClick.AddListener(ReturnMainMenu);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Back"))
            ReturnMainMenu();
    }

    public void ReturnMainMenu()
    {
        if (Pause.Instance.isPaused)
        {
            Pause.Instance.ResumeGame();
        }

        GameManager.gm.GetComponent<LoadScene>().LoadSceneByIndex(0); // not liking how this is invoked.
    }
    private void OnDestroy()
    {
        button.onClick?.RemoveAllListeners();
    }
}

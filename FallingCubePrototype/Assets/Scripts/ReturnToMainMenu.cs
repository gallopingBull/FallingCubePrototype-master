using UnityEngine;
using UnityEngine.UI;


// I think this class should be part of a scene controller class to handle loading scenes.
public class ReturnToMainMenu : MonoBehaviour
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

        GameManager.gm.GetComponent<LoadScene>().LoadSceneByIndex(0);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}

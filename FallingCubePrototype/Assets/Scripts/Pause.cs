using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public bool isPaused;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseSelectedBut;
    [SerializeField] GameObject hudMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Start"))
        {
            if (!pauseMenu || !hudMenu) 
            {
                Debug.LogWarning("Pause or HUD Gameobjects are not assigned."); 
                return; 
            }

            if (!isPaused)
                PauseGame();    
            else
                ResumeGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("Pausing game");
        isPaused = true;
        pauseMenu.SetActive(true);
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            hudMenu.SetActive(false);
            if (GameManager.gm.countingDown)
                GameManager.gm.HideCountdownScreen();   
        }
        EventSystem.current.SetSelectedGameObject(pauseSelectedBut);
        Time.timeScale = 0;
    }
        
    public void ResumeGame()
    {
        Debug.Log("Resuming game");
        isPaused = false;
        pauseMenu.SetActive(false);
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            hudMenu.SetActive(true);
            if (GameManager.gm.countingDown)
                GameManager.gm.DisplayCountdownScreen();
        }
 
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1;
    }
}

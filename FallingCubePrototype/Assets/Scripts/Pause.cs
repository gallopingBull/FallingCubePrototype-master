using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public static bool isPaused;
    [SerializeField] static GameObject pauseMenu;
    [SerializeField] static GameObject pauseSelectedBut; // reference to first button that should be selected.

    [SerializeField] static GameObject hudMenu;

    public static Action onPause;
    public static Action onResume;

    // Start is called before the first frame update
    static void Start()
    {
        //onPause += PauseGame;
        //onResume += ResumeGame;
    }

    // Update is called once per frame
    static void Update()
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

    static public void PauseGame()
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
        
    static public void ResumeGame()
    {
        Debug.Log("Resuming game");
        isPaused = false;
        pauseMenu.SetActive(false);
        onResume.Invoke();  
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            hudMenu.SetActive(true);
            if (GameManager.gm.countingDown)
                GameManager.gm.DisplayCountdownScreen();
        }
 
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1;
    }
    //static public void OnDestroy()
    //{
    //    onPause -= PauseGame;
    //    onResume -= ResumeGame;
    //}
}

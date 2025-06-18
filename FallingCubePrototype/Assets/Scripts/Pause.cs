using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    private static Pause _instance;
    public static Pause Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Pause>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("Pause");
                    _instance = singletonObject.AddComponent<Pause>();
                }
            }
            return _instance;
        }
    }

    // State variables
    public bool isPaused;
    private GameObject pauseMenu;
    private GameObject pauseSelectedBut; // reference to first button that should be selected.

    // Events for pausing/resuming
    public event Action onPause;
    public event Action onResume;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        // Ensure UI objects exists
        pauseMenu = GameObject.Find("PauseMenuPanel");
        if (pauseMenu == null) 
        { 
            Debug.LogWarning("PauseMenuPanel not found!");
            return;
        } 

        pauseSelectedBut = GameObject.Find("ButtonReturnToGame");
        if (pauseSelectedBut == null)
        { 
            Debug.LogWarning("ButtonReturnToGame not found!");
            return;
        }

        pauseMenu.SetActive(false); 
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Start"))
        {
            if (!pauseMenu || !GameManager.gm.HUDPanel)
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

    // Pause the game
    public void PauseGame()
    {
        Debug.Log("Pausing game");
        Time.timeScale = 0;
        isPaused = true;
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(pauseSelectedBut);
        onPause?.Invoke(); // Invoke onPause if it's subscribed
    }

    // Resume the game
    public void ResumeGame()
    {
        Debug.Log("Resuming game");
        isPaused = false;
        pauseMenu.SetActive(false);
        onResume?.Invoke(); // Invoke onResume if it's subscribed
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1;
    }

    //public void OnDestroy()
    //{
    //    onPause -= PauseGame;
    //    onResume -= ResumeGame;
    //}
}




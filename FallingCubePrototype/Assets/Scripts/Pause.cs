using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    private GameObject hudMenu;

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
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        pauseMenu = GameObject.Find("Panel_PauseMenu");
        pauseSelectedBut = GameObject.Find("ButtonReturnToGame");

        pauseMenu.SetActive(false); 

        // Debugging if objects are not found
        if (pauseMenu == null) Debug.LogError("Panel_PauseMenu not found!");
        if (pauseSelectedBut == null) Debug.LogError("ButtonReturnToGame not found!");

        hudMenu = GameManager.gm.GameHudPanel;
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Start"))
        {
            Debug.Log("Pause.Update() - Input detected!");

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

    // Pause the game
    public void PauseGame()
    {
        Debug.Log("Pausing game");
        Time.timeScale = 0;
        isPaused = true;
        pauseMenu.SetActive(true);

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            hudMenu.SetActive(false);
            GameManager.gm.HideCountdownScreen();
        }

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

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            hudMenu.SetActive(true);
            if (GameManager.gm.initialCountingDown && GameManager.gm.gameInit)
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




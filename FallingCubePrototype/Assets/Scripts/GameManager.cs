using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region variables
    public static GameManager gm;

    [HideInInspector]
    public GameObject Player;

    public bool isDebug = false;

    [Header("Game Session Variables")]
    public bool gameInit;
    public bool gameCompleted;
    private bool gameWon;
    private bool isDoorOpen;
    [HideInInspector]


    [SerializeField]
    private int MAXScore = 3;
    [HideInInspector]
    public int Score;
    private TMP_Text ScoreText;


    // Variables for initial countdown prior to game start.
    [Header("Initial Countdown Variables")]
    public bool initialCountingDown = false;
    [HideInInspector]
    public float initialCountdownTime;
    private int maxInitialCountdownTime;
    private GameObject InitalTimerPanel;
    private TMP_Text initialTimerText;
    private TMP_Text beginGameText;

    // Variables for main countdown during a game session.
    [Header("Main Countdown Variables")]
    [SerializeField]
    private bool TimeGameMode; // Check if in time game mode. 
    [HideInInspector]
    public bool countingDown = false;
    public int currentCountdownTime = 0; // Time passed during the game session.
    public int totalGameTime = 30; // Total time allowed for the game session.
    private TMP_Text countdownText; // Displays the remaining time in the game.

    //[Header("Cube Management Variables")]
    // cube reference stuff
    private List<GameObject> CubeTargets;

    private GameObject cameraTarget; // this is the camera target for the invector 3rd person controller
    [SerializeField]
    private GameObject ObjectiveGate;
    private CubeManager cubeManager;
    private AerialCubeSpawner? aerialCubeSpawner;
    private Pause pause;

    [HideInInspector]
    public GameObject HUDPanel;
    private GameObject GameWonPanel;
    private GameObject GameFailedPanel;

    static public Action OnGameBegin { get; set; }
    #endregion

    #region functions

    void Awake()
    {
        if (gm != null && gm != this)
        {
            Destroy(gameObject);
        }
        else
        {
            gm = this;
        }
    }

    private void Start()
    {
        Debug.Log("GameManager.Start()");
        gameInit = false;

        // Assume null in main menu scene. Cube manager will not be present.
        cubeManager = FindObjectOfType<CubeManager>();
        if (cubeManager == null) 
        {
            Debug.Log("cameraTarget is null");
            return;
        }

        pause = GetComponent<Pause>();
        if (pause == null)
        {
            Debug.Log("pause is null");
            return;
        }

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            aerialCubeSpawner = cubeManager.GetComponent<AerialCubeSpawner>();
            ArenaGenerator.OnFloorComplete += StartGame;

            InitUI();

            Player = GameObject.FindGameObjectWithTag("Player");
            cameraTarget = GameObject.Find("MainCameraTarget");

            Player.SetActive(false);
            gameInit = true;

            // Skip initial countdown in debug mode.
            if (isDebug)
            {
                if (InitalTimerPanel != null && InitalTimerPanel.activeInHierarchy)
                    InitalTimerPanel.SetActive(false);
                StartGame();
            }
            else
                InitialCountdownTimerCaller();
        }
        else if (SceneManager.GetActiveScene().name == "PlaygroundScene")
        {
            InitUI();
            InitalTimerPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Back"))
            GetComponent<LoadScene>().LoadSceneByIndex(0);

        if (Score >= MAXScore)
        {
            if (TimeGameMode && !gameCompleted && !gameWon)
                GameWon();

            if (!isDoorOpen && !TimeGameMode)
                OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (isDoorOpen)
            {
                if (!gameCompleted && !gameWon)
                    GameWon();
            }
            else
            {
                FailedGame();
            }
        }
    }

    private void OpenDoor()
    {
        Debug.Log("door opened");
        isDoorOpen = true;
        ObjectiveGate.SetActive(false);
    }

    private void GameWon()
    {
        Debug.Log("game won!");
        GameWonPanel.SetActive(true);
        Debug.Log(GameWonPanel.activeSelf);
        HUDPanel.SetActive(false);
        gameWon = true;
        gameCompleted = true;
    }

    public void FailedGame()
    {
        Debug.Log("game failed!");
        GameFailedPanel.SetActive(true);
        HUDPanel.SetActive(false);
        gameCompleted = true;
    }

    public void AddPoints(int qty, int multiplier)
    {
        if (Score < MAXScore && !isDebug)
        {
            Score += (qty * multiplier);
            ScoreText.text = Score.ToString();
        }
    }

    private void InitialCountdownTimerCaller()
    {
        initialCountingDown = true;
        StartCoroutine("InitialCountdownTimer");
    }

    //only used at begining of round
    IEnumerator InitialCountdownTimer()
    {
        maxInitialCountdownTime = 3;

        initialCountdownTime = maxInitialCountdownTime;
        initialTimerText.text = initialCountdownTime.ToString();

        for (int i = maxInitialCountdownTime; i >= 0; i--)
        {
            // Only increment when initial timer panel object is active.
            if (!InitalTimerPanel)
                continue;

            yield return new WaitForSeconds(1f);

            if (initialCountdownTime > 0)
            {
                initialCountdownTime--;
                initialTimerText.text = initialCountdownTime.ToString();
            }

            if (initialCountdownTime == 0)
            {
                initialTimerText.transform.gameObject.SetActive(false);
                beginGameText.transform.gameObject.SetActive(true);

                yield return new WaitForSeconds(.75f);
                InitalTimerPanel.SetActive(false);
                HUDPanel.SetActive(true);
                initialCountingDown = false;
                yield return new WaitForSeconds(.1f);
                StartGame();
                break;
            }
        }
        initialCountingDown = false; // Keeping this in case initialCountdownTime never reaches zero
        StopCoroutine("InitialCountdownTimer");
    }

    public void StartGame()
    {
        if (!Player.activeInHierarchy)
            SpawnPlayer();

        if (!isDebug)
        {
            if (countingDown)
                StopCoroutine("Timer");

            StartCoroutine("Timer");
        }
        Debug.Log("Game has begun!");
        OnGameBegin?.Invoke();
    }

    private void InitUI()
    {
        Pause.Instance.onPause += HideCountdownScreen;
        Pause.Instance.onPause += HideHUD;

        Pause.Instance.onResume += DisplayCountdownScreen;
        Pause.Instance.onResume += DisplayHUD;

        InitalTimerPanel = GameObject.Find("Panel_StartGameCountDown");
        initialTimerText = GameObject.Find("InitialCountdownTextValue").GetComponent<TMP_Text>();
        beginGameText = GameObject.Find("BeginText").GetComponent<TMP_Text>();
        beginGameText.gameObject.SetActive(false);
        countdownText = GameObject.Find("TimerValueText").GetComponent<TMP_Text>();
        ScoreText = GameObject.Find("ScoreValueText").GetComponent<TMP_Text>();


        GameWonPanel = GameObject.Find("GameWonPanel");
        GameWonPanel.SetActive(false);
        GameFailedPanel = GameObject.Find("GameFailedPanel_TimerEnded");
        GameFailedPanel.SetActive(false);
        HUDPanel = GameObject.Find("HUDPanel");
        HUDPanel.SetActive(false);
    }

    IEnumerator Timer()
    {
        countingDown = true;
        currentCountdownTime = totalGameTime;
        countdownText.text = currentCountdownTime.ToString();

        for (int i = totalGameTime; i >= 0; i--)
        {
            yield return new WaitForSeconds(1f);
            currentCountdownTime--;
            countdownText.text = currentCountdownTime.ToString();
            if (currentCountdownTime == 0 && !gameCompleted)
            {
                if (Score >= MAXScore)
                    GameWon();
                else
                    FailedGame();

                countingDown = false;
                break;
            }
        }

        countingDown = false;
        StopCoroutine("Timer");
    }

    public void AddCubeTarget(GameObject target)
    {
        // prevent cube meshes to be added as a cube target
        if (!CubeTargets.Contains(target) && target.name != "CubeMesh")
        {
            target.GetComponent<CubeBehavior>().PlaySFX(target.GetComponent<CubeBehavior>().contactSFX);
            CubeTargets.Add(target);
        }

        Invoke("DestoryCubeTargets", .15f);
        //DestoryCubeTargets();
    }

    private void DestoryCubeTargets()
    {
        if (CubeTargets != null)
        {
            int _multiplier;
            if (CubeTargets.Count < 1)
                _multiplier = 1;
            else
                _multiplier = CubeTargets.Count;

            foreach (GameObject target in CubeTargets)
            {
                if (target != null)
                {
                    AddPoints(target.GetComponent<CubeBehavior>().ScoreValue, _multiplier);
                    aerialCubeSpawner.Spawn();
                    target.GetComponent<CubeBehavior>().DestroyCube();
                }
            }
        }
        CubeTargets.Clear();
    }

    private void SpawnPlayer()
    {
        Debug.Log("Spawning player");
        // Scale by cube size and grid size first (then subtract by one to account for zero) to get the center of the grid
        float x, z;
        x = ((cubeManager.gridSizeX * cubeManager.CubeSize) / 2) - 1;
        z = ((cubeManager.gridSizeZ * cubeManager.CubeSize) / 2) - 1;
        //Debug.Log($"player spawn point - x: {x}, z: {z}");
        Player.transform.position = new Vector3(x, 6, z);
        Player.SetActive(true);
        cameraTarget.transform.position = new Vector3(x, 0, z);
    }

    private void DisplayHUD()
    {
        Debug.Log("DisplayHUD");
        if (SceneManager.GetActiveScene().name == "MainScene")
            HUDPanel.SetActive(true);
    }

    private void HideHUD()
    {
        Debug.Log("HideHUD");
        if (SceneManager.GetActiveScene().name == "MainScene")
            HUDPanel.SetActive(false);
    }

    public void DisplayCountdownScreen() 
    {
        Debug.Log("DisplayCountdownScreen");
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (initialCountingDown && gameInit)
                InitalTimerPanel.SetActive(true);
        }
    }

    public void HideCountdownScreen()
    {
        Debug.Log("HideCountdownScreen");
        if (SceneManager.GetActiveScene().name == "MainScene")
            InitalTimerPanel.SetActive(false);
    }

    public void OnDestroy()
    {
        Pause.Instance.onPause -= HideCountdownScreen;
        Pause.Instance.onPause -= HideHUD;

        Pause.Instance.onResume -= DisplayCountdownScreen;
        Pause.Instance.onResume -= DisplayHUD;

        ArenaGenerator.OnFloorComplete -= StartGame;
    }
    #endregion
}

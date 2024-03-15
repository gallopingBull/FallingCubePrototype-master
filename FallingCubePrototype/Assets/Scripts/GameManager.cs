using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region variables
    public static GameManager gm;

    [HideInInspector]
    public GameObject Player;
    //public GameObject PlayerPrefab;

    public bool isTesting = false;

    private bool isDoorOpen;
    [HideInInspector]
    public bool gameCompleted;
    private bool gameWon;

    [SerializeField]
    private int MAXScore = 3;
    [HideInInspector]
    public int Score;
    public TMP_Text ScoreText;

    [SerializeField]
    private bool TimeGameMode;
    [HideInInspector]
    public int CountdownTime = 0;

    public int MAXCountdownTime = 30;
    public TMP_Text text;

    //initial timer variables.
    [HideInInspector]
    public float _time;
    private int _MAXCountdownTime;
    static private bool countingDown = false;
    public TMP_Text Text_InitialTimer;
    public TMP_Text Text_Begin;

    public List<GameObject> CubeTargets;

    private GameObject cameraTargt; // this is the camera target for the invector 3rd person controller
    [SerializeField]
    private GameObject ObjectiveGate;
    private CubeManager cubeManager;
    private AerialCubeSpawner? aerialCubeSpawner;

    [SerializeField]
    private GameObject InitalTimerPanel;
    [SerializeField]
    private GameObject GameHudPanel;
    [SerializeField]
    private GameObject GameWonPanel;
    [SerializeField]
    private GameObject GameFailedPanel;

    static public Action OnGameBegin { get; set; }
    #endregion

    #region functions

    void Awake()
    {
        gm = this;
    }

    private void Start()
    {
        cubeManager = FindObjectOfType<CubeManager>();
        aerialCubeSpawner = cubeManager.GetComponent<AerialCubeSpawner>();
        ArenaGenerator.OnFloorComplete += StartGame;

        Player = GameObject.FindGameObjectWithTag("Player");
        cameraTargt = GameObject.Find("MainCameraTarget");
        Player.SetActive(false);
        //SpawnPlayer();

        if (!isTesting)
            Invoke("InitialCountdownTimerCaller", 1f);
        else if (InitalTimerPanel != null && InitalTimerPanel.activeInHierarchy)
        {
            InitalTimerPanel.SetActive(false);
            StartGame();
        }
    }

    private void OnDestroy()
    {
        ArenaGenerator.OnFloorComplete -= StartGame;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Back"))
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
        print("door opened");
        isDoorOpen = true;
        ObjectiveGate.SetActive(false);
    }

    private void GameWon()
    {
        print("game won!");
        GameWonPanel.SetActive(true);
        print(GameWonPanel.activeSelf);
        GameHudPanel.SetActive(false);
        gameWon = true;
        gameCompleted = true;
    }

    public void FailedGame()
    {
        print("gameFailed");
        GameFailedPanel.SetActive(true);
        GameHudPanel.SetActive(false);
        gameCompleted = true;
    }

    public void AddPoints(int qty, int multiplier)
    {
        if (Score < MAXScore && !isTesting)
        {
            Score += (qty * multiplier);
            ScoreText.text = Score.ToString();
        }
    }

    private void InitialCountdownTimerCaller()
    {
        StartCoroutine("InitialCountdownTimer");
    }

    //only used at begining of round
    IEnumerator InitialCountdownTimer()
    {
        _MAXCountdownTime = 3;

        _time = _MAXCountdownTime;
        Text_InitialTimer.text = _time.ToString();

        for (int i = _MAXCountdownTime; i >= 0; i--)
        {
            yield return new WaitForSeconds(1f);
            if (_time > 0)
            {
                _time--;
                Text_InitialTimer.text = _time.ToString();
            }

            if (_time == 0)
            {
                //StartGame();
                Text_InitialTimer.transform.gameObject.SetActive(false);
                Text_Begin.transform.gameObject.SetActive(true);
                yield return new WaitForSeconds(.75f);
                InitalTimerPanel.gameObject.SetActive(false);
                GameHudPanel.SetActive(true);
                yield return new WaitForSeconds(1.5f);

                StartGame();
                break;
            }
        }

        StopCoroutine("InitialCountdownTimer");
    }

    public void StartGame()
    {
        if (!Player.activeInHierarchy)
            SpawnPlayer();

        if (!isTesting)
        {
            if (countingDown)
                StopCoroutine("Timer");

            StartCoroutine("Timer");
        }
        Debug.Log("Game has begun!");
        OnGameBegin?.Invoke();
    }

    IEnumerator Timer()
    {
        countingDown = true;
        CountdownTime = MAXCountdownTime;
        text.text = CountdownTime.ToString();

        for (int i = MAXCountdownTime; i >= 0; i--)
        {
            yield return new WaitForSeconds(1f);
            CountdownTime--;
            text.text = CountdownTime.ToString();
            if (CountdownTime == 0 && !gameCompleted)
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
        // hacky fix to prevent cube meshes to be added to cube target
        if (!CubeTargets.Contains(target) && target.name != "CubeMesh")
        {
            Debug.Log($"target.GetComponent<CubeBehavior>(): {target.GetComponent<CubeBehavior>()}");
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
        Debug.Log($"player spawn point - x: {x}, z: {z}");
        Player.transform.position = new Vector3(x, 6, z);
        Player.SetActive(true);
        cameraTargt.transform.position = new Vector3(x, 0, z);
    }
    #endregion
}

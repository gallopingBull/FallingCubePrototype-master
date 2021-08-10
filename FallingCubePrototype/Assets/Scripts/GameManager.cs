﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region variables
    public static GameManager gm;
    public GameObject Player;

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
    public TMP_Text Text_InitialTimer;
    public TMP_Text Text_Begin;

    public List<GameObject> CubeTargets;

    [SerializeField]
    private GameObject ObjectiveGate;

    [SerializeField]
    private BlockSpawner bs;

    [SerializeField]
    private GameObject InitalTimerPanel;
    [SerializeField]
    private GameObject GameHudPanel;
    [SerializeField]
    private GameObject GameWonPanel;
    [SerializeField]
    private GameObject GameFailedPanel;
    #endregion


    #region functions
    // Start is called before the first frame update
    void Awake()
    {
        gm = this;
        //Player = GameObject.Find("ThirdPersonController_LITE");
    }
    private void Start()
    {
        //StartCoroutine("Timer");
        if (!isTesting)
        {
           
            Invoke("InitialCountdownTimerCaller", 1f);
        }
        else
        {
            if (InitalTimerPanel != null && 
                InitalTimerPanel.activeInHierarchy)
            {
                InitalTimerPanel.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Back"))
        {
            GetComponent<LoadScene>().LoadSceneByIndex(0);
        }
            

        if (Score >= MAXScore)
        {
            if (TimeGameMode && !gameCompleted && !gameWon)
            {
                GameWon();
            }
            if (!isDoorOpen && !TimeGameMode)
            {
                OpenDoor();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (isDoorOpen)
            {
                if (!gameCompleted && !gameWon)
                {
                    GameWon();
                }
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
        GameWonPanel.SetActive(true);
        print(GameWonPanel.activeSelf);
        GameHudPanel.SetActive(false);
        gameWon = true;
        gameCompleted = true;
        print("beath game");
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
        if (!bs.spawnPlayer)
        {
            bs.SpawnPlayerCaller();
        }

        if (!Player.activeInHierarchy && bs.spawnPlayer)
        {
            //StartGame();
            //return;
        }
        else
        {
            StartCoroutine("Timer");

            //small delay after round begins before new cubers are allowed to 
            //add a random.range on the time so it's never exatcly the same
            Invoke("EnableCubeSpawnerCaller", 3f);
        }

    }

    private void EnableCubeSpawnerCaller()
    {
        bs.EnableCubeSpawner();
    }

    IEnumerator Timer()
    {
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
                {
                    GameWon();
                }
                else
                {
                    FailedGame();
                }
                break;
            }

            //print("time = " + CountdownTime);
        }
        StopCoroutine("Timer");
    }

    public void AddCubeTarget(GameObject target)
    {
        if (!CubeTargets.Contains(target))
        {
            target.GetComponent<BlockBehavior>().PlaySFX(target.GetComponent<BlockBehavior>().contactSFX);
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
            {
                _multiplier = 1;
            }
            else
            {
                _multiplier = CubeTargets.Count;
            }

            foreach (GameObject target in CubeTargets)
            {
                if (target != null)
                {

                    AddPoints(target.GetComponent<BlockBehavior>().ScoreValue, _multiplier);
                    bs.Spawn();
                    target.GetComponent<BlockBehavior>().DestroyCube();
                }
            }
        }
        CubeTargets.Clear();
    }

    #endregion
}

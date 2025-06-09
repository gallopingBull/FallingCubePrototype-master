using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public bool isPaused;
    [SerializeField] GameObject pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape") || Input.GetButtonDown("Start"))
        {
            if (!isPaused)
            {
                isPaused = true;    
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                isPaused = false;   
                pauseMenu.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

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
            {
                isPaused = true;    
                hudMenu.SetActive(false);
                pauseMenu.SetActive(true);
                //EventSystem.current.SetSelectedGameObject(pauseSelectedBut);  
                Time.timeScale = 0;
            }
            else
            {
                isPaused = false;   
                pauseMenu.SetActive(false);
                hudMenu.SetActive(true);
                Time.timeScale = 1;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] List<string> sceneNames;
    private List<GameObject> levelSelectionButtons = new();
    [SerializeField] GameObject levelSelectButPrefab;
    [SerializeField] GameObject firstBut;
    [SerializeField] GameObject rootPanel; 

    // Start is called before the first frame update
    void Start()
    {
        var numScenes = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"numScenes: {numScenes}");

        sceneNames = new List<string>(numScenes);

        AudioSource audioSource = GameObject.Find("Audio").GetComponent<AudioSource>();

        // Get all active and inactive scenes in the Build Settings 
        for (int i = 0; i < numScenes; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Ignore scene(s) that aren't gameplay related
            if (sceneName.Contains("MainMenuScene"))
                continue;

            sceneNames.Add(sceneName);
            Debug.Log($"Adding scene name: {sceneName}");

            // initializing ui buttons
            GameObject button = Instantiate(levelSelectButPrefab, rootPanel.transform);
            button.GetComponent<Button>().onClick.AddListener(() => GetComponent<LoadScene>().LoadSceneByName(sceneName));
            button.GetComponent<Button>().onClick.AddListener(() => audioSource.Play());

            // weird way to trigger audio to onSelect on button
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = null;

            foreach (EventTrigger.Entry ent in eventTrigger.triggers)
                entry = ent;
            entry.eventID = EventTriggerType.Select; 
            entry.callback.AddListener((eventData) => audioSource.Play());

            if (sceneName.Contains("MainScene"))
            {
                firstBut = button;
                button.name = $"Infinite Mode Button";
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Infinite Mode";
                EventSystem.current.SetSelectedGameObject(button);
                button.GetComponent<Button>().OnSelect(null);
            }
            else
            {
                button.name = $"{sceneName} Button";
                button.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
            }
            
            levelSelectionButtons.Add(button);
            button.transform.SetAsFirstSibling();
        }

        Debug.Log($"levelSelectionButtons.Count: {levelSelectionButtons.Count}");

        // Reorder list
        for (int i = levelSelectionButtons.Count ; i > 0; i--)
            levelSelectionButtons[i - 1].transform.SetAsFirstSibling();

    }
    IEnumerator ReselectFirstButton()
    {
        // arbitrtary delay in order for Unity Event System to succesfully reassign first selected button
        // in level selection menu. Keep delay short.
        yield return new WaitForSeconds(.01f);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstBut);
        firstBut.GetComponent<Button>().Select();
    }

    private void OnEnable()
    {
        if (firstBut)
            StartCoroutine(ReselectFirstButton());
    }
}

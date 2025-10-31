using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] List<string> sceneNames;
    [SerializeField] GameObject levelSelectButPrefab;
    private List<GameObject> levelSelectionButtons = new();
    [SerializeField] GameObject rootPanel; 

    // Start is called before the first frame update
    void Start()
    {
        var numScenes = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"numScenes: {numScenes}");

        sceneNames = new List<string>(numScenes);

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
            button.name = $"{sceneName} Button";    
            button.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
            levelSelectionButtons.Add(button);
            button.transform.SetAsFirstSibling();
        }
        Debug.Log($"levelSelectionButtons.Count: {levelSelectionButtons.Count}");

        // traverse in reverse throught the button list so ordered in correct order.
        for (int i = levelSelectionButtons.Count ; i > 0; i--)
        {
            Debug.Log($"{levelSelectionButtons[i- 1].transform.name} - {i -1}");    
            levelSelectionButtons[i - 1].transform.SetAsFirstSibling(); 
        }
    }   
}

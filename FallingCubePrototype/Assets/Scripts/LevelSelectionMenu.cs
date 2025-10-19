using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] List<string> sceneNames;
    [SerializeField] GameObject levelSelectButPrefab; 

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
        }
    }
}

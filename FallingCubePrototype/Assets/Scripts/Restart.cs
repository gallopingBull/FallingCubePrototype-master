using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Restart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RestartLevel());
    }

    private void Update()
    {
        if (Input.GetButtonDown("A"))
        {
            SceneManager.LoadScene(1);
        }
    }
    public void RestartLevelCaller()
    {
        StartCoroutine(RestartLevel());
    }
    
    // Update is called once per frame
    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(1);
        StopCoroutine(RestartLevel());
    }
}

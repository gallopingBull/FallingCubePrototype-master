using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [HideInInspector]
    public static AudioManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    public void PlaySFX(AudioClip sfx)
    {
        GetComponent<AudioSource>().PlayOneShot(sfx);
    }
}

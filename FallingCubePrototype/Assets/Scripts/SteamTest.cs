using Steamworks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SteamTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steamworks not initialized.");
            return;
        }

        string name = SteamFriends.GetPersonaName();    
        Debug.Log($"Steamworks initialized w/ account: {name}");
    }

}

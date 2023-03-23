using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine;

public class SingleplayerSceneSwitch : NetworkBehaviour
{
    public void StartSingleplayer()
    {
        if (NetworkManager.Singleton.StartHost())
            NetworkManager.SceneManager.LoadScene("Game Scene", LoadSceneMode.Single);
        else
            Debug.LogError("Couldnt load Scene in Singleplayer");
    }
}

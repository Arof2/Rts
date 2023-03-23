using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class AssignHostToButton : MonoBehaviour
{
    public Button host;
    public Button join;
    public TextMeshProUGUI statusDisplay;
    public TextMeshProUGUI statusInfo;

    public void Start()
    {
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            host.onClick.AddListener(Host);
            join.onClick.AddListener(Join);

            statusDisplay.text = "Please choose a multiplayer connection method";
            statusInfo.text = "Not Connected";
        }
        else
        {
            host.interactable = false;
            join.interactable = false;
            if(NetworkManager.Singleton.IsClient)
            {
                statusDisplay.text = "Connected as Client";
                statusInfo.text = "Joined as Client";
            }
            else
            {
                statusDisplay.text = "Connected as Host";
                statusInfo.text = "Hosting";
            }
                
        }
        
    }

    public void Host()
    {
        if(NetworkManager.Singleton.StartHost())
        {
            statusDisplay.text = "Connected as Host";
            statusInfo.text = "Hosting";
        }
        else
        {
            statusDisplay.text = "Could not host game";
        }
    }

    public void Join()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            statusDisplay.text = "Connected as Client";
            statusInfo.text = "Joined as Client";
        }
        else
        {
            statusDisplay.text = "Could not join game";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInLobbyInfo : MonoBehaviour
{
    public Player myPlayer;
    public GameObject owner, host, readyCheck;
    public TMP_InputField inputName;

    public void Activate()
    {
        if (myPlayer.owner)
            inputName.interactable = true;
    }

    public void ChangeName()
    {
        myPlayer.SynchPlayerNameServerRpc(inputName.text);
    }
}

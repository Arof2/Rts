using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyInformation : MonoBehaviour
{
    private string lobbyName, password;
    private bool passwordProtected;
    private int playerCount;

    public TextMeshProUGUI lname, count;
    public Toggle tgl;
    public Button joinButton;

    public string JoinCode;

    public void UpdateLobbyInformation(string newName, bool protection, string newpassword, int currentPlayers)
    {
        lobbyName = newName;
        lname.text = lobbyName;

        passwordProtected = protection;
        tgl.isOn = protection;

        password = newpassword;

        playerCount = currentPlayers;
        count.text = currentPlayers + "/4";
    }

    public string GetName()
    {
        return lobbyName;
    }

    public bool GetProtection()
    {
        return passwordProtected;
    }

    public string GetPassword()
    {
        return password;
    }
}

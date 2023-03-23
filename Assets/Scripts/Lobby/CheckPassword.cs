using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using TMPro;

public class CheckPassword : MonoBehaviour
{
    public Lobby currentLobby;
    public LobbyInformation infos;
    public LobbyHandler handler;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI statusText;

    public void PasswordCheck()
    {
        if(inputPassword.text == currentLobby.Data["Password"].Value)
        {
            handler.JoinLobby(currentLobby, infos);
            gameObject.SetActive(false);
        }
        else
        {
            statusText.text = "Wrong Password";
        }
    }

    public void WaitForPassword()
    {
        statusText.text = "Waiting for Password";
    }


}

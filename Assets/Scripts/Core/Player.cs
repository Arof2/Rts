using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Threading.Tasks;

public class Player : NetworkBehaviour
{
    public Teams myTeam;
    public static Player myPlayer;
    public string PlayerName = "no Player Name assigned";
    public bool owner = false, host = false;
    public bool ready = false;
    private bool begon = false;
    public string playerId;

    public override async void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
        {
            Debug.Log("this is mine");
            playerId = LobbyHandler.instance.playerId;
            owner = true;
            myPlayer = this;
            if (IsHost)
                host = true;
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game Scene"))
                Begin();

            SynchAllValuesServerRpc();

            await Task.Delay(1000);

            LobbyHandler.instance.SpawnInAllPlayers();
        }
    }

    #region Synch values in Lobby
    [ServerRpc(RequireOwnership = false)]
    public void SynchAllValuesServerRpc()
    {
        UploadMyValuesClientRpc();
    }

    [ClientRpc]
    public void UploadMyValuesClientRpc()
    {
        if (owner)
            SynchClientValueServerRpc(playerId, host);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SynchClientValueServerRpc(string Id, bool newHost)
    {
        SynchClientValueClientRpc(Id, newHost);
    }

    [ClientRpc]
    public void SynchClientValueClientRpc(string Id, bool newHost)
    {
        playerId = Id;
        host = newHost;
    }
    //Synch Name
    [ServerRpc(RequireOwnership = false)]
    public void SynchPlayerNameServerRpc(string newName)
    {
        SynchPlayerNameClientRpc(newName);
    }

    [ClientRpc]
    public void SynchPlayerNameClientRpc(string newName)
    {
        PlayerName = newName;
        LobbyHandler.instance.UpdatePlayerName(this);
    }

    #endregion

    public void OnLevelWasLoaded(int level)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game Scene"))
        {
            Begin();
        }
    }

    public void Begin()
    {
        if (begon == false)
        {
            StartCoroutine(GetTeam());
            begon = true;
        }

    }

    IEnumerator GetTeam()
    {
        yield return 0;
        TeamsMananger T = TeamsMananger.instance;
        while (!T.GetSpawned())
            yield return 0;

        myTeam = TeamsMananger.instance.ClaimTeam(this);
        BuildingScript.instance.SetTeam(myTeam.teamNumber);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleReadyServerRpc()
    {
        ToggleReadyClientRpc();
        //ReadyCheck
    }

    [ClientRpc]
    public void ToggleReadyClientRpc()
    {
        if (ready)
            ready = false;
        else
            ready = true;
    }
}

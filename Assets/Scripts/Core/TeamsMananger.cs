using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TeamsMananger : NetworkBehaviour
{
    public Teams[] allTeams = new Teams[4];
    [Tooltip("there have to be at least 4 start Color")]
    public static TeamsMananger instance;
    private bool spawned = false;
    public bool GetSpawned() { return spawned; }

    private void Start()
    {
        
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        for (int i = 0; i < allTeams.Length; i++)
        {
            allTeams[i].teamNumber = i;
        }
    }

    public void ChangeTeamColor()
    {

    }

    

    public override void OnNetworkSpawn()
    {
        UpdateClientsOnTeamOccupationServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateClientsOnTeamOccupationServerRpc()
    {
        bool[] occupied = new bool[allTeams.Length];

        for (int i = 0; i < allTeams.Length; i++)
        {
            occupied[i] = allTeams[i].occupied;
        }

        UpdateClientsOnTeamOccupationClientRpc(occupied);
    }

    [ClientRpc]
    public void UpdateClientsOnTeamOccupationClientRpc(bool[] occupence)
    {
        spawned = true;
        if (!IsServer)
        {
            for (int i = 0; i < allTeams.Length; i++)
            {
                allTeams[i].occupied = occupence[i];
            }
        }
    }

    public Teams ClaimTeam(Player player)
    {
        for (int i = 0; i < allTeams.Length; i++)
        {
            if (!allTeams[i].occupied)
            {
                ClaimTeamServerServerRpc(i);
                return allTeams[i];
            }
        }
        Debug.LogWarning("All Teams are occupied, cant assign a new team");
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClaimTeamServerServerRpc(int i)
    {
        ClaimTeamClientRpc(i);
    }

    [ClientRpc]
    public void ClaimTeamClientRpc(int i)
    {
        allTeams[i].occupied = true;
    }

    public Teams GetTeam(int teamNumber)
    {
        foreach (Teams T in allTeams)
        {
            if (T.teamNumber == teamNumber)
                return T;
        }
        return null;
    }

    public override void OnDestroy()
    {
        foreach (Teams T in allTeams)
        {
            T.Reset();
        }

        base.OnDestroy();
    }
}

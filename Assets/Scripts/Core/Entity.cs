using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Events;

public enum ETypes
{
    littleTower,
    GuardTower,
    Camp,
    Building,
    SampleUnit,
    None
}

[System.Serializable]
public struct ColorObjects
{
    public MeshRenderer Renderer;
    public Color mutiplikator;
    public int positionInList;
    public bool emissive;
    public float emissionStrength;
}

public class Entity : NetworkBehaviour
{
    [Tooltip("Each Entity has to be assigned a Team on spawn")]
    public Teams myTeam;

    public ETypes type;
    public GameObject UnitDisplay;

    [Tooltip("These Meshrenderes change their material Color based on which Team they are")]
    public List<ColorObjects> teamColoredObjects;

    [Tooltip("Der Radius des Pathing agents")]
    public float pathingRadius;

    public string networkId;

    public virtual void Awake()
    {
        networkId = GetComponent<NetworkObject>().NetworkObjectId.ToString();
    }

    public virtual void Start()
    {
        if (UnitDisplay == null)
            UnitDisplay = DefaultPrefabs.instance.defaultUnitDisplay;

        foreach (ColorObjects C in teamColoredObjects)
        {
            if(C.emissive)
            {
                C.Renderer.materials[C.positionInList].EnableKeyword("_EMISSION");
                C.Renderer.materials[C.positionInList].SetColor("_EmissionColor", myTeam.currentColor * C.emissionStrength);
                C.Renderer.materials[C.positionInList].color = (C.mutiplikator);
            }
            else
            {
                C.Renderer.materials[C.positionInList].color = (myTeam.currentColor + C.mutiplikator * 3) / 4;
            }
        }
    }

    public virtual void GiveCommands()
    {
        UICommands coms = UICommands.instance;

        Button B = coms.AddCommand("Destroy");
        B.onClick.AddListener(Destroy);

    }

    public void Destroy() //Things that should happen on only this client
    {
        SelectorManager.instance.RemoveEntity(gameObject);
        UICommands.instance.ResetCommands();

        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc() //Spawning/Despawning
    {
        DestroyClientRpc();

        GetComponent<NetworkObject>().DontDestroyWithOwner = true;
        GetComponent<NetworkObject>().Despawn(); //Destroy Objects on all clients
    }

    [ClientRpc]
    public virtual void DestroyClientRpc() //Things that should happen on every client
    {
        // nothing happens here. Its only here to be overwritten
    }

    [ClientRpc]
    public void SetTeamClientRpc(int teamN)
    {
        myTeam = TeamsMananger.instance.GetTeam(teamN);
    }

    [ClientRpc]
    public void SetNameClientRpc(string name)
    {
        gameObject.name = name;
    }

    public virtual void OnSelected() //wird von Selectormanager aufgerufen
    {

    }

    public virtual void OnDeselected() //wird von Selectormanager aufgerufen
    {

    }
}

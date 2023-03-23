using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Events;

public class ProductionBuilding : Structur
{
    ///<summary> The Prefab that will be spawned </summary> 
    public GameObject output;
    public GameObject progressBar;
    public List<UnitProductionComponents> process;
    public List<GameObject> particleEffects;
    
    
    private List<int> productionQueue = new List<int>();
    private List<int> TeamNumbersQueue = new List<int>();

    public override void GiveCommands()
    {
        UICommands coms = UICommands.instance;

        for (int i = 0; i < process.Count; i++)
        {
            int index = i;
            Button B = coms.AddCommand(process[i].commandNames);
            B.onClick.AddListener(() => ProduceUnit(index, myTeam.teamNumber));
        }

        base.GiveCommands();
    }

    public override void Start()
    {
        StartCoroutine(Production());
        base.Start();
    }

    public void ProduceUnit(int i, int teamNumber)
    {
        ProduceUnitServerRpc(i, teamNumber);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProduceUnitServerRpc(int i, int teamNumber)
    {
        ProduceunitClientRpc(i, teamNumber);
    }

    [ClientRpc]
    public void ProduceunitClientRpc(int i, int teamNumber)
    {
        productionQueue.Add(i);
        TeamNumbersQueue.Add(teamNumber);
    }


    IEnumerator Production()
    {
        if(productionQueue.Count == 0)
        {
            yield return 0;
            StartCoroutine(Production());
        }
        else
        {
            int listPos = productionQueue[0];
            int teamnumber = TeamNumbersQueue[0];
            float maxTime = process[listPos].productionTime;
            float remainingTime = maxTime;
            progressBar.SetActive(true);
            StartCoroutine(activateEffects(true));
            


            while (remainingTime > 0)
            {
                remainingTime -= 0.05f;
                progressBar.GetComponent<RectTransform>().localScale = new Vector3(remainingTime/maxTime*4,0.5f,0.5f);
                yield return new WaitForSeconds(0.05f);
            }
            progressBar.SetActive(false);
            activateEffects(false);


            //Spawn unit on Server
            if (IsServer)
            {
                GameObject networkUnit = Instantiate(process[listPos].producableUnit, output.transform.position, Quaternion.identity, DefaultPrefabs.instance.unitHolder);
                networkUnit.GetComponent<NetworkObject>().Spawn();
                networkUnit.GetComponent<Entity>().SetTeamClientRpc(teamnumber);
                networkUnit.GetComponent<Entity>().SetNameClientRpc(networkUnit.name + InputManager.instance.unitCount.Value);
                InputManager.instance.unitCount.Value++;
            } 

            productionQueue.Remove(0);
            TeamNumbersQueue.Remove(0);
            if (productionQueue.Count <= 0)
                StartCoroutine(activateEffects(false));
            StartCoroutine(Production());
        }
    }

    private IEnumerator activateEffects(bool state)
    {
        foreach (GameObject G in particleEffects)
        {
            ParticleSystem P = G.GetComponent<ParticleSystem>();
            if (!state)
            {
                P.Stop();
            }
            else
            {
                P.Play();
            }
        }
        if(!state)
            yield return new WaitForSeconds(4);

        foreach (GameObject G in particleEffects)
        {
            if(productionQueue.Count <= 0 || state)
                G.SetActive(state);
        }
    }
}

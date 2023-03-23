using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
public class PathingAgent : NetworkBehaviour
{
    public NavMeshAgent agent;
    private Entity myEntity;

    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        myEntity = GetComponent<Entity>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void GotoServerRpc(Vector3 targetPos, float targetPathingRadius)
    {
        GotoClientRpc(targetPos, targetPathingRadius);
    }

    [ClientRpc]
    private void GotoClientRpc(Vector3 targetPos, float targetPathingRadius)
    {
        agent.stoppingDistance = myEntity.pathingRadius + targetPathingRadius + 0.1f;
        agent.SetDestination(targetPos);
    }
}

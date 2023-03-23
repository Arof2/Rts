using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(HealthScript))]
[RequireComponent(typeof(PathingAgent))]
public class Unit : Entity
{
    private PathingAgent pathing;
    private Animator animator;
    public bool inAttack = false;

    public float damageValue = 5;

    private IEnumerator attackManager;

    public GameObject targetToFollow;

    public override void Start()
    {
        pathing = GetComponent<PathingAgent>();
        animator = GetComponent<Animator>();

        base.Start();
    }

    public void attackTarget()
    {
        if(IsServer && targetToFollow != null)
            targetToFollow.GetComponent<HealthScript>().ChangeHealthServerRpc(-damageValue);
        else
        {
            animator.SetBool("attacking", false);
            SetInAttackFalse();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoToServerRpc(string target)
    {
        GoToClientRpc(target);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoToServerRpc(Vector3 target)
    {
        GoToClientRpc(target);
    }

    [ClientRpc]
    private void GoToClientRpc(string targetname)
    {
        GameObject target = GameObject.Find(targetname);
        targetToFollow = target;
        bool hostile = target.GetComponent<Entity>().myTeam != myTeam ? true : false ;
        float targetPathingRadius = target.GetComponent<Entity>().pathingRadius;
        Vector3 pos = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        pathing.GotoServerRpc(pos, targetPathingRadius);
        if(attackManager != null)
            StopCoroutine(attackManager);
        attackManager = ManageAttacking(target);
        animator.SetBool("attacking", false);
        animator.SetBool("moving",true);
        SetInAttackFalse();
        if (hostile)
            StartCoroutine(attackManager);
    }

    [ClientRpc]
    private void GoToClientRpc(Vector3 target)
    {
        pathing.GotoServerRpc(target, 0);
        if (attackManager != null)
            StopCoroutine(attackManager);
        animator.SetBool("attacking", false);
        animator.SetBool("moving", true);
    }

    IEnumerator ManageAttacking(GameObject aim)
    {
        while(aim != null)
        {
            if (!inAttack)
            {
                Vector3 pos = new Vector3(aim.transform.position.x, 0, aim.transform.position.z);
                float targetPathingRadius = aim.GetComponent<Entity>().pathingRadius;
                pathing.GotoServerRpc(pos, targetPathingRadius);
            }
                

            if (pathing.agent.remainingDistance <= pathing.agent.stoppingDistance && (!pathing.agent.hasPath || pathing.agent.velocity.sqrMagnitude == 0f))
            {
                animator.SetBool("attacking", true);
                animator.SetBool("moving", false);
            }
            else if(!inAttack)
            {
                animator.SetBool("attacking", false);
            }
                

            yield return 10;
        }
    }

    public void SetInAttackTrue()
    {
        inAttack = true;
    }

    public void SetInAttackFalse()
    {
        inAttack = false;
    }
}

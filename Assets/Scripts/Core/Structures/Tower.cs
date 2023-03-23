using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Tower : Structur
{
    public float range = 80;
    public float damagePerShot = -20;
    public float shotCooldown = 2.5f;
    public List<GameObject> inRange;
    public RangeIndicator indicator;
    public GameObject targetedEntity;
    public GameObject bulletPrefab;
    public Transform shotOrigin;
    private float lastShot;

    public override void Start()
    {
        indicator.UpdateRange(range);
        indicator.OnEnter += TargetEnter;
        indicator.OnExit += TargetExit;

        base.Start();
    }

    public void Update()
    {
        if(targetedEntity == null && inRange.Count != 0)
        {
            TargetSomebody();
        }
        lastShot += Time.deltaTime;
    }

    public override void OnSelected()
    {
        indicator.EnableIndicator();
        indicator.UpdateRange(range);

        base.OnSelected();
    }

    public override void OnDeselected()
    {
        indicator.DisableIndicator();

        base.OnDeselected();
    }

    public void SetRange(float newRange)
    {
        range = newRange;
    }

    public void TargetEnter()
    {
        foreach(GameObject G in indicator.detectedGameobjects)
        {
            if(G.GetComponent<Entity>().myTeam != myTeam && !inRange.Contains(G))
            {
                inRange.Add(G);
                TargetSomebody();
            }
        }
    }

    public void TargetExit()
    {
        GameObject leaving = indicator.lastRemovedGameobject;
        if (leaving.GetComponent<Entity>().myTeam != myTeam)
        {
            inRange.Remove(leaving);

            if(leaving == targetedEntity)
            {
                targetedEntity = null;
                TargetSomebody();
            }
        }
    }

    public void TargetSomebody()
    {
        if(targetedEntity == null)
        {
            targetedEntity = getLowestHealth();
            StartCoroutine(Shooting(targetedEntity));
        }
    }

    private GameObject getLowestHealth()
    {
        GameObject lowest = null;
        float lowestValue = 0;

        foreach (GameObject G in inRange)
        {
            if (G == null)
            {
                inRange.Remove(G);
                break;
            }
                

            if(lowest == null)
            {
                lowest = G;
                lowestValue = G.GetComponent<HealthScript>().getHealth();
            }
            else if(G.GetComponent<HealthScript>().getHealth() < lowestValue)
            {
                lowest = G;
                lowestValue = G.GetComponent<HealthScript>().getHealth();
            }
        }

        return lowest;
    }

    IEnumerator Shooting(GameObject target)
    {
        if (lastShot <= shotCooldown)
            yield return new WaitForSeconds(shotCooldown-lastShot);

        while(target != null && inRange.Contains(target))
        {
            GameObject B = Instantiate(bulletPrefab, shotOrigin.position, Quaternion.identity, transform);
            Bullet BS = B.GetComponent<Bullet>();
            BS.target = target;
            BS.damageValue = damagePerShot;
            BS.asServer = IsServer;

            lastShot = 0;

            yield return new WaitForSeconds(shotCooldown);
        }
    }
}

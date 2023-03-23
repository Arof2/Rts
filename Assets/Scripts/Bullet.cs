using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float damageValue;
    public GameObject target;
    private float lerpValue = 0.2f;
    private bool done = false;
    public bool asServer;

    public void Update()
    {
        if(!done)
        {
            if (target == null)
                Destroy(gameObject);
            else
            {
                transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime * lerpValue * 10f);
                lerpValue += 0.05f;

                if (Vector3.Distance(transform.position, target.transform.position) < 0.5f)
                {
                    if (asServer)
                        target.GetComponent<HealthScript>().ChangeHealthServerRpc(damageValue);
                    Destroy(this.gameObject, 0.5f);
                    done = true;
                }
            }
        }
    }
}

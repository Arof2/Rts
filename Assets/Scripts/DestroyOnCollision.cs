using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    private bool didDestruct = false;
    public GameObject deathEffect;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Structures")
        {
            Selfdestruct();
        }
    }

    public void Selfdestruct()
    {
        if(!didDestruct)
        {
            GameObject g = Instantiate(deathEffect, transform.position, Quaternion.identity, transform.parent);
            Destroy(g, 0.75f);
            Destroy(gameObject);
            didDestruct = true;
        }
    }
}

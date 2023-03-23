using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedDestroyed : MonoBehaviour
{
    public float delaytime = 1f;

    private void Start()
    {
        Destroy(gameObject,delaytime);
    }
}

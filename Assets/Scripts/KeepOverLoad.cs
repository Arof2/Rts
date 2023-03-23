using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepOverLoad : MonoBehaviour
{
    public string tagToCompare;
    public void Awake()
    {
        foreach(KeepOverLoad G in FindObjectsOfType<KeepOverLoad>())
        {
            if(G.gameObject != gameObject)
            {
                if (G.tagToCompare == tagToCompare)
                    Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}

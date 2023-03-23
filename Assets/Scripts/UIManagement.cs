using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    public List<GameObject> UIElemente = new List<GameObject>();

    public void SetNewFocus(GameObject newFocus)
    {
        foreach (GameObject G in UIElemente)
        {
            if(G != newFocus)
            {
                G.GetComponent<Animator>().SetTrigger("OutofFocus");
            }
                
        }
    }
}

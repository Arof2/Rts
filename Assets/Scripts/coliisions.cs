using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coliisions : MonoBehaviour
{
    public List<GameObject> collision = new List<GameObject>();
    public List<GameObject> temp = new List<GameObject>();

    private void Update()
    {
        if (!SelectorManager.instance.choosing)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        foreach (GameObject G in temp)
        {
            if (!collision.Contains(G))
            {
                collision.Add(G);
            }
        }

        List<GameObject> nurKurzJa = new List<GameObject>();

        foreach (GameObject G in collision)
        {
            if (!temp.Contains(G))
            {
                nurKurzJa.Add(G);

            }
        }

        foreach (GameObject G in nurKurzJa)
        {
            collision.Remove(G);
        }

        temp.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.tag == "Structures" || other.gameObject.tag == "Units"))
        {
            temp.Add(other.gameObject);
        }
    }
}

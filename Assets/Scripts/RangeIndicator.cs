using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private float range;
    public GameObject indicator, lastRemovedGameobject;
    public List<GameObject> detectedGameobjects;
    public System.Action OnEnter, OnExit;

    private void Start()
    {
        indicator = transform.GetChild(0).gameObject;
    }

    public void UpdateRange(float newrange)
    {
        range = newrange;
        transform.localScale = new Vector3(range, 30 , range);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Structures" || other.gameObject.tag == "Units")
        {
            detectedGameobjects.Add(other.gameObject);
            CheckforNull();
            OnEnter.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Structures" || other.gameObject.tag == "Units")
        {
            detectedGameobjects.Remove(other.gameObject);
            lastRemovedGameobject = other.gameObject;
            CheckforNull();
            OnExit.Invoke();
        }
    }

    public void DisableIndicator()
    {
        indicator.SetActive(false);
    }

    public void EnableIndicator()
    {
        indicator.SetActive(true);
    }

    private void CheckforNull()
    {
        for (int i = 0; i < detectedGameobjects.Count; i++)
        {
            if(detectedGameobjects[i] == null)
            {
                detectedGameobjects.RemoveAt(i);
            }
        }
    }
}

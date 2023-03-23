using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSettings : MonoBehaviour
{
    public GameObject settingsHolder;
    public static UiSettings instance;


    public bool inSettings = false;

    public void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(inSettings && Input.GetKeyDown(KeyCode.Escape))
        {
            settingsHolder.SetActive(false);
            inSettings = false;
        }
        else if(!inSettings && Input.GetKeyDown(KeyCode.Escape))
        {
            settingsHolder.SetActive(true);
            inSettings = true;
        }
    }
}

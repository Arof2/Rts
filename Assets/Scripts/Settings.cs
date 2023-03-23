using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [Range(60,480)]
    public int targetFps = 240;
    public static Settings instace;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instace = this;

        targetFps = PlayerPrefs.GetInt("fpsCap");
        ChangeFpsCap(targetFps);
    }

    public void Save()
    {
        PlayerPrefs.SetInt("fpsCap", targetFps);

        PlayerPrefs.Save();
    }

    public void ChangeFpsCap(int newcap)
    {
        targetFps = newcap;
        Application.targetFrameRate = targetFps;
    }

    private void OnDestroy()
    {
        Save();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SoundPlayer : MonoBehaviour
{
    public SoundPrefab[] sounds;
    private AudioSource thisSource;
    public static SoundPlayer instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (SoundPrefab s in sounds)
        {
            if(s.loop || s.randomizePitch)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.loop = s.loop;
            }
        }

        DontDestroyOnLoad(gameObject);

        thisSource = gameObject.AddComponent<AudioSource>();

        if(SceneManager.GetActiveScene().name == "Main Menu")
        {
            playSound("Main Menu Ambient");
        }
    }



    public void playSound(string name)
    {
        SoundPrefab S = Array.Find(sounds, sound => sound.name == name);

        if(S == null)
        {
            Debug.LogWarning("Sound: " + name + " not found.");
            return;
        }

        if(S.loop)
        {
            S.source.Play();
        }
        else if(S.randomizePitch)
        {
            S.source.pitch = UnityEngine.Random.Range(0.9f,1.3f);
            S.source.Play();
        }
        else
            thisSource.PlayOneShot(S.clip, S.volume);
    }
}
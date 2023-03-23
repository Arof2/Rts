using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundPrefab
{
    public string name;
    public AudioClip clip;
    public bool loop;
    [Range(0f,1f)]public float volume = 1;
    //[Range(0.1f, 3f)] public float pitch;
    public AudioSource source;
    public bool randomizePitch;
}

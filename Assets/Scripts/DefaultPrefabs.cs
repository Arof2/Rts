using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultPrefabs : MonoBehaviour
{
    public static DefaultPrefabs instance;

    public GameObject defaultUnitDisplay, healthbarPrefab;
    public Transform unitHolder;
    public List<StructuresTypes> allStructures;


    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}

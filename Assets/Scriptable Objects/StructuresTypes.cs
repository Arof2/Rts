using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureType", menuName = "CustomObjects/Structures")]
public class StructuresTypes : ScriptableObject
{
    public ETypes tpye;
    public GameObject prefab;
}
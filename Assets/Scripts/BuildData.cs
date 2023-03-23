using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BuildData : MonoBehaviour
{

    public GameObject building, transparentBuilding;
    private int height = 1, width = 1;
    public float ID;

    private void Start()
    {
        ID = GetInstanceID();

        StructuresTypes S = ScriptableObject.CreateInstance<StructuresTypes>();
        S.prefab = building;
        S.tpye = building.GetComponent<Structur>().type;
        height = building.GetComponent<Structur>().height;
        width = building.GetComponent<Structur>().width;

        DefaultPrefabs.instance.allStructures.Add(S);

        enabled = false;
    }

    public void StartBuilding()
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<InputManager>().ActivatedBuildMode(building,transparentBuilding, width, height, ID);
    }
}

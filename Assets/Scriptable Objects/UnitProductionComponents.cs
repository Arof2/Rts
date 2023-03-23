using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitComponents", menuName = "CustomObjects/UnitComponents")]
public class UnitProductionComponents : ScriptableObject
{
    public GameObject producableUnit; //The integer which represents a position of the Lists above
    public float productionTime; //The Time it will take to produce the Prefab
    public string commandNames; //The name of the command. Also has to be inputed in the List of the script UICommands
}

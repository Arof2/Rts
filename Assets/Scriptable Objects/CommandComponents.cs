using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CommandComponents", menuName = "CustomObjects/CommandComponents")]
public class CommandComponents : ScriptableObject
{
    public GameObject commandsPrefabs;
    public string commandNames;
    public KeyCode commandKeys;
    public bool commandsActive;
}

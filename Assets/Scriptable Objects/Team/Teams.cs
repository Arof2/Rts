using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Team: ", menuName = "CustomObjects/Team")]
public class Teams : ScriptableObject
{
    public List<Color> possibleColors;
    public Color currentColor;
    public int teamNumber;
    public bool occupied;

    public void Reset()
    {
        occupied = false;
        teamNumber = -2; // I didnt use -1 cause im afraid that GetTeam() might find a unassigned team if it is -1
        currentColor = possibleColors[0];
    }
}

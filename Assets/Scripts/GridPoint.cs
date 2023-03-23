using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint
{
    public enum State
    {
        idle,
        occupied,
        parented,
    }

    public float Radius;
    public Vector3 Koords, cornerKoords;
    public string name;
    private GameObject setBuilding;
    private GridPoint parent;
    private List<GridPoint> children = new List<GridPoint>();
    private State status;

    public GridPoint(float newRadius, Vector3 newKoords, string newName)
    {
        Radius = newRadius;
        Koords = newKoords;
        name = newName;
        cornerKoords = Koords + new Vector3(-Radius, 0, -Radius);
    }

    public bool Inherits(Vector3 inputVek)
    {
        float xShort = Koords.x - Radius;
        float xLong = Koords.x + Radius;
        if(!(inputVek.x >= xShort && inputVek.x <= xLong))
        {
            return false;
        }
        float zShort = Koords.z - Radius;
        float zLong = Koords.z + Radius;
        if (!(inputVek.z >= zShort && inputVek.z <= zLong))
        {
            return false;
        }
        return true;
    }

    public void DrawGridPoint()
    {
        Vector3 Ecke1 = Koords + new Vector3(Radius,0,Radius);
        Vector3 Ecke2 = Koords + new Vector3(-Radius,0,Radius);
        Vector3 Ecke3 = Koords + new Vector3(Radius,0,-Radius);
        Vector3 Ecke4 = Koords + new Vector3(-Radius,0,-Radius);

        Debug.DrawLine(Ecke1, Ecke2);
        Debug.DrawLine(Ecke2, Ecke4);
        Debug.DrawLine(Ecke3, Ecke4);
        Debug.DrawLine(Ecke1, Ecke3);
    }

    public void ChangeCondition(State newCondition = State.idle, GameObject newBuilding = null, GridPoint newparent = null, List<GridPoint> newChildren = null)
    {
        switch(newCondition)
        {
            case State.idle:
                if(status == State.parented)
                {
                    parent = null;
                    setBuilding = null;
                    status = State.idle;
                }
                else if(status == State.occupied)
                {
                    setBuilding = null;
                    status = State.idle;

                    //clear all Children
                    foreach (GridPoint G in children)
                    {
                        G.ChangeCondition();
                    }
                }
                break;

            case State.parented:
                if (status == State.idle)
                {
                    setBuilding = newBuilding;
                    status = State.parented;
                    parent = newparent;
                }
                break;

            case State.occupied:
                if(status == State.idle)
                {
                    setBuilding = newBuilding;
                    status = State.occupied;
                    children = newChildren;
                    if(children != null)
                    {
                        foreach (GridPoint G in children)
                        {
                            G.ChangeCondition(State.parented, setBuilding, this, null);
                        }
                    }
                }
                 break;

        }
    }

    public State Condition()
    {
        return status;
    }
}

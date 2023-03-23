using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width = 5, length = 5;
    public float margin = 0.5f, radius = 2f;
    public GameObject Origin;
    public GridPoint[,] Gitter;

    private void Start()
    {
        Gitter = new GridPoint[width,length];
        for(int i = 0; i < width; i++)
        {
            for (int m = 0; m < length; m++)
            {
                float iterationI = Mathf.Ceil(i - width/2f);
                float iterationM = Mathf.Ceil(m - length /2f);
                Vector3 V = Origin.transform.position + new Vector3(iterationI * (radius*2 + margin), 0, iterationM * (radius * 2 + margin));
                Gitter[i,m] = new GridPoint(radius, V, "Gridpoint " + iterationI + "/" + iterationM);
            }
        }
    }

    public void UpdateGrid(int newWidth, int newLength, float newMargin)
    {
        width = newWidth;
        length = newLength;
        margin = newMargin;

        for (int i = 0; i < width; i++)
        {
            for (int m = 0; m < length; m++)
            {
                float iterationI = Mathf.Ceil(i - width / 2f);
                float iterationM = Mathf.Ceil(m - length / 2f);
                Vector3 V = Origin.transform.position + new Vector3(iterationI * (radius * 2 + margin), 0, iterationM * (radius * 2 + margin));
                Gitter[i, m] = new GridPoint(radius, V, "Gridpoint " + iterationI + "/" + iterationM);
            }
        }
    }

    public GridPoint FindGridPoint(Vector3 inputKoords)
    {
        for (int i = 0; i < width; i++)
        {
            for (int m = 0; m < length; m++)
            {
                if(Gitter[i,m].Inherits(inputKoords))
                {
                    return Gitter[i, m];
                }
            }
        }
        return null;
    }

    public void DrawGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int m = 0; m < length; m++)
            {
                Gitter[i, m].DrawGridPoint();
            }
        }
    }

    public GridPoint FindBelow(GridPoint startpoint)
    {
        if(startpoint != null)
        {
            Vector3 searchkoords = startpoint.Koords + new Vector3(0, 0, -radius * 2 - margin);
            return FindGridPoint(searchkoords);
        }
        return null;
    }

    public GridPoint FindBeside(GridPoint startpoint)
    {
        if(startpoint != null)
        {
            Vector3 searchkoords = startpoint.Koords + new Vector3(-radius * 2 - margin, 0, 0);
            return FindGridPoint(searchkoords);
        }
        return null;
    }
}

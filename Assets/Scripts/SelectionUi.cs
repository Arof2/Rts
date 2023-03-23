using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum Scaling
{
    Min,
    Medium,
    Max,
}

public class SelectionUi : MonoBehaviour
{
    public GameObject holder, backgroundUi;
    private Scaling sizeStatus = Scaling.Min;
    public static SelectionUi instance;

    public void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        if (holder.transform.childCount == 0)
        {
            backgroundUi.SetActive(false);
            holder.SetActive(false);
        }
        else
        {
            backgroundUi.SetActive(true);
            holder.SetActive(true);
        } 
    }

    public void AddUIElement(GameObject UIElement, GameObject assignedEntity)
    {
        GameObject newElement = Instantiate(UIElement, holder.transform);
        newElement.GetComponent<IsUIElement>().assignedEntity = assignedEntity;

        ScaleAll();
    }

    public void RemoveUIElement(GameObject Entity)
    {
        foreach (IsUIElement G in holder.transform.GetComponentsInChildren<IsUIElement>())
        {
            if(G.assignedEntity == Entity)
            {
                Destroy(G.gameObject);
                ScaleAll();
            }
        }
    }

    public void ScaleAll()
    {
        if (holder.transform.childCount >= 10 && holder.transform.childCount <= 16 && sizeStatus != Scaling.Medium)
        {
            switch (sizeStatus)
            {
                case Scaling.Min:
                    ScaleUp();
                    break;
                case Scaling.Max:
                    ScaleDown();
                    break;
            }
        }
        else if (holder.transform.childCount < 10 && sizeStatus != Scaling.Min)
            ScaleDown();
        else if (holder.transform.childCount >= 17 && sizeStatus != Scaling.Max)
            ScaleUp();
        else
            Rescale();
    }

    private void ScaleDown()
    {
        switch (sizeStatus)
        {
            case Scaling.Medium:
                ChangeScale(1);
                sizeStatus = Scaling.Min;
                break;
            case Scaling.Max:
                ChangeScale(0.67f);
                sizeStatus = Scaling.Medium;
                break;
        }
    }

    private void ScaleUp()
    {
        switch (sizeStatus)
        {
            case Scaling.Min:
                ChangeScale(0.67f);
                sizeStatus = Scaling.Medium;
                break;
            case Scaling.Medium:
                ChangeScale(0.52f);
                sizeStatus = Scaling.Max;
                break;
        }
    }

    private void ChangeScale(float newScale = 1)
    {
        for (int i = 0; i < holder.transform.childCount; i++)
        {
            RectTransform G = holder.transform.GetChild(i).GetComponent<RectTransform>();
            G.localScale = new Vector3(newScale, newScale, newScale);
        }
        holder.GetComponent<GridLayoutGroup>().cellSize = new Vector2(newScale * 100, newScale * 100);
    }

    private void Rescale()
    {
        switch(sizeStatus)
        {
            case Scaling.Min:
                ChangeScale(1);
                break;
            case Scaling.Medium:
                ChangeScale(0.67f);
                break;
            case Scaling.Max:
                ChangeScale(0.52f);
                break;
        }
    }
}

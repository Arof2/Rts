using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarHandler : MonoBehaviour
{
    public int hoeheViewingArea = 800;
    public RectTransform target;
    public Scrollbar scroll;

    public void ScrollbarValueChange()
    {
        target.anchoredPosition = new Vector3(target.anchoredPosition.x, (target.sizeDelta.y - 800) * scroll.value, 0);
    }
}

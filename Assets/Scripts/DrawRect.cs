using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRect : MonoBehaviour
{
    public GameObject E2, E3, E4;
    public GameObject L1, L2, L3, L4;
    public Vector3 targetVector;

    void Update()
    {
        if (!SelectorManager.instance.choosing)
            Destroy(gameObject);

        JustUpdateDude();
    }

    public void JustUpdateDude()
    {
        Vector3 origin = transform.position;
        E2.transform.position = targetVector;
        E3.transform.position = origin + new Vector3(targetVector.x - origin.x, 0, 0);
        E4.transform.position = origin + new Vector3(0, targetVector.y - origin.y, 0);

        L1.transform.position = Vector3.Lerp(origin, E3.transform.position, 0.5f);
        L1.GetComponent<RectTransform>().sizeDelta = new Vector2(Vector3.Distance(origin, E3.transform.position) * 0.9f, 5);

        L2.transform.position = Vector3.Lerp(origin, E4.transform.position, 0.5f);
        L2.GetComponent<RectTransform>().sizeDelta = new Vector2(5, Vector3.Distance(origin, E4.transform.position) * 0.9f);

        L3.transform.position = Vector3.Lerp(targetVector, E4.transform.position, 0.5f);
        L3.GetComponent<RectTransform>().sizeDelta = new Vector2(Vector3.Distance(targetVector, E4.transform.position) * 0.9f, 5);

        L4.transform.position = Vector3.Lerp(targetVector, E3.transform.position, 0.5f);
        L4.GetComponent<RectTransform>().sizeDelta = new Vector2(5, Vector3.Distance(targetVector, E3.transform.position) * 0.9f);
    }
}

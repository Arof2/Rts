using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public GameObject rotator; // Rotate y Axis
    public float speed = 10;
    public float scale = 1;

    private void Update()
    {
        rotator.transform.rotation *= Quaternion.Euler(0, speed * Time.deltaTime,0);
    }

    public void UpdateScale(float newScale)
    {
        scale = newScale;
        rotator.transform.localScale = new Vector3(scale,scale,scale);
    }
}

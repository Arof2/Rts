using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public void Update()
    {
        gameObject.GetComponent<Transform>().LookAt(Camera.main.GetComponent<Transform>(), Vector3.up);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public float camSpeed = 5;
    public int farthest = 15, nearest = 2;
    private GameObject Kamera;
    private Vector3 aimVector;
    private int count = 5;
    private bool state = true;
    private Vector3 latestMousePos;
    void Start()
    {
        Kamera = Camera.main.gameObject;
        aimVector = Kamera.transform.position;
    }

    private void FixedUpdate()
    {
        float timeCamSpeed = camSpeed * Time.deltaTime;
        if(state)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) || Input.GetKeyUp(KeyCode.W) || Input.mousePosition.y > Screen.height * 0.99f && Input.mousePosition.y < Screen.height)
            {
                Kamera.transform.position += Vector3.forward * timeCamSpeed;
                aimVector += Vector3.forward * timeCamSpeed;
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) || Input.GetKeyUp(KeyCode.A) || Input.mousePosition.x < Screen.width * 0.01f && Input.mousePosition.x > 0)
            {
                Kamera.transform.position += Vector3.left * timeCamSpeed;
                aimVector += Vector3.left * timeCamSpeed;
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) || Input.GetKeyUp(KeyCode.S) || Input.mousePosition.y < Screen.height * 0.01f && Input.mousePosition.y > 0)
            {
                Kamera.transform.position += Vector3.back * timeCamSpeed;
                aimVector += Vector3.back * timeCamSpeed;
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) || Input.GetKeyUp(KeyCode.D) || Input.mousePosition.x > Screen.width * 0.99f && Input.mousePosition.x < Screen.width)
            {
                Kamera.transform.position += Vector3.right * timeCamSpeed;
                aimVector += Vector3.right * timeCamSpeed;
            } 
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(2) || Input.GetMouseButtonDown(2))
        {
            if (Input.GetMouseButtonDown(2))
                latestMousePos = Input.mousePosition;
            else
            {
                Vector3 diff = -(Input.mousePosition - latestMousePos) / 70;
                latestMousePos = Input.mousePosition;
                Kamera.transform.position += new Vector3(diff.x, 0, diff.y);
                aimVector += new Vector3(diff.x, 0, diff.y);
            }
        }

        if (!UiSettings.instance.inSettings)
        {
            Kamera.transform.position = Vector3.Lerp(Kamera.transform.position, aimVector, Time.deltaTime * camSpeed);
            if (Input.mouseScrollDelta.y < 0 && count < farthest)
            {
                aimVector += new Vector3(0, 1, -0.5f);
                count++;
            }
            else if (Input.mouseScrollDelta.y > 0 && count > nearest)
            {
                aimVector += new Vector3(0, -1, 0.5f);
                count--;
            }
        }
    }

    public void SetState(bool t)
    {
        state = t;
    }
}

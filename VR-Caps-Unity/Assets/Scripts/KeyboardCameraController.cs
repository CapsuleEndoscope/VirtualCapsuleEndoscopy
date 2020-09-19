using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardCameraController : MonoBehaviour
{

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw;
    private float pitch;



    // Use this for initialization
    void Start()
    {
        pitch = transform.localEulerAngles.x;
        yaw =  transform.localEulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {

        yaw -= speedH * Input.GetAxis("Horizontal");
        pitch -= speedV * Input.GetAxis("Vertical");
        print(yaw);
        transform.localEulerAngles = new Vector3(pitch, yaw, 0.0f);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotFixed : MonoBehaviour
{
    Quaternion InitialRotation;
    public Camera cam;
    void Start()
    {
        InitialRotation = transform.rotation;
    }
    // Update is called once per frame
    void Update()
    {
        // transform.rotation = InitialRotation;
        transform.rotation = Quaternion.Euler(90,cam.transform.eulerAngles.y,cam.transform.eulerAngles.z);

    }
}

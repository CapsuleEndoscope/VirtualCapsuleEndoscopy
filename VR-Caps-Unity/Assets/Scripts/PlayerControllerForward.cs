using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerForward : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;
    public Camera capsuleCam;
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        Vector3 noise = AddNoiseOnAngle(-30,30);
        transform.position = transform.position + (capsuleCam.transform.forward+noise) * speed;
        // transform.Translate(capsuleCam.transform.forward);
        // rb.AddForce((-capsuleCam.transform.forward+noise)* speed);
    }


    Vector3 AddNoiseOnAngle ( float min, float max)  {
    // Find random angle between min & max inclusive
    float xNoise = Random.Range (min, max);
    float yNoise = Random.Range (min, max);
    float zNoise = Random.Range (min, max);
    
    // Convert Angle to Vector3
    Vector3 noise = new Vector3 ( 
        Mathf.Sin (2 * Mathf.PI * xNoise /360), 
        Mathf.Sin (2 * Mathf.PI * yNoise /360), 
        Mathf.Sin (2 * Mathf.PI * zNoise /360)
        );
    return noise;
    }


}

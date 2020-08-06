// Rotate an object around its Y (upward) axis in response to
// left/right controls.
using UnityEngine;
using System.Collections;

public class AddTorque : MonoBehaviour
{
    public float torque;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float turn = Input.GetAxis("Horizontal");
        rb.AddTorque(transform.up * torque * turn);
    }
}
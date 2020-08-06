using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_oldSchool : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis ("Horizontal");
        float moveVertical = Input.GetAxis ("Vertical");

        Vector3 movement = new Vector3 (moveHorizontal, 0f, moveVertical);
        rb.AddForce (movement*speed);
    }
    private void Update() {

        if (Input.GetKey(KeyCode.W))
            rb.AddForce(Vector3.up);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(Vector3.down);

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        // float moveHorizontal = Input.GetAxis ("Horizontal");
        // float moveVertical = Input.GetAxis ("Vertical");
        // Vector3 MovePlayer = new Vector3 (moveHorizontal, moveVertical, .0f);
        // Vector3 movement = new Vector3 (moveHorizontal, moveVertical, .0f);
        
        // if (Input.GetKey(KeyCode.W)){
        //     MovePlayer[2] = 1;
        //     movement[2] = 1;
        // }
        // if (Input.GetKey(KeyCode.S)){
        //     MovePlayer[2] = -1;
        //     movement[2] = -1;
        // }


        // // rb.velocity = MovePlayer;

        // // moveForward = -moveForward;

        
        // //rb.AddForce (movement*speed);
        // rb.AddForce(transform.forward * speed);
        // //Vector2 MovePlayer = new Vector2(-moveHorizontal, moveVertical);




        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x - 0.01f*speed;
            this.transform.position = position;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x + 0.01f*speed;
            this.transform.position = position;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 position = this.transform.position;
            position.z = position.z + 0.01f*speed;
            this.transform.position = position;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 position = this.transform.position;
            position.z = position.z - 0.01f*speed;
            this.transform.position = position;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 position = this.transform.position;
            position.y = position.y + 0.01f*speed;
            this.transform.position = position;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 position = this.transform.position;
            position.y = position.y - 0.01f*speed;
            this.transform.position = position;
        }


    }
    // private void Update() {

    //     if (Input.GetKey(KeyCode.W))
    //         rb.AddForce(Vector3.up);
    //     if (Input.GetKey(KeyCode.S))
    //         rb.AddForce(Vector3.down);

    // }

}

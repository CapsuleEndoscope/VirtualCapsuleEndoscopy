using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardMovement : MonoBehaviour
{
	public float speed;
  private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody>();
    }

   	// Update is called once per frame
    void Update(){

  		if (Input.GetKey(KeyCode.UpArrow)){
          transform.Translate(Vector3.forward * Time.deltaTime);

        	// rb.AddForce(new Vector3(0,0,1) * speed, ForceMode.Impulse);
        }
       	if (Input.GetKey(KeyCode.DownArrow)){
                 transform.Translate(Vector3.back * Time.deltaTime);
			// rb.AddForce(new Vector3(0,0,-1) * speed, ForceMode.Impulse);
       	}
        if (Input.GetKey(KeyCode.LeftArrow)){
        	       transform.Translate(Vector3.left * Time.deltaTime);
            // rb.AddForce(new Vector3(-1,0,0) * speed, ForceMode.Impulse);
        }
		if (Input.GetKey(KeyCode.RightArrow)){
    transform.Translate(Vector3.right * Time.deltaTime);

			// rb.AddForce(new Vector3(1,0,0) * speed, ForceMode.Impulse);
		}
        if (Input.GetKey(KeyCode.W)){
      transform.Translate(Vector3.up * Time.deltaTime);

			// rb.AddForce(new Vector3(0,1,0) * speed, ForceMode.Impulse);
        }
      if (Input.GetKey(KeyCode.S)){
      transform.Translate(Vector3.down * Time.deltaTime);

      // rb.AddForce(new Vector3(0,01,0) * speed, ForceMode.Impulse);
        }
    }
}

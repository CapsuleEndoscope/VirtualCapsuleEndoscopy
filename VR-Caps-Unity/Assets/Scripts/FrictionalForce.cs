using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionalForce : MonoBehaviour
{
    private Rigidbody capsule;

    void Start ()
    {

        capsule = GetComponent<Rigidbody>();
    }

    private void Update() {

        // get the capsule current velocity
        Vector3 vel = capsule.velocity;
        // calculate frictional resistance
        float friction_mag = calculate_friction(vel.magnitude);

        // check if it's less than speed
        if (vel.magnitude > friction_mag){

            // get the unit friction vector (opposite direction)
            Vector3 friction_vec = -vel/vel.magnitude;

            // add the frictional force
            capsule.AddForce(friction_vec * friction_mag);

        }

    }

    // calculate the frictional force obtained from natural logarithm curve fit
    private static float calculate_friction(float speed){
        return 53.04f * Mathf.Log(0.23f * speed + 1.04f) + 100.0f; 
    }

}

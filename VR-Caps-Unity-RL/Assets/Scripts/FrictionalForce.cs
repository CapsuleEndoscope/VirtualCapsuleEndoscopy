using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionalForce : MonoBehaviour
{
    public Rigidbody capsule;
    // public Camera cam;
    // public static Vector3 friction_force;

    void Start ()
    {

        capsule = GetComponent<Rigidbody>();
    }

    private void Update() {

        // get the capsule current velocity
        Vector3 vel = capsule.velocity;
        
        // calculate frictional resistance
        float friction_mag = 0.2f * calculate_friction(vel.magnitude);
        capsule.drag = friction_mag;
        capsule.angularDrag = friction_mag/1.9f; // angular vel = vel / radius
        // Debug.Log("Velocity: " + vel.ToString());
        // Debug.Log("Drag: " + capsule.drag.ToString());
        // Debug.Log("Angular Drag: " + capsule.angularDrag.ToString());
        // Debug.Log("Camera: " + cam.transform.rotation);
        // Debug.Log("caps: " + capsule.transform.rotation);




        // // check if it's less than speed
        // if (vel.magnitude > friction_mag){

        //     // get the unit friction vector (opposite direction)
        //     Vector3 friction_vec = -vel/vel.magnitude;

        //     // add the frictional force
        //    friction_force  = (friction_vec * friction_mag);

        // }

    }

    // calculate the frictional force obtained from natural logarithm curve fit
    private static float calculate_friction(float speed){
        return 53.04f * Mathf.Log(0.23f * speed + 1.04f) + 100f; 
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//SAME REFERENCE IN "MeshDeformer" SCRIPT
//THE COLLISION PART HAS NO SPECIFIC REFERENCE AS IT IS TAKEN FROM THE UNITY DOCUMENTATIONS


//This script is attached to the object that causes the deformation hence called "MeshDeformationInput", which in our case is attached to the capsule.
public class MeshDeformerInput : MonoBehaviour {

    //Configurable input force
	public float force = 10f;
    //Force offset is used to apply forces in different directions at surrounding points based on the normals direction
    public float forceOffset = 0.1f;


    //Mouse Input Deformation (NOT USED)
    /*
    void Update () {
		if (Input.GetMouseButton(0)) {
			HandleInput();
		}
	}
    */


    //If Collision occurs we retrieve the "MeshDeformer" component from the object it collided with (the organs in our case)
    //We add a deforming force at the each contact point with "AddDeformingForce" that is applied in the "MeshDeformer" script
    //
    void OnCollisionEnter(Collision col)
    {
        MeshDeformer deformer = col.collider.GetComponent<MeshDeformer>();
        if (deformer) {
            for (int c = 0; c < col.contacts.Length; c++){
				Vector3 point = col.contacts[c].point;
                point += col.contacts[c].normal * forceOffset;
				deformer.AddDeformingForce(point, force);
			}}
    }

    //OnCollisionStay method assures that the force is still appiled as long as the capsule keeps pushing on the same contact point
    void OnCollisionStay(Collision col)
    {
        MeshDeformer deformer = col.collider.GetComponent<MeshDeformer>();
        if (deformer) {
            for (int c = 0; c < col.contacts.Length; c++){
				Vector3 point = col.contacts[c].point;
                point += col.contacts[c].normal * forceOffset;
				deformer.AddDeformingForce(point, force);
			}}
    }


    //Related to the mouse input (NOT USED)
    /*
    void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

		if (Physics.Raycast(inputRay, out hit)) {
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer) {
				Vector3 point = hit.point;
                point += hit.normal * forceOffset;
				deformer.AddDeformingForce(point, force);
			}
		}
	}
    */
    

}

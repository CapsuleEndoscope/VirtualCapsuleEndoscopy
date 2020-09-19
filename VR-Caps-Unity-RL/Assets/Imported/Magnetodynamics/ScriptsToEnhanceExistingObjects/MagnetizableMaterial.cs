using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Forces between magnetic dipoles and magnetizable materials are implemented with SpringJoints, rather than explicit forces, to get them to collide gracefully (not fly through each other, experiencing infinite forces, NaN, etc.).

// Colliding gracefully is more difficult for material than for the dipoles themselves because the entire volume of the (Collider of the) material can get temporarily magnetized, and an edge of a volume can get much closer to the dipoles than dipoles can get to each other (assuming that you've properly buried them inside of an object).

// This volumetric force is necessary for magnetizable material and not for permanant dipoles because the force law for magnetized objects is so much steeper (1/r^7 rather than 1/r^4).  And that's because it has to get polarized by the dipoles (1/r^3) before it can get attracted to them (1/r^4).

// This *does not* implement the induced magnetic fields (the H-field and all that), which would allow one magnetized paperclip to attract others.  We can only take realism so far...

public class MagnetizableMaterial : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    protected ElectromagneticFieldControllerScript controller;
    
    public float strength = 1e-14f;  // susceptibility/(1 + susceptibility) times volume
    public bool applyExternalForce = false;
    public bool applyDipoleForce = true;

    private Dictionary<int,SpringJoint> sjToDipole = new Dictionary<int,SpringJoint>();
    private Dictionary<int,SpringJoint> sjFromDipole = new Dictionary<int,SpringJoint>();

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
	    if (!GetComponent<Collider>()) {
	        throw new System.Exception("Only objects with a collider can be made magnetizable");
	    }
	    
        if (!GetComponent<Rigidbody>()) {
	        throw new System.Exception("Only objects with a rigidbody can be made magnetizable");
	    }
	    
	    if (strength < 0.0f) {
	        throw new System.Exception("Magnetization strength must be non-negative");
	    }
	}
	
	void FixedUpdate() {
	    controller.ApplyForcesOnMagnetizable(applyExternalForce, applyDipoleForce, strength, gameObject, ref sjToDipole, ref sjFromDipole);
	}
}

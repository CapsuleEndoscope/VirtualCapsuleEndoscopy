using UnityEngine;
using System;
using System.Collections;

// This is the model for all hand-built electric and magnetic fields.  You supply a function that calculates custom electric fields, magnetic fields, and magnetic field derivatives (see solenoid example) and send them to the ElectromagneticFieldController.  The controller calls them (and all other field sources) whenever it needs to find out what is the field at a given point in space.

public class ExternalFieldConstantScript : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    private ElectromagneticFieldControllerScript controller;

    public Vector3 electricField;
    public Vector3 magneticField;
    public bool onlyWithinBox = true;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
	    controller.RegisterElectricField(ElectricField);
	    controller.RegisterMagneticField(MagneticField);
	}
	
	bool InsideBox(Vector3 pos) {
	    Vector3 localPos = transform.InverseTransformPoint(pos);
	    return Mathf.Abs(localPos.x) < 0.5f  &&  Mathf.Abs(localPos.y) < 0.5f  &&  Mathf.Abs(localPos.z) < 0.5f;
	}
	
	Vector3 ElectricField(Vector3 pos) {
	    if (!onlyWithinBox  ||  InsideBox(pos)) {
	        return electricField;
	    }
	    else {
	        return Vector3.zero;
	    }
	}
	
	Vector3 MagneticField(Vector3 pos) {
	    if (!onlyWithinBox  ||  InsideBox(pos)) {
	        return magneticField;
	    }
	    else {
	        return Vector3.zero;
	    }
	}
}

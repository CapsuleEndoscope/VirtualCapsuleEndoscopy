using UnityEngine;
using System;
using System.Collections;

public class ExternalFieldTemplateScript : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    private ElectromagneticFieldControllerScript controller;

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
	    controller.RegisterMagneticFieldDerivatives(MagneticFieldDx, MagneticFieldDy, MagneticFieldDz);
	}

	Vector3 ElectricField(Vector3 pos) {
        return Vector3.zero;
	}
	
	Vector3 MagneticField(Vector3 pos) {
        return Vector3.zero;
	}

	Vector3 MagneticFieldDx(Vector3 pos) {
        return Vector3.zero;
	}

	Vector3 MagneticFieldDy(Vector3 pos) {
        return Vector3.zero;
	}

	Vector3 MagneticFieldDz(Vector3 pos) {
        return Vector3.zero;
	}
}

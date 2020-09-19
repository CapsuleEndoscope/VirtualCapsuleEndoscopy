using UnityEngine;
using System.Collections;

public class SuperconductorScript : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    protected ElectromagneticFieldControllerScript controller;
}

public class SuperconductorInfinitePlaneScript : SuperconductorScript {
    // cache derivatives so that you don't have to compute them three times (for dx, dy, dz)
    private Vector3 lastpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 lastselfpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 lastselfdir = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 ddx_B = Vector3.zero;
    private Vector3 ddy_B = Vector3.zero;
    private Vector3 ddz_B = Vector3.zero;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
	    controller.RegisterSuperconductorMagneticField(MagneticField);    
	    controller.RegisterSuperconductorMagneticFieldDerivatives(MagneticFieldBx, MagneticFieldBy, MagneticFieldBz);    
    }
    
    Vector3 MagneticField(Vector3 pos, int excludeSuperconductors) {
        Vector3 mirrorPos = transform.InverseTransformPoint(pos);
        mirrorPos.y = -mirrorPos.y;
        mirrorPos = transform.TransformPoint(mirrorPos);
        return -controller.MagneticField(mirrorPos, -1, excludeSuperconductors + 1);
    }

    Vector3 MagneticFieldBx(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddx_B;
    }
    
    Vector3 MagneticFieldBy(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddy_B;
    }
    
    Vector3 MagneticFieldBz(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddz_B;
    }
    
    void derivatives(Vector3 pos, int excludeSuperconductors) {
        if (pos == lastpos  &&  transform.position == lastselfpos  &&  transform.eulerAngles == lastselfdir) { return; }
        lastpos = pos;
        lastselfpos = transform.position;
        lastselfdir = transform.eulerAngles;

        Vector3 mirrorPos = transform.InverseTransformPoint(pos);
        mirrorPos.y = -mirrorPos.y;
        mirrorPos = transform.TransformPoint(mirrorPos);
        Vector3 dummy;
        controller.MagneticFieldDerivatives(mirrorPos, out dummy, out ddx_B, out ddy_B, out ddz_B, -1, excludeSuperconductors + 1);
        ddx_B *= -1.0f;
        ddy_B *= -1.0f;
        ddz_B *= -1.0f;
    }
}

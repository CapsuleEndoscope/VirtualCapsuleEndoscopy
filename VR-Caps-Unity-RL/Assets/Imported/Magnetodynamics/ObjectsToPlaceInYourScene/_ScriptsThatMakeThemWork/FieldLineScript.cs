// #define I_HAVE_VECTROSITY
#define SHOW_VECTROSITY_ERRORS

using UnityEngine;
using System;
using System.Collections;
#if I_HAVE_VECTROSITY
using Vectrosity;
#endif

// NOTE: Requires the Vectrosity package (from the Asset Store).
// This draws an electric or magnetic field line, starting at the position of the empty GameObject and stopping after numberOfSegments*segmentLength, or when it doubles back on itself.
// The line solves a differential equation on the fly using Euler's method, the Runge-Kutta method, or an intermediate second-order method (a good balance of accuracy and performance).

public class FieldLineScript: MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    private ElectromagneticFieldControllerScript controller;

    public bool calculateOnlyOnce = false;
    private bool calculated = false;

    public enum WhichField {
        Electric,
        Magnetic
    };
    public enum Accuracy {
        EulersMethod,
        SecondOrderMethod,
        RungeKuttaMethod
    };

    public WhichField whichField = WhichField.Magnetic;
    public int numberOfSegments = 32;    // number of line segments
    public float segmentLength = 0.1f;   // length of each line segment
    public Accuracy accuracy = Accuracy.SecondOrderMethod;
    public float backTolerance = 0.75f;
    public float stopTolerance = 0.95f;  // how close to a previous point to consider "doubling back on itself".  Should be slightly less than 1.
    // If you want to avoid this potentially expensive calculation (it turns an O(numberOfSegments) algorithm into an O(numberOfSegments**2) algorithm), set stopTolerance to -1.
    
    public Material lineMaterial = null;
    public Color color = Color.green;
    public float thickness = 8.0f;

    #if I_HAVE_VECTROSITY
    private VectorLine line;
    #endif
    private Func<Vector3,int,int,Vector3> field;
    private Func<Vector3,Vector3> calculate;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();

        #if I_HAVE_VECTROSITY
	    line = new VectorLine(gameObject.name + "Line", new Vector3[numberOfSegments], color, lineMaterial, thickness, LineType.Continuous);
	    line.joins = Joins.Weld;
	    line.maxWeldDistance = 5;
        #else
        #if SHOW_VECTROSITY_ERRORS
        throw new System.Exception("FieldLines have been disabled because Vectrosity might not be available on your system.  To enable them, open Magnetodynamics/ObjectsToPlaceInYourScene/_ScriptsThatMakeThemWork/FieldLineScript and remove the '//' before '#define I_HAVE_VECTROSITY'.  To hide this error message instead, remove the '#define SHOW_VECTROSITY_ERRORS' line in the same script.");
        #endif
	    #endif
	    
	    if (whichField == WhichField.Electric) {
	        field = controller.ElectricField;
	    }
	    else if (whichField == WhichField.Magnetic) {
	        field = controller.MagneticField;
	    }
	    
	    if (accuracy == Accuracy.EulersMethod) {
	        calculate = EulersMethod;
	    }
	    else if (accuracy == Accuracy.SecondOrderMethod) {
	        calculate = SecondOrderMethod;
	    }
	    else if (accuracy == Accuracy.RungeKuttaMethod) {
	        calculate = RungeKuttaMethod;
	    }
	}
	
	Vector3 EulersMethod(Vector3 pos) {
	    Vector3 k1 = field(pos, -1, 0);
	    if (k1 == Vector3.zero) { throw new System.Exception(); }
	    return k1.normalized;
	}
	
	Vector3 SecondOrderMethod(Vector3 pos) {
	    Vector3 k1 = field(pos, -1, 0);
	    if (k1 == Vector3.zero) { throw new System.Exception(); }
	    Vector3 k2 = field(pos + 0.5f * segmentLength * k1.normalized, -1, 0);
	    if (k2 == Vector3.zero) { throw new System.Exception(); }
	    return k2.normalized;
	}
	
	Vector3 RungeKuttaMethod(Vector3 pos) {
	    Vector3 k1 = field(pos, -1, 0);
	    if (k1 == Vector3.zero) { throw new System.Exception(); }
	    Vector3 k2 = field(pos + 0.5f * segmentLength * k1.normalized, -1, 0);
	    if (k2 == Vector3.zero) { throw new System.Exception(); }
	    Vector3 k3 = field(pos + 0.5f * segmentLength * k2.normalized, -1, 0);
	    if (k3 == Vector3.zero) { throw new System.Exception(); }
	    Vector3 k4 = field(pos + segmentLength * k3.normalized, -1, 0);
	    if (k4 == Vector3.zero) { throw new System.Exception(); }
	    return (k1/6.0f + k2/3.0f + k3/3.0f + k4/6.0f).normalized;
	}

	void FixedUpdate() {
	    #if I_HAVE_VECTROSITY
	    if (!calculateOnlyOnce  ||  !calculated) {
            Vector3 pos = transform.position;
            line.points3[0] = pos;
            
            Vector3 lastStep = Vector3.zero;
            Vector3 step = Vector3.zero;
            int i;
            for (i = 1;  i < numberOfSegments;  i++) {
                try {
                    step = calculate(pos);
                }
                catch { }
                
                if (i > 1) {
                    if (Vector3.Dot(lastStep, step) < backTolerance) {
                        break;
                    }
                }
                lastStep = step;
    
                pos += segmentLength * step;
                line.points3[i] = pos;
                    
                if (stopTolerance > 0.0f) {
                    bool breakHere = false;
                    for (int j = 0;  j < i;  j++) {
                        if (Vector3.Distance(line.points3[i], line.points3[j]) < stopTolerance * segmentLength) {
                            breakHere = true;
                            break;
                        }
                    }
                    if (breakHere) break;
                }
            }
            
            while (i < numberOfSegments) {
                line.points3[i] = pos;
                i++;
            }
            calculated = true;
	    }
	    #endif
	}
	
	void Update() {
	    #if I_HAVE_VECTROSITY
	    line.Draw3D();
	    #endif
	}
}

using UnityEngine;
using System.Collections;

public class ElectromagneticInfinitesimal : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    protected ElectromagneticFieldControllerScript controller;

    protected int rigidbodyId;

	public int GetRigidbodyId() {
	    return rigidbodyId;
	}
	
	// When magnets collide, we have to be careful that they don't go inside each other and create divergent forces.  Keep track of which magnets' rigidbodies are touching which other ones so that we can turn off the forces in those cases.
	void OnTriggerEnter(Collider other) {
	    ElectromagneticInfinitesimal otherScript = other.gameObject.GetComponent<MagneticDipoleScript>();
	    if (!otherScript) { otherScript = other.gameObject.GetComponent<StaticChargeScript>(); }
	    if (otherScript) {
	        controller.UpdateCollisionState(rigidbodyId, otherScript.GetRigidbodyId(), 1);
	    }
	}
	void OnTriggerExit(Collider other) {
	    ElectromagneticInfinitesimal otherScript = other.gameObject.GetComponent<MagneticDipoleScript>();
	    if (!otherScript) { otherScript = other.gameObject.GetComponent<StaticChargeScript>(); }
	    if (otherScript) {
	        controller.UpdateCollisionState(rigidbodyId, otherScript.GetRigidbodyId(), 0);
	    }
	}
}

public class MagneticDipoleScript : ElectromagneticInfinitesimal {
    public float strength = 300.0f;  // in units of Ampere-square meters (A m^2)
    public bool applyTorqueOnly = false;
    public float overrideColliderPoof = -1.0f;

    private Transform parentTransform;
    private Rigidbody parentRigidbody;
    private Collider parentCollider;
    private int magneticDipoleId;

	void Start () {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();

        parentTransform = gameObject.transform.parent;
        if (!parentTransform) { throw new System.Exception("MagneticDipole must be a child of a GameObject (nested in the Hierarchy view)"); }
        
        parentCollider = parentTransform.gameObject.GetComponent<Collider>();
        
        parentRigidbody = parentTransform.GetComponent<Rigidbody>();
        while (!parentRigidbody) {
            parentTransform = parentTransform.parent;
            if (!parentCollider) {
                parentCollider = parentTransform.gameObject.GetComponent<Collider>();
            }
            parentRigidbody = parentTransform.GetComponent<Rigidbody>();
            if (!parentTransform) { throw new System.Exception("MagneticDipole must be a child of a GameObject that has a Rigidbody component"); }
        }
	    
	    controller.AssignMagneticDipoleId(gameObject, parentRigidbody, parentCollider, overrideColliderPoof, applyTorqueOnly, out rigidbodyId, out magneticDipoleId);
	    
	    controller.UpdateMagneticDipole(rigidbodyId, magneticDipoleId, transform.position, transform.TransformDirection(strength * Vector3.forward));
	}
	
	void FixedUpdate() {
	    // Inform the controller about the current position and direction of this dipole.
	    controller.UpdateMagneticDipole(rigidbodyId, magneticDipoleId, transform.position, transform.TransformDirection(strength * Vector3.forward));
	}

	// To clean up the dictionaries that keep track of the dipoles (so that you can create and destroy them at will).
	~MagneticDipoleScript() {
	    controller.RemoveMagneticDipole(rigidbodyId, magneticDipoleId);
	}
}

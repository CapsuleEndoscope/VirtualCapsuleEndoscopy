using UnityEngine;
using System.Collections;

// The book-keeping for static charges is mostly a copy of the book-keeping for magnetic dipoles.  Charges are a little simpler because they don't have a direction (they're monopoles).

public class StaticChargeScript : ElectromagneticInfinitesimal {
    public float strength = 1e-8f;  // in units of Coulumbs
    public float overrideColliderPoof = -1.0f;

    private Transform parentTransform;
    private Rigidbody parentRigidbody;
    private Collider parentCollider;
    private int staticChargeId;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();

        parentTransform = gameObject.transform.parent;
        if (!parentTransform) { throw new System.Exception("StaticCharge must be a child of a GameObject (nested in the Hierarchy view)"); }
        
        parentCollider = parentTransform.gameObject.GetComponent<Collider>();
        
        parentRigidbody = parentTransform.GetComponent<Rigidbody>();
        while (!parentRigidbody) {
            parentTransform = parentTransform.parent;
            if (!parentCollider) {
                parentCollider = parentTransform.gameObject.GetComponent<Collider>();
            }
            parentRigidbody = parentTransform.GetComponent<Rigidbody>();
            if (!parentTransform) { throw new System.Exception("StaticCharge must be a child of a GameObject that has a Rigidbody component"); }
        }

        controller.AssignStaticChargeId(gameObject, parentRigidbody, parentCollider, overrideColliderPoof, out rigidbodyId, out staticChargeId);
        controller.UpdateStaticCharge(rigidbodyId, staticChargeId, transform.position, strength);
	}

	void FixedUpdate() {
	    controller.UpdateStaticCharge(rigidbodyId, staticChargeId, transform.position, strength);
	}

	~StaticChargeScript() {
	    controller.RemoveStaticCharge(rigidbodyId, staticChargeId);
	}
}

using UnityEngine;
using System.Collections;

public class PlasmaIonizedGas : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    private ElectromagneticFieldControllerScript controller;

    public float chargeOverMass = 1000.0f;  // in units of Coulumbs per kilogram
    public int maxParticles = 256;   // VERY IMPORTANT: You have to keep this in sync with the "Max Particles" in the ParticleSystem (only accessible by GUI; otherwise, I would have done it automatically).

    private ParticleSystem theParticleSystem;
    private ParticleSystem.Particle[] particles;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
        theParticleSystem = gameObject.GetComponent<ParticleSystem>();
        if (!theParticleSystem) {
            throw new System.Exception("Could not find ParticleSystem");
        }
        particles = new ParticleSystem.Particle[maxParticles];
	}
	
	void FixedUpdate() {
	    // Accelerate the particles under the action of the electric and magnetic fields by replacing the velocities.
	    
        int numParticles = theParticleSystem.GetParticles(particles);
        for (int i = 0;  i < numParticles;  i++) {
            Vector3 E = controller.ElectricField(particles[i].position);
            Vector3 B = controller.MagneticField(particles[i].position);
            particles[i].velocity += chargeOverMass * (E + Vector3.Cross(particles[i].velocity, B));
        }
        theParticleSystem.SetParticles(particles, numParticles);
	}
}

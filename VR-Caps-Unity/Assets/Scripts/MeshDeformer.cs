using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	//THE REFERENCE LINK https://catlikecoding.com/unity/tutorials/mesh-deformation/


	//Mesh component to access the mesh of the object
	Mesh deformingMesh;
	//Meshcollider component to access the collider of the object
	MeshCollider meshCollider;
	//Storing the original mesh vertices position, and keeping track of the deformed vertices
	Vector3[] originalVertices, displacedVertices;
	//Since vertices are moving during deformation, we store the velocities of each vertex
    Vector3[] vertexVelocities;
	//Spring force is used to make the object return to its original shape after deformation
    public float springForce = 20f;
	//Damping force used since the spring force keeps the vertices bouncing back and forth
    public float damping = 5f;
	//This is used to adjust the force with the scale of the object 
    float uniformScale = 1f;

	//Store the mesh and its vertices, also makes a copy of the original vertices to the displaced vertices
    void Start () {
		meshCollider = gameObject.GetComponent<MeshCollider>();
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++){
			displacedVertices[i] = originalVertices[i];
		}
        vertexVelocities = new Vector3[originalVertices.Length];
	}

	//This method gets the input variables "point" and "force" from the "MeshDeformerInput" script and applies the given "force" on the given "point"
	//It loops through the displaced vertices and applies the deforming force to each vertex
    public void AddDeformingForce (Vector3 point, float force) {
        point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++){
			AddForceToVertex(i, point, force);
		}
	}

	//Since not all vertices are deformed equally, there should be a gradual decrease of the effect on the vertices as they get farther away from the collision point
	//We get the direction and the distance of each vertex from the deforming force
    void AddForceToVertex (int i, Vector3 point, float force) {
		//Distance between vertex and collision point
		Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
		//The attenuated force is calculated by using the inverse-square law
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		//Force is converted to a velocity delta (assuming mass is 1 for each vertex)
		float velocity = attenuatedForce * Time.deltaTime;
		//Normalizing to get the dircetion
        vertexVelocities[i] += pointToVertex.normalized * velocity;

	}

	//Now that we have a velocity for each vertex, we move them
	//Updating mesh info by assigning the displaced vertices to the mesh, and recalculate normals and update mesh collider
    void Update () {
        uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
		meshCollider.sharedMesh = deformingMesh;
	}

	//The deformation is done by updating the vertices' position over time	
    void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		//The spring force is used here
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		//damping is used here to eliminate the continuous spring force
        velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}

}
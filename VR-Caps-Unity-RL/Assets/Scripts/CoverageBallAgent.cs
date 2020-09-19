using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using System.Linq;
using static System.Math;
using System.IO;
using System;

public class CoverageBallAgent : Agent
{

    // object parameters
    private Rigidbody magnet_rbody;
    private Rigidbody capsule_rbody;
    public GameObject capsule;
    public GameObject arrow;
    private Vector3 capsule_init_pos;
    private Quaternion capsule_init_rot;
    private Vector3 magnet_init_pos; 
    private Quaternion magnet_init_rot;

    // Constants for episode limits
    private const float XLIMIT_LOW = -6.0f, XLIMIT_UP =  3.5f;
    private const float YLIMIT_LOW = 10.0f, YLIMIT_UP = 15.0f;
    private const float ZLIMIT_LOW = -8.0f, ZLIMIT_UP = 3.5f;
    private const float CAPSULE_YLIMIT_LOW = 8.0f, CAPSULE_YLIMIT_UP = 12.0f;

    // other parameters
    public int speed = 5;
    private bool is_collided = false;
    private float prev_coverage = 0;
    private Vector3 action_vector;
    private int count = 0;
    private Vector3 capsule_prev_pos; 

    void Start () {
        magnet_rbody = GetComponent<Rigidbody>();
        capsule_rbody = capsule.GetComponent<Rigidbody>();
        capsule_init_pos = capsule.transform.position;
        capsule_init_rot = capsule.transform.rotation;
        magnet_init_pos = this.transform.position;
        magnet_init_rot = this.transform.rotation;
        action_vector = Vector3.zero;
        capsule_prev_pos = capsule.transform.position;
    }
    
    public override void OnEpisodeBegin()
    {   
        // Reset the scene parameters
        count = 0;
        magnet_rbody.angularVelocity = Vector3.zero;
        magnet_rbody.velocity = Vector3.zero;
        this.transform.position = magnet_init_pos;
        this.transform.rotation = magnet_init_rot;
        capsule.transform.position = capsule_init_pos;
        capsule.transform.rotation = capsule_init_rot;
        is_collided  = false;

        // reset the map parameters
        MeshMakerCoverage.visibleVerticesColon.Clear();
        arrow.GetComponent<TrailRenderer>().Clear();
        MeshMakerCoverage.mesh.Clear();
        // MeshMaker.Start();
        // MeshMaker.mesh = new Mesh();
        // GetComponent<MeshFilter>().mesh = MeshMaker.mesh;
        // GetComponent<MeshRenderer> ().material = new Material (Shader.Find("Custom/VertexColor")); 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positions
        sensor.AddObservation(this.transform.localPosition); // permanent magnet's pose data
        sensor.AddObservation(capsule.transform.position);
        sensor.AddObservation(capsule.transform.rotation); 
    }

    public override void OnActionReceived(float[] action)
    {
        // Ball magnet actions
        action_vector.x = action[0];
        action_vector.y = action[1];
        action_vector.z = action[2];
        this.transform.position += 0.01f * speed * action_vector;

        // Reached target
        float curr_coverage = MeshMakerCoverage.totalCoverageColon; 
        float reward = curr_coverage - prev_coverage;
        AddReward(reward);
        prev_coverage = curr_coverage;

        if (count % 250 == 0){
            if (Vector3.Distance(capsule_prev_pos, capsule.transform.position) < 1)
                AddReward(-0.2f);
        }

        count++;
        // Fell off platform
        if ( is_collided || (this.transform.position.x > XLIMIT_UP) || (this.transform.position.x < XLIMIT_LOW) || (this.transform.position.z > ZLIMIT_UP) || (this.transform.position.z < ZLIMIT_LOW) || (this.transform.position.y < YLIMIT_LOW) || (this.transform.position.y > YLIMIT_UP) )
        {
            EndEpisode();
        }

    }

    public void OnCollisionEnter(){
        is_collided = true;
    }

}

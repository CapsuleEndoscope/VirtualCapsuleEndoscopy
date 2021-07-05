using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using static System.Math;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEditor;

public class CoverageMagnetAgent : Agent
{
    public GameObject arrow;
    public GameObject capsule;
    public GameObject magnet;
    public Camera cam;
    private Rigidbody capsule_rbody;
    private Rigidbody magnet_rbody;
    private Vector3 capsule_init_pos;
    private Quaternion capsule_init_rot;
    private Vector3 magnet_init_pos;
    private Quaternion magnet_init_rot;
    private Vector3 end_point;
    // private float Y_UP_BOUND;
    // private float Y_LOW_BOUND;
    private float Z_UP_BOUND;
    private float Z_LOW_BOUND;
    private float X_UP_BOUND;
    private float X_LOW_BOUND;
    private float CAPSULE_UP_BOUND;
    private float CAPSULE_LOW_BOUND;
    private float SPEED_LIMIT;
    private float Y_AXIS_DISTANCE;
    private Vector3 action_move;

    void Start () {
        capsule_rbody = capsule.GetComponent<Rigidbody>();
        capsule_init_pos = capsule_rbody.transform.position;
        capsule_init_rot = capsule_rbody.transform.rotation;
        magnet_rbody = magnet.GetComponent<Rigidbody>();
        magnet_init_pos = magnet_rbody.transform.position;
        magnet_init_rot = magnet_rbody.transform.rotation;
        // Y_UP_BOUND = 14.0f;
        // Y_LOW_BOUND = 12.5f;
        X_UP_BOUND = 2f;
        X_LOW_BOUND = -5.5f;
        Z_UP_BOUND = 3.5f;
        Z_LOW_BOUND = -5.5f;
        CAPSULE_UP_BOUND = 11.3f;    
        CAPSULE_LOW_BOUND = 10.0f;
        SPEED_LIMIT = 0.35f;
        Y_AXIS_DISTANCE = 2.5f;
        action_move = Vector3.zero;
    }
    
    public override void OnEpisodeBegin()
    {   
        // reset the capsule parameters
        capsule_rbody.angularVelocity = Vector3.zero;
        capsule_rbody.velocity = Vector3.zero;
        capsule_rbody.transform.position = capsule_init_pos;
        capsule_rbody.transform.rotation = capsule_init_rot;

        // reset the magnet parameters
        magnet_rbody.angularVelocity = Vector3.zero;
        magnet_rbody.velocity = Vector3.zero;
        magnet_rbody.transform.position = magnet_init_pos;
        magnet_rbody.transform.rotation = magnet_init_rot;

        // reset the map parameters
        MeshMaker.organ_visible_vertices.Clear();
        arrow.GetComponent<TrailRenderer>().Clear();
        MeshMaker.generated_mesh.Clear();
        MeshMaker.curr_coverage = 0;
        MeshMaker.prev_coverage = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // agent positions
        sensor.AddObservation(Normalize(capsule_rbody.velocity)); // 3
        sensor.AddObservation(cam.transform.position); // 3
        sensor.AddObservation(cam.transform.rotation); // 4
        sensor.AddObservation(magnet_rbody.transform.position); // 3

        float reward = MeshMaker.diff_coverage;
        if (reward>0.03){
            AddReward(reward * 0.1f);
        }
        else{
            AddReward(-0.01f);
            Debug.Log("Negative Reward: " + reward.ToString());
        }
        Debug.Log("Reward: " + reward.ToString());
    }

    public override void OnActionReceived(float[] action)
    {

        int z_move = Mathf.FloorToInt(action[0]);
        int x_move = Mathf.FloorToInt(action[1]);
        if (z_move == 1) { action_move = Vector3.forward;}
        if (z_move == 2) { action_move = Vector3.back;}
        if (z_move == 3) { action_move = Vector3.zero;}
        if (x_move == 1) { action_move = Vector3.left;}
        if (x_move == 2) { action_move = Vector3.right;}
        if (x_move == 3) { action_move = Vector3.zero;}
        transform.Translate(action_move * Time.deltaTime);

        // just to regulate y axis
        if (magnet_rbody.position.y - capsule_rbody.position.y > Y_AXIS_DISTANCE){
            magnet_rbody.transform.Translate(Vector3.down * Time.deltaTime);
        }
        else{
            magnet_rbody.transform.Translate(Vector3.up * Time.deltaTime);
        }     

        // restart episode for undesired cases
        if (capsule_rbody.velocity.magnitude > SPEED_LIMIT){
            AddReward(-0.1f);
            EndEpisode();
        }
        if (magnet_rbody.transform.position.x > X_UP_BOUND || magnet_rbody.transform.position.x < X_LOW_BOUND){
            AddReward(-0.1f);
            EndEpisode();
        }
        if (magnet_rbody.transform.position.z > Z_UP_BOUND || magnet_rbody.transform.position.z < Z_LOW_BOUND){
            AddReward(-0.1f);
            EndEpisode();
        }
        if (capsule_rbody.transform.position.y >  CAPSULE_UP_BOUND || capsule_rbody.transform.position.y < CAPSULE_LOW_BOUND){
            AddReward(-0.1f);
            EndEpisode();
        }
    }

    public static Vector3 Normalize(Vector3 vec)
    {
        return new Vector3(Sigmoid(vec.x), Sigmoid(vec.y), Sigmoid(vec.z));
    }


    private static float Sigmoid(float val)
    {
        return 2f / (1f + Mathf.Exp(-2f * val)) - 1f;
    }
}

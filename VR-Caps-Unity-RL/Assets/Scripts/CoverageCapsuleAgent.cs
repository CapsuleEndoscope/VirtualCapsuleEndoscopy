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

public class CoverageCapsuleAgent : Agent
{
    public static double GetStandardDeviation(List<float> values)
    {
    double avg = values.Average();
    double sum = values.Sum(v => (v - avg) * (v-avg));
    double denominator = values.Count - 1;
    return denominator > 0.0 ? Sqrt(sum / denominator) : -1;
    }

    private string currentTime;
    private StreamWriter sw;
    float reward = 0.0f;
    List<float> DynamicRewardCum = new List<float>();
    float currVelo;
    float safeVelo = 2.3f, beta = 0.1f;
    public Text Score_UIText;
    // object parameters
    private Rigidbody capsule_rbody;
    // public GameObject capsule;
    public GameObject arrow;
    private Vector3 capsule_init_pos;
    private Vector3 capsule_prev_pos;
    private Quaternion capsule_init_rot;

    // private const float CAPSULE_YLIMIT_LOW = 8.0f, CAPSULE_YLIMIT_UP = 12.2f;
    private const float CAPSULE_YLIMIT_LOW = 9.5f, CAPSULE_YLIMIT_UP = 11.5f;
    // private const float CAPSULE_XLIMIT_LOW = -4.5f, CAPSULE_XLIMIT_UP = 2f;
    private const float CAPSULE_XLIMIT_LOW = -3.0f, CAPSULE_XLIMIT_UP = -1.0f;
    private const float CAPSULE_VELOCITY_MAX = 3.0f;
    private const float CAPSULE_ANGULAR_VELOCITY_MAX = 0.5f;


    int force_direction_x = 0;
    int force_direction_y = 0;
    int force_direction_z = 0;
    int torque_direction_x = 0;
    int torque_direction_y = 0;
    int torque_direction_z = 0;


    // other parameters
    private float prev_coverage = 0;
    private Vector3 action_pos_vector;
    private Vector3 action_rot_vector;
    int count = 0;
    bool fellOff = false;

    void Start () {
        capsule_rbody = this.GetComponent<Rigidbody>();
        capsule_init_pos = this.transform.position;
        capsule_init_rot = this.transform.rotation;
        capsule_prev_pos = this.transform.position;
        action_pos_vector = Vector3.zero;
        action_rot_vector = Vector3.zero; //.Set(0,0,0,0);
        capsule_prev_pos = this.transform.position;
        
        sw = new StreamWriter("CoverageRewards.csv");
        sw.Flush();
    }
    
    public override void OnEpisodeBegin()
    {   
        // Reset the scene parameters
        Debug.Log("NEW EPISODE!");
        capsule_rbody.angularVelocity = Vector3.zero;
        capsule_rbody.velocity = Vector3.zero;
        this.transform.position = capsule_init_pos;
        this.transform.rotation = capsule_init_rot;
        capsule_prev_pos = capsule_init_pos;

        // reset the map parameters
        MeshMakerCoverage.visibleVerticesColon.Clear();
        arrow.GetComponent<TrailRenderer>().Clear();

        MeshMakerCoverage.mesh.Clear();
        MeshMakerCoverage.totalCoverageColon = 0;
        prev_coverage = 0;


        // if (DynamicRewardCum.Count != 0){
        //     sw.WriteLine( DynamicRewardCum.Average().ToString() + "," + GetStandardDeviation(DynamicRewardCum).ToString() + "\n" );
        //     sw.Flush();
        //     DynamicRewardCum = new List<float>();
        // }
            
        count = 0;
        fellOff = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positions
        sensor.AddObservation(this.transform.position);
        sensor.AddObservation(this.transform.rotation);
        sensor.AddObservation(capsule_rbody.velocity.magnitude);
        // sensor.AddObservation(capsule_rbody.angularVelocity.magnitude);
    }

    public override void OnActionReceived(float[] action)
    {

        reward = 0;
        count ++;
        currentTime = Time.time.ToString("f2");
        Score_UIText.text =  "Elapsed Time [s] : " +  currentTime ;
        if (Time.time % 2.5 == 0){
            // EditorApplication.isPaused = true;
        }
        // Debug.Log("count - " + count);
        // // action parameters
        // action_pos_vector.x = action[0];
        // action_pos_vector.y = action[1];
        // action_pos_vector.z = action[2];
        // action_rot_vector.x = action[3];
        // action_rot_vector.y = action[4];
        // action_rot_vector.z = action[5];
        // // action_rot_vector.w = action[6];
        // // float speed = action[7];
        // // float angular_speed = action[8];
        // // Debug.Log(action_pos_vector.magnitude);
        // // actions

        // AddReward(-0.01f*Mathf.Max(action_pos_vector.magnitude, 0.001f));
        // AddReward(-0.01f*Mathf.Max(action_rot_vector.magnitude, 0.001f));

        
        // // Mathf.Clamp(action[1], 0, 0.5f);
        // // Mathf.Clamp(action[2], 0, 0.5f);
        // // Mathf.Clamp(action[3], 0, 0.5f);
        // // Mathf.Clamp(action[4], 0, 0.5f);
        // // Mathf.Clamp(action[5], 0, 0.5f);

        // action_pos_vector.x = Mathf.Clamp(action[0], 0, 0.001f);
        // action_pos_vector.y = Mathf.Clamp(action[1], 0, 0.001f);
        // action_pos_vector.z = Mathf.Clamp(action[2], 0, 0.001f);
        // action_rot_vector.x = Mathf.Clamp(action[3], 0, 0.001f);
        // action_rot_vector.y = Mathf.Clamp(action[4], 0, 0.001f);
        // action_rot_vector.z = Mathf.Clamp(action[5], 0, 0.001f);

        int force_x = Mathf.FloorToInt(action[0]);
        int force_y = Mathf.FloorToInt(action[1]);
        int force_z = Mathf.FloorToInt(action[2]);
        int torque_x = Mathf.FloorToInt(action[3]);
        int torque_y = Mathf.FloorToInt(action[4]);
        int torque_z = Mathf.FloorToInt(action[5]);

        if (force_x == 1) {  force_direction_x = -1; }
        if (force_x == 2) {  force_direction_x = 1; }
        if (force_x == 3) {  force_direction_x = 0; }

        if (force_y == 1) {  force_direction_y = 1; }
        if (force_y == 2) {  force_direction_y = -1; }
        if (force_y == 3) {  force_direction_y = -0; }

        if (force_z == 1) {  force_direction_z = -1; }
        if (force_z == 2) {  force_direction_z = 1; }
        if (force_z == 3) {  force_direction_z = 0; }

        if (torque_x == 1) {  torque_direction_x = -1; }
        if (torque_x == 2) {  torque_direction_x = 1; }
        if (torque_x == 3) {  torque_direction_x = 0; }

        if (torque_y == 1) {  torque_direction_y = -1; }
        if (torque_y == 2) {  torque_direction_y = 1; }
        if (torque_y == 3) {  torque_direction_y = 0; }

        if (torque_z == 1) {  torque_direction_z = -1; }
        if (torque_z == 2) {  torque_direction_z = 1; }
        if (torque_z == 3) {  torque_direction_z = 0; }


        // Debug.Log(action_pos_vector.magnitude);
        capsule_rbody.AddForce(new Vector3(force_direction_x * 0.001f, force_direction_y * 0.001f, force_direction_z * 0.001f));
        capsule_rbody.AddTorque(new Vector3(torque_direction_x * 0.001f, torque_direction_y * 0.001f, torque_direction_z* 0.001f));

        // Reached target
        float diff = MeshMakerCoverage.totalCoverageColon - prev_coverage;
        // Debug.Log(" MeshMakerCoverage.totalCoverageColon) = " + MeshMakerCoverage.totalCoverageColon);
        // AddReward(MeshMakerCoverage.totalCoverageColon/100);
        // reward += MeshMakerCoverage.totalCoverageColon/100;
        // Debug.Log("MeshMakerCoverage.totalCoverageColon/100 = " + MeshMakerCoverage.totalCoverageColon/100 );
        // Debug.Log("MeshMakerCoverage.totalCoverageColon= " + MeshMakerCoverage.totalCoverageColon); 
        // Debug.Log("prev_coverage = " +  prev_coverage);

        // AddReward(10*diff);
        // reward += 10*diff;
        // Debug.Log("10*diff= "+10*diff);
        // prev_coverage = MeshMakerCoverage.totalCoverageColon;

        // currVelo = capsule_rbody.velocity.magnitude;
        // Debug.Log("currVelo = " + currVelo);
        // if (currVelo > 0.1f){
            // Debug.Log("beta*(safeVelo - currVelo) = " + beta*(safeVelo - currVelo) );
        // AddReward(beta*(safeVelo - currVelo));
        // reward += beta*(safeVelo - currVelo);
        // Debug.Log("beta*(safeVelo - currVelo)= " + beta*(safeVelo - currVelo));

        // }
        // else{
        //     // Debug.Log("tooSlow: -0.5");
        //     AddReward(-0.01f);
        //     reward -= 0.01f;
        // }

        if (diff > 0.5){
            AddReward(0.5f);
            reward += 0.5f;
            Debug.Log("Coverage Reward");
            prev_coverage = MeshMakerCoverage.totalCoverageColon;
        }

        // if (currVelo > safeVelo){
        //     AddReward(-0.5f);
        //     reward -= 0.5f;
        //     Debug.Log("Velocity Punish");
        // }

        // if (count%500 == 0){
        //     if (diff < 1){
        //         AddReward(-0.5f);
        //         Debug.Log("Negative Reward");
        //         EndEpisode();
        //     }
        // }
        // count++;

        // Fell off platform
        
        if ( (this.transform.position.x < CAPSULE_XLIMIT_LOW) || (this.transform.position.x > CAPSULE_XLIMIT_UP) || (this.transform.position.y < CAPSULE_YLIMIT_LOW) || (this.transform.position.y > CAPSULE_YLIMIT_UP))
        {  
            fellOff = true;
            // AddReward(-0.3f);
            // reward -= 0.3f;
            // Debug.Log("Negative Reward");
            EndEpisode();
        }

        // Debug.Log("reward = " + reward);
        DynamicRewardCum.Add(reward);

        if (count % 7500 == 0 || fellOff){
            sw.WriteLine( DynamicRewardCum.Average().ToString() + "," + GetStandardDeviation(DynamicRewardCum).ToString() );
            sw.Flush();
            DynamicRewardCum = new List<float>();
        }
        

    }
}

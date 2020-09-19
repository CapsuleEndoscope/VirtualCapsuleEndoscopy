using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using System.Linq;
using static System.Math;
using System.IO;
using System;

public class BallAgent : Agent
{
    public static double GetStandardDeviation(List<float> values)
    {
        double avg = values.Average();
        double sum = values.Sum(v => (v - avg) * (v-avg));
        double denominator = values.Count - 1;
        return denominator > 0.0 ? Sqrt(sum / denominator) : -1;
    }
    private StreamWriter sw;
    private String fileName;

    Rigidbody rBody, Capsule_rBody;
    public GameObject Capsule;
    Vector3 CapsuleInitialPosition;
    Vector3 CapsulePrePos;
    float distanceToTarget;
    float distanceToCapsule;
    float capsuleInitialToTarget;
    float reward;
    float currVelo;
    float safeVelo = 2.3f;
    bool speedLimit = false;
    bool tooSlow = false;
    bool target1achieved = false;
    float alfa = 1.0f, beta = 0.01f, gamma = 0.1f;
    Quaternion CapsuleInitialRotation;
    Vector3 BallInitialPosition; 
    Quaternion BallInitialRotation;
    float XLIMIT_LOW = -5.5f, XLIMIT_UP =  3.5f;
    float YLIMIT_LOW = 10.0f, YLIMIT_UP = 15.0f;
    float ZLIMIT_LOW = -6.0f, ZLIMIT_UP = 3.5f;
    float Capsule_YLIMIT_LOW = 8.5f, Capsule_YLIMIT_UP = 11.7f;
    bool ifCollision = false;
    List<float> DynamicRewardCum = new List<float>();
    // Texture2D prevFrame;
    // SSIM ssim;
    int counter = 0;
    Vector3 Target;
    Vector3 Target1 = new Vector3(-3.0f, 10.9f, -2.2f);
    Vector3 Target2 = new Vector3(-3.14f, 9.45f, 0.53f);

    void Start () {
        rBody = GetComponent<Rigidbody>();
        Capsule_rBody = Capsule.GetComponent<Rigidbody>();
        CapsuleInitialPosition = Capsule.transform.position;
        CapsulePrePos = CapsuleInitialPosition;
        CapsuleInitialRotation = Capsule.transform.rotation;
        BallInitialPosition = this.transform.position;
        BallInitialRotation = this.transform.rotation;
        
        // prevFrame = CaptureImage();
        // ssim = new SSIM();
        fileName = "rewards.csv";
        sw = new StreamWriter(fileName);
        sw.Flush();

    }
    
    void OnCollisionEnter(){
        ifCollision = true;
    }
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin");
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.position = BallInitialPosition;
        this.transform.rotation = BallInitialRotation;
        Capsule.transform.position = CapsuleInitialPosition;
        Capsule_rBody.velocity = Vector3.zero;
        ifCollision  = false;
        target1achieved = false;
        Target = Target1;
        CapsulePrePos = CapsuleInitialPosition;
        // counter = 0;

                       
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Vector3.Distance(Capsule.transform.position, Target));
        sensor.AddObservation(this.transform.position);
        // sensor.AddObservation(this.transform.localRotation);
        sensor.AddObservation(Capsule.transform.position);
        sensor.AddObservation(Capsule_rBody.velocity.magnitude);

        // sensor.AddObservation(CaptureImage());


        // Agent velocity
        // sensor.AddObservation(rBody.velocity.x);
        // sensor.AddObservation(rBody.velocity.z);
    }


    public float speed = 1;
    public override void OnActionReceived(float[] vectorAction)
    {
        reward = 0.0f;
        counter ++;
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.y = vectorAction[1];
        controlSignal.z = vectorAction[2];
        Vector3 position;
        position = this.transform.position + 0.01f*speed*controlSignal;
        this.transform.position = position;
        // Vector3 controlSignalRotation = Vector3.zero;
        // controlSignalRotation.x = vectorAction[3];
        // controlSignalRotation.y = vectorAction[4];
        // controlSignalRotation.z = vectorAction[5];

        // rBody.AddForce(controlSignal * speed);
        // rBody.AddTorque(controlSignalRotation * speed);
        // Rewards
        // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
       
        if (Vector3.Distance(Capsule.transform.position, Target1)<0.75f)
            target1achieved = true;

        if(target1achieved && Target == Target1){
            Target = Target2;
            Debug.Log("target1achieved +0.5");
            // AddReward(0.5f);
            reward += 0.5f;
        }
            
        distanceToTarget = Vector3.Distance(Capsule.transform.position, Target);
        Debug.Log("distanceToTarget = " + distanceToTarget);
        float distanceToPrePos = Vector3.Distance(Capsule.transform.position, CapsulePrePos);
        CapsulePrePos = Capsule.transform.position;
        currVelo = Capsule_rBody.velocity.magnitude;
        // //Frame Rewards
        // if (counter % 50 == 0)
        // {
        //     Texture2D currFrame  = CaptureImage();
        //     // ImageCompare(currFrame, prevFrame);
        //     double ssimScore = ssim.Index(prevFrame,currFrame);
        //     print("SSIMReward = " + (1 - (float)ssimScore));
        //     AddReward((1 - (float)ssimScore));
        //     prevFrame = currFrame;
        // }
        // counter ++;
    
        // Reached target
        // float DynamicReward = (1 / ((distanceToTarget + 0.0000001f)));
        if(target1achieved)
            capsuleInitialToTarget = Vector3.Distance(Target1, Target2);
        else
            capsuleInitialToTarget = Vector3.Distance(Target1, CapsuleInitialPosition);
        float DynamicReward =  1 - distanceToTarget/capsuleInitialToTarget;
        Debug.Log("alfa*distanceToPrePos= " + alfa*distanceToPrePos);
        Debug.Log("beta*(currVelo - safeVelo)= " + beta*(currVelo - safeVelo));
        Debug.Log("DynamicReward =  " + DynamicReward);
        Debug.Log("gamma*(3.5f-distanceToCapsule)= "+ gamma*(3.5f-distanceToCapsule));
        
        Debug.Log( "capsuleToBallDistance= " + Vector3.Distance(Capsule.transform.position, this.transform.position));
        distanceToCapsule =  Vector3.Distance(Capsule.transform.position, this.transform.position);
        reward += (DynamicReward + alfa*distanceToPrePos - beta*(currVelo - safeVelo) + gamma*(3.5f-distanceToCapsule) );
        // float reward = DynamicReward;
        speedLimit = currVelo > 5;
        tooSlow = currVelo < 0.1;
        // AddReward(reward);
        if(tooSlow){
            Debug.Log("tooSlow -0.1");
            // AddReward(-0.1f);
            reward -= 0.1f;
        }
        if (distanceToTarget < 0.5f){
            Debug.Log("ACHIEVED! +0.75");
            // AddReward(0.75f);
            reward += 0.75f;
            SetReward(reward);
            EndEpisode();
        }
        else
        {
            Debug.Log("notAchieved -0.2");
            // AddReward(-0.2f);
            reward -= 0.2f;
        }

        // Fell off platform
        if ( speedLimit || ifCollision || (this.transform.position.x > XLIMIT_UP) || (this.transform.position.x < XLIMIT_LOW) || (this.transform.position.z > ZLIMIT_UP) || (this.transform.position.z < ZLIMIT_LOW) || (this.transform.position.y < YLIMIT_LOW) || (this.transform.position.y > YLIMIT_UP) )
        {
            Debug.Log("constraints! -0.5");
            // AddReward(-0.5f);
            reward -= 0.5f;
            SetReward(reward);
            EndEpisode();
        }
        Debug.Log("reward =  " + reward);
        DynamicRewardCum.Add(reward);
        if (counter % 1000 == 0)
            sw.WriteLine(DynamicRewardCum.Average().ToString() + "," + GetStandardDeviation(DynamicRewardCum).ToString() + "\n");
        
        SetReward(reward);
}

    // public override void Heuristic(float[] actionsOut)
    // {
    //     actionsOut[0] = Input.GetAxis("Horizontal");
    //     actionsOut[1] = Input.GetAxis("Vertical");
    // }

    // public Texture2D CaptureImage()
    // {
    //     int width = 64;
    //     int height = 64;
    //     RenderTexture rt = new RenderTexture(width, height, 24);
    //     GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().targetTexture = rt;

    //     //Create a new texture with the width and height of the screen
    //     Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
    //     GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().Render();
    //     RenderTexture.active = rt;
    //     //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
    //     texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);        
    //     GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().targetTexture = null;
    //     RenderTexture.active = null; // JC: added to avoid errors
    //     Destroy(rt);
    //     // byte[] bytes = texture.EncodeToPNG();
    //     // string filename = "1.png";
    //     // System.IO.File.WriteAllBytes(filename, bytes);
    //     // print("Size is " + texture.width + " by " + texture.height);
    //     return texture;
    // }

    // public float ImageCompare(Texture2D Image1, Texture2D Image2){
    //     float difference = 0;
    //     for(var xcoord =0; xcoord < 320; xcoord++)
    //     {   
    //         for(var ycoord = 0; ycoord < 320; ycoord++)
    //         {
    //             var pixel1 = Image1.GetPixel(xcoord,ycoord);
    //             var pixel2 = Image2.GetPixel(xcoord,ycoord);
    //             print(pixel1);
    //             // difference += pixel1.white  - pixel2.white; 
    //             // if(col != Color.white && check_color == Color.red)
    //             // {
    //             //     match++;
    //             // }
    //         }
    //     }
    //     return difference;
    // }


}

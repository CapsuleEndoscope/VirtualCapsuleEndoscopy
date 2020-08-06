using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class BallAgent : Agent
{

    Rigidbody rBody;
    public GameObject Capsule;
    Vector3 CapsuleInitialPosition;
    Quaternion CapsuleInitialRotation;
    Vector3 BallInitialPosition; 
    Quaternion BallInitialRotation;
    Texture2D prevFrame;
    SSIM ssim;
    int counter = 0;
    // Vector3 Target = new Vector3(-2.74f, 10.66f, -0.331f);
    Vector3 Target = new Vector3(-3.097f, 10.061f, 1.209f);
    public GameObject TargetObject;
    void Start () {
        rBody = GetComponent<Rigidbody>();
        CapsuleInitialPosition = Capsule.transform.position;
        CapsuleInitialRotation = Capsule.transform.rotation;
        BallInitialPosition = this.transform.position;
        BallInitialRotation = this.transform.rotation;
        TargetObject.transform.position = Target;
        prevFrame = CaptureImage();
        ssim = new SSIM();

    }
    
    public override void OnEpisodeBegin()
    {
        if ( (this.transform.position.x > 4) || (this.transform.position.x < -5) || (this.transform.position.z > 5) || (this.transform.position.z < -6) || (this.transform.position.y < 12) || (this.transform.position.y > 13) )
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            // this.transform.position = new Vector3( -1, 13, -5);
            this.transform.position = BallInitialPosition;
            this.transform.rotation = BallInitialRotation;
            // Capsule.transform.position = new Vector3( -1.19f, 9.54f, -5.21f);
            Capsule.transform.position = CapsuleInitialPosition;
            counter = 0;

            // Move the target to a new spot
            // Target.localPosition = new Vector3(Random.value * 8 - 4,
            //                                 0.5f,
            //                                 Random.value * 8 - 4);

            // Target.localPosition = new Vector3(-1.14f, 9.85f, -3.05f);
            // Target.localPosition = new Vector3(0.33f, 11.04f, -0.853f); 
        }                        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);
        sensor.AddObservation(Capsule.transform.position);

        // sensor.AddObservation(CaptureImage());


        // Agent velocity
        // sensor.AddObservation(rBody.velocity.x);
        // sensor.AddObservation(rBody.velocity.z);
    }


    public float speed = 1;
    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.y = vectorAction[1];
        controlSignal.z = vectorAction[2];
        Vector3 controlSignalRotation = Vector3.zero;
        controlSignalRotation.x = vectorAction[3];
        controlSignalRotation.y = vectorAction[4];
        controlSignalRotation.z = vectorAction[5];

        rBody.AddForce(controlSignal * speed);
        rBody.AddTorque(controlSignalRotation * speed);
        // Rewards
        // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        float distanceToTarget = Vector3.Distance(Capsule.transform.position, Target);
        // //Frame Rewards

        if (counter % 50 == 0)
        {
            Texture2D currFrame  = CaptureImage();
            // ImageCompare(currFrame, prevFrame);
            double ssimScore = ssim.Index(prevFrame,currFrame);
            print("SSIMReward = " + (1 - (float)ssimScore));
            AddReward((1 - (float)ssimScore));
            prevFrame = currFrame;
        }
        counter ++;
    
        // Reached target
        float DynamicReward = (1 / (2*(distanceToTarget + 0.0000001f)));
        AddReward(DynamicReward);
        print("DynamicReward = " + DynamicReward);
        if (distanceToTarget < 0.75f)
        {
            // SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        if ( (this.transform.position.x > 4) || (this.transform.position.x < -5) || (this.transform.position.z > 5) || (this.transform.position.z < -6) || (this.transform.position.y < 12) || (this.transform.position.y > 13)  )
        {
            EndEpisode();
        }
}

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }

    public Texture2D CaptureImage()
    {
        int width = 64;
        int height = 64;
        RenderTexture rt = new RenderTexture(width, height, 24);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().targetTexture = rt;

        //Create a new texture with the width and height of the screen
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().Render();
        RenderTexture.active = rt;
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);        
        GameObject.FindGameObjectWithTag("Player").GetComponent<Camera>().targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        // byte[] bytes = texture.EncodeToPNG();
        // string filename = "1.png";
        // System.IO.File.WriteAllBytes(filename, bytes);
        // print("Size is " + texture.width + " by " + texture.height);
        return texture;
    }

    public float ImageCompare(Texture2D Image1, Texture2D Image2){
        float difference = 0;
        for(var xcoord =0; xcoord < 320; xcoord++)
        {   
            for(var ycoord = 0; ycoord < 320; ycoord++)
            {
                var pixel1 = Image1.GetPixel(xcoord,ycoord);
                var pixel2 = Image2.GetPixel(xcoord,ycoord);
                print(pixel1);
                // difference += pixel1.white  - pixel2.white; 
                // if(col != Color.white && check_color == Color.red)
                // {
                //     match++;
                // }
            }
        }
        return difference;
    }


}

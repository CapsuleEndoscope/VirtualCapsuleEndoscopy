using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMC : MonoBehaviour
{
    //The organs model is divided into segments where orientation changes
    //This script was made based on the description from the articles referenced in the article


    //A gameobject component to reference the organ
    private GameObject Organ;
    //A refrence to the organ's script component "MotionM" which is responsible for the visual movement
    private MotionM Motion;


    //Each gameobject component below holds a segment of the organ in order
    private GameObject SI1;
    private GameObject SI2;
    private GameObject SI3;
    private GameObject SI4;
    private GameObject SI5;
    private GameObject SI6;
    private GameObject SI7;
    private GameObject SI8;
    private GameObject SI9;
    private GameObject SI10;
    private GameObject SI11;
    private GameObject SI12;
    private GameObject SI13;
    private GameObject SI14;
    private GameObject SI15;
    private GameObject SI16;
    private GameObject SI17;
    private GameObject SI18;
    private GameObject SI19;
    private GameObject SI20;
    private GameObject SI21;
    private GameObject SI22;
    private GameObject SI23;
    private GameObject SI24;
    private GameObject SI25;
    private GameObject SI26;
    private GameObject SI27;
    private GameObject SI28;
    private GameObject SI29;
    private GameObject SI30;
    private GameObject SI31;
    private GameObject SI32;
    private GameObject SI33;
    private GameObject SI34;
    private GameObject SI35;

    //Each segment has a script "PeristalticMotion", we reference it to get the direction vector,
    //and we give it the speed depending on the segment's location
    private PeristalticMotion PM1;
    private PeristalticMotion PM2;
    private PeristalticMotion PM3;
    private PeristalticMotion PM4;
    private PeristalticMotion PM5;
    private PeristalticMotion PM6;
    private PeristalticMotion PM7;
    private PeristalticMotion PM8;
    private PeristalticMotion PM9;
    private PeristalticMotion PM10;
    private PeristalticMotion PM11;
    private PeristalticMotion PM12;
    private PeristalticMotion PM13;
    private PeristalticMotion PM14;
    private PeristalticMotion PM15;
    private PeristalticMotion PM16;
    private PeristalticMotion PM17;
    private PeristalticMotion PM18;
    private PeristalticMotion PM19;
    private PeristalticMotion PM20;
    private PeristalticMotion PM21;
    private PeristalticMotion PM22;
    private PeristalticMotion PM23;
    private PeristalticMotion PM24;
    private PeristalticMotion PM25;
    private PeristalticMotion PM26;
    private PeristalticMotion PM27;
    private PeristalticMotion PM28;
    private PeristalticMotion PM29;
    private PeristalticMotion PM30;
    private PeristalticMotion PM31;
    private PeristalticMotion PM32;
    private PeristalticMotion PM33;
    private PeristalticMotion PM34;
    private PeristalticMotion PM35;


    //User choice in the (Inspector Menu) setting the velocities of propagation based on the ranges from the articles
    [Header("Velocity (cm/min):")]
    public bool RandomVelocity = true;
    [Tooltip("Velocity between Proximal Duodenum and Distal Duodenum")]
    [Range(6, 16)]
    public float D1ToD2 = 6;
    [Tooltip("Velocity between Distal Duodenum and Proximal Jejunum")]
    [Range(4, 14)]
    public float D2ToJ1 = 4;
    [Tooltip("Velocity between Proximal Jejunum to Distal Jejunum")]
    [Range(2, 13)]
    public float J1ToJ2 = 2;

    //User choice of the starting phase, also from the phases based on the articles
    [Header("Starting Phase:")]
    public bool RandomPhase = true;
    public bool PhaseI = false;
    public bool PhaseII = false;
    public bool PhaseIII = false;

    int mode = 0;

    //Duration of each phase
    [Header("Current:")]
    public string phase;
    public float seconds = 0.0f;
    float recoverySeconds = 0.0f;
    float t = 0;
    [Tooltip("Duration of Phase 1")]
    public float durationI = 0;
    [Tooltip("Duration of Phase 2")]
    public float durationII = 0;
    [Tooltip("Duration of Phase 3")]
    public float durationIII = 0;
    float recoveryDuration = 3;


    //We start with assigning the gameobjects and the scripts of the segments
    void Start()
    {
        Organ = GameObject.Find("Small Intestine");
        Motion = Organ.GetComponent<MotionM>();
        SI1 = GameObject.Find("Small Intestines");
        PM1 = SI1.GetComponent<PeristalticMotion>();
        SI2 = GameObject.Find("Small Intestines.034");
        PM2 = SI2.GetComponent<PeristalticMotion>();
        SI3 = GameObject.Find("Small Intestines.033");
        PM3 = SI3.GetComponent<PeristalticMotion>();
        SI4 = GameObject.Find("Small Intestines.032");
        PM4 = SI4.GetComponent<PeristalticMotion>();
        SI5 = GameObject.Find("Small Intestines.031");
        PM5 = SI5.GetComponent<PeristalticMotion>();
        SI6 = GameObject.Find("Small Intestines.030");
        PM6 = SI6.GetComponent<PeristalticMotion>();
        SI7 = GameObject.Find("Small Intestines.029");
        PM7 = SI7.GetComponent<PeristalticMotion>();
        SI8 = GameObject.Find("Small Intestines.028");
        PM8 = SI8.GetComponent<PeristalticMotion>();
        SI9 = GameObject.Find("Small Intestines.027");
        PM9 = SI9.GetComponent<PeristalticMotion>();
        SI10 = GameObject.Find("Small Intestines.026");
        PM10 = SI10.GetComponent<PeristalticMotion>();
        SI11 = GameObject.Find("Small Intestines.025");
        PM11 = SI11.GetComponent<PeristalticMotion>();
        SI12 = GameObject.Find("Small Intestines.024");
        PM12 = SI12.GetComponent<PeristalticMotion>();
        SI13 = GameObject.Find("Small Intestines.023");
        PM13 = SI13.GetComponent<PeristalticMotion>();
        SI14 = GameObject.Find("Small Intestines.022");
        PM14 = SI14.GetComponent<PeristalticMotion>();
        SI15 = GameObject.Find("Small Intestines.021");
        PM15 = SI15.GetComponent<PeristalticMotion>();
        SI16 = GameObject.Find("Small Intestines.020");
        PM16 = SI16.GetComponent<PeristalticMotion>();
        SI17 = GameObject.Find("Small Intestines.019");
        PM17 = SI17.GetComponent<PeristalticMotion>();
        SI18 = GameObject.Find("Small Intestines.018");
        PM18 = SI18.GetComponent<PeristalticMotion>();
        SI19 = GameObject.Find("Small Intestines.017");
        PM19 = SI19.GetComponent<PeristalticMotion>();
        SI20 = GameObject.Find("Small Intestines.016");
        PM20 = SI20.GetComponent<PeristalticMotion>();
        SI21 = GameObject.Find("Small Intestines.015");
        PM21 = SI21.GetComponent<PeristalticMotion>();
        SI22 = GameObject.Find("Small Intestines.014");
        PM22 = SI22.GetComponent<PeristalticMotion>();
        SI23 = GameObject.Find("Small Intestines.013");
        PM23 = SI23.GetComponent<PeristalticMotion>();
        SI24 = GameObject.Find("Small Intestines.012");
        PM24 = SI24.GetComponent<PeristalticMotion>();
        SI25 = GameObject.Find("Small Intestines.011");
        PM25 = SI25.GetComponent<PeristalticMotion>();
        SI26 = GameObject.Find("Small Intestines.010");
        PM26 = SI26.GetComponent<PeristalticMotion>();
        SI27 = GameObject.Find("Small Intestines.009");
        PM27 = SI27.GetComponent<PeristalticMotion>();
        SI28 = GameObject.Find("Small Intestines.008");
        PM28 = SI28.GetComponent<PeristalticMotion>();
        SI29 = GameObject.Find("Small Intestines.007");
        PM29 = SI29.GetComponent<PeristalticMotion>();
        SI30 = GameObject.Find("Small Intestines.006");
        PM30 = SI30.GetComponent<PeristalticMotion>();
        SI31 = GameObject.Find("Small Intestines.005");
        PM31 = SI31.GetComponent<PeristalticMotion>();
        SI32 = GameObject.Find("Small Intestines.004");
        PM32 = SI32.GetComponent<PeristalticMotion>();
        SI33 = GameObject.Find("Small Intestines.003");
        PM33 = SI33.GetComponent<PeristalticMotion>();
        SI34 = GameObject.Find("Small Intestines.002");
        PM34 = SI34.GetComponent<PeristalticMotion>();
        SI35 = GameObject.Find("Small Intestines.001");
        PM35 = SI35.GetComponent<PeristalticMotion>();

        //If the user chooses the "random" option for velocities
        if (RandomVelocity == true){
            D1ToD2 = Random.Range(6.0f, 16.0f);
            D2ToJ1 = Random.Range(4.0f, 14.0f);
            J1ToJ2 = Random.Range(2.0f, 13.0f);
        }

        //Assigning the velocities to the segments 
        PM1.speed = D1ToD2;
        PM2.speed = D1ToD2;
        PM3.speed = D1ToD2;
        PM4.speed = D1ToD2;
        PM5.speed = D1ToD2;
        PM6.speed = D1ToD2;
        PM7.speed = D1ToD2;
        PM8.speed = D1ToD2;
        PM9.speed = D1ToD2;
        PM10.speed = D2ToJ1;
        PM11.speed = D2ToJ1;
        PM12.speed = D2ToJ1;
        PM13.speed = D2ToJ1;
        PM14.speed = D2ToJ1;
        PM15.speed = D2ToJ1;
        PM16.speed = D2ToJ1;
        PM17.speed = D2ToJ1;
        PM18.speed = D2ToJ1;
        PM19.speed = D2ToJ1;
        PM20.speed = D2ToJ1;
        PM21.speed = D2ToJ1;
        PM22.speed = D2ToJ1;
        PM23.speed = J1ToJ2;
        PM24.speed = J1ToJ2;
        PM25.speed = J1ToJ2;
        PM26.speed = J1ToJ2;
        PM27.speed = J1ToJ2;
        PM28.speed = J1ToJ2;
        PM29.speed = J1ToJ2;
        PM30.speed = J1ToJ2;
        PM31.speed = J1ToJ2;
        PM32.speed = J1ToJ2;
        PM33.speed = J1ToJ2;
        PM34.speed = J1ToJ2;
        PM35.speed = J1ToJ2;

        //Disabling the scripts in each segment
        Motion.enabled = false;
        PM1.enabled = false;
        PM2.enabled = false;
        PM3.enabled = false;
        PM4.enabled = false;
        PM5.enabled = false;
        PM6.enabled = false;
        PM7.enabled = false;
        PM8.enabled = false;
        PM9.enabled = false;
        PM10.enabled = false;
        PM11.enabled = false;
        PM12.enabled = false;
        PM13.enabled = false;
        PM14.enabled = false;
        PM15.enabled = false;
        PM16.enabled = false;
        PM17.enabled = false;
        PM18.enabled = false;
        PM19.enabled = false;
        PM20.enabled = false;
        PM21.enabled = false;
        PM22.enabled = false;
        PM23.enabled = false;
        PM24.enabled = false;
        PM25.enabled = false;
        PM26.enabled = false;
        PM27.enabled = false;
        PM28.enabled = false;
        PM29.enabled = false;
        PM30.enabled = false;
        PM31.enabled = false;
        PM32.enabled = false;
        PM33.enabled = false;
        PM34.enabled = false;
        PM35.enabled = false;


        //If user defines the phase or chooses "random" and based on it initalize the phase and assign a duration
        if (PhaseI == false && PhaseII == false && PhaseIII == false && RandomPhase == true)
        {
            RandomPhase = true;
            mode = Random.Range(1, 4);
            if (mode == 1)
            {
                durationI = Random.Range(30, 41);
            }
            else if (mode == 2)
            {
                durationII = Random.Range(15, 20);
            }
            else if (mode == 3)
            {
                durationIII = Random.Range(3, 9);
            }
        }
        else if (RandomPhase == false && PhaseII == false && PhaseIII == false && PhaseI == true)
        {
            mode = 1;
            durationI = Random.Range(30, 41);
        }
        else if (RandomPhase == false && PhaseI == false && PhaseIII == false && PhaseII == true)
        {
            mode = 2;
            durationII = Random.Range(15, 20);
        }
        else if (RandomPhase == false && PhaseI == false && PhaseII == false && PhaseIII == true)
        {
            mode = 3;
            durationIII = Random.Range(3, 9);

        }
    }

    
    // According to the current phase call the coressponding method
    void Update()
    {
        if (mode == 1)
        {
            phase = "Phase I";
            Phase1();
        }
        else if (mode == 2)
        {
            phase = "Phase II";
            Phase2();
        }
        else if (mode == 3)
        {
            phase = "Phase III";
            Phase3();
        }
        else if (mode == 4)
        {
            phase = "Recovery";
            recoveryPhase();
        }

    }

    //In the first phase there is no motion, so we disable the scripts in the segments throughout the duration
    //set for the first phase, then call the method "checker"
    void Phase1()
    {
        Motion.enabled = false;
        PM1.enabled = false;
        PM2.enabled = false;
        PM3.enabled = false;
        PM4.enabled = false;
        PM5.enabled = false;
        PM6.enabled = false;
        PM7.enabled = false;
        PM8.enabled = false;
        PM9.enabled = false;
        PM10.enabled = false;
        PM11.enabled = false;
        PM12.enabled = false;
        PM13.enabled = false;
        PM14.enabled = false;
        PM15.enabled = false;
        PM16.enabled = false;
        PM17.enabled = false;
        PM18.enabled = false;
        PM19.enabled = false;
        PM20.enabled = false;
        PM21.enabled = false;
        PM22.enabled = false;
        PM23.enabled = false;
        PM24.enabled = false;
        PM25.enabled = false;
        PM26.enabled = false;
        PM27.enabled = false;
        PM28.enabled = false;
        PM29.enabled = false;
        PM30.enabled = false;
        PM31.enabled = false;
        PM32.enabled = false;
        PM33.enabled = false;
        PM34.enabled = false;
        PM35.enabled = false;
        
        seconds += Time.deltaTime;
        checker();
    }

    //In the second phase the scripts are enabled
    //Propagation velocity, visual motion scale, and visual motion speed starts increasing gradually throughout the duration
    //that is set for the second pahse, and then call the method "checker"
    void Phase2()
    {
        Motion.enabled = true;
        PM1.enabled = true;
        PM2.enabled = true;
        PM3.enabled = true;
        PM4.enabled = true;
        PM5.enabled = true;
        PM6.enabled = true;
        PM7.enabled = true;
        PM8.enabled = true;
        PM9.enabled = true;
        PM10.enabled = true;
        PM11.enabled = true;
        PM12.enabled = true;
        PM13.enabled = true;
        PM14.enabled = true;
        PM15.enabled = true;
        PM16.enabled = true;
        PM17.enabled = true;
        PM18.enabled = true;
        PM19.enabled = true;
        PM20.enabled = true;
        PM21.enabled = true;
        PM22.enabled = true;
        PM23.enabled = true;
        PM24.enabled = true;
        PM25.enabled = true;
        PM26.enabled = true;
        PM27.enabled = true;
        PM28.enabled = true;
        PM29.enabled = true;
        PM30.enabled = true;
        PM31.enabled = true;
        PM32.enabled = true;
        PM33.enabled = true;
        PM34.enabled = true;
        PM35.enabled = true;
        seconds += Time.deltaTime;
        t += Time.deltaTime / durationII;
        Motion.scale = Mathf.Lerp(0.0f, 1.5f, t);
        Motion.speed = Mathf.Lerp(0.0f, 2.0f, t);
        PM1.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM2.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM3.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM4.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM5.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM6.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM7.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM8.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM9.speed = Mathf.Lerp(0.0f,D1ToD2, t);
        PM10.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM11.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM12.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM13.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM14.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM15.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM16.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM17.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM18.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM19.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM20.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM21.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM22.speed = Mathf.Lerp(0.0f,D2ToJ1, t);
        PM23.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM24.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM25.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM26.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM27.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM28.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM29.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM30.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM31.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM32.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM33.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM34.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        PM35.speed = Mathf.Lerp(0.0f,J1ToJ2, t);
        checker();
    }

    //In phase 3 all motions are enabled and run throught the duration set for phase 3, and then call "checker"
    void Phase3()
    {
        seconds += Time.deltaTime;
        Motion.enabled = true;
        Motion.scale = 1.5f;
        Motion.speed = 2.0f;
        PM1.speed = D1ToD2;
        PM2.speed = D1ToD2;
        PM3.speed = D1ToD2;
        PM4.speed = D1ToD2;
        PM5.speed = D1ToD2;
        PM6.speed = D1ToD2;
        PM7.speed = D1ToD2;
        PM8.speed = D1ToD2;
        PM9.speed = D1ToD2;
        PM10.speed = D2ToJ1;
        PM11.speed = D2ToJ1;
        PM12.speed = D2ToJ1;
        PM13.speed = D2ToJ1;
        PM14.speed = D2ToJ1;
        PM15.speed = D2ToJ1;
        PM16.speed = D2ToJ1;
        PM17.speed = D2ToJ1;
        PM18.speed = D2ToJ1;
        PM19.speed = D2ToJ1;
        PM20.speed = D2ToJ1;
        PM21.speed = D2ToJ1;
        PM22.speed = D2ToJ1;
        PM23.speed = J1ToJ2;
        PM24.speed = J1ToJ2;
        PM25.speed = J1ToJ2;
        PM26.speed = J1ToJ2;
        PM27.speed = J1ToJ2;
        PM28.speed = J1ToJ2;
        PM29.speed = J1ToJ2;
        PM30.speed = J1ToJ2;
        PM31.speed = J1ToJ2;
        PM32.speed = J1ToJ2;
        PM33.speed = J1ToJ2;
        PM34.speed = J1ToJ2;
        PM35.speed = J1ToJ2;
        checker();
    }

    //Recovery phase is described as the phase where the motions starts gradually decreasing, ending the cycle to start phase 1 all over again
    //All values decrease gradually and the organs mesh return to its original position too throughout the duration, then call "checker"
    void recoveryPhase()
    {
        float dec = 0.5f;
        if (Motion.scale > 0)
        {
            Motion.scale -= Time.deltaTime * dec;
            Motion.speed -= Time.deltaTime * dec;
            PM1.speed -= Time.deltaTime * dec;
            PM2.speed -= Time.deltaTime * dec;
            PM3.speed -= Time.deltaTime * dec;
            PM4.speed -= Time.deltaTime * dec;
            PM5.speed -= Time.deltaTime * dec;
            PM6.speed -= Time.deltaTime * dec;
            PM7.speed -= Time.deltaTime * dec;
            PM8.speed -= Time.deltaTime * dec;
            PM9.speed -= Time.deltaTime * dec;
            PM10.speed -= Time.deltaTime * dec;
            PM11.speed -= Time.deltaTime * dec;
            PM12.speed -= Time.deltaTime * dec;
            PM13.speed -= Time.deltaTime * dec;
            PM14.speed -= Time.deltaTime * dec;
            PM15.speed -= Time.deltaTime * dec;
            PM16.speed -= Time.deltaTime * dec;
            PM17.speed -= Time.deltaTime * dec;
            PM18.speed -= Time.deltaTime * dec;
            PM19.speed -= Time.deltaTime * dec;
            PM20.speed -= Time.deltaTime * dec;
            PM21.speed -= Time.deltaTime * dec;
            PM22.speed -= Time.deltaTime * dec;
            PM23.speed -= Time.deltaTime * dec;
            PM24.speed -= Time.deltaTime * dec;
            PM25.speed -= Time.deltaTime * dec;
            PM26.speed -= Time.deltaTime * dec;
            PM27.speed -= Time.deltaTime * dec;
            PM28.speed -= Time.deltaTime * dec;
            PM29.speed -= Time.deltaTime * dec;
            PM30.speed -= Time.deltaTime * dec;
            PM31.speed -= Time.deltaTime * dec;
            PM32.speed -= Time.deltaTime * dec;
            PM33.speed -= Time.deltaTime * dec;
            PM34.speed -= Time.deltaTime * dec;
            PM35.speed -= Time.deltaTime * dec;
        }
        checker();
    }


    //This method checks the phase, if the duration is over, it moves to the next phase
    void checker()
    {
        if (mode == 1)
        {
            if (seconds >= durationI)
            {
                mode = 2;
                seconds = 0;
                t = 0;
                if (durationII == 0)
                {
                    durationII = Random.Range(15, 20);
                }
            }
        }
        else if (mode == 2)
        {
            if (seconds >= durationII)
            {
                mode = 3;
                seconds = 0;
                t = 0;
                if (durationIII == 0)
                {
                    durationIII = Random.Range(3, 9);
                }
            }
        }
        else if (mode == 3)
        {
            if (seconds >= durationIII)
            {
                mode = 4;
                recoverySeconds = 0;
                t = 0;
            }
        }
        else if (mode == 4)
        {
            if (Motion.scale <= 0.0f)
            {
                mode = 1;
                seconds = 0;
                t = 0;
                if (durationI == 0)
                {
                    durationI = Random.Range(30, 41);
                }
            }
        }
    }
}
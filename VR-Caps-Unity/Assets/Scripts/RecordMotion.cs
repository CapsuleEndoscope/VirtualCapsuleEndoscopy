using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class RecordMotion : MonoBehaviour
{
    private Rigidbody rb;
    private StreamWriter sw;
    private String fileName;
    private float fire_start_time;

    void Start ()
    {
        fire_start_time = Time.time;
        rb = GetComponent<Rigidbody>();
        fileName = "position_rotation.csv";
        sw = new StreamWriter(fileName);
        sw.WriteLine ("tX,tY,tZ,rX,rY,rZ,rW,time(s)");
        sw.Flush();
    }
    void FixedUpdate()
    {
        sw.WriteLine(
                     transform.position.x.ToString() + "," +
                     transform.position.y.ToString() + "," +
                     transform.position.z.ToString() + "," +
                     transform.rotation.x.ToString() + "," +
                     transform.rotation.y.ToString() + "," +
                     transform.rotation.z.ToString() + "," +
                     transform.rotation.w.ToString() + "," +
                     (Time.time - fire_start_time).ToString()
                     );

    }


}

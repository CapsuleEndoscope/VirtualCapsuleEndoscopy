using System;
using System.Collections.Generic;
using UnityEngine;

public class MagLev : MonoBehaviour
{
    [Header("Front")]
    public Magnet FL;
    public Magnet FM;
    public Magnet FR;

    [Header("Middle")]
    public Magnet ML;
    public Magnet MM;
    public Magnet MR;

    [Header("Back")]
    public Magnet BL;
    public Magnet BM;
    public Magnet BR;

    [Header("Engine")]
    public float DefaultPower;
    public float BalancePower;
    public float DrivePower;

    public GameObject MagTrack;

    [Flags]
    public enum MagnetWheel
    {
        None = 0,
        FL = 1,
        FM = 2,
        FR = 4,
        ML = 8,
        MM = 16,
        MR = 32,
        BL = 64,
        BM = 128,
        BR = 256,
        Front = FL | FM | FR,
        Left = FL | ML | BL,
        Right = FR | MR | BR,
        Back = BL | BM | BR,
        Outside = Front | Left | Right | Back
    }

    // Use this for initialization
    void Start()
    {
        SetPower(MagnetWheel.Outside, DefaultPower);
        SetPower(MagnetWheel.MM, BalancePower);
    }

    void SetPower(MagnetWheel wheels, float power)
    {
        if((wheels & MagnetWheel.FL) != 0)
            FL.MagnetForce = power;
        if ((wheels & MagnetWheel.FM) != 0)
            FM.MagnetForce = power;
        if ((wheels & MagnetWheel.FR) != 0)
            FR.MagnetForce = power;

        if ((wheels & MagnetWheel.ML) != 0)
            ML.MagnetForce = power;
        if ((wheels & MagnetWheel.MM) != 0)
            MM.MagnetForce = power;
        if ((wheels & MagnetWheel.MR) != 0)
            MR.MagnetForce = power;

        if ((wheels & MagnetWheel.BL) != 0)
            BL.MagnetForce = power;
        if ((wheels & MagnetWheel.BM) != 0)
            BM.MagnetForce = power;
        if ((wheels & MagnetWheel.BR) != 0)
            BR.MagnetForce = power;
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        pos.y = 0.0f;
        MagTrack.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        SetPower(MagnetWheel.Outside, DefaultPower);
        SetPower(MagnetWheel.MM, BalancePower);

        if (Input.GetKey(KeyCode.UpArrow))
        {
            SetPower(MagnetWheel.Front, DrivePower);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            SetPower(MagnetWheel.Left, DrivePower);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            SetPower(MagnetWheel.Right, DrivePower);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            SetPower(MagnetWheel.Back, DrivePower);
        }
    }
}

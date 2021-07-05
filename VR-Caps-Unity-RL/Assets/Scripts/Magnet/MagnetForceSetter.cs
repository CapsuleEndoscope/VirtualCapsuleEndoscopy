using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetForceSetter : MonoBehaviour
{
    public float MagnetForce;
    Magnet[] m_magnets;

    // Use this for initialization
    void Start()
    {
        m_magnets = GetComponentsInChildren<Magnet>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < m_magnets.Length; i++)
        {
            m_magnets[i].MagnetForce = MagnetForce;
        }
    }
}

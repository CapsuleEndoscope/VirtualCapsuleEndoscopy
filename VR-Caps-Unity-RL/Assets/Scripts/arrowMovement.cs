using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Capsule;
    public Camera cam;
    Vector3 capsulePos, updatedCapsulePos;
    void Start()
    {
        capsulePos = Capsule.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        updatedCapsulePos = Capsule.transform.position;
        Vector3 offset = updatedCapsulePos - capsulePos;
        transform.position += offset;
        transform.rotation = Quaternion.Euler(90,cam.transform.eulerAngles.y,cam.transform.eulerAngles.z);
        capsulePos = updatedCapsulePos;
    }
}

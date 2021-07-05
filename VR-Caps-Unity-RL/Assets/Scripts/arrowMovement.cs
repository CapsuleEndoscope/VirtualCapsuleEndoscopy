using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
	// variables
    public GameObject capsule;
    public Camera cam;
    Vector3 capsule_pos;

    // Start is called before the first frame update
    void Start()
    {
        capsule_pos = capsule.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 updated_capsule_pos = capsule.transform.position;
        Vector3 offset = updated_capsule_pos - capsule_pos;
        transform.position += offset;
        transform.rotation = Quaternion.Euler(90, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);
        capsule_pos = updated_capsule_pos;
    }
}

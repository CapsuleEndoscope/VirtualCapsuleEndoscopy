using System;
using UnityEngine;
using System.Collections;

public class MotionM : MonoBehaviour
{

    //Read "MMC" script first
    
    //This script is responsible for the visual motion

  //Scale and speed of the motion adjustable in the Inspector Menu
  public float scale = 0.0f;
  public float speed = 0f;
  //public float noiseStrength = 0f;
  //public float noiseWalk = 0f;

  private Vector3[] baseHeight;
     
  //Read Mesh and MeshCollider every frame
  void Update () {
    Mesh mesh = GetComponent<MeshFilter> ().mesh;
    MeshCollider collider = GetComponent<MeshCollider>();

    //Copy mesh vertices into "baseheight" if not done already
    if (baseHeight == null)
      baseHeight = mesh.vertices;

    //A temporary list to place changed vertices
    Vector3[] vertices = new Vector3[baseHeight.Length];
    for (int i=0;i<vertices.Length;i++)
    {
      Vector3 vertex = baseHeight[i];
      
      //Applying a Sine Wave movement to each vertex on its x,y,z components based on scale and speed
      vertex.y += Mathf.Sin(Time.time * speed + baseHeight[i].x + baseHeight[i].y + baseHeight[i].z) * scale;
      
      //Noise (NOT USED)
      //vertex.y += Mathf.PerlinNoise(baseHeight[i].x + noiseWalk, baseHeight[i].y + Mathf.Sin(Time.time * 0.1f)) * noiseStrength;

      vertices[i] = vertex;
    }

    //Applying changes to the mesh vertices and recalculating normals and updating mesh collider
    mesh.vertices = vertices;
    mesh.RecalculateNormals();
    collider.sharedMesh = null;
    collider.sharedMesh = mesh;

    
  }
}
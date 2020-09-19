/*
using UnityEngine;

public class RayCast : MonoBehaviour
{
    void FixedUpdate()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100.0f))
            print("Found an object - distance: " + hit.distance);
    }
}
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMaker : MonoBehaviour {
    Vector3[] vertices;
    public float ze;
    public Camera cam;
    private Mesh mesh;
    private Mesh ColonMesh; //, IntestineMesh, StomachMesh;
    // private Mesh OrgansMesh;
    // public GameObject Organs;
    // public GameObject Stomach;
    // public GameObject Intestine;
    public GameObject Organ;
    HashSet<Vector3> visibleVerticesStomach, visibleVerticesSmallIntestine, visibleVerticesColon;
    private GUIStyle guiStyle = new GUIStyle(); //create a new variable
    public float totalCoverageStomach, totalCoverageSmallIntestine, totalCoverageColon = 0.0f;
    public Text Score_UIText;
    bool isPaused = false;
    // const int TOTALVERTICES = 7565241;
    const int STOMACHVERTICES = 597455;
    const int SMALLINTESTINEVERTICES = 2720822;
    const int COLONVERTICES = 4246964;
    

    void Start(){

        mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshRenderer> ().material = new Material (Shader.Find("Custom/VertexColor"));  
        
        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;

        visibleVerticesStomach = new HashSet<Vector3>();
        visibleVerticesSmallIntestine = new HashSet<Vector3>();
        visibleVerticesColon = new HashSet<Vector3>();
    }
    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus){
        isPaused = pauseStatus;
    }
        
    void Update(){

        // if(isPaused){
        //     Debug.Log("PAUSED");
        //     ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        //     vertices = ColonMesh.vertices;
        // }

        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;
        // int[] triangles = ColonMesh.triangles;


        List<int> visibleTriIndecies = new List<int>();

        //--find out the visible vertices
        Vector3 visibleVertex = new Vector3();
        
        for(int i=0; i < vertices.Length;i+=1)
        {
            // to avoid point behind the camera.
            var worldPoint = Organ.transform.TransformPoint(vertices[i]);
            if (Vector3.Dot(cam.transform.forward, worldPoint-cam.transform.position) >= 0){
                // visibleVertex = cam.WorldToScreenPoint(Colon.transform.TransformPoint(vertices[i]));
                visibleVertex = cam.WorldToViewportPoint(Organ.transform.TransformPoint(vertices[i]));
                //---if vertex is visible
                // if(visibleVertex.x <= Screen.width && visibleVertex.x >= 0 && visibleVertex.y >= 0 && visibleVertex.y <= Screen.height)
                if(visibleVertex.x <= 1 && visibleVertex.x >= 0 && visibleVertex.y >= 0 && visibleVertex.y <= 1 && visibleVertex.z > 0  && visibleVertex.z <= ze )
                // if (Rect(0, 0, Screen.width, Screen.height).Contains(visibleVertex))
                {   
                    // Debug.Log("vertices[i] = " + vertices[i]);
                    if(Organ.name == "Stomach"){
                        visibleVerticesStomach.Add(vertices[i]);
                        totalCoverageStomach = 75000*visibleVerticesStomach.Count/STOMACHVERTICES;
                    }
                    else if(Organ.name == "Small Intestines"){
                        visibleVerticesSmallIntestine.Add(vertices[i]);
                        totalCoverageSmallIntestine = 75000*visibleVerticesSmallIntestine.Count/SMALLINTESTINEVERTICES;
                    }
                    else{
                        visibleVerticesColon.Add(vertices[i]);
                        totalCoverageColon = 1*visibleVerticesColon.Count/COLONVERTICES;
                    }

                    Score_UIText.text = 
                    "Stomach Coverage: " +  totalCoverageStomach.ToString() + " % \n" +
                    "Intestine Coverage: " +  totalCoverageSmallIntestine.ToString() + " % \n" +
                    "Colon Coverage: " +  totalCoverageColon.ToString() + " %";
                }
            }
        }

        
        // if(visibleVerticesStomach.Count != 0)
        //     CreateMesh(visibleVerticesStomach);
        // if(visibleVerticesSmallIntestine.Count != 0)
        //     CreateMesh(visibleVerticesSmallIntestine);
        // if(visibleVerticesColon.Count != 0)
        //     CreateMesh(visibleVerticesColon);
        visibleVerticesStomach.UnionWith(visibleVerticesSmallIntestine);
        visibleVerticesStomach.UnionWith(visibleVerticesColon);
        CreateMesh(visibleVerticesStomach);
    }

	void CreateMesh(HashSet<Vector3> visibleVertices) {
        int numPoints = visibleVertices.Count;
        // Debug.Log( "numPoints= " + numPoints);
		Vector3[] points = new Vector3[numPoints];
		int[] indecies = new int[numPoints];
		Color[] colors = new Color[numPoints];

		// int max = 10; 
		// int min = -10;
		// for(int i=0;i<points.Length;++i) {
        int i = 0;
        foreach(Vector3 element in visibleVertices){
            
			// int x = Random.Range (min, max);
            float x = element.x+3;
			// int y = Random.Range (min, max);
            float y = element.y+3;
			// int z = Random.Range (min, max);
            float z = element.z-7;
			points[i] = new Vector3(x, y, z);
			indecies[i] = i;
			// float value = (float)1.0 * (((float)y - (float)min) / ((float)max - (float)min));

			colors[i] = new Color(0.2f,0.7f,0,1.0f);
            i ++;
		}

		mesh.vertices = points;
		mesh.colors = colors;
		mesh.SetIndices(indecies, MeshTopology.Points,0);

	}





    // void Start(){
        // clothMesh = Instantiate(GetComponent<MeshFilter>().sharedMesh);

        // Vector3[] vertices = clothMesh.vertices;
        // int[] triangles = clothMesh.triangles;

            
        // List<Vector3> visibleVertices = new List<Vector3>();
        // List<int> visibleTriIndecies = new List<int>();

        // //--find out the visible vertices
        // Vector3 visibleVertex = new Vector3();

        // for(int i=0; i < vertices.Length;i++)
        // {
        //     visibleVertex = cam.WorldToScreenPoint(transform.TransformPoint( vertices[i]));

        //     //---if vertex is visible
        //     if(visibleVertex.x <= cam.pixelWidth && visibleVertex.x >= 0 && visibleVertex.y >= 0 && visibleVertex.y <= cam.pixelHeight)
        //     {
        //         visibleVertices.Add(vertices[i]);
                
        //     }
        // }

        // //---find out the vertex index of visible vertices
        // for (int i = 0; i < triangles.Length; i+=3)
        // {
        //     foreach(Vector3 vpoint in visibleVertices)
        //     {
        //         if(vpoint == vertices[triangles[i]])
        //         {
        //             visibleTriIndecies.Add(triangles[i]);
        //             visibleTriIndecies.Add(triangles[i+1]);
        //             visibleTriIndecies.Add(triangles[i+2]);
        //         }
        //     }
        // }

        // Mesh mesh =  clothMesh;
        // int[] newtris = mesh.triangles;

        // for(int i = 0; i < visibleTriIndecies.Count; i+=3)
        // {
        //     for(int j = 0; j < newtris.Length; j+=3)
        //     {
        //         if(newtris[j] == visibleTriIndecies[i] && newtris[j + 1] == visibleTriIndecies[i + 1] && newtris[j + 2] == visibleTriIndecies[i + 2])
        //         {
        //                         newtris[j] = 0;
        //                         newtris[j + 1] = 0;
        //                         newtris[j + 2] = 0;
        //         }
        //     }
        // }


        // mesh.triangles = newtris;

        // Debug.Log("Vertex count: " + visibleVertices.Count);



        // SaveMeshAsObj(mesh);
        // Destroy(mesh);
 
    // }
}




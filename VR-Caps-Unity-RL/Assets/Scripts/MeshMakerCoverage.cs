using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMakerCoverage : MonoBehaviour {
    Vector3[] vertices;
    public float ze;
    public Camera cam;
    public static Mesh mesh;
    private Mesh ColonMesh; //, IntestineMesh, StomachMesh;
    // private Mesh OrgansMesh;
    // public GameObject Organs;
    // public GameObject Stomach;
    // public GameObject Intestine;
    public GameObject Organ;
    public static HashSet<Vector3> visibleVerticesColon;
    // List<Vector3> visibleVerticesColonList = new List<Vector3>(24822);
    int[] visibleVerticesColonList = new int[24822];
    List<int> visibleTriIndecies = new List<int>();
    private GUIStyle guiStyle = new GUIStyle(); //create a new variable
    public static float totalCoverageColon = 0.0f;
    public Text Score_UIText;
    // const int TOTALVERTICES = 7565241;
    // const float COLONVERTICES = 11556f; //GI_low_connected -> colon
    const float COLONVERTICES = 24822f; // GI_2_high_connected ->stomach
    Plane[] planes;
    int count;

    Mesh targetmesh;

    int[] newTriangles;// = new int[49047];
    public void Start(){

        mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshRenderer> ().material = new Material (Shader.Find("Custom/VertexColor"));  
        
        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;
   
        visibleVerticesColon = new HashSet<Vector3>();

        targetmesh = Organ.GetComponent<MeshFilter>().mesh;
        targetmesh.vertices = ColonMesh.vertices;
        targetmesh.normals = ColonMesh.normals;
        targetmesh.uv = ColonMesh.uv;
        targetmesh.triangles = ColonMesh.triangles;
        targetmesh.tangents = ColonMesh.tangents;

    }
        
    void Update(){

        // if(isPaused){
        //     Debug.Log("PAUSED");
        //     ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        //     vertices = ColonMesh.vertices;
        // }

        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;

        planes = GeometryUtility.CalculateFrustumPlanes(cam);

        
        // Debug.Log(triangles.Length);
    
        //--find out the visible vertices
        Vector3 visibleVertex = new Vector3();
        // Vector3 inVisibleVertex = new Vector3();
        int j = 0;
        for(int i=0; i < visibleVerticesColonList.Length;i+=1){
            visibleVerticesColonList[i] = 0;
        }

        
        for(int i=0; i < vertices.Length;i+=1)
        {
            // to avoid point behind the camera.
            var worldPoint = Organ.transform.TransformPoint(vertices[i]);
            if (Vector3.Dot(cam.transform.forward, worldPoint-cam.transform.position) >= 0){

                // visibleVertex = cam.WorldToScreenPoint(Colon.transform.TransformPoint(vertices[i]));
                visibleVertex = cam.WorldToViewportPoint(Organ.transform.TransformPoint(vertices[i]));
                //---if vertex is visible
                if(visibleVertex.x <= 1 && visibleVertex.x >= 0 && visibleVertex.y >= 0 && visibleVertex.y <= 1 && visibleVertex.z > 0  && visibleVertex.z <= ze )
                {   
                    visibleVerticesColon.Add(vertices[i]);
                    visibleVerticesColonList[j] = i;
                    // visibleVerticesColonList.Add(i);
                    // visibleTriIndecies.Add(triangles[j]);
                    // newTriangles[j] = triangles[j];
                    // newTriangles[j+1] = triangles[j+1];
                    // newTriangles[j+2] = triangles[j+2];

                    totalCoverageColon = 100*(visibleVerticesColon.Count/COLONVERTICES);
                    // Debug.Log(totalCoverageColon);
                    Score_UIText.text =  "Coverage: " +  totalCoverageColon.ToString() + " %";
                    j ++;
                }     
            }
            
        }



        // //---find out the vertex index of visible vertices
        // for (int i = 0; i < triangles.Length; i+=3)
        // {
        //     foreach(Vector3 vpoint in visibleVerticesColon)
        //     {
        //         if(vpoint == vertices[triangles[i]])
        //         {
        //             visibleTriIndecies.Add(triangles[i]);
        //             visibleTriIndecies.Add(triangles[i+1]);
        //             visibleTriIndecies.Add(triangles[i+2]);
        //         }
        //     }
        // }

    // Debug.Log("triangles.Length = "+triangles.Length);
    // Debug.Log("visibleVerticesColonList.Length = "+visibleVerticesColonList.Length);
    // for(int i=0; i < triangles.Length;i+=1){
    //     if(visibleVerticesColonList.Contains(triangles[i]))
    //     {
    //         if (visibleVerticesColonList[triangles[i]] == vertices[triangles[i]]){
    //             visibleTriIndecies.Add(triangles[i]);
    //         }
    //     }
    // }

        // mesh.colors = colors2;
        // targetmesh.triangles = newTriangles;
        CreateMesh(visibleVerticesColon);
        // Exportable.GetComponent<MeshFilter>().mesh = targetmesh;
        // count = 0;
        //  foreach (Plane plane in planes) {
        //      foreach (Vector3 vertex in vertices) {
        //          if (!(plane.GetDistanceToPoint(Organ.transform.TransformPoint(vertex)) < 0)) {
        //              count++;
        //          }
        //      }
        //  }
        // print("count = " + count/COLONVERTICES);

        // var takeHiResShot = Input.GetKeyDown("l");
        // if (takeHiResShot)
        // {

            // var object = new GameObject("Empty", MeshFilter, MeshRenderer);
            // object.GetComponent(MeshFilter).mesh = mesh;
            // AssetDatabase.CreateAsset( mesh, "mesh.asset");
            
            // go = new GameObject("Empty");
            // go.AddComponent<MeshFilter>();
            // go.AddComponent<MeshRenderer>();
            // go.GetComponent<MeshFilter>().mesh = mesh;


            // UnityFBXExporter.FBXExporter.ExportGameObjToFBX(go,"exported1.fbx",false,false);
        // }


    }

	void CreateMesh(HashSet<Vector3> visibleVertices) {
        int numPoints = visibleVertices.Count;
        // Debug.Log( "numPoints= " + numPoints);
		Vector3[] points = new Vector3[numPoints];
		int[] indecies = new int[numPoints];
		Color[] colors = new Color[numPoints];
        int[] triangles = ColonMesh.triangles;
        int[] newtris = ColonMesh.triangles;// new int[49047];

        // List<int> visibleTriIndecies = new List<int>();



        int i = 0;
        // int j = 0;
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
            
            // newtris[j] = visibleTriIndecies[j];
            // j +=3;
		}

		// targetmesh.vertices = points;
        // targetmesh.triangles = newtris;
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



// for(int i = 0; i < newtris.Length; i+=3) { 
//     if(newtris[i] == triangles[i] && newtris[i+1] == triangles[i+1] && newtris[i+2] == triangles[i+2]) { 
//         newtris[i] = 0; newtris[i + 1] = 0; newtris[i + 2] = 0; 
//         } 
//     else { 
//         newtris[i] = triangles[i]; newtris[i + 1] = triangles[i + 1]; newtris[i + 2] = triangles[i + 2]; 
//         } 
//     }
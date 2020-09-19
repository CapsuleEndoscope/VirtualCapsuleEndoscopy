using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMakerCoverage2 : MonoBehaviour {
    Vector3[] vertices;
    public float ze;
    public Camera cam;
    public static Mesh mesh;
    public static Mesh targetmesh;
    private Mesh ColonMesh; //, IntestineMesh, StomachMesh;
    // private Mesh OrgansMesh;
    // public GameObject Organs;
    // public GameObject Stomach;
    // public GameObject Intestine;
    public GameObject Organ;
    public static HashSet<Vector3> visibleVerticesColon;
    private GUIStyle guiStyle = new GUIStyle(); //create a new variable
    public static float totalCoverageColon = 0.0f;
    // const int TOTALVERTICES = 7565241;
    // const float COLONVERTICES = 11556f; //GI_low_connected -> colon
    const float COLONVERTICES = 24822f; // GI_2_high_connected ->stomach
    Plane[] planes;
    int count;
    
    public static HashSet<Vector3> VerticesColon;
    Color[] colors2 = new Color[24822];
    //public GameObject Exportable;
    public void Start(){

        // mesh = new Mesh();
		// GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshRenderer> ().material = new Material (Shader.Find("Custom/VertexColor"));  
        
        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;

        visibleVerticesColon = new HashSet<Vector3>();
        // VerticesColon = new HashSet<Vector3>();

        // string path = Path.Combine(Application.persistentDataPath, "data");
        // path = Path.Combine(path, "carmodel"+ ".fbx");
        // //Create Directory if it does not exist
        // if (!Directory.Exists(Path.GetDirectoryName(path)))
        // {
        //     Directory.CreateDirectory(Path.GetDirectoryName(path));
        // }
        // FBXExporter.ExportGameObjToFBX(mesh, path, true, true);

        targetmesh = GetComponent<MeshFilter>().mesh;
        targetmesh.vertices = ColonMesh.vertices;
        targetmesh.normals = ColonMesh.normals;
        targetmesh.uv = ColonMesh.uv;
        targetmesh.triangles = ColonMesh.triangles;
        targetmesh.tangents = ColonMesh.tangents;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            // colors[i] = Color.Lerp(Color.red, Color.green, vertices[i].y);
            colors[i] = new Color(1.0f, 0.0f, 0.0f, 1.0f);

        // assign the array of colors to the Mesh.
        targetmesh.colors = colors;
        colors2 = colors;

        // MeshFilter meshfilter = Exportable.AddComponent<MeshFilter>();
        //Exportable.GetComponent<MeshFilter>().mesh = targetmesh;
        // meshfilter.gameObject.AddComponent<MeshRenderer>();
        // UnityFBXExporter.FBXExporter.ExportGameObjToFBX(Exportable, "exportedFBX.fbx", true, true);

    }
    void Update(){

        targetmesh = GetComponent<MeshFilter>().mesh;
        ColonMesh = Organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = ColonMesh.vertices;
        
        //--find out the visible vertices
        Vector3 visibleVertex = new Vector3();
        // Vector3 inVisibleVertex = new Vector3();

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
                    colors2[i] = new Color(0.2f,0.7f,0,1.0f);
                }     
            }
        }
        targetmesh.colors = colors2;
    }

}





using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMaker : MonoBehaviour {

    // accesible variables
    public Camera cam;
    public GameObject organ;
    public Text ui_text_cov;
    public Text ui_text_time;
    public static HashSet<Vector3> organ_visible_vertices;
    public static float prev_coverage = 0.0f;
    public static float curr_coverage = 0.0f;
    public static float diff_coverage = 0.0f;    
    public static Mesh generated_mesh;

    // in-class variables
    public  float SIGHT_RANGE = 0.4f;
    private float vertex_count;
    private Mesh organ_mesh;
    private Vector3[] vertices;

    public void Start(){

        generated_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = generated_mesh;
		GetComponent<MeshRenderer>().material = new Material (Shader.Find("Custom/VertexColor"));  
        organ_mesh = organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = organ_mesh.vertices;
        organ_visible_vertices = new HashSet<Vector3>();
        vertex_count = (float)organ_mesh.vertexCount; // test stomach vertexcount = 26762/9067 unity takes it wrong!
        Debug.Log(vertex_count);
    }
        
    void Update(){

        organ_mesh = organ.GetComponent<MeshFilter>().sharedMesh;
        vertices = organ_mesh.vertices;
        prev_coverage = 100 * ((organ_visible_vertices.Count/9067f));
        //--find out the visible vertices
        Vector3 visibleVertex = new Vector3();
        for(int i=0; i < vertices.Length;i+=1)
        {
            // to avoid point behind the camera.
            var worldPoint = organ.transform.TransformPoint(vertices[i]);
            if (Vector3.Dot(cam.transform.forward, worldPoint-cam.transform.position) >= 0){

                visibleVertex = cam.WorldToViewportPoint(organ.transform.TransformPoint(vertices[i]));

                // if vertex is visible by cam
                if(visibleVertex.x <= 1 && visibleVertex.x >= 0 && visibleVertex.y >= 0 && visibleVertex.y <= 1 && visibleVertex.z > 0  && visibleVertex.z <= SIGHT_RANGE )
                {   
                    organ_visible_vertices.Add(vertices[i]);
                }     
            }
        }
        // Debug.Log("Coverage: " +  total_coverage_percent.ToString("F2") + " %");
        curr_coverage = 100 * (organ_visible_vertices.Count/9067f);
        diff_coverage = curr_coverage-prev_coverage;
        CreateMesh(organ_visible_vertices);
        ui_text_cov.text =  "Coverage: " +  (curr_coverage).ToString("F2");
        ui_text_time.text =  "Elapsed Time: " +  Time.realtimeSinceStartup.ToString("F2");
        // Debug.Log("Coverage: " +  curr_coverage.ToString("F2"));
        // Debug.Log(vertex_count.ToString());


    }

	void CreateMesh(HashSet<Vector3> visible_vertices) {

        int num_points = (int)vertex_count;
		Vector3[] points = new Vector3[num_points];
		int[] indices = new int[num_points];
		Color[] colors = new Color[num_points];
        // int i = 0;

        for(int i=0; i < vertices.Length;i+=1){
            
            // float x = vertex.x; // +3
            // float y = vertex.y; // +3
            // float z = vertex.z; // -7

            points[i] = vertices[i];//new Vector3(x, y, z);
            indices[i] = i;
            if (visible_vertices.Contains(points[i])){
            	colors[i] = new Color(0.2f,0.7f,0,1.0f); // green
            }
            else{
            	colors[i] = new Color(1.0f,0f,0f,1.0f); // green
            }
            i++; 
		}

        generated_mesh.vertices = points;
		generated_mesh.colors = colors;
		generated_mesh.SetIndices(indices, MeshTopology.Points,0);

	}

}
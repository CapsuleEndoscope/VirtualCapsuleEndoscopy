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
using System.IO;

public class RayCast : MonoBehaviour {
    public Camera camera;
    private GUIStyle guiStyle = new GUIStyle(); //create a new variable

    void Start(){
        print("Camera Pixel = " + camera.pixelWidth + "x" + camera.pixelHeight);
        
        RaycastHit hit;
        
        StreamWriter writer = new StreamWriter("pixel.txt"); //true to append
        var counter = 0;
        for (var i = 0; i < (camera.pixelHeight) ; i+=1) {
            for (var j = 0; j < (camera.pixelWidth); j+=1) {
                Ray ray = camera.ScreenPointToRay(new Vector3(j,i,0));
                
                if (Physics.Raycast(ray, out hit, 1.0f)) {
                    print( "Pixel (WxH) " + j + ", " + i + " - distance: " + hit.distance);
                    //Write some text to the test.txt file
                    // writer.WriteLine( hit.distance + ",");
                    Vector3 newObjectCoordinate = hit.point;
                    GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    newObject.transform.position = newObjectCoordinate;
                    newObject.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
                    newObject.GetComponent<Renderer>().material.color = Color.blue;
                    if (counter == 10){
                        break;
                    }
                    counter ++;

                }
            }
        }
                    
        writer.Close();
    }
    
    private void OnGUI()
    {
        
        guiStyle.fontSize = 30; //change the font size
        Event   currentEvent = Event.current;
        Vector2 mousePos = new Vector2();
        
        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = camera.pixelHeight - currentEvent.mousePosition.y;
        
        GUILayout.BeginArea(new Rect(20, 20, 500, 500));
        GUILayout.Label("Screen pixels: " + camera.pixelWidth + ":" + camera.pixelHeight, guiStyle);
        GUILayout.Label("Mouse position: " + mousePos, guiStyle);
        GUILayout.EndArea();
    }
    
 
    void FixedUpdate(){

        /*
        RaycastHit hit;
        
        for (var i = 0; i < (camera.pixelWidth) ; i+=100) {
            for (var j = 0; j < (camera.pixelHeight); j+=100) {
                Ray ray = camera.ScreenPointToRay(new Vector3(i,j,0));
                
                if (Physics.Raycast(ray, out hit, 5.0f)) {
                    //Transform objectHit = hit.transform;
                    print( "Pixel " + i + ", " + j + " - distance: " + hit.distance);
                    // Do something with the object that was hit by the raycast.
                    
                    }
                }
        }
    */
    }
}



using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// This script should be attached to the player/camera that are casting the ray


// Tagging the Objects
// need to tag the objects of interested in, In the Unity Editor:

// 1. Select each object you want to track in the scene.
// 2. In the Inspector window, find the "Tag" dropdown and click "Add Tag...".
// 3. Add a new tag for these objects "TrackableObject" and assign this tag to each of your objects.

public class RaycastCounter : MonoBehaviour
{
    public Camera camera;
    private Dictionary<string, int> hitsCount = new Dictionary<string, int>();
    public bool isRecording = false; // Flag, recording state
    private bool wasButtonPressed = false;
    private RaycastHit hit; // Declare this at the class level
    //private GameObject lastHitObject = null; // To keep track of the last hit object
    private List<string> objectTransitions = new List<string>(); // To track transitions
    public XRController rightHandController; // Assign this in the inspector
    public LayerMask layerToDetect;
    public int maxDistance = 20; // ray casting distance for valid detection

    private bool wasPressed = true;



    // new code
    private GameObject lastHitObject = null; // To keep track of the last hit object
    private bool hasTransitioned = false;
    private List<string> hitRecords = new List<string>(); // List to hold data before writing to file

    private string filePath;

    public int type;
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;

    void oldPerformRaycast()
    {
        // gp væk fra screenPointToRay
        //Ray ray = camera.ScreenPointToRay(camera.transform.position);
        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        //Debug.Log("mouse" + Input.mousePosition);
        Debug.DrawRay(camera.transform.position, camera.transform.forward*10, Color.green);
        // "TrackableObject"
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("sending raycast");
            Debug.Log("i Hit " + hit.collider.gameObject.name);
            
            if (hit.collider.CompareTag("TrackableObject"))
            {
                string objectName = hit.transform.name;
                //Debug.Log("Hit <3 " + hit.collider.gameObject.name);

                // Update hits count
                if (hitsCount.ContainsKey(objectName))
                {
                    hitsCount[objectName]++;
                }
                else
                {
                    hitsCount.Add(objectName, 1);
                }

                // Check and record object transition55
                if (lastHitObject != null && lastHitObject.name != objectName)
                {
                    string transition = $"{lastHitObject.name} to {objectName}";
                    objectTransitions.Add(transition);
                }

                lastHitObject = hit.collider.gameObject; // Update last hit object
            }
        }
        
    }

    // new
    public void PerformRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        // make a copy, use this for violin plot
        Ray ray2 = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit2;

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 30, Color.green);

        if (Physics.Raycast(ray, out hit))
        {

        
            int layer = 3;
            int LayerMask = 1 << layer;

            Vector3 localHitPoint = new Vector3(0,0,0);
            //
            if (Physics.Raycast(ray2, out hit2, Mathf.Infinity, LayerMask))
            {

                // Access the hit object's transform
                Transform hitTransform = hit2.transform;

                // Convert the world position of the hit to a local position
                localHitPoint = hitTransform.InverseTransformPoint(hit2.point);

                
            }


            if(hit.collider.CompareTag("TrackableObject"))
            {
                //Debug.Log("i Hit " + hit.collider.gameObject.name);
                string objectName = hit.transform.name;

                string transition = "No transition";
                if (lastHitObject != null && lastHitObject.name != objectName)
                {
                    transition = $"{lastHitObject.name} to {objectName}";
                    hasTransitioned = true;
                }
                else
                {
                    hasTransitioned = false;
                }

                lastHitObject = hit.collider.gameObject; // Update last hit object

                // Prepare the record string
                string record = PrepareRecord(objectName, transition, hasTransitioned, localHitPoint);
                // Add the record to the list
                //Debug.Log("record: " + record);
                hitRecords.Add(record);
            }

            
        }


        // local xz


    }
    string PrepareRecord(string hitObject, string transition, bool flag, Vector3 localhit)
    {
        //Debug.Log("PrepareRecord ");

        string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string test = time + "},{" + hitObject + "},{" + transition + "},{" + flag + "},{" + localhit.x + "},{" + localhit.y + "},{" + localhit.z;
       // return $"{time},{hitObject},{transition},{flag}";
        return  $"{test}";
    }
    public void SaveHitRecords()
    {

        if (hitRecords.Count == 0)
        {
            Debug.LogError("hitRecords list is empty. No data to save.");
            return;
        }

        string fileNamePart = ("HitTracking" + "_" + type.ToString() + "_" + name + "_" + controlScheme + "_" + startPose + "_" + gridLocation + "_" + userInterface); // Customize as needed
        string fileName = $"{fileNamePart}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("time,hitObject,transition,flag,x,y,z");

        foreach (var record in hitRecords)
        {
            string csvLine = record;//$"{record.time},{record.hitObject},{record.transition},{record.flag}";
            csvContent.AppendLine(csvLine);
        }
        // Write to file
        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log($"Data saved to {filePath}");

    }

    public void ClearhitRecords()
    {
        hitRecords.Clear();
        Debug.Log("hitRecords list cleared.");
    }

  
}
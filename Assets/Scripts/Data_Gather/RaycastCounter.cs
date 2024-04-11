using System.Collections.Generic;
using UnityEngine;
using System.IO;
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
    private GameObject lastHitObject = null; // To keep track of the last hit object
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

    public string type;
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;



//     public bool StartRaycast()
//     {
//         return isRecording = true;
//     }
//     public bool StopRaycast()
//     {
//         return isRecording = false;
//     }
//     public void ClearObjectTransitions()
//     {
//         objectTransitions.Clear();
//         Debug.Log("objectTransitions list cleared.");
//     }
//     // Toggle recording on and off
//     public void ToggleRaycast()
//     {
//         isRecording = !isRecording;
//         if (isRecording)
//         {
//             Debug.Log("Recording started.");
//         }
//         else
//         {
//             Debug.Log("Recording stopped.");
//             SaveDetectedObjectsHits();
//         }
//     }
//     void Start()
//     {
    
//     }
// //FIX THIS FUNCTION TO incorporate the START AND STOP functions 
//     void Update()
//     {
        
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             isRecording = !isRecording; //toggle the state
//             if(!isRecording)
//             {
//                 SaveDetectedObjectsHits();
//                 Debug.Log("Recording stopped.");
//                 wasPressed = true;
//             }
//         }

//         if (isRecording)
//         {
//             PerformRaycast();
//             if (wasPressed)
//             {
//                 Debug.Log("Recording started.");
//                 wasPressed = false;
//             }
            
//         }
//         // else
//         // {   
//         //     SaveDetectedObjectsHits();
//         //     Debug.Log("Recording stopped.");
//         // }
    

       
//     }

    // void PerformRaycast()
    // {
    //     if(rightHandController && rightHandController.enableInputActions)
    //     {
    //         //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         Ray ray = new Ray(rightHandController.transform.position, rightHandController.transform.forward);
            
    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             if (hit.collider.CompareTag("TrackableObject"))
    //             {
    //                 string objectName = hit.transform.name;
    //                 Debug.Log("Hit " + hit.collider.gameObject.name);
    //                 // Update hits count
    //                 if (hitsCount.ContainsKey(objectName))
    //                 {
    //                     hitsCount[objectName]++;
    //                 }
    //                 else
    //                 {
    //                     hitsCount.Add(objectName, 1);
    //                 }

    //                 // Check and record object transition
    //                 if (lastHitObject != null && lastHitObject.name != objectName)
    //                 {
    //                     string transition = $"{lastHitObject.name} to {objectName}";
    //                     objectTransitions.Add(transition);
    //                 }

    //                 lastHitObject = hit.collider.gameObject; // Update last hit object
    //             }
    //         }
    //     }
    // }
    void oldPerformRaycast()
    {
        // gp væk fra screenPointToRay
        //Ray ray = camera.ScreenPointToRay(camera.transform.position);
        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        //Debug.Log("mouse" + Input.mousePosition);
        Debug.DrawRay(camera.transform.position, camera.transform.forward*30, Color.green);
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
    void PerformRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 30, Color.green);

        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("TrackableObject"))
        {
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
            string record = PrepareRecord(objectName, transition, hasTransitioned);
            // Add the record to the list
            hitRecords.Add(record);
        }
    }

    string PrepareRecord(string hitObject, string transition, bool flag)
    {
        string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return $"{time},{hitObject},{transition},{flag}";
    }


    public void SaveHitRecords()
    {

         if (hitRecords.Count == 0)
        {
            Debug.LogError("hitRecords list is empty. No data to save.");
            return;
        }

        string fileNamePart = "HitTracking" + "_" type + "_" name + "_" + controlScheme + "_"  startPose + "_" gridLocation + "_" userInterface; // Customize as needed
        string fileName = $"{fileNamePart}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("time,hitObject,transition,flag");

        foreach (var record in hitRecords)
        {
            string csvLine = = $"{record.time},{record.hitObject},{record.transition},{record.flag}";
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

    // private void SaveDetectedObjectsHits()
    // {
    //     string hitsFileName = $"ObjectHitsData_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
    //     string transitionsFileName = $"ObjectTransitionsData_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
    //     string hitsFilePath = Path.Combine(Application.persistentDataPath, hitsFileName);
    //     string transitionsFilePath = Path.Combine(Application.persistentDataPath, transitionsFileName);

    //     // Save hits
    //     using (StreamWriter writer = new StreamWriter(hitsFilePath))
    //     {
    //         foreach (KeyValuePair<string, int> pair in hitsCount)
    //         {
    //             writer.WriteLine($"{pair.Key}: {pair.Value}");
    //         }
    //     }

    //     // Save transitions
    //     using (StreamWriter writer = new StreamWriter(transitionsFilePath))
    //     {
    //         foreach (string transition in objectTransitions)
    //         {
    //             writer.WriteLine(transition);
    //         }
    //     }

    //     Debug.Log($"Data saved to {hitsFilePath} and {transitionsFilePath}");
    // }
}
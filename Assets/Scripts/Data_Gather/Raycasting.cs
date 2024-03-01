using System.Collections.Generic;
using UnityEngine;
using System.IO;

// This script should be attached to the player/camera that are casting the ray


// Tagging the Objects
// need to tag the objects of interested in, In the Unity Editor:

// 1. Select each object you want to track in the scene.
// 2. In the Inspector window, find the "Tag" dropdown and click "Add Tag...".
// 3. Add a new tag for these objects "TrackableObject" and assign this tag to each of your objects.

public class RaycastCounter : MonoBehaviour
{
    private Dictionary<string, int> hitsCount = new Dictionary<string, int>();
    private bool isRecording = false; // Flag, recording state
    private RaycastHit hit; // Declare this at the class level
    private GameObject lastHitObject = null; // To keep track of the last hit object
    private List<string> objectTransitions = new List<string>(); // To track transitions


    public bool StartRaycast()
    {
        return isRecording = true;
    }
    public bool StopRaycast()
    {
        return isRecording = false;
    }

    // Toggle recording on and off
    public void ToggleRaycast()
    {
        isRecording = !isRecording;
        if (isRecording)
        {
            Debug.Log("Recording started.");
        }
        else
        {
            Debug.Log("Recording stopped.");
            SaveDetectedObjectsHits();
        }
    }
    void Start()
    {
        
    }
// FIX THIS FUNCTION TO incorporate the START AND STOP functions 
    void Update()
    {

        if (isRecording)
        {
            PerformRaycast();
            Debug.Log("Recording started.");
        }
        else
        {   
            SaveDetectedObjectsHits();
            Debug.Log("Recording stopped.");
        }
    

       
    }

    void PerformRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("TrackableObject"))
            {
                string objectName = hit.transform.name;

                // Update hits count
                if (hitsCount.ContainsKey(objectName))
                {
                    hitsCount[objectName]++;
                }
                else
                {
                    hitsCount.Add(objectName, 1);
                }

                // Check and record object transition
                if (lastHitObject != null && lastHitObject.name != objectName)
                {
                    string transition = $"{lastHitObject.name} to {objectName}";
                    objectTransitions.Add(transition);
                }

                lastHitObject = hit.collider.gameObject; // Update last hit object
            }
        }
    }

    private void SaveDetectedObjectsHits()
    {
        string hitsFileName = $"ObjectHitsData_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
        string transitionsFileName = $"ObjectTransitionsData_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
        string hitsFilePath = Path.Combine(Application.persistentDataPath, hitsFileName);
        string transitionsFilePath = Path.Combine(Application.persistentDataPath, transitionsFileName);

        // Save hits
        using (StreamWriter writer = new StreamWriter(hitsFilePath))
        {
            foreach (KeyValuePair<string, int> pair in hitsCount)
            {
                writer.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }

        // Save transitions
        using (StreamWriter writer = new StreamWriter(transitionsFilePath))
        {
            foreach (string transition in objectTransitions)
            {
                writer.WriteLine(transition);
            }
        }

        Debug.Log($"Data saved to {hitsFilePath} and {transitionsFilePath}");
    }
}
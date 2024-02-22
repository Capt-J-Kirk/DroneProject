using System.Collections.Generic;
using UnityEngine;


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

    void Update()
    {
        // Toggle recording with the R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            isRecording = !isRecording;

            if (isRecording)
            {
                Debug.Log("Recording started.");
            }
            else
            {
                Debug.Log("Recording stopped.");
            }
        }

        if (isRecording)
        {
            PerformRaycast();
        }
    }

    void PerformRaycast()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("TrackableObject"))
            {
                string objectName = hit.transform.name;

                if (hitsCount.ContainsKey(objectName))
                {
                    hitsCount[objectName]++;
                }
                else
                {
                    hitsCount.Add(objectName, 1);
                }
            }
        }
    }

    // access the hit counts
    public Dictionary<string, int> GetHitCounts()
    {
        return hitsCount;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AccessData : MonoBehaviour
{
    public RaycastCounter raycastCounter; // Reference to the Raycasting script

    void Start()
    {
        if (raycastCounter == null)
        {
            Debug.LogError("RaycastCounterWithControl reference not set.");
            return;
        }

        // Accessing the hit counts
        Dictionary<string, int> hitCounts = raycastCounter.GetHitCounts();

        // use the attempt number, and start time for the naming of the txt file
        //fileCount = attempt + start_time;
        int fileCount = 1;
        string fileName = $"ObjectHitsData_{fileCount}.txt";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (KeyValuePair<string, int> hit in hitCounts)
            {
                writer.WriteLine($"{hit.Key}: {hit.Value}");
            }
            Debug.Log($"Data saved to {filePath}");
        }
    }
}

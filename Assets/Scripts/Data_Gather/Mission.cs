using System.IO;
using UnityEngine;
using UnityEngine.UI;




// UI Button for Next Attempt
// 1. UI Setup: Create a UI Button in your scene by right-clicking in the Hierarchy panel, navigating to UI -> Button. This button will be used to start a new attempt.
// 2. Button Script: Attach a script to the button that resets the game objects to their starting positions and resets the timer.


public class MissionManager : MonoBehaviour
{
    public GameObject[] gameObjectsToPosition; // Assign in inspector
    public Button nextAttemptButton; // Assign in inspector
    private int attemptCount = 0; 
    private float startTime;
    private float endTime;
    private int totalAvailablePnts = 0; // Used in the cleaning score
    private int scoredPnts = 0; // Used in the cleaning score

    private string filePath = "YourPath/attemptTimes.txt";

    private void Start()
    {
        nextAttemptButton.onClick.AddListener(StartNewAttempt);
    }

    private void StartNewAttempt()
    {
        //add raycasting script 
        // add raycasting
        GetComponent<RaycastCounter>().StartRaycast();
        PositionGameObjects();
        startTime = Time.time;
        attemptCount++;
        // Reset any other necessary game state here
        totalAvailablePnts = 0;
        scoredPnts = 0;
    }

    private void PositionGameObjects()
    {
        // Position your game objects. This is just a placeholder.
        // Use transform.position = new Vector3(x, y, z) for each game object.
    }


    public void GetCleanScore(int availablePoints,int scoredPoints)
    {
        totalAvailablePnts = availablePoints;
        scoredPnts = scoredPoints;
    }

    public void GetRayDetections()
    {

    }




    public void FinishAttempt()
    {
        endTime = Time.time;
        //GetCleanScore();
        //GetRayDetections();
        //WriteToFile();
        // add save raycasting data
        GetComponent<RaycastCounter>().StopRaycast();
        // Call this method when the player finishes the attempt
    }

    private void WriteToFile()
    {
        string times = $"Attempt: {attemptCount}, Start Time: {startTime}, End Time: {endTime}, Duration: {endTime - startTime}, CleanAvailableScorePnts: {totalAvailablePnts}, CleanScoredPnts: {scoredPnts}";
        File.AppendAllText(filePath, times + "\n");
    }
}

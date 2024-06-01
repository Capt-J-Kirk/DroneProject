using UnityEngine;

public class EnterTrigger : MonoBehaviour
{
    
    // private bool inCleaning = false;
    // private bool inFlight = false;
    private GameObject mission;
    private MissionManager mis;

    void Start()
    {
        mission = GameObject.Find("Mission");
        if (mission == null)
        {
            Debug.LogError("Please assign the mission");
            return;
        }
        mis = mission.GetComponent<MissionManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered the trigger has the tag "inCleaning"
        if (other.CompareTag("inCleaning"))
        {
            Debug.Log("Changing state to inCleaning!");
            // inCleaning = true;
            // inFlight = false;
            mis.inCleaning = true;
            mis.inFlight = false;
        }
    }
}

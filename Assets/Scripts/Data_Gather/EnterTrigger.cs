using UnityEngine;

public class EnterTrigger : MonoBehaviour
{
    // These variables should be defined if you're using them in your script
    private bool inCleaning = false;
    private bool inFlight = false;

    // OnTriggerEnter is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered the trigger has the tag "inCleaning"
        if (other.CompareTag("inCleaning"))
        {
            Debug.Log("Changing state to inCleaning!");
            inCleaning = true;
            inFlight = false;
        }
    }
}

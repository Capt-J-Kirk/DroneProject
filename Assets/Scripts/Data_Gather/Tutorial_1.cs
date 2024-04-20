using UnityEngine;

public class ToggleScript : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        Debug.Log("tutorial 1");
        toggleTutorial.Tutorial = 1;
        toggleTutorial.runTutorial = true;
       
    }
}
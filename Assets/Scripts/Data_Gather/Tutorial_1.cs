using UnityEngine;

public class ToggleScript : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        toggleTutorial.Tutorial = 1;
        toggleTutorial.runTutorial = true;
       
    }
}
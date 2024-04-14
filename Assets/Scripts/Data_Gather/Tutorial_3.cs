using UnityEngine;

public class ToggleScript3 : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        toggleTutorial.Tutorial = 2;
        toggleTutorial.runTutorial = true;
    }
}
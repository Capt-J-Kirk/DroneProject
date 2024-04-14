using UnityEngine;

public class ToggleScript2 : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        toggleTutorial.Tutorial = 2;
        toggleTutorial.runTutorial = true;
    }
}
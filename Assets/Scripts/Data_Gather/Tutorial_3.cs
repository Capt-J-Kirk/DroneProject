using UnityEngine;

public class ToggleScript3 : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        Debug.Log("tutorial 3");
        toggleTutorial.Tutorial = 2;
        toggleTutorial.runTutorial = true;
    }
}
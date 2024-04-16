using UnityEngine;

public class ToggleScript2 : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        Debug.Log("tutorial 2");
        toggleTutorial.Tutorial = 2;
        toggleTutorial.runTutorial = true;
    }
}
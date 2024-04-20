using UnityEngine;

public class ToggleScriptSTART : MonoBehaviour
{
    public MissionManager toggleTutorial;

    public void ToggleBoolean()
    {
        toggleTutorial.selectCombination = true;
       
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dual_Display : MonoBehaviour
{

    void Start()
    {
        for (int i=0; i<Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
        Debug.Log("Display count: " + Display.displays.Length);
    }


}

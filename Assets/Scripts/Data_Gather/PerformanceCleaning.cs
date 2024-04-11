using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PerformanceCleaning: MonoBehaviour
{

    // implement own fonction to log individual data on each gameobject

    // data class!

    public string type;
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;


    // public summet values for datacollector
    public float cleaningPercent = 0;
    public float maxCleanValuePossible = 0;
    public float currentCleanValue = 0;
    public float cleaningPerSecond = 0;



}
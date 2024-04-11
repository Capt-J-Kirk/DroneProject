using UnityEngine;

[System.Serializable]
public class DroneData
{
    // Basic parameters
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;

    // Positions and rotations
    public Vector3 main_pos;
    public Quaternion main_rot;
    public Vector3 sec_pos;
    public Quaternion sec_rot;

    // Drone states
    public bool inFlight;
    public bool inCleaning;

    // Cleaning data
    public float cleaningPercent;
    public float maxCleanValuePossible;
    public float currentCleanValue;
    public float cleaningPerSecond;

    // User input for drone control
    public float throttle1;
    public float pitch1;
    public float yaw1;
    public float roll1;
    public float throttle2;
    public float pitch2;
    public float yaw2;
    public float roll2;

    // Spherical user input
    public float radius;
    public float theta;
    public float phi;

    // User button clicks
    public bool followMode;
    public bool switchDrone;
    public bool controlMainDrone;
    public bool switchCamFeed;

    public bool isSpraying;
    // Avoidance
    public float distanceToObject1;
    public float distanceToObject2;

    // Constructor to initialize all values
    public DroneData(string name, string controlScheme, string startPose, string gridLocation, string userInterface,
                     Vector3 main_pos, Quaternion main_rot, Vector3 sec_pos, Quaternion sec_rot,
                     bool inFlight, bool inCleaning, float cleaningPercent, float maxCleanValuePossible,
                     float currentCleanValue, float cleaningPerSecond, 
                     float throttle1, float pitch1, float yaw1, float roll1, 
                     float throttle2, float pitch2, float yaw2, float roll2, float radius, float theta, float phi, bool followMode, bool switchDrone, 
                     bool controlMainDrone, bool switchCamFeed, bool isSpraying, float distanceToObject1, float distanceToObject2)
    {
        this.name = name;
        this.controlScheme = controlScheme;
        this.startPose = startPose;
        this.gridLocation = gridLocation;
        this.userInterface = userInterface;
        this.main_pos = main_pos;
        this.main_rot = main_rot;
        this.sec_pos = sec_pos;
        this.sec_rot = sec_rot;
        this.inFlight = inFlight;
        this.inCleaning = inCleaning;
        this.cleaningPercent = cleaningPercent;
        this.maxCleanValuePossible = maxCleanValuePossible;
        this.currentCleanValue = currentCleanValue;
        this.cleaningPerSecond = cleaningPerSecond;
        this.throttle1 = throttle1;
        this.pitch1 = pitch1;
        this.yaw1 = yaw1;
        this.roll1 = roll1;
        this.throttle2 = throttle2;
        this.pitch2 = pitch2;
        this.yaw2 = yaw2;
        this.roll2 = roll2;
        this.radius = radius;
        this.theta = theta;
        this.phi = phi;
        this.followMode = followMode;
        this.switchDrone = switchDrone;
        this.controlMainDrone = controlMainDrone;
        this.switchCamFeed = switchCamFeed;
        this.isSpraying = isSpraying;
        this.distanceToObject = distanceToObject1;
        this.distanceToObject = distanceToObject2;
    }
}

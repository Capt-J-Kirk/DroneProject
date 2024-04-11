using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;




// UI Button for Next Attempt
// 1. UI Setup: Create a UI Button in your scene by right-clicking in the Hierarchy panel, navigating to UI -> Button. This button will be used to start a new attempt.
// 2. Button Script: Attach a script to the button that resets the game objects to their starting positions and resets the timer.


public class MissionManager : MonoBehaviour
{
    // Objects and Scripts the data should be collected from
    public GameObject main_drone;
    public QuadcopterController quadcopterController;

    public GameObject sec_drone;
    public QuadcopterController_sec quadcopterController_Sec;
    public ObjectTransform objectTransform;

    public PerformanceCleaning performanceCleaning;

    public UserInput userInput;
    
    // tracking script needs its own function for data collection
    public RaycastCounter raycastCounter;


    // init the datacollector
    private DataCollector dataCollectionIntance = new DataCollector();

    // still need to pass the data to the datacollector!
    public string name = "sofie";
    public string controlScheme = "scheme 1";
    public string startPose = "start1";
    public string gridLocation = "grid1";
    public string userInterface = "2screen";

    // Drone states // handle in this script!
    public bool inFlight = true;
    public bool inCleaning = false;

    public bool ready = false;
    private string mission = "controller";

    void Start()
    {
        dataCollectionIntance.type = mission;
        if (ready)
        {
            Invoke("DataUpdate", 0.05f); // call 1/20 a sec 
        }
        
        
    }

    void Update()
    {
        // toggle the ready state here

        // setting the mission type here

    }

    private void OnTriggerEnter(Collider inChange)
    {
        // selecting the object in Unity's editor, then in the Collider component (e.g., BoxCollider, SphereCollider, etc.), check the "Is Trigger" option.
        // remenber to set the tag "inCleaning"
        if (inChange.CompareTag("inCleaning"))
        {
            Debug.Log("changing state to inCleaning!");
            inCleaning = true;
            inFlight = false;
        }
    }
    
    void DataUpdate()
    {
        // Positions and rotations
        private Vector3 main_pos = main_drone.transform.position;
        private Quaternion main_rot = main_drone.transform.rotation;
        private Vector3 sec_pos = sec_drone.transform.position;
        private Quaternion sec_rot = sec_drone.transform.rotation;

        // Cleaning data
        private float cleaningPercent = performanceCleaning.cleaningPercent;
        private float maxCleanValuePossible = performanceCleaning.maxCleanValuePossible;
        private float currentCleanValue = performanceCleaning.currentCleanValue;
        private float cleaningPerSecond = performanceCleaning.cleaningPerSecond;

        // User input for drone control
        private float throttle1 = quadcopterController.desiredPosition.y;
        private float pitch1 = quadcopterController.desiredEulerAngles.x;
        private float yaw1 = quadcopterController.desiredEulerAngles.y;
        private float roll1 = quadcopterController.desiredEulerAngles.z;

        private float throttle2 = quadcopterController_Sec.desiredPosition.y;
        private float pitch2 = quadcopterController.desiredEulerAngles.x;
        private float yaw2 = quadcopterController.desiredEulerAngles.y;
        private float roll2 = quadcopterController.desiredEulerAngles.z;

        // Spherical user input
        private float radius = objectTransform.radius;
        private float theta = objectTransform.theta;
        private float phi = objectTransform.phi;

        // User button clicks
        private bool followMode = objectTransform.toggleFollow;
        private bool switchDrone = userInput.togglesecondarDrone;
        private bool controlMainDrone = !userInput.togglesecondarDrone;
        private bool switchCamFeed = userInput.switchCamFeed;
        private bool isSpraying = userInput.isSpraying;

        // Avoidance
        private float distanceToObject1 = 0;
        private float distanceToObject2 = 0;

        dataCollectionIntance.CollectData(name, controlScheme, startPose, gridLocation, userInterface,
                     main_pos, main_rot, sec_pos, sec_rot,
                     inFlight, inCleaning, cleaningPercent, maxCleanValuePossible,
                     currentCleanValue, cleaningPerSecond, throttle1, pitch1, yaw1, 
                     roll1, throttle2, pitch2, yaw2, 
                     roll2, radius, theta, phi, followMode, switchDrone, 
                     controlMainDrone, switchCamFeed, isSpraying, distanceToObject1, distanceToObject2);
    }
    
}

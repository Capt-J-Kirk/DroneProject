using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 



// UI Button for Next Attempt
// 1. UI Setup: Create a UI Button in your scene by right-clicking in the Hierarchy panel, navigating to UI -> Button. This button will be used to start a new attempt.
// 2. Button Script: Attach a script to the button that resets the game objects to their starting positions and resets the timer.


public class MissionManager : MonoBehaviour
{
    //[Serializable]
    public class DroneTransform {
        public Vector3 position;
        public Quaternion rotation;
    }

    //[Serializable]
    public class StartPositions {
        public DroneTransform main_drone;
        public DroneTransform sec_drone;
    }

    //[Serializable]
    public class GridPositions {
        public DroneTransform windblade;
    }

    //[Serializable]
    public class LevelConfigurations {
        public StartPositions start1;
        public StartPositions start2;
        public GridPositions grid1;
        public GridPositions grid2;
    }

    // Objects and Scripts the data should be collected from
    public GameObject main_drone;
    public QuadcopterController quadcopterController;

    public GameObject sec_drone;
    public QuadcopterController_sec quadcopterController_Sec;
    public ObjectTransform objectTransform;

    public GridManager performanceCleaning;

    public UserInput userInput;
    
    // tracking script needs its own function for data collection
    //public RaycastCounter raycastCounter;

    // grid gameobject here
    public GameObject grid;

    // screen 
    public GameObject  TwoScreen;
    public GameObject  OneScreen;

    // init the datacollector
    private DataCollector dataCollectionIntance = new DataCollector();
    // init the raycaster for tracking
    private RaycastCounter raycastCounter = new RaycastCounter();



    // still need to pass the data to the datacollector!
    public string name = "sofie";
    public string controlScheme = "scheme 1";
    public string startPose = "start1";
    public string gridLocation = "grid1";
    public string userInterface = "2screen";

    // Drone states // handle in this script!
    public bool inFlight = true;
    public bool inCleaning = false;

    // menu toggle options
    private string mission = "controller";
    public bool startMission = false;
    public bool selectCombination = true;
    public int count = 0;

    //
    public bool isRecording = false;
    public bool missionActive = false;
   

    private int missionCombination = 0;
    
    public int Tutorial = 0;
    public bool runTutorial = false;

    public GameObject Menu;

    public TMP_Text username;
    public TMP_Text mission_count;


    private List<int> usedCombinations = new List<int>();

    private bool test = true;
    // timer 
    private float timer = 0.0f;

    // target time, 3 minutes = 180 sec
    private float targetTime = 10.0f; 

    void Start()
    {
        dataCollectionIntance.type = mission;
        InvokeRepeating("DataUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        InvokeRepeating("TrackingUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        InvokeRepeating("PerformanceUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        
    }
   
    void FixedUpdate()
    {
        // user push button then ready for new mission, before that selects mission type

        username.text = "Name: " + name;
        mission_count.text = "Number: " + count + "/16";


        if (runTutorial)
        {
            Menu.SetActive(false);
            if(Tutorial == 1)
            {
                userInput.ManualControl = true;
                objectTransform.ControlScheme = 0;
            }
            if(Tutorial == 2)
            {
                userInput.ManualControl = false;
                objectTransform.ControlScheme = 1;
            }
            if(Tutorial == 3)
            {
                userInput.ManualControl = false;
                objectTransform.ControlScheme = 2;
            }

            
            if(timer >= targetTime )
            {
                // stop running the tutorial
                runTutorial = false;  
                Menu.SetActive(true); 
            }
        }


        if (selectCombination && !runTutorial)
        {
            Menu.SetActive(false);
            if (mission == "controller")
            {
                // total combination
                // 3 schemes
                // 2 start poses
                // 2 grid location
                // 1 userinterfaces
                // total = 12
                bool selectVALIDCombo = true;
                while(selectVALIDCombo)
                {
                    missionCombination = Random.Range(1,13);
                    if (!usedCombinations.Contains(missionCombination)) // Check if the number hasn't been used
                    {
                        usedCombinations.Add(missionCombination); // Add the new unique number to the list
                        selectVALIDCombo = false; // Break the loop
                    }
                }
                
            }
            if (mission == "userInterface")
            {
                // total combination
                // 1 schemes
                // 2 start poses
                // 2 grid location
                // 2 userinterfaces
                // total = 8
                bool selectVALIDCombo = true;
                while(selectVALIDCombo)
                {
                    missionCombination = Random.Range(1,9);
                    if (!usedCombinations.Contains(missionCombination)) // Check if the number hasn't been used
                    {
                        usedCombinations.Add(missionCombination); // Add the new unique number to the list
                        selectVALIDCombo = false; // Break the loop
                    }
                }
            }
            count += 1;
            selectCombination = false;
        }

        // then the system have found the next combination, the user cliks on startMission
        // run through once
        if (startMission && !runTutorial)
        {
            if (mission == "controller")
            {
                selectControlCombination(missionCombination);
            }
            if (mission == "userInterface")
            {
                selectUserInterfaceCombination(missionCombination);

            }

            startMission = false;

            // load the config
            loadConfig();

            // setActive() the userinterface 
            if(userInterface == "2screen")
            {
                TwoScreen.SetActive(true);
                OneScreen.SetActive(false);
            }
            if(userInterface == "1screen")
            {
                TwoScreen.SetActive(false);
                OneScreen.SetActive(true);
            }

            if(controlScheme == "scheme0")
            {
                userInput.ManualControl = true;
                objectTransform.ControlScheme = 0;
            }
            if(controlScheme == "scheme1")
            {
                userInput.ManualControl = false;
                objectTransform.ControlScheme = 1;
            }
            if(controlScheme == "scheme2")
            {
                userInput.ManualControl = false;
                objectTransform.ControlScheme = 2;
            }

            missionActive = true;
            isRecording = true;
            timer = 0.0f;

            // parse the combination to raycaster
            raycastCounter.type = mission;
            raycastCounter.name = name;
            raycastCounter.controlScheme = controlScheme;
            raycastCounter.startPose = startPose;
            raycastCounter.gridLocation = gridLocation;
            raycastCounter.userInterface = userInterface;

            // parse the combination to performance
            performanceCleaning.type = mission;
            performanceCleaning.name = name;
            performanceCleaning.controlScheme = controlScheme;
            performanceCleaning.startPose = startPose;
            performanceCleaning.gridLocation = gridLocation;
            performanceCleaning.userInterface = userInterface;
        }
        
        // start the timer, and finish the mission then it runs out
        timer += Time.fixedDeltaTime;

        if(timer >= targetTime && test)
        {
            Menu.SetActive(true);
            test = false;
            missionActive = false;
            isRecording = false;
            Debug.Log("3 minutes have passed.");
            // data 
            dataCollectionIntance.SaveDataToCSV();
            dataCollectionIntance.ClearDataList();
            // tracking
            raycastCounter.SaveHitRecords();
            raycastCounter.ClearhitRecords();
            // missing performance
        }
        
        // resets the current mission 
        if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Mission is reset: " + missionCombination);
            timer = 0.0f;
            missionActive = false;
            startMission = true;
            isRecording = false;
            dataCollectionIntance.ClearDataList();
            raycastCounter.ClearhitRecords();
            // add performance
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            raycastCounter.SaveHitRecords();
            raycastCounter.ClearhitRecords();
        }

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
        //Debug.Log("DataUpdate called");
        // Positions and rotations
        Vector3 main_pos = main_drone.transform.position;
        Quaternion main_rot = main_drone.transform.rotation;
        Vector3 sec_pos = sec_drone.transform.position;
        Quaternion sec_rot = sec_drone.transform.rotation;

        // Cleaning data
        float cleaningPercent = performanceCleaning.cleaningPercent;
        float maxCleanValuePossible = performanceCleaning.maxCleanValuePossible;
        float currentCleanValue = performanceCleaning.currentCleanValue;
        float cleaningPerSecond = performanceCleaning.cleaningPerSecond;

        // User input for drone control
        float throttle1 = quadcopterController.desiredPosition.y;
        float pitch1 = quadcopterController.desiredEulerAngles.x;
        float yaw1 = quadcopterController.desiredEulerAngles.y;
        float roll1 = quadcopterController.desiredEulerAngles.z;

        float throttle2 = quadcopterController_Sec.desiredPosition.y;
        float pitch2 = quadcopterController_Sec.desiredEulerAngles.x;
        float yaw2 = quadcopterController_Sec.desiredEulerAngles.y;
        float roll2 = quadcopterController_Sec.desiredEulerAngles.z;

        // Spherical user input
        float radius = objectTransform.radius;
        float theta = objectTransform.theta;
        float phi = objectTransform.phi;

        // User button clicks
        bool followMode = objectTransform.toggleFollow;
        bool switchDrone = userInput.togglesecondarDrone;
        bool controlMainDrone = !userInput.togglesecondarDrone;
        bool switchCamFeed = userInput.switchCamFeed;
        bool isSpraying = userInput.isSpraying;

        // Avoidance
        float distanceToObject1 = quadcopterController.distanceToObject;
        float distanceToObject2 = quadcopterController_Sec.distanceToObject;

        if (missionActive)
        {
            dataCollectionIntance.CollectData(name, controlScheme, startPose, gridLocation, userInterface,
                     main_pos, main_rot, sec_pos, sec_rot,
                     inFlight, inCleaning, cleaningPercent, maxCleanValuePossible,
                     currentCleanValue, cleaningPerSecond, throttle1, pitch1, yaw1, 
                     roll1, throttle2, pitch2, yaw2, 
                     roll2, radius, theta, phi, followMode, switchDrone, 
                     controlMainDrone, switchCamFeed, isSpraying, distanceToObject1, distanceToObject2);
        }
        
    }

    void TrackingUpdate()
    {
        if(true)//isRecording)
        {
            //Debug.Log("TrackingUpdate called");
            raycastCounter.PerformRaycast();
        }
    }

    void PerformanceUpdate()
    {
        // missing 
    }

    private void selectControlCombination(int combi)
    {
        switch (combi)
        {
            case 1:
                controlScheme = "scheme0";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 2:
                controlScheme = "scheme0";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 3:
                controlScheme = "scheme0";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 4:
                controlScheme = "scheme0";
                startPose = "start1";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 5:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 6:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 7:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 8:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 9:
                controlScheme = "scheme2";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 10:
                controlScheme = "scheme2";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 11:
                controlScheme = "scheme2";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 12:
                controlScheme = "scheme2";
                startPose = "start1";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;   
        }
    }
    
    private void selectUserInterfaceCombination(int combi)
    {
         switch (combi)
        {
            case 1:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 2:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "2screen";
                break;
            case 3:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 4:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid2";
                userInterface = "2screen";
                break;
            case 5:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "1screen";
                break;
            case 6:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "1screen";
                break;
            case 7:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "1screen";
                break;
            case 8:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid2";
                userInterface = "1screen";
                break; 
        }
    }


    private void loadConfig()
    {
        string filePath = Path.Combine(Application.dataPath, "gameConfiguration.json");
        if(File.Exists(filePath)) 
        {
            string jsonContent = File.ReadAllText(filePath);
            LevelConfigurations levelConfigs = JsonUtility.FromJson<LevelConfigurations>(jsonContent);

            if(startPose == "start1")
            {
                ApplyDroneTransform(levelConfigs.start1);
            }
            if(startPose == "start2")
            {
                ApplyDroneTransform(levelConfigs.start2);
            }
            if(gridLocation == "grid1")
            {
                ApplyWindbladeTransform(levelConfigs.grid1);
            }
             if(gridLocation == "grid2")
            {
                ApplyWindbladeTransform(levelConfigs.grid2);
            }

        } else {
            Debug.LogError("Cannot find JSON file!");
        }
    }

    void ApplyDroneTransform(StartPositions startPos) 
    {
        main_drone.transform.position = startPos.main_drone.position;
        main_drone.transform.rotation = startPos.main_drone.rotation;

        sec_drone.transform.position = startPos.sec_drone.position;
        sec_drone.transform.rotation = startPos.sec_drone.rotation;
    }

    void ApplyWindbladeTransform(GridPositions gridPos)
    {
        grid.transform.position = gridPos.windblade.position;
        grid.transform.rotation = gridPos.windblade.rotation;
    }
}

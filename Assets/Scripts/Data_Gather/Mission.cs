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

    public GameObject grid1;
    public GameObject grid2;

    // screen 
    public GameObject  TwoScreen;
    public GameObject  OneScreen;

    public GameObject Windmill;

    // init the datacollector
    private DataCollector dataCollectionIntance = new DataCollector();
    // init the raycaster for tracking
    private RaycastCounter raycastCounter = new RaycastCounter();
    // init the performance 
    //private GridManager gridManager = new GridManager();


    // still need to pass the data to the datacollector!
    public string name = "sofie";
    public string controlScheme = "scheme0";
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
   

    public int missionCombination = 0;
    
    public int Tutorial = 0;
    
    public GameObject Menu;

    public TMP_Text username;
    public TMP_Text mission_count;
    public TMP_Text mission_combi;
    public TMP_Text scheme;
    public TMP_Text ActiveScheme1;
    public TMP_Text ActiveScheme;

    private List<int> usedCombinations = new List<int>();

    private bool test = true;
    // timer 
    private float timer = 0.0f;

    // target time, 3 minutes = 180 sec
    private float targetTime = 90.0f; 


    private bool manualMenu = false;
    private bool test22 = false;

    private bool runMenu = true;
    public bool runTutorial = false;
    private bool runMission = false;
    private bool runDebugging = false;
    bool id = true;

    // 2 minuts per tutorial run
    private float Tutorial_Timer = 120;
    private float Inflight_Timer = 0.0f;

    private int first = 0;
    private int second = 0;
    bool selectVALIDCombo = true;

    bool testtest = true;

    bool check = true;

    void Start()
    {
        //performanceCleaning.GenerateGrid();
        dataCollectionIntance.type = missionCombination;
        InvokeRepeating("DataUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        InvokeRepeating("TrackingUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        InvokeRepeating("PerformanceUpdate", 0.05f, 0.05f); // call 1/20 a sec 
        
    }
   
    void FixedUpdate()
    {
        // user push button then ready for new mission, before that selects mission type

        username.text = "Name: " + name;
        mission_count.text = "Number: " + count + "/16";
        mission_combi.text = "Combi: " + "\n" + missionCombination;
        scheme.text = "Scheme active: " + "\n" + controlScheme;
        ActiveScheme1.text = controlScheme;
        ActiveScheme.text = controlScheme;
        // update timer
        timer += Time.fixedDeltaTime;

        // Four states, RUN:
        // Menu
        // Tutorial
        // Mission
        // Debug

        if(false)//runMenu)
        {
            Menu.SetActive(true);
            bool waithere = true;  
            if(id)
            {
                IDgenerator();
            }
            if(waithere && !id)
            {
                // 
               if(Input.GetKeyDown(KeyCode.Q))
               {
                    runTutorial = true;
                    runMission = false;
                    runDebugging = false;
                    Debug.Log("runTutorial");
               }
               if(Input.GetKeyDown(KeyCode.W))
               {
                    runTutorial = false;
                    runMission = true;
                    runDebugging = false;
                    Debug.Log("runMission");
                    waithere = false;
               }
               if(Input.GetKeyDown(KeyCode.E))
               {
                    runTutorial = false;
                    runMission = false;
                    runDebugging = true;
                    Debug.Log("runDebugging");
                    waithere = false;
               }


                if(runTutorial)
                {
                   waithere = RunTutorial();
    
                }


            }
            if(!waithere)
            {
                runMenu = false;
                Menu.SetActive(false);
            }
            
        }
        
           
        
        if (runTutorial)
        {   // parse this to a text box
            float timeLeft = Tutorial_Timer-timer;

            if(timer >= Tutorial_Timer)
            {
                // set the correct booleans for toggling the menu to be active again
                runMenu = true;
                // stop running the tutorial
                runTutorial = false;  
                Debug.Log("Times up!");
                 
            }
        }

        if(runMission)
        {
            // select mission combination
            if (selectCombination)
            {
                if(selectVALIDCombo)
                {
                    SelectMission();
                }
                
                if(!selectVALIDCombo)
                {
                    startMission = true;
                    selectCombination = false;
                    
                }
                inFlight = false;
                inCleaning = false;
            }

            // load that combination in
            if (startMission && Input.GetKeyDown(KeyCode.S))
            {
                LoadMission();
                startMission = false;
                inFlight = true;
            }
            
           
            // Update the inflight_timer aslong inflight
            if(inFlight)
            {
                Inflight_Timer = timer;
            }
                    
            if(inCleaning)
            {
                // give the user xxx time to wash the blade.
                if(timer >= (Inflight_Timer + targetTime))
                {
                    OnTimeOut();
                    selectCombination = true;
                }
            }

            // resets the current mission 
            if(Input.GetKeyDown(KeyCode.R))
            {
                ResetMission();
            }
        }
        

        if(Input.GetKeyDown(KeyCode.Alpha7))
        {
            missionCombination = 1;
            LoadMission();
        }
        if(Input.GetKeyDown(KeyCode.Alpha8))
        {
            missionCombination = 6;//5;
            LoadMission();
        }
        if(Input.GetKeyDown(KeyCode.Alpha9))
        {
            missionCombination = 11;//9;
            LoadMission();
        }
        
        if(true)//runDebugging)
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                // Debugging 
                dataCollectionIntance.SaveDataToCSV();
                dataCollectionIntance.ClearDataList();
                raycastCounter.SaveHitRecords();
                raycastCounter.ClearhitRecords();
                performanceCleaning.SaveToCSV();
                performanceCleaning.ClearGridData();
            }

            if(Input.GetKeyDown(KeyCode.M))
            {
                manualMenu = !manualMenu;
                Menu.SetActive(manualMenu);
            }
            // Debug.Log("grid 1");
            // //UnityEditor.TransformWorldPlacementJSON:{"position":{"x":395.4469909667969,"y":116.12999725341797,"z":638.197021484375},"rotation":{"x":0.0,"y":0.7071068286895752,"z":-0.7071068286895752,"w":0.0},"scale":{"x":1.0,"y":1.0,"z":1.0}}
            // performanceCleaning.width = 30;
            // performanceCleaning.height = 20;
            // //grid.transform.position = new Vector3(395,116,638);
            // //grid.transform.rotation = Quaternion.Euler(90,180,0);
            // grid.transform.position = new Vector3(399.839996f,131.279999f,638f);
            // grid.transform.rotation = Quaternion.Euler(90,180,0);

            if(testtest)
            {
                performanceCleaning.GenerateGrid();
                testtest = false;
            }
            
            if(Input.GetKeyDown(KeyCode.G))
            {
                test22 = !test22;
                //performanceCleaning.ClearGeneratedGrid();
                if(test22)
                {
                    Debug.Log("grid 1");
                    //UnityEditor.TransformWorldPlacementJSON:{"position":{"x":395.4469909667969,"y":116.12999725341797,"z":638.197021484375},"rotation":{"x":0.0,"y":0.7071068286895752,"z":-0.7071068286895752,"w":0.0},"scale":{"x":1.0,"y":1.0,"z":1.0}}
                    performanceCleaning.width = 30;
                    performanceCleaning.height = 20;
                    //grid.transform.position = new Vector3(395,116,638);
                    //grid.transform.rotation = Quaternion.Euler(90,180,0);
                    grid.transform.position = new Vector3(399.839996f,131.279999f,638f);
                    grid.transform.rotation = Quaternion.Euler(90,180,0);
                }
                else
                {
                    Debug.Log("grid 2");
                    performanceCleaning.width = 30;
                    performanceCleaning.height = 5;
                    grid.transform.position = new Vector3(397.26001f,116.07f,638.130005f);
                    grid.transform.rotation = Quaternion.Euler(35,0,0);
                }
                // clear old grid
                //performanceCleaning.ClearGeneratedGrid();
                // generate a new grid
                //performanceCleaning.GenerateGrid();
            }
        }
        



        // make the mission time, two timers, first for inflight 
        

    }

    private void IDgenerator()
    {   
              
        if (check) // Check if the number hasn't been used
        {
            check = false;
            first = Random.Range(10,100);
            second = Random.Range(10,100);
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            id = false;
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            check = true;
        }
        

        string Id_name = first.ToString() + second.ToString();
        Debug.Log("ID: " + Id_name);
        name = Id_name;
    }

    private bool RunTutorial()
    {

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            userInput.ManualControl = true;
            objectTransform.ControlScheme = 0;
            return false;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            userInput.ManualControl = false;
            objectTransform.ControlScheme = 1;
            return false;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            userInput.ManualControl = false;
            objectTransform.ControlScheme = 2;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void SelectMission()
    {
               
        missionCombination = Random.Range(1,17);
        if (!usedCombinations.Contains(missionCombination)) // Check if the number hasn't been used
        {
            usedCombinations.Add(missionCombination); // Add the new unique number to the list
            selectVALIDCombo = false; // Break the loop
            count += 1;
            selectCombination = false;
        }
    
    }

    private void LoadMission()
    {
        // Look up the combination
        ALLCombinations(missionCombination);
        Debug.Log("Mission combination: "+ missionCombination);

        dataCollectionIntance.type = missionCombination;
        // parse the combination to raycaster
        raycastCounter.type = missionCombination;
        raycastCounter.name = name;
        raycastCounter.controlScheme = controlScheme;
        raycastCounter.startPose = startPose;
        raycastCounter.gridLocation = gridLocation;
        raycastCounter.userInterface = userInterface;

        // parse the combination to performance
        performanceCleaning.type = missionCombination;
        performanceCleaning.name = name;
        performanceCleaning.controlScheme = controlScheme;
        performanceCleaning.startPose = startPose;
        performanceCleaning.gridLocation = gridLocation;
        performanceCleaning.userInterface = userInterface;

        // set start pose of both drones
        if(startPose == "start1")
        {
            // main_drone.transform.position = new Vector3(100,80,110);
            // main_drone.transform.rotation = Quaternion.Euler(0, 0, 0);
            // sec_drone.transform.position = new Vector3(100,80,100);
            // sec_drone.transform.rotation = Quaternion.Euler(0, 0, 0);
            Windmill.transform.rotation = Quaternion.Euler(0,-90,0);
            
        }
        if(startPose == "start2")
        {
            // main_drone.transform.position = new Vector3(100,80,110);
            // main_drone.transform.rotation = Quaternion.Euler(0, 0, 0);
            // sec_drone.transform.position = new Vector3(100,80,100);
            // sec_drone.transform.rotation = Quaternion.Euler(0, 0, 0);
            Windmill.transform.rotation = Quaternion.Euler(0,120,0);
        }

        // set grid location
        // grid should be pre-generated and set active 
        if(gridLocation == "grid1")
        {
            grid1.SetActive(true);
            grid2.SetActive(false);
            performanceCleaning.ClearboxList();
            performanceCleaning.PopulateBoxListFromExistingGrid();
            performanceCleaning.ClearGridData();
            //UnityEditor.TransformWorldPlacementJSON:{"position":{"x":395.4469909667969,"y":116.12999725341797,"z":638.197021484375},"rotation":{"x":0.0,"y":0.7071068286895752,"z":-0.7071068286895752,"w":0.0},"scale":{"x":1.0,"y":1.0,"z":1.0}}
            // performanceCleaning.width = 10;
            // performanceCleaning.height = 10;
            // grid.transform.position = new Vector3(395,116,638);
            // grid.transform.rotation = Quaternion.Euler(90,180,0);
        }
        if(gridLocation == "grid2")
        {
            grid1.SetActive(false);
            grid2.SetActive(true);
            performanceCleaning.ClearboxList();
            performanceCleaning.PopulateBoxListFromExistingGrid();
            performanceCleaning.ClearGridData();
            // performanceCleaning.width = 200;
            // performanceCleaning.height = 10;
            // grid.transform.position = new Vector3(395,116,638);
            // grid.transform.rotation = Quaternion.Euler(90,180,0);
        }
        // clear old grid
        //performanceCleaning.ClearGeneratedGrid();
        // generate a new grid
        //performanceCleaning.GenerateGrid();

        // set user interface

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

        // set control scheme

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

        // start the recording of data logging of:
        // overall data
        // head tracking

        missionActive = true;
        isRecording = true;

        // starting of performance logging of cleaning, is started then transcition from inflight to incleaning
       

    
    }

    private void OnTimeOut()
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
        // performance
        performanceCleaning.SaveToCSV();
        performanceCleaning.ClearGridData();
    }

    private void ResetMission()
    {
        Debug.Log("Mission is reset: " + missionCombination);
        timer = 0.0f;
        missionActive = false;
        startMission = true;
        isRecording = false;
        dataCollectionIntance.ClearDataList();
        raycastCounter.ClearhitRecords();
        performanceCleaning.ClearGridData();

        // add count of how many resets and i which mission
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
        if(isRecording)
        {
            //Debug.Log("TrackingUpdate called");
            raycastCounter.PerformRaycast();
        }
    }

    void PerformanceUpdate()
    {
        if(inCleaning && userInput.isSpraying)
        {
            //Debug.Log("performanceCleaning called");
            //performanceCleaning.UpdateBoxValues();
            performanceCleaning.UpdateBoxValuesWithRayCast();
        } 
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


     private void ALLCombinations(int combi)
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
             case 13:
                controlScheme = "scheme1";
                startPose = "start1";
                gridLocation = "grid1";
                userInterface = "1screen";
                break;
            case 14:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid1";
                userInterface = "1screen";
                break;
            case 15:
                controlScheme = "scheme1";
                startPose = "start2";
                gridLocation = "grid2";
                userInterface = "1screen";
                break;
            case 16:
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

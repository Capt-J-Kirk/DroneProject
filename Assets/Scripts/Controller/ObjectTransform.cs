using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectTransform: MonoBehaviour
{
    //&string obsDroneName = "Sec Drone";
    string obsDroneName = "Observing Drone";

    // Reference to the first GameObject
    public GameObject Quadcopter_main;

    // Reference to the second GameObject
    public GameObject Quadcopter_secondary;
    public QuadcopterController_sec ControlInput;

    // Transformation matrix for rotation and translation
    private Matrix4x4 transformationMatrix;

    // Set up transformation parameters
    private Vector3 translationVector =  new Vector3(0.0f, 0.0f, 0.0f); 
    private Vector3 rotationVector =  new Vector3(0.0f, 0.0f, 0.0f); 

    public float fixedDistance = 10.0f; // desired distance offset 
    // Fixed angles
    public float fixedYawDegrees = 20; // Fixed left-right angle
    public float fixedPitchDegrees = 20; // Fixed up-down angle
    private Vector3 direction; 

    public int ControlScheme = 0;

    private Vector3 main_position; //=  new Vector3(0.0f, 0.0f, 0.0f);
    private Quaternion main_rotation; // = new Quaternion.EulerAngels(0.0f, 0.0f, 0.0f);

    private Vector3 sec_position; // =  new Vector3(0.0f, 0.0f, 0.0f);
    private Quaternion sec_rotation; // = new Quaternion.Euler(0.0f, 0.0f, 0.0f);

    private float yaw2;
    private float pitch2;
    private float roll2;
    private float throttle2; // used to set the distance 
    private bool SettingParameterValues = false;
    private bool settingDistance = false;
    private bool settingYaw = false;
    private bool settingPitch = false;
    void Start()
    {
        // find the script for the secondary drone
        ControlInput = FindFirstObjectByType<QuadcopterController_sec>();
        // assigning drone objects
        Quadcopter_main = GameObject.Find("Washing Drone");
        Quadcopter_secondary = GameObject.Find(obsDroneName);

        // Ensure that a GameObject is assigned
        if (Quadcopter_main == null || Quadcopter_secondary == null)
        {
            Debug.LogError("Please assign the drones in the inspector!");
            return;
        }
    }

    // Update/fixedUpdate is the main loop
    void FixedUpdate()
    {
        GetPoses();
        GetParameterUpdate();
        if (!SettingParameterValues)
        {
            if (ControlScheme == 0)
            {
                Debug.Log("Changing to control scheme 1");
                Scheme_1();
            }
            if (ControlScheme == 1)
            {
                Debug.Log("Changing to control scheme 2");
                Scheme_2(yaw2,pitch2);
            }
        }
       

    }
    void GetPoses()
    {
        // Get the pose of the Quadcopter_main
        main_position = Quadcopter_main.transform.position;
        main_rotation = Quadcopter_main.transform.rotation;

        // Get the pose of the Quadcopter_secondary
        sec_position = Quadcopter_secondary.transform.position;
        sec_rotation = Quadcopter_secondary.transform.rotation;
    }
    void GetParameterUpdate()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            settingDistance = !settingDistance;
            Debug.Log("Updating fixedDistance");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            settingYaw = !settingYaw;
             Debug.Log("Updating fixedYaw");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            settingPitch = !settingPitch;
            Debug.Log("Updating fixedPitch");
        }

        if (settingDistance == true || settingPitch == true || settingYaw == true)
        {
            SettingParameterValues = true;
        }
        else
        {
            SettingParameterValues = false;
        }

        if (settingDistance)
        {
            UpdateFixedDistance();
        }
        if (settingPitch)
        {
            UpdatefixedPitchDegrees();
        }
        if (settingYaw)
        {
            UpdatefixedYawDegrees();
        }

    }
    void Scheme_1()
    {

        // calc direction from the sec drone to the main drone
        direction = (main_position - sec_position).normalized;

        // calc new position for Quadcopter_secondary
        Vector3 newPosition = sec_position - direction * fixedDistance;

        // calc the orintation (Quick FIX TO NOT GET ERROR) 
       // Vector3 newRotation = new Vector3.zero;

        //ApplyNewPose(newPosition, newRotation);

        // // Create a transformation matrix for the Quadcopter_main
        // Matrix4x4 poseMatrix1 = Matrix4x4.TRS(main_position, main_rotation, Vector3.one);

        // // Create the transformation matrix from the desired offset
        // Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotationVector));
        // Matrix4x4 translationMatrix = Matrix4x4.Translate(translationVector);

        // // Combine rotation and translation matrices
        // transformationMatrix = translationMatrix * rotationMatrix;

        // // Apply the transformation to get the pose of the second object
        // Matrix4x4 poseMatrix2 = transformationMatrix * poseMatrix1;

        // // Extract position and rotation from the resulting matrix
        // Vector3 position2 = poseMatrix2.GetColumn(3);
        // Quaternion rotation2 = Quaternion.LookRotation(poseMatrix2.GetColumn(2), poseMatrix2.GetColumn(1));

        // Find the QuadcopterController for the second drone
        //QuadcopterController_sec quadcopterController_2 = Quadcopter_secondary.GetComponent<QuadcopterController_sec>();
        //QuadcopterController_sec quadcopterController_2 = Quadcopter_secondary.GetComponent<QuadcopterController_sec>();       
    }

    void Scheme_2(float yaw, float pitch)
    {
        float orbitYawDegrees = yaw; // Actual orbit left-right angle
        float orbitPitchDegrees = pitch; // Actual orbit up-down angle

        // Convert orbit angles from degrees to radians for Unity calculations
        float orbitYawRadians = orbitYawDegrees * Mathf.Deg2Rad;
        float orbitPitchRadians = orbitPitchDegrees * Mathf.Deg2Rad;

        // Calculate new position for the secondary drone based on orbit angles and fixed distance
        Vector3 newPosition = new Vector3(
            fixedDistance * Mathf.Sin(orbitPitchRadians) * Mathf.Cos(orbitYawRadians),
            fixedDistance * Mathf.Cos(orbitPitchRadians),
            fixedDistance * Mathf.Sin(orbitPitchRadians) * Mathf.Sin(orbitYawRadians)
        ) + main_position;

        // calc the rotation facing direction using the fixed angles
        Quaternion newfixedRotation = Quaternion.Euler(fixedPitchDegrees, fixedYawDegrees, 0);

        // Set the secondary drone's new pose
        ApplyNewPose(newPosition,newfixedRotation);
         
    }


    void ApplyNewPose(Vector3 pos, Quaternion rot)
    {
   
        // Call the method to set desired position and rotation
        GetComponent<QuadcopterController_sec>().SetQuadcopterPose(pos, rot);
     
    }
    private void UpdateFixedDistance()
    {
        fixedDistance = throttle2;
    }
    private void UpdatefixedYawDegrees()
    {
        fixedYawDegrees = yaw2;
    }
    private void UpdatefixedPitchDegrees()
    {
        fixedPitchDegrees = pitch2;
    }
    public Vector3 GetRotationVector()
    {
        return rotationVector;
    }

    public Vector3 GetTranslationVector()
    {
        return translationVector;
    }
    public void SetControlScheme(int val)
    {
        ControlScheme = val;
    }
    public void SetTransformationParameters(Vector3 rotation, Vector3 translation)
    {
        translationVector = translation;
        rotationVector = rotation;
    }
    public void SetUserInput(float roll, float pitch, float yaw, float throttle)
    {
        // not actually used for such, but remapped
        yaw2 = yaw;
        pitch2 = pitch;
        throttle2 = throttle;
        roll2 = roll;
    }
}

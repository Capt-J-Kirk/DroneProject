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

    public int ControlScheme = 1;
    
    private Vector3 main_position; //=  new Vector3(0.0f, 0.0f, 0.0f);
    private Quaternion main_rotation; // = new Quaternion.EulerAngels(0.0f, 0.0f, 0.0f);

    private Vector3 sec_position; // =  new Vector3(0.0f, 0.0f, 0.0f);
    private Quaternion sec_rotation; // = new Quaternion.Euler(0.0f, 0.0f, 0.0f);
    private Quaternion Sec_nuetralOrientation; 

    private float yaw2;
    private float pitch2;
    private float roll2;
    private float throttle2; // used to set the distance 
    private bool SettingParameterValues = false;
    private bool settingDistance = false;
    private bool settingYaw = false;
    private bool settingPitch = false;
    public bool toggleDebug = false;

    // Used for control scheme 1
    public float radius = 5f; // Sphere's radius
    public float theta = 0f; // Horizontal angle
    public float phi = Mathf.PI / 2; // Vertical angle, starting vertically upwards
    private float prewYaw = 0f;
    private float yaw3 = 0f;
    public float newYaw = 0f;

    public bool toggleFollow = true;

    public bool changeInPosition = false;
    public int point = 0;
    private float starttime =0;
    private int count = 0;
    private float minRadius = 2f;
    private float maxRadius = 8f;

    private float minLow = 3f;
    private float maxHigh = 3f;

    private bool isReversing = false;
    private bool initStart = true;
    private float waypointTimer = 0.5f; // Time between waypoint changes
    private float timer; // Current timer
    private int currentWaypointIndex = 0;
    // Waypoints in spherical coordinates (theta, phi)
    private Vector2[] waypointsSpherical = new Vector2[]
    {
        new Vector2(-100, -10), // Point 1
        new Vector2(-155, -10), // Point 2
        new Vector2(155, 10),   // Point 3
        new Vector2(100, 10)    // Point 4
    };



    private float left_right = 0f; 
    private float up_down = 0f;  
        


    void Start()
    {
        // find the script for the secondary drone
        //ControlInput = FindFirstObjectByType<QuadcopterController_sec>();
        // assigning drone objects
        Quadcopter_main = GameObject.Find("Washing Drone");
        Quadcopter_secondary = GameObject.Find(obsDroneName);

        // Ensure that a GameObject is assigned
        if (Quadcopter_main == null || Quadcopter_secondary == null)
        {
            Debug.LogError("Please assign the drones in the inspector!");
            return;
        }
        Sec_nuetralOrientation = GetComponent<QuadcopterController_sec>().getneutralOrientation();
        // Sec_nuetralOrientation = Quadcopter_secondary.getneutralOrientation();
        
    }

    // Update/fixedUpdate is the main loop
    void FixedUpdate()
    {
        GetPoses();
        ChangeControlScheme();
        if (toggleFollow)
        {
            if (ControlScheme == 1)
            { 
                //Scheme_1_Spherical();
                Scheme_1_Cylindrical();
            }
            if (ControlScheme == 2)
            {
                // predefined spherical location, with varying radius and yaw for control
                Scheme_2();
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
    void ChangeControlScheme()
    {
        // key: 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Changing to control scheme 1");
            ControlScheme = 1;
        }
        // key: 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Changing to control scheme 2");
            ControlScheme = 2;
        }
        // key: L
        if (Input.GetKeyDown(KeyCode.L))
        {
            toggleFollow = !toggleFollow;
            if (toggleFollow)
            {
                Debug.Log("Drone: following!");
            }
            else
            {
                Debug.Log("Drone: NOT following!");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            changeInPosition = true;
            starttime = Time.deltaTime;
            count = 0;
            Debug.Log("changing pose " + changeInPosition);

            // if (point == 0)
            // {
            //     point = 1;
            // }
            // else
            // {
            //     point = 0;
            // }
        }
    }
    // void GetParameterUpdate()
    // {
    //     if (Input.GetKeyDown(KeyCode.J))
    //     {
    //         settingDistance = !settingDistance;
    //         Debug.Log("Updating fixedDistance");
    //     }
    //     if (Input.GetKeyDown(KeyCode.K))
    //     {
    //         settingYaw = !settingYaw;
    //          Debug.Log("Updating fixedYaw");
    //     }
    //     if (Input.GetKeyDown(KeyCode.L))
    //     {
    //         settingPitch = !settingPitch;
    //         Debug.Log("Updating fixedPitch");
    //     }

    //     if (settingDistance == true || settingPitch == true || settingYaw == true)
    //     {
    //         SettingParameterValues = true;
    //     }
    //     else
    //     {
    //         SettingParameterValues = false;
    //     }

    //     if (settingDistance)
    //     {
    //         UpdateFixedDistance();
    //     }
    //     if (settingPitch)
    //     {
    //         UpdatefixedPitchDegrees();
    //     }
    //     if (settingYaw)
    //     {
    //         UpdatefixedYawDegrees();
    //     }

    // }
    // float WrapAngle(float angle)
    // {
    //     while (angle > 180) angle -= 360;
    //     while (angle < -180) angle += 360;
    //     return angle;
    // }
    void updateSphericalParameters(float roll, float pitch, float throttle, float yaw)
    {
        // Spherical Orbit control 
        float thetaSensitivity = 2.0f;
        float phiSensitivity = 2.0f;
        float radiusSensitivity = 2.0f;

        theta += roll * thetaSensitivity; // horizontal movement
        phi += pitch * phiSensitivity; // vertical movement
        radius += throttle * radiusSensitivity; // change radius 
        
        radius = Mathf.Clamp(radius, minRadius, maxRadius);

        yaw3 += yaw;
    }

    void updateCylindricalParameters(float roll, float pitch, float throttle, float yaw)
    {
        // Spherical Orbit control 
        float thetaSensitivity = 2.0f;
        float phiSensitivity = 2.0f;
        float radiusSensitivity = 2.0f;

        radius += roll * phiSensitivity; // horizontal movement
        theta += pitch * thetaSensitivity; // vertical movement
        up_down += throttle * radiusSensitivity; // change radius 
        

        // ADD A BASELINE for up/down main_drones.y location
        up_down = Mathf.Clamp(up_down, main_position.y-minLow, maxHigh+main_position.y);
        radius = Mathf.Clamp(radius, minRadius, maxRadius);

        yaw3 += yaw;
    }
    void Scheme_1_Spherical()
    {
        // fejl fundet. fixed  yaw, so it dosn't rotate all the time. fixed it to nuatral point
        
        // Spherical Orbit control 
        // float thetaSensitivity = 2.0f;
        // float phiSensitivity = 2.0f;
        // float radiusSensitivity = 2.0f;

        // theta += roll * thetaSensitivity; // horizontal movement
        // phi += pitch * phiSensitivity; // vertical movement
        // radius += throttle * radiusSensitivity; // change radius 

        // Calculate desired positions on sphere
        float x = main_position.x + radius * Mathf.Sin(phi * Mathf.Deg2Rad) * Mathf.Cos(theta * Mathf.Deg2Rad);
        float y = main_position.y + radius * Mathf.Sin(phi * Mathf.Deg2Rad) * Mathf.Sin(theta * Mathf.Deg2Rad);
        float z = main_position.z + radius * Mathf.Cos(phi * Mathf.Deg2Rad);
        Vector3 targetPosition = new Vector3(x, y, z);
        //Debug.Log("targetPosition: " + targetPosition); 
        //Debug.Log("actual pos: " + sec_position);

 
        // test this out; xz is the horizontal plane and y is vertical 
        //float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
        //float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
        //float z = radius * Mathf.Cos(phi);

        // Addded Yaw orientation lock +- 45degs from lock point

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
        newYaw = Sec_nuetralOrientation.eulerAngles.y + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        newYaw = Mathf.Clamp(newYaw, -45f, 45f);
        //Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        

        // face towards the main drone
        Vector3 targetDirection = (new Vector3(0, 0, 0) - main_position).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);
        // Adding yaw adjustment 45-degree yaw
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        Quaternion targetOrientation  = yawRotation * baseRotation;

        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }

    void Scheme_1_Cylindrical()
    {
        // phi in this case is the vertical movement from the joystick
        // Calculate desired positions for cylindrical coordinates
        float x = main_position.x + radius * Mathf.Cos(theta * Mathf.Deg2Rad);
        float z = main_position.z + radius * Mathf.Sin(theta * Mathf.Deg2Rad);
        
        float y = main_position.y + up_down;
        
        Vector3 targetPosition = new Vector3(x, y, z);

        // Addded Yaw orientation lock +- 45degs from lock point

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
        newYaw = Sec_nuetralOrientation.eulerAngles.y + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        newYaw = Mathf.Clamp(newYaw, -45f, 45f);
        //Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        

        // face towards the main drone
        // use the alitude from the secondary drone itself
        Vector3 focusPoint = new Vector3(main_position.x, sec_position.y, main_position.z);

        Vector3 targetDirection = (focusPoint - sec_position).normalized;
        // may need to add Vector3.up
        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);
        // Adding yaw adjustment 45-degree yaw
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        Quaternion targetOrientation  = yawRotation * baseRotation;

        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }    

       void Scheme_2()
    {
        Vector3 targetPosition = new Vector3(0, 0, 0);

        if (initStart)
        {
            //targetPosition =  SphericalToCartesian(waypointsSpherical[0].x, waypointsSpherical[0].y, radius);
            targetPosition = PolarToCartesian(waypointsSpherical[currentWaypointIndex].x,radius);
            initStart = false;
        }


        // then flipping point
        if (changeInPosition)
        {
            timer += Time.deltaTime;

            if (timer >= waypointTimer)
            {
                timer = 0f; // Reset the timer
                //targetPosition = SphericalToCartesian(waypointsSpherical[currentWaypointIndex].x, waypointsSpherical[currentWaypointIndex].y, radius);
                targetPosition = PolarToCartesian(waypointsSpherical[currentWaypointIndex].x,radius);
                // Check the direction and update the waypoint index accordingly
                if (isReversing)
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex <= 0)
                    {
                        // at index 0
                        isReversing = false;
                        changeInPosition = false; // Stop moving 
                        currentWaypointIndex = 0; // Reset index
                    }
                }
                else
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypointsSpherical.Length)
                    {
                        // Reached the end point
                        currentWaypointIndex = waypointsSpherical.Length - 2;
                        isReversing = true;
                        changeInPosition = false; // Stop moving
                    }
                }
            }
        }
        else
        {
            //targetPosition = SphericalToCartesian(waypointsSpherical[currentWaypointIndex].x, waypointsSpherical[currentWaypointIndex].y, radius);
            targetPosition = PolarToCartesian(waypointsSpherical[currentWaypointIndex].x,radius);
        }
     
        // Addded Yaw orientation lock +- 45degs from lock point

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
        newYaw = Sec_nuetralOrientation.eulerAngles.y + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        newYaw = Mathf.Clamp(newYaw, -45f, 45f);
        //Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        

        // face towards the main drone
        // use the alitude from the secondary drone itself
        Vector3 focusPoint = new Vector3(main_position.x, sec_position.y, main_position.z);

        Vector3 targetDirection = (focusPoint - sec_position).normalized;
        // may need to add Vector3.up        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);
        // Adding yaw adjustment 45-degree yaw
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        Quaternion targetOrientation  = yawRotation * baseRotation;
        
         
        ApplyNewPose(targetPosition, targetOrientation);

    }


    private Vector3 SphericalToCartesian(float theta, float phi, float radius)
    {
        // Convert angles from degrees to radians
        float phiRadian = phi * Mathf.Deg2Rad;
        float thetaRadian = theta * Mathf.Deg2Rad;

        // Calculate Cartesian coordinates
        float x = main_position.x + radius * Mathf.Sin(phiRadian) * Mathf.Cos(thetaRadian);
        float y = main_position.y + radius * Mathf.Sin(phiRadian) * Mathf.Sin(thetaRadian);
        float z = main_position.z + radius * Mathf.Cos(phiRadian);

        return new Vector3(x, y, z);
    }



    
    private Vector3 PolarToCartesian(float theta, float radius)
    {
        // Calculate Cartesian coordinates
        float x = main_position.x + radius * Mathf.Cos(theta * Mathf.Deg2Rad);
        float z = main_position.z + radius * Mathf.Sin(theta * Mathf.Deg2Rad);


        // set the hight offset
        float y = main_position.y + 1f;

        return new Vector3(x, y, z);
    }



    private void ApplyNewPose(Vector3 pos, Quaternion rot)
    {
   
        // Call the method to set desired position and rotation
        if (false)
        {
            Debug.Log("pos to sec drone: " + pos);
            Debug.Log("rot to sec drone: " + rot);
        }
        GetComponent<QuadcopterController_sec>().SetQuadcopterPose(pos, rot);
     
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

        updateSphericalParameters(roll, pitch, throttle, yaw);

        if (toggleDebug)
        {
            Debug.Log("yaw transform: " + yaw);
            Debug.Log("pitch transform: " + pitch);
            Debug.Log("throttle transform: " + throttle);
            Debug.Log("roll transform: " + roll);
        }
    }
}

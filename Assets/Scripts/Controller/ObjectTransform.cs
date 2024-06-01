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


    // Set up transformation parameters
    private Vector3 translationVector =  new Vector3(0.0f, 0.0f, 0.0f); 
    private Vector3 rotationVector =  new Vector3(0.0f, 0.0f, 0.0f); 

  

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

    public bool toggleDebug = false;

    // Used for control scheme 1
    public float radius = 5f; // Sphere's radius
    public float theta = 0f; // Horizontal angle
    public float phi = Mathf.PI / 2; // Vertical angle, starting vertically upwards
    public float prewYaw = 0f;
    private float yaw3 = 0f;
    public float newYaw = 0f;

    public bool toggleFollow = true;

    public bool changeInPosition = false;
    public int point = 0;
    private float starttime =0;
    private int count = 0;
    private float minRadius = 3f;
    private float maxRadius = 6f;

    private float minLow = 3f;
    private float maxHigh = 1.5f;

    public bool InitTheta = true;

    private bool isReversing = false;
    private bool initStart = true;
    //private float waypointTimer = 0.5f; // Time between waypoint changes
    private float timer; // Current timer


    private float maxAllowedYaw = 75f;

    //private float left_right = 0f; 
    private float up_down = 0f;  
    float currentAngle = 0f;

    //public float testAngle = 45f;

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
        if(Input.GetKeyDown(KeyCode.H))
        {
            resetYaw();
            //changeInPosition =true;
            Debug.Log("changeInPosition " + changeInPosition);
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

           
        }
    }
   
    public void resetYaw()
    {
        newYaw = 0f;
    }
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

    void Scheme_1_Spherical()
    {
        
        
  
        radius = Mathf.Clamp(radius, minRadius, maxRadius);
        // Calculate desired positions on sphere
        float x = main_position.x + radius * Mathf.Sin(phi * Mathf.Deg2Rad) * Mathf.Cos(theta * Mathf.Deg2Rad);
        float y = main_position.y + radius * Mathf.Sin(phi * Mathf.Deg2Rad) * Mathf.Sin(theta * Mathf.Deg2Rad);
        float z = main_position.z + radius * Mathf.Cos(phi * Mathf.Deg2Rad);
        Vector3 targetPosition = new Vector3(x, y, z);
       
        // Addded Yaw orientation lock +- 45degs from lock point

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
        newYaw = Sec_nuetralOrientation.eulerAngles.y + prewYaw + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        newYaw = Mathf.Clamp(newYaw, -45f, 45f);
        //Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        

        // face towards the main drone
        Vector3 targetDirection = (new Vector3(0, 0, 0) - main_position).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);
        // Adding yaw adjustment 45-degree yaw
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        //Quaternion targetOrientation  = yawRotation * baseRotation;
        Quaternion targetOrientation = baseRotation;

        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }

    void updateCylindricalParameters(float roll, float pitch, float throttle, float yaw)
    {
        // Spherical Orbit control 
        float thetaSensitivity = 2.0f;
        float phiSensitivity = 2.0f;
        float radiusSensitivity = 4.0f;
        float minAngle = 170f;
        float maxAngle = 370f;

        // filter out small noise values from joystick
        if (Mathf.Abs(roll) < 0.4f) roll = 0f;
        if (Mathf.Abs(pitch) < 0.4f) pitch = 0f;
        if (Mathf.Abs(yaw) < 0.4f) yaw = 0f;
        if (Mathf.Abs(throttle) < 0.4f) throttle = 0f;


        radius += roll * phiSensitivity; // horizontal movement
        theta += pitch * thetaSensitivity; // vertical movement
        // limit theta angle
        theta = Mathf.Clamp(theta, minAngle, maxAngle);
        up_down = throttle * radiusSensitivity; // change radius 
        

        
        //up_down = Mathf.Clamp(main_position.y + up_down, main_position.y-minLow, maxHigh+main_position.y);
        radius = Mathf.Clamp(radius, minRadius, maxRadius);

        yaw3 = yaw;
    }
    void Scheme_1_Cylindrical()
    {
        if(InitTheta)
        {
            // start the follow mode at:
            theta = 270f;
            InitTheta = false;
        }
        // phi in this case is the vertical movement from the joystick
        // Calculate desired positions for cylindrical coordinates


        Vector3 targetPosition = newPolarToCartesian(theta,radius);
        
        // added the sec drones y component
        float temp = sec_position.y + up_down;
        float y = Mathf.Clamp(temp, main_position.y-maxHigh, main_position.y+maxHigh);

        targetPosition.y = y;




        Vector3 focusPoint = new Vector3(main_position.x, sec_position.y, main_position.z);
        Vector3 targetDirection = (focusPoint - sec_position).normalized;

        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);



        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
     
        newYaw = Sec_nuetralOrientation.eulerAngles.y + prewYaw + (yaw3 * yawSensitivity); // yaw can freely move

        // Correct any potential wrap-around issues
        newYaw = newYaw % 360;

        // face towards the main drone
        // use the alitude from the secondary drone itself

       
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        Quaternion targetOrientation  = yawRotation * baseRotation;
       
        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }    

    void Scheme_2()
    {
        Vector3 targetPosition = new Vector3(0, 0, 0);
        float angle1 = 170f;
        float angle2 = 370f;
        

        if (initStart)
        {
            //targetPosition = PolarToCartesian(angle1,radius);
            initStart = false;
            Debug.Log("currentAngle " + angle1);
            currentAngle = angle1;
        }

        // then flipping point
        if (changeInPosition)
        {
            // Check the direction and update accordingly
            if (isReversing)
            {
                currentAngle -= 1f;
                targetPosition = newPolarToCartesian(currentAngle,radius);
                if (currentAngle <= angle1)
                {
                    // 
                    isReversing = false;
                    changeInPosition = false; // Stop moving 
                    currentAngle = angle1;
                }
            }
            else
            {
                currentAngle +=1f;
                targetPosition = newPolarToCartesian(currentAngle,radius);
                if (currentAngle >= angle2)
                {
                    // Reached the end point
                    isReversing = true;
                    changeInPosition = false; // Stop moving
                    currentAngle = angle2;
                }
            }
            resetYaw();
            
        }
        else
        {
            targetPosition = newPolarToCartesian(currentAngle,radius);
        }
     
        // Addded Yaw orientation lock +- 45degs from lock point
        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        // update prewious Yew
        prewYaw = newYaw;
        // possible add previous yaw, such it dosn't reset
        newYaw = Sec_nuetralOrientation.eulerAngles.y + prewYaw + (yaw3 * yawSensitivity); // yaw can freely move
       
        
    
        // face towards the main drone
        // use the alitude from the secondary drone itself
        Vector3 focusPoint = new Vector3(main_position.x, sec_position.y, main_position.z);

        Vector3 targetDirection = (focusPoint - sec_position).normalized;
        // may need to add Vector3.up       
        Quaternion baseRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        // Adding yaw adjustment 45-degree yaw
        Quaternion yawRotation = Quaternion.Euler(0, newYaw, 0); 
        Quaternion targetOrientation  = yawRotation * baseRotation;
        
         
        ApplyNewPose(targetPosition, targetOrientation);

    }

    
    private Vector3 PolarToCartesian(float theta, float radius)
    {
        // Normalize the angle to stay within 0 to 360 degrees
        theta %= 360;
        // Calculate Cartesian coordinates
        float x = main_position.x + radius * Mathf.Cos(theta * Mathf.Deg2Rad);
        float z = main_position.z + radius * Mathf.Sin(theta * Mathf.Deg2Rad);


        // set the hight offset
        float y = main_position.y;

        return new Vector3(x, y, z);
    }

    private Vector3 newPolarToCartesian(float theta, float radius)
    {
        // Convert polar coordinates to Cartesian, but offset around main drone
        // Normalize angle within 0-360 degrees
        theta %= 360;  
        Vector3 offset = new Vector3(
            radius * Mathf.Cos(theta * Mathf.Deg2Rad),
            0,  
            radius * Mathf.Sin(theta * Mathf.Deg2Rad)
        );

        offset = main_rotation * offset;
        return main_position + offset;
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
        updateCylindricalParameters(roll, pitch, throttle, yaw);
        //updateSphericalParameters(roll, pitch, throttle, yaw);

        if (toggleDebug)
        {
            Debug.Log("yaw transform: " + yaw);
            Debug.Log("pitch transform: " + pitch);
            Debug.Log("throttle transform: " + throttle);
            Debug.Log("roll transform: " + roll);
        }
    }
}

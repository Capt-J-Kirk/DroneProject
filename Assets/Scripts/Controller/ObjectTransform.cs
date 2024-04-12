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

    public bool toggleFollow = true;

    public bool changeInPosition = false;
    public int point = 0;
    private float starttime =0;
    private int count = 0;
    private float minRadius = 1.5f;
    private float maxRadius = 8f;

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
                // Spherical full control 
                Scheme_1();
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

            if (point == 0)
            {
                point = 1;
            }
            else
            {
                point = 0;
            }
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

        yaw3 = yaw;
    }
    void Scheme_1()
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

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        float newYaw = Sec_nuetralOrientation.eulerAngles.y + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        
        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }

    // void Scheme_2(float yaw, float pitch)
    // {
    //     float orbitYawDegrees = yaw; // Actual orbit left-right angle
    //     float orbitPitchDegrees = pitch; // Actual orbit up-down angle

    //     // Convert orbit angles from degrees to radians for Unity calculations
    //     float orbitYawRadians = orbitYawDegrees * Mathf.Deg2Rad;
    //     float orbitPitchRadians = orbitPitchDegrees * Mathf.Deg2Rad;

    //     // Calculate new position for the secondary drone based on orbit angles and fixed distance
    //     Vector3 newPosition = new Vector3(
    //         fixedDistance * Mathf.Sin(orbitPitchRadians) * Mathf.Cos(orbitYawRadians),
    //         fixedDistance * Mathf.Cos(orbitPitchRadians),
    //         fixedDistance * Mathf.Sin(orbitPitchRadians) * Mathf.Sin(orbitYawRadians)
    //     ) + main_position;

    //     // calc the rotation facing direction using the fixed angles
    //     Quaternion newfixedRotation = Quaternion.Euler(fixedPitchDegrees, fixedYawDegrees, 0);

    //     // Set the secondary drone's new pose
    //     ApplyNewPose(newPosition,newfixedRotation);
         
    // }

    void Scheme_2()
    {
        // {theta, phi}
        Vector2 leftPoint = new Vector2(-10,-100);//Vector2(45, 20);
        Vector2 rightPoint = new Vector2(10, 100);//Vector2(-45, 20);

        float phiFixed = 0;
        float thetaFixed = 0;

        int[] ThetaAngles = {-100, -155, 155, 100};//{45, -85, -120, -160, 160, 120, 85, 45};
        int[] phiAngles = {-10, -10, 10, 10};
        int i = 4;
        Vector3 targetPosition = new Vector3(0, 0, 0);
;

        if(point == 0)
        {
            phiFixed = leftPoint.y * Mathf.Deg2Rad;
            thetaFixed = leftPoint.x * Mathf.Deg2Rad;
        }
        else
        {
            phiFixed = rightPoint.y * Mathf.Deg2Rad;
            thetaFixed = rightPoint.x * Mathf.Deg2Rad;
        }

        if (false)//changeInPosition)
        {
            // using slerp to interpolate between the two points
            if (point == 0)
            {
                // // Calculate desired positions
                // float x = main_position.x + radius * Mathf.Sin(leftPoint.y) * Mathf.Cos(leftPoint.x);
                // float y = main_position.y + radius * Mathf.Sin(leftPoint.y) * Mathf.Sin(leftPoint.x);
                // float z = main_position.z + radius * Mathf.Cos(leftPoint.y);
                // Vector3 newPosition = new Vector3(x, y, z);


                // // Calculate old positions
                // float x = main_position.x + radius * Mathf.Sin(rightPoint.y) * Mathf.Cos(rightPoint.x);
                // float y = main_position.y + radius * Mathf.Sin(rightPoint.y) * Mathf.Sin(rightPoint.x);
                // float z = main_position.z + radius * Mathf.Cos(rightPoint.y);
                // Vector3 oldPosition = new Vector3(x, y, z);
                if ((Time.deltaTime - starttime >= 0.5f) && (count < 8)) 
                {
                    float x = main_position.x + radius * Mathf.Sin(phiAngles[i -count] * Mathf.Deg2Rad) * Mathf.Cos(ThetaAngles[i - count] * Mathf.Deg2Rad);
                    float y = main_position.y + radius * Mathf.Sin(phiAngles[i -count] * Mathf.Deg2Rad) * Mathf.Sin(ThetaAngles[i - count] * Mathf.Deg2Rad);
                    float z = main_position.z + radius * Mathf.Cos(phiAngles[i -count] * Mathf.Deg2Rad);
                    targetPosition = new Vector3(x, y, z);

                    count +=1;
                    starttime = Time.deltaTime;

                    if (count == 8)
                    {
                        changeInPosition = false;
                    }
                }

            }
            if (point == 1)
            {
                // // Calculate desired positions
                // float x = main_position.x + radius * Mathf.Sin(rightPoint.y) * Mathf.Cos(rightPoint.x);
                // float y = main_position.y + radius * Mathf.Sin(rightPoint.y) * Mathf.Sin(rightPoint.x);
                // float z = main_position.z + radius * Mathf.Cos(rightPoint.y);
                // Vector3 newPosition new Vector3(x, y, z);

                // // Calculate old positions
                // float x = main_position.x + radius * Mathf.Sin(leftPoint.y) * Mathf.Cos(leftPoint.x);
                // float y = main_position.y + radius * Mathf.Sin(leftPoint.y) * Mathf.Sin(leftPoint.x);
                // float z = main_position.z + radius * Mathf.Cos(leftPoint.y);
                // Vector3 oldPosition = new Vector3(x, y, z);

                 if ((Time.deltaTime - starttime >= 0.5f) && (count < 8)) 
                {
                    float x = main_position.x + radius * Mathf.Sin(phiAngles[count] * Mathf.Deg2Rad) * Mathf.Cos(ThetaAngles[count] * Mathf.Deg2Rad);
                    float y = main_position.y + radius * Mathf.Sin(phiAngles[count] * Mathf.Deg2Rad) * Mathf.Sin(ThetaAngles[count] * Mathf.Deg2Rad);
                    float z = main_position.z + radius * Mathf.Cos(phiAngles[count] * Mathf.Deg2Rad);
                    targetPosition = new Vector3(x, y, z);

                    count +=1;
                    starttime = Time.deltaTime;

                    if (count == 8)
                    {
                        changeInPosition = false;
                    }
                }
            }

            
            //Vector3 targetPosition = StartCoroutine(InterpolatePositionOverTime(oldPosition, newPosition, 5f));

        }
        else
        {
            // THEN NOT CHANGING POINT
            // Calculate desired positions
            float x = main_position.x + radius * Mathf.Sin(phiFixed) * Mathf.Cos(thetaFixed);
            float y = main_position.y + radius * Mathf.Sin(phiFixed) * Mathf.Sin(thetaFixed);
            float z = main_position.z + radius * Mathf.Cos(phiFixed);
            targetPosition = new Vector3(x, y, z);
        }
        

        // Calculate desired orientation
        float yawSensitivity = 5.0f;
        float newYaw = Sec_nuetralOrientation.eulerAngles.y + (yaw3 * yawSensitivity); // yaw can freely move
        //newYaw = WrapAngle(newYaw);
        Quaternion targetOrientation = Quaternion.Euler(0, newYaw, 0);
        
        // Apply to drone controller
        ApplyNewPose(targetPosition, targetOrientation);
    }

    // IEnumerator InterpolatePositionOverTime(Vector3 start, Vector3 end, float duration)
    //     {
    //         float elapsed = 0f;

    //         if (elapsed < duration)
    //         {
    //             float t = elapsed / duration;
    //             elapsed += Time.deltaTime;
    //             return  Vector3.slerp(start, end, t);
    //         }
    //         else
    //         {
    //             // can first change position after the transsition is done
    //             changeInPosition = false;
    //         }
    //     }
    // }


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
    // private void UpdateFixedDistance()
    // {
    //     fixedDistance = throttle2;
    // }
    // private void UpdatefixedYawDegrees()
    // {
    //     fixedYawDegrees = yaw2;
    // }
    // private void UpdatefixedPitchDegrees()
    // {
    //     fixedPitchDegrees = pitch2;
    // }
    // public Vector3 GetRotationVector()
    // {
    //     return rotationVector;
    // }

    // public Vector3 GetTranslationVector()
    // {
    //     return translationVector;
    // }
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

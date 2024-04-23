using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class QuadcopterController: MonoBehaviour
{
    private float maxVelocity = 200f; // Maximum linear velocity
    private float maxAcceleration = 20f; // Maximum linear acceleration
    private float drag = 0.5f; // Drag coefficient
    //private float mass = 5f; // Mass of the quadcopter
    private float maxAngularVelocity = 5f; // Maximum angular velocity
    //private float maxAngularAcceleration = 2f; // Maximum angular acceleration
    //public Vector3 thrustForce = new Vector3(0f, 5f, 0f); // Thrust force
    //private float speed = 5.0f;

    // Max angle
    public float maxYaw = 20.0f;
    public float maxRoll = 10.0f;
    public float maxPitch = 10.0f; 

    // Max velocity
    private float maxVelYaw = 100.0f;
    private float maxVelRoll = 100.0f;
    private float maxVelPitch = 100.0f; 
    private float gravityComp = 0f;
    private float prewYaw = 0f;
    public bool toggleDebug = false;


    public Rigidbody rb;
    public PIDController AltitudePID;
    public PIDController rollPIDQuaternion;
    public PIDController pitchPIDQuaternion;
    public PIDController yawPIDQuaternion;
    public PIDController xPID;
    public PIDController zPID;

    public UserInput inputController;


    // Tested to be good values
    //private float rollKp = 5.0f, rollKi = 0.3f, rollKd = 0.08f;
    private float rollKp = 5.0f, rollKi = 0.3f, rollKd = 0.58f;
    //private float pitchKp = 5.0f, pitchKi = 0.3f, pitchKd = 0.08f;
    private float pitchKp = 5.0f, pitchKi = 0.3f, pitchKd = 0.58f; 
    //private float yawKp = 5.0f, yawKi = 0.3f, yawKd = 0.08f; 
    private float yawKp = 5.0f, yawKi = 0.3f, yawKd = 0.58f;
    private float altitudeKp = 7.82403f, altitudeKi = 18.87807f, altitudeKd = 5f; 
    //private float xKp = 5.0f, xKi = 0.3f, xKd = 0.08f; 
    private float xKp = 7.82403f, xKi = 18.87807f, xKd = 5f; 
    //private float zKp = 5.0f, zKi = 0.3f, zKd = 0.08f; 
    private float zKp = 7.82403f, zKi = 18.87807f, zKd = 5f; 
    // // needs fine tuning! 
    // public float rollKp = 1.0f, rollKi = 0.1f, rollKd = 0.01f;
    // public float pitchKp = 1.0f, pitchKi = 0.1f, pitchKd = 0.01f;
    // public float yawKp = 1.0f, yawKi = 0.1f, yawKd = 0.01f;
    // public float altitudeKp = 2.0f, altitudeKi = 0.1f, altitudeKd = 0.01f;    

    // desired pose
    public Vector3 desiredPosition;
    public Vector3 desiredEulerAngles; 
    public Quaternion desiredOrientation;
    // Neutral plane for calc max rotation limit (gyro)
    Quaternion neutralOrientation;


    int delay = 0;
    // calc desired pose

    public GameObject Visual_Quadcopter_main;
    

    // object avoidance
    public Transform windblade;
    private Collider targetCollider; // Collider of the target object
    private string currentState = "No Danger";
    public float distanceToObject = 0;
    Vector3 closestPoint;

    // UserInterface's infobars TWO SCREEN 
    public TMP_Text TWO_t_main_dist;
    public Image TWO_i_main_dist;

    // UserInterface's infobars ONE SCREEN 
    public TMP_Text ONE_t_main_dist;
    public Image ONE_i_main_dist;


    // add blinking functionality
    private bool shouldBlink = false;


    public float xlocalChange = 0f;
    public float ylocalChange = 0f;
    public float zlocalChange = 0f;


    // Tuning 
    public bool isTuning = false;
    private float tuningKpIncrement = 0.1f;
    private float lastOscillationTime;
    private float lastOutput;
    private int oscillationCounter = 0;
    public float Ku; // Ultimate gain
    public float Pu; // Oscillation period
    private bool tuningStarted = false;
    private bool stepInputApplied = false;


    float newPositionChangeX = 0f;
    float newPositionChangeZ = 0f;

    void StartTuningPID()
    {
        // Set the flag to start the tuning process
        tuningStarted = true;

        // Starting the tuning process for altitude PID as an example
        isTuning = true;
        // pitchPIDQuaternion.proportionalGain = 0f;
        // pitchPIDQuaternion.integralGain = 0f;
        // pitchPIDQuaternion.derivativeGain = 0f;

        // Reset tuning parameters
        stepInputApplied = false;
        lastOscillationTime = Time.time;
        lastOutput = rb.transform.rotation.eulerAngles.x;
        oscillationCounter = 0;
    }

    void TunePID()
    {
        // Ziegler-Nichols tuning 
        // Use the altitude PID as an example, replace with other PID instances as needed
        // Apply a step input, i.e., set a desired altitude that is higher than current to induce error
        //desiredPosition.y += 5.0f; // This is a step change to start the tuning process
        
        if (!tuningStarted)
            return;

        

        // Measure the response
        float currentOutput = rb.transform.position.y;
        float currentTime = Time.time;

        // Check for oscillations by seeing if the output crosses the last output
        if ((lastOutput < desiredOrientation.eulerAngles.x && currentOutput >= desiredOrientation.eulerAngles.x) ||
            (lastOutput > desiredOrientation.eulerAngles.x && currentOutput <= desiredOrientation.eulerAngles.x))
        {
            // We have an oscillation
            Pu = currentTime - lastOscillationTime; // Measure the period
            lastOscillationTime = currentTime;
            oscillationCounter++;

            // After a few oscillations, assume we have found the ultimate gain
            if (oscillationCounter > 2)
            {
                // The current Kp is approximately the ultimate gain
                Ku = pitchPIDQuaternion.proportionalGain;

                // Stop the tuning
                isTuning = false;

                // Set PID parameters based on Ziegler-Nichols formulas
                pitchPIDQuaternion.proportionalGain = 0.6f * Ku;
                pitchPIDQuaternion.integralGain = 2f * pitchPIDQuaternion.proportionalGain / Pu;
                pitchPIDQuaternion.derivativeGain = pitchPIDQuaternion.proportionalGain * Pu / 8f;

                // Log results for verification
                Debug.Log($"Tuning complete: Ku = {Ku}, Pu = {Pu}");
                Debug.Log($"Tuning complete: Kp = {pitchPIDQuaternion.proportionalGain}, Ki = { pitchPIDQuaternion.integralGain}, Kd = { pitchPIDQuaternion.derivativeGain}");
            }
        }

        // Increment Kp after every oscillation and before reaching ultimate gain
        if (isTuning && oscillationCounter <= 2)
        {
            pitchPIDQuaternion.proportionalGain += tuningKpIncrement;
        }

        lastOutput = currentOutput;
    }

    private void TestingDesiredPose()
    {
        // used for PID tuning! 

        // Euler angles
        float newPitch = 5f;
        float newRoll = 0;
        float newYaw = 0;
           
        //Debug.Log("newPitch: " + newPitch);
        //Debug.Log("newRoll: " + newRoll);
        //Debug.Log("newYaw: " + newYaw);
        //Debug.Log("newYaw: " + newYaw);
        Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;
        newPitch = Mathf.Clamp(neutralEulerAngles.x + newPitch, -maxPitch, maxPitch);
        newRoll = Mathf.Clamp(neutralEulerAngles.z + newRoll, -maxRoll, maxRoll);

        newYaw = neutralEulerAngles.y + newYaw; // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        // newPitch = WrapAngle(newPitch);
        // newRoll = WrapAngle(newRoll);
        // newYaw = WrapAngle(newYaw);
        // update prewious Yew
        //prewYaw = newYaw;
        //Debug.Log("DesiredPitch: " + newPitch);
        //Debug.Log("DesiredYaw: " + newYaw);
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);
        // Debug.Log("altitudeKp: " + altitudeKp);
        // Debug.Log("altitudeKi: " + altitudeKi);
        // Debug.Log("altitudeKd: " + altitudeKd);

        //Vector3 currentDesiredPosition = new Vector3(0, 80f, 0);
        //Debug.Log("currentDesiredPosition: " + currentDesiredPosition);
        //Vector3 newPositionChange = Vector3.up * (currentDesiredPosition.y + 10f); // Assumes altitudeChange controls vertical movement
        //Debug.Log("newPositionChange: " + newPositionChange);
        //desiredPosition = newPositionChange;
        
        float xSensitivity = 1.0f;
        float ySensitivity = 1.0f;
        float zSensitivity = 1.0f;



        Vector3 newPositionChangeLocal = new Vector3(
            xlocalChange * xSensitivity,
            ylocalChange * ySensitivity,
            zlocalChange * zSensitivity);

        newPositionChangeX = xlocalChange * xSensitivity;
        // float newPositionChangeY = ylocalChange * ySensitivity;
        newPositionChangeZ = zlocalChange * zSensitivity;

        // Convert local position change to world space
        Vector3 newPositionChangeWorld = transform.TransformPoint(newPositionChangeLocal) - transform.position;
        Vector3 currentDesiredPosition = desiredPosition;
        //Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
        // Update desired position
        desiredPosition = currentDesiredPosition + newPositionChangeWorld;

    }
    private void new_CalcDesiredPose(float rollChange, float pitchChange, float yawChange, float throttleChange)
    {
        // Sensitivity factors (determine how much each input affects the pose)
        float pitchSensitivity = 10.0f;
        float pitch_x = 2.0f;
        float rollSensitivity = 10.0f;
        float roll_z = 2.0f;
        float yawSensitivity = 5.0f;
        float altitudeSensitivity = 0.50f;
        float newPitch = 0;
        float newRoll = 0;
        float xSensitivity = 0.50f;
        float ySensitivity = 0.50f;
        float zSensitivity = 0.50f;

        // quick mapping
        float xlocalChange = pitchChange;
        float ylocalChange = throttleChange;
        float zlocalChange = rollChange;

        float safeDistance = 1.2f;

        // // Converting neutral orientation from Quaternion to euler angles
         Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;

        // // Calculate new angles by applying the userInput changes to the neutral orientation and limiting the angle 
        // float newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        // float newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
        // float newYaw = neutralEulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move
        Vector3 newPositionChange = new Vector3(0,0,0);


        Vector3 currentDesiredPosition = desiredPosition;
        float newYaw = 0;
        // find the distance to windblade
        Vector3 localPos = transform.InverseTransformPoint(windblade.position);

     

        if (Mathf.Abs(localPos.z) < safeDistance)
        {
            // Target is in the forward or backward local space
            if (localPos.z > 0)
            {
                // Object is in forward direction
                Debug.Log("Object is too close in forward direction! Moving backward.");
                newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, 0);
            }
            else
            {
                // Object is in backward direction
                Debug.Log("Object is too close in backward direction! Moving forward.");
                newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), 0, maxRoll);
            }
        }
        else{
            if (rollChange == 0)
            {
                newRoll = neutralOrientation.eulerAngles.z;
                // To cancel out the left/right momentum, make a Impuls for a counter momentum
                //Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
                //ApplyCounterTorque(counterTorque);
                
            }
            else
            {
                newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
                //float newRoll = Mathf.Clamp(rollChange * rollSensitivity, -maxRoll, maxRoll);
                //newRoll = WrapAngle(newRoll);
            }
            //newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
        }
        
        // Check if the target is within the safe distance in the right direction
        if (Mathf.Abs(localPos.x) < safeDistance)
        {
            // Target is in the right or left local space
            if (localPos.x > 0)
            {
                // Object is in right direction
                Debug.Log("Object is too close in right direction! Moving left.");
                newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, 0);
            }
            else
            {
                // Object is in left direction
                Debug.Log("Object is too close in left direction! Moving right.");
                newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), 0, maxPitch);
            }
        }
        else{
            if (pitchChange == 0)
            {
                // No input detected, set desiredOrientation to neutralOrientation
                newPitch = neutralOrientation.eulerAngles.x;

                // To cancel out the forward momentum, make a Impuls for a counter momentum 
                //Vector3 counterTorque = new Vector3(-1*rb.angularVelocity.x,0,0);
                //ApplyCounterTorque(counterTorque);
            
            }
            else
            {
                // Calculate new angles directly within bounds
                newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
                //float newPitch = Mathf.Clamp(pitchChange * pitchSensitivity, -maxPitch, maxPitch);
                //newPitch = WrapAngle(newPitch);
            }
            //newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        }

        // Check if the target is within the safe distance in the up direction
        if (Mathf.Abs(localPos.y) < safeDistance)
        {
            // Target is in the up or down local space
            if (localPos.y > 0)
            {
                // Object is in up direction
                Debug.Log("Object is too close in up direction! Moving down.");
                newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
                desiredPosition = currentDesiredPosition + newPositionChange;
                desiredPosition.y = Mathf.Clamp(currentDesiredPosition.y + newPositionChange.y, Mathf.Abs(currentDesiredPosition.y)-100f, Mathf.Abs(currentDesiredPosition.y));
            }
            else
            {
                // Object is in down direction
                Debug.Log("Object is too close in down direction! Moving up.");
                newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
                desiredPosition = currentDesiredPosition + newPositionChange;
                desiredPosition.y = Mathf.Clamp(currentDesiredPosition.y + newPositionChange.y, Mathf.Abs(currentDesiredPosition.y), Mathf.Abs(currentDesiredPosition.y)+100f);

            }
        }
        else{
            
            newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
            // Update desired position
            desiredPosition = currentDesiredPosition + newPositionChange;
        }

        // update prewious Yew
        
        newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity);
        prewYaw = newYaw;
        // only used for datacollector
        desiredEulerAngles = new Vector3(newPitch, newYaw, newRoll);
        //

        //Update desired orientation
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);

    
    
        // if (pitchChange == 0)
        // {
        //     // No input detected, set desiredOrientation to neutralOrientation
        //     newPitch = neutralOrientation.eulerAngles.x;

        //     // To cancel out the forward momentum, make a Impuls for a counter momentum 
        //     Vector3 counterTorque = new Vector3(-1*rb.angularVelocity.x,0,0);
        //     //ApplyCounterTorque(counterTorque);
        
        // }
        // else
        // {
        //     // Calculate new angles directly within bounds
        //     newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        //     //float newPitch = Mathf.Clamp(pitchChange * pitchSensitivity, -maxPitch, maxPitch);
        //     //newPitch = WrapAngle(newPitch);
        // }

        // if (rollChange == 0)
        // {
        //     newRoll = neutralOrientation.eulerAngles.z;
        //     // To cancel out the left/right momentum, make a Impuls for a counter momentum
        //     Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
        //     //ApplyCounterTorque(counterTorque);
            
        // }
        // else
        // {
        //     newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
        //     //float newRoll = Mathf.Clamp(rollChange * rollSensitivity, -maxRoll, maxRoll);
        //     //newRoll = WrapAngle(newRoll);
        // }


        // // update prewious Yew
        // prewYaw = newYaw;
        // float newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity);
        // // only used for datacollector
        // desiredEulerAngles = new Vector3(newPitch, newYaw, newRoll);
        // //

        // //Update desired orientation
        // desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);
    
        //Debug.Log("desiredOrientation euler: " + desiredOrientation.eulerAngles);
    
        // current position should be the desired position, as its the starting position. 

        //Vector3 currentDesiredPosition = desiredPosition;

        // // x
        // float newPositionChangeX = currentDesiredPosition.x + pitchChange * pitch_x;
        // // y
        // float newPositionChangeY = currentDesiredPosition.y + throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement
        // // z 
        // float newPositionChangeZ = currentDesiredPosition.z + rollChange * roll_z;
        // Vector3 newPositionChangeLocal = new Vector3(
        //     xlocalChange * xSensitivity,
        //     ylocalChange * ySensitivity,
        //     zlocalChange * zSensitivity);

        // newPositionChangeX = xlocalChange * xSensitivity;
        // // float newPositionChangeY = ylocalChange * ySensitivity;
        // newPositionChangeZ = zlocalChange * zSensitivity;

        // // Convert local position change to world space
        // //Vector3 newPositionChangeWorld = transform.TransformPoint(newPositionChangeLocal) - transform.position;

        // Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
        // // Update desired position
        // desiredPosition = currentDesiredPosition + newPositionChange;
    
    


        // 270 
        
        // may need to be the current orientation of the drone, to be able to spin 360
         // yaw can freely move
        //float newYaw = transform.rotation.eulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        //newYaw = WrapAngle(newYaw);
          
        if (toggleDebug)
        {
            Debug.Log("DesiredPitch: " + newPitch);
            Debug.Log("currentPitch: " + rb.transform.rotation.eulerAngles.x);
            Debug.Log("DesiredYaw: " + newYaw);
            // Debug.Log("PrewYaw: " + prewYaw);
            Debug.Log("DesiredRoll: " + newRoll);
            Debug.Log("currentRoll: " + rb.transform.rotation.eulerAngles.z);
        }

       
      
        //desiredPosition = currentDesiredPosition + newPositionChangeWorld;
       
    }


    private void CalcDesiredPose(float rollChange, float pitchChange, float yawChange, float throttleChange)
    {
        // Sensitivity factors (determine how much each input affects the pose)
        float pitchSensitivity = 10.0f;
        float pitch_x = 2.0f;
        float rollSensitivity = 10.0f;
        float roll_z = 2.0f;
        float yawSensitivity = 5.0f;
        float altitudeSensitivity = 0.50f;
        float newPitch = 0;
        float newRoll = 0;
        float xSensitivity = 0.50f;
        float ySensitivity = 0.50f;
        float zSensitivity = 0.50f;

        // quick mapping
        // float xlocalChange = pitchChange;
        // float ylocalChange = throttleChange;
        // float zlocalChange = rollChange;



        // // Converting neutral orientation from Quaternion to euler angles
        Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;
        // // Calculate new angles by applying the userInput changes to the neutral orientation and limiting the angle 
        // float newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        // float newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
        // float newYaw = neutralEulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move

        if (pitchChange == 0)
        {
            // No input detected, set desiredOrientation to neutralOrientation
            newPitch = neutralOrientation.eulerAngles.x;

            // To cancel out the forward momentum, make a Impuls for a counter momentum 
            //Vector3 counterTorque = new Vector3(-1*rb.angularVelocity.x,0,0);
            //ApplyCounterTorque(counterTorque);
           
        }
        else
        {
            // Calculate new angles directly within bounds
            newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
            //float newPitch = Mathf.Clamp(pitchChange * pitchSensitivity, -maxPitch, maxPitch);
            //newPitch = WrapAngle(newPitch);
        }

        if (rollChange == 0)
        {
            newRoll = neutralOrientation.eulerAngles.z;
            // To cancel out the left/right momentum, make a Impuls for a counter momentum
            //Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
            //ApplyCounterTorque(counterTorque);
            
        }
        else
        {
            newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
            
            //float newRoll = Mathf.Clamp(rollChange * rollSensitivity, -maxRoll, maxRoll);
            //newRoll = WrapAngle(newRoll);
        }


        // 270 
        
        // may need to be the current orientation of the drone, to be able to spin 360
        float newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = transform.rotation.eulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        //newYaw = WrapAngle(newYaw);
          
        if (toggleDebug)
        {
            //Debug.Log("DesiredPitch: " + newPitch);
            //Debug.Log("currentPitch: " + rb.transform.rotation.eulerAngles.x);
            //Debug.Log("DesiredYaw: " + newYaw);
            // Debug.Log("PrewYaw: " + prewYaw);
            Debug.Log("DesiredRoll: " + newRoll);
            Debug.Log("currentRoll: " + rb.transform.rotation.eulerAngles.z);
        }

        // update prewious Yew
        prewYaw = newYaw;
        // only used for datacollector
        desiredEulerAngles = new Vector3(newPitch, newYaw, newRoll);
        //

        //Update desired orientation
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);
       
        //Debug.Log("desiredOrientation euler: " + desiredOrientation.eulerAngles);
    
        // current position should be the desired position, as its the starting position. 

        Vector3 currentDesiredPosition = desiredPosition;

        // // x
        // float newPositionChangeX = currentDesiredPosition.x + pitchChange * pitch_x;
        // // y
        // float newPositionChangeY = currentDesiredPosition.y + throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement
        // // z 
        // float newPositionChangeZ = currentDesiredPosition.z + rollChange * roll_z;
        // Vector3 newPositionChangeLocal = new Vector3(
        //     xlocalChange * xSensitivity,
        //     ylocalChange * ySensitivity,
        //     zlocalChange * zSensitivity);

        // newPositionChangeX = xlocalChange * xSensitivity;
        // // float newPositionChangeY = ylocalChange * ySensitivity;
        // newPositionChangeZ = zlocalChange * zSensitivity;

        // Convert local position change to world space
        //Vector3 newPositionChangeWorld = transform.TransformPoint(newPositionChangeLocal) - transform.position;

        Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity;
        // Update desired position
        desiredPosition = currentDesiredPosition + newPositionChange;
        //desiredPosition = currentDesiredPosition + newPositionChangeWorld;
       
    }

    float WrapAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void Awake()
    {
        inputController = FindFirstObjectByType<UserInput>();
        desiredPosition = rb.transform.position;
        desiredOrientation = rb.transform.rotation;

        
        // Initialize PID controllers for each axis
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);
        xPID = new PIDController(xKp, xKi, xKd);
        zPID = new PIDController(zKp, zKi, zKd);

        rollPIDQuaternion = new PIDController(rollKp, rollKi, rollKd);
        pitchPIDQuaternion = new PIDController(pitchKp, pitchKi, pitchKd);
        yawPIDQuaternion = new PIDController(yawKp, yawKi, yawKd);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.maxAngularVelocity = maxAngularVelocity;
        // starting baseAltitude;
        desiredPosition = rb.transform.position;
        //desiredPosition.y = rb.transform.position.y;
        neutralOrientation = rb.transform.rotation;
        desiredOrientation = rb.transform.rotation;
        gravityComp = Mathf.Abs(Physics.gravity.y) * rb.mass;

        Visual_Quadcopter_main = GameObject.Find("Washing Drone");

        // Activate only then tuning
        //StartTuningPID();

    
        // object avoidance
        if(windblade != null)
        {
            targetCollider = windblade.GetComponent<MeshCollider>();
            //targetCollider.Convex = true;
            if (targetCollider == null)
            {
                Debug.LogError("windblade does not have a Collider!");
            }
        }
        else
        {
            Debug.LogError("windblade is not assigned!");
        }
      
    }

    void FixedUpdate()
    {
        // visual update the washing drones location
        Visual_Quadcopter_main.transform.position = transform.position;
        Visual_Quadcopter_main.transform.rotation = transform.rotation;

     
        
        //TestingDesiredPose();
        // if (isTuning)
        // {
        //     if (!stepInputApplied)
        //     {
        //         // Apply the step change only once to start the tuning process
        //         //desiredPosition.x += 5.0f; // Induce a significant error
        //         stepInputApplied = true; // Ensure the step change is not applied again
        //     }

        //     TunePID(); // Continue with the tuning process
        // }
      
        UpdatePID();
        Debug.DrawRay(rb.transform.position, Vector3.up, Color.red, duration: 5f);

        if (targetCollider != null)
        {
            // Use ClosestPoint to get the closest point on the target's surface to this GameObject
            closestPoint = targetCollider.ClosestPoint(transform.position);
            // Calculate the distance from this GameObject's position to the closest point
            distanceToObject = Vector3.Distance(transform.position, closestPoint);



            // Update the state based on the distance
            // if (distanceToObject < 1f)
            // {
            //     TWO_t_main_dist.text = "Dist Extreme Close";
            //     TWO_i_main_dist.color = Color.red;
            //     ONE_t_main_dist.text = "Dist Extreme Close";
            //     ONE_i_main_dist.color =  Color.red;
            // }
            // else if (distanceToObject < 3f)
            // {
            //     TWO_t_main_dist.text = "Dist Close";
            //     TWO_i_main_dist.color = Color.yellow;
            //     ONE_t_main_dist.text = "Dist Close";
            //     ONE_i_main_dist.color =  Color.yellow;
            // }
            // else
            // {
            //     TWO_t_main_dist.text = "Dist safe";
            //     TWO_i_main_dist.color = Color.green;
            //     ONE_t_main_dist.text = "Dist safe";
            //     ONE_i_main_dist.color =  Color.green;
            // }
            if (distanceToObject < 3f)
            {
                shouldBlink = true;
                StartBlinking(Color.red);
                TWO_t_main_dist.text = "Dist Extreme Close";
                ONE_t_main_dist.text = "Dist Extreme Close";
            }
            else if (distanceToObject < 8f)
            {
                shouldBlink = false;
                StopBlinking();
                TWO_t_main_dist.text = "Dist Close";
                TWO_i_main_dist.color = Color.yellow;
                ONE_t_main_dist.text = "Dist Close";
                ONE_i_main_dist.color = Color.yellow;
            }
            else
            {
                shouldBlink = false;
                StopBlinking();
                TWO_t_main_dist.text = "Dist Safe";
                TWO_i_main_dist.color = Color.green;
                ONE_t_main_dist.text = "Dist Safe";
                ONE_i_main_dist.color = Color.green;
            }


        }
      
    }

    IEnumerator Blink(Color blinkColor)
    {
        while (shouldBlink)
        {
            TWO_i_main_dist.color = blinkColor;
            ONE_i_main_dist.color = blinkColor;
            yield return new WaitForSeconds(0.5f); // Blink interval
            TWO_i_main_dist.color = Color.clear; // Choose the off color, e.g., clear or white
            ONE_i_main_dist.color = Color.clear;
            yield return new WaitForSeconds(0.5f);
        }
        // Reset color to current state
        TWO_i_main_dist.color = blinkColor;
        ONE_i_main_dist.color = blinkColor;
    }

    void StartBlinking(Color color)
    {
        StopCoroutine("Blink");
        StartCoroutine(Blink(color));
    }

    void StopBlinking()
    {
        StopCoroutine("Blink");
        TWO_i_main_dist.color = TWO_i_main_dist.color; // Reset to current non-blinking color
        ONE_i_main_dist.color = ONE_i_main_dist.color;
    }


    void UpdatePID()
    {
    
        // visual show pitch and roll rotation
        //rb.transform.rotation = Quaternion.Euler(desiredEulerAngles.x, 0, desiredEulerAngles.z);

        // get time since last update 
        float deltaTime = Time.fixedDeltaTime;

        // Get current state from sensors 
        Vector3 currentPosition = rb.transform.position;        
        //Vector3 currentEulerAngles = transform.eulerAngles;

        // Quaternion-based PID control
        Quaternion currentOrientation = rb.transform.rotation; // current orientation
        //Quaternion currentOrientation = transform.localRotation;
        //Quaternion desiredOrientation = Quaternion.Euler(desiredEulerAngles);
        
        //Debug.Log("desiredOrientation euler current: " + currentOrientation.eulerAngles);


        Vector3 angularVelocityError = AngularVelocities(currentOrientation, desiredOrientation, deltaTime);


        // Calc orientation delta
        //Quaternion orientationDelta = Quaternion.Inverse(currentOrientation) * desiredOrientation;


        // Convert orientation delta to angular velocity vector (for linearity)
        //Vector3 angularVelocityError = slerp_OrientationDeltaToAngularVelocity(orientationDelta, deltaTime);
        //Vector3 angularVelocityError = OrientationDeltaToAngularVelocity(orientationDelta);
       

        // get current angularVelocity 
        Vector3 currentAngularVelocity = rb.angularVelocity;


        if (toggleDebug)
        {
            // //Debug.Log("UpdatePID called");
            Debug.Log("desiredOrientation: " + desiredOrientation);
            Debug.Log("currentOrientation: " + currentOrientation);
            //Debug.Log("orientationDelta: " + orientationDelta);
            Debug.Log("angularVelocityError: " + angularVelocityError);
            // //Debug.DrawRay(transform.position, angularVelocityError * 200, Color.yellow);
            Debug.Log("currentAngularVelocity: " + currentAngularVelocity);
            // //Debug.DrawRay(transform.position, rb.angularVelocity * 200, Color.black);
        }

       

        // Object avoidance 
        // float minDistance = 0.5f;
        // if (distanceToObject < minDistance)
        // {
        //     // Calculate the direction from the object to the GameObject
        //     Vector3 directionFromObject = currentPosition - closestPoint;
        //     directionFromObject.Normalize();  // Normalize the direction vector

        //     // Set the new desired position to maintain at least minDistance
        //    // desiredPosition = closestPoint + directionFromObject * minDistance;
        // }


        // calculate the velocity 
        Vector3 vectorToTarget = desiredPosition - currentPosition;
        Vector3 Targetvelocity = vectorToTarget / deltaTime;

        // get current Velocity 
        Vector3 currentVelocity = rb.velocity;


        // Debug.Log("desiredPosition: " + desiredPosition);
        // Debug.Log("currentPosition: " + currentPosition);
        // Debug.Log("Targetvelocity: " + Targetvelocity);
        // Debug.Log("currentVelocity: " + currentVelocity);

        // PID control on the angular velocity error (closed feedback loop)

        float rollControlInput = rollPIDQuaternion.UpdateAA(angularVelocityError.z, currentAngularVelocity.z, deltaTime);
        float pitchControlInput = pitchPIDQuaternion.UpdateAA(angularVelocityError.x, currentAngularVelocity.x, deltaTime);
        float yawControlInput = yawPIDQuaternion.UpdateAA(angularVelocityError.y, currentAngularVelocity.y, deltaTime);
    
        // x
        float xControlInput = xPID.UpdateAA(Targetvelocity.x, currentVelocity.x, deltaTime);
        //float xControlInput = xPID.UpdateAA(desiredPosition.x, currentPosition.x, deltaTime);
        // y
        //float altitudeError = AltitudePID.UpdateAA(desiredPosition.y, currentPosition.y, deltaTime);
        float altitudeError = AltitudePID.UpdateAA(Targetvelocity.y, currentVelocity.y, deltaTime);
        // z
        float zControlInput = zPID.UpdateAA(Targetvelocity.z, currentVelocity.z, deltaTime);
        //float zControlInput = xPID.UpdateAA(desiredPosition.z, currentPosition.z, deltaTime);
        // Gravity compensation 
        float altitudeControlInput = altitudeError + gravityComp;

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput, xControlInput, zControlInput);
 
    }
    


    
    public Vector3 AngularVelocities(Quaternion q1, Quaternion q2, float dt)
    {
        // Method to calculate angular velocity from two quaternions
        // Equation formula:
        // https://mariogc.com/post/angular-velocity-quaternions/?fbclid=IwAR1kXM70dvSfZLy7gwaALP_S9maYRiPM4jL5rcdO7ZJHg0Cu9uKEnkuepvE_aem_AStzX-28bYkrUS3ynU9xCW2W4pF7QOhyzEcDJQgidg3DdW5VZSaeVHdQyTHC2RA-b1oOOO4zi-uEjznNvuVJsFAI
        // converted from python to c#
        Vector3 angularVelocity = new Vector3(
            (2 / dt) * (q1.w * q2.x - q1.x * q2.w - q1.y * q2.z + q1.z * q2.y),
            (2 / dt) * (q1.w * q2.y + q1.x * q2.z - q1.y * q2.w - q1.z * q2.x),
            (2 / dt) * (q1.w * q2.z - q1.x * q2.y + q1.y * q2.x - q1.z * q2.w)
        );

        // Convert from rad/s to degrees/s
        //angularVelocity *= Mathf.Rad2Deg;

        return angularVelocity;
    }

    void ControlMotors(float roll, float pitch, float yaw, float lift, float x, float z)
    {
    
        
        // Vector3 controlForceWorld = new Vector3(x, lift, z);
        // //Debug.Log("controlForceWorld: " + controlForceWorld);
        // // Convert the world space force vector into local space
        // Vector3 controlForceLocal = transform.InverseTransformDirection(controlForceWorld);

        // controlForceLocal.x = Mathf.Clamp(controlForceLocal.x, -maxVelocity, maxVelocity);
        // controlForceLocal.y = Mathf.Clamp(controlForceLocal.y, -maxVelocity, maxVelocity);
        // controlForceLocal.z = Mathf.Clamp(controlForceLocal.z, -maxVelocity, maxVelocity);
        //rb.AddForce(controlForceLocal, ForceMode.Force);


        // x 
        // float x_force = Mathf.Clamp(x, -maxVelocity, maxVelocity);
        //rb.AddForce(transform.right * newPositionChangeX, ForceMode.Force);

        // // z
        // float z_force = Mathf.Clamp(z, -maxVelocity, maxVelocity);
        //rb.AddForce(transform.forward * newPositionChangeZ, ForceMode.Force);

        // // Apply Clamp to simulate actuators that limits the control signal. 
        float throttle2 = Mathf.Clamp(lift, -maxVelocity, maxVelocity);
        // // Throttle, the upward force 
        rb.AddForce(transform.up * throttle2, ForceMode.Force);

        // pitch, forward and backward
        float clampPitch = Mathf.Clamp(pitch, -maxVelPitch, maxVelPitch);
        rb.AddTorque(transform.right * clampPitch, ForceMode.Force);


        // roll, left and right
        float clampRoll = Mathf.Clamp(roll, -maxVelRoll, maxVelRoll);
        rb.AddTorque(transform.forward * clampRoll, ForceMode.Force); // might need to invert roll. 

        // yaw, left and right
        float clampYaw = Mathf.Clamp(yaw, -maxVelYaw, maxVelYaw);
        rb.AddTorque(transform.up * clampYaw, ForceMode.Force);
      

        if (toggleDebug)
        {
            //Debug.Log("lift: " + lift2);
            //Debug.Log("clamplift: " + throttle2);
            Debug.Log("Pitch: " + pitch);
            Debug.Log("clampPitch: " + clampPitch);
            Debug.Log("Roll: " + roll);
            Debug.Log("clampRoll: " + clampRoll);
            Debug.Log("Yaw: " + yaw);
            Debug.Log("clampYaw: " + clampYaw);

            // Debug.Log("Drone pos: " + rb.position);
            Debug.Log("Drone vel: " + rb.velocity);
            // Debug.Log("Drone rot: " + rb.transform.rotation);
            Debug.Log("Drone ang: " + rb.angularVelocity);
            // Debug.Log("throttle: " + throttle);
        }
    
    }

    public void ApplyUserInput(float roll, float pitch, float yaw, float throttle)
    {
        // yaw and throttle coming from the userInput should be swapted! 
        float temp = yaw;
        yaw = throttle;
        throttle = temp;

        // roll and pitch should be swapted aswell
        temp = roll;
        roll = pitch;
        pitch = temp;

        // filter out small noise values from joystick
        if (Mathf.Abs(roll) < 0.4f) roll = 0f;
        if (Mathf.Abs(pitch) < 0.4f) pitch = 0f;
        if (Mathf.Abs(yaw) < 0.4f) yaw = 0f;
        if (Mathf.Abs(throttle) < 0.4f) throttle = 0f;

        CalcDesiredPose(roll, pitch, yaw, throttle);
        //CalcDesiredPose(roll, pitch, yaw, throttle);
     
        if (toggleDebug)
        {
            //Debug.Log("drone r/p/y/th: " + roll + "/" + pitch + "/" + yaw + "/" + throttle);
        }
         
    }

    public void reset_drone()
    {
        desiredPosition = rb.transform.position;
        neutralOrientation = rb.transform.rotation;
        rb.velocity = new Vector3(0,0,0);
        rb.angularVelocity = new Vector3(0,0,0);
        AltitudePID.ClearPID();
        pitchPIDQuaternion.ClearPID();
        yawPIDQuaternion.ClearPID();
        rollPIDQuaternion.ClearPID();
        xPID.ClearPID();
        zPID.ClearPID();
        desiredOrientation = new Quaternion(0,0,0,0);
       
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class QuadcopterController_sec: MonoBehaviour
{
    public float maxVelocity = 200f; // Maximum linear velocity
    public float maxAcceleration = 5f; // Maximum linear acceleration
    public float drag = 0.5f; // Drag coefficient
    public float mass = 5f; // Mass of the quadcopter
    public float maxAngularVelocity = 5f; // Maximum angular velocity
    public float maxAngularAcceleration = 2f; // Maximum angular acceleration
    //public Vector3 thrustForce = new Vector3(0f, 5f, 0f); // Thrust force
    public float speed = 5.0f;
    public float maxYaw = 50.0f;
    public float maxRoll = 50.0f;
    public float maxPitch = 50.0f; 

    public float PitchValue = 0f;
    public float RollValue = 0f;
    public float YawValue = 0f;
    //public float throttleValue = 0;
    public float baseAltitude;
    public float altitudeChangeRate = 10f; // Change in altitude per unit of throttle input

    public Rigidbody rb;
    private PIDController rollPID;
    private PIDController PitchPID;
    private PIDController YawPID;
    private PIDController AltitudePID;
    private PIDController xPID;
    private PIDController zPID;

    public UserInput inputController;

    // needs fine tuning! 
    public float rollKp = 3.0f, rollKi = 0.1f, rollKd = 0.01f;
    public float pitchKp = 3.0f, pitchKi = 0.1f, pitchKd = 0.01f;
    public float yawKp = 3.0f, yawKi = 0.1f, yawKd = 0.01f;
    public float altitudeKp = 1.0f, altitudeKi = 0.1f, altitudeKd = 0.01f;    
    public float xKp = 1.0f, xKi = 0.1f, xKd = 0.01f;   
    public float zKp = 1.0f, zKi = 0.1f, zKd = 0.01f;   

    // desired pose
    public Vector3 desiredPosition;
    public Vector3 desiredOrientation;



    // // Set target roll value
    // public void SetTargetRoll(float value)
    // {
    //     targetRoll = value;
    // }

    // // Set target pitch value
    // public void SetTargetPitch(float value)
    // {
    //     targetPitch = value;
    // }

    // // Set target yaw value
    // public void SetTargetYaw(float value)
    // {
    //     targetYaw = value;
    // }

    // target pose of the secondary drone  
    public void SetQuadcopterPose(Vector3 position, Quaternion rotation)
    {
        // sets the desired translation 
        desiredPosition = position;
       

        // Calculate desired pitch, roll, and yaw from the provided quaternion 
        Vector3 eulerAngles = rotation.eulerAngles;
        float desiredRoll = eulerAngles.x;
        float desiredPitch = eulerAngles.y;
        float desiredYaw = eulerAngles.z;

        // Set the desired orientation
        desiredOrientation = new Vector3(desiredRoll, desiredPitch, desiredYaw); 
    }
   private void Awake()
    {
        // find the user input script for movement
        inputController = FindFirstObjectByType<UserInput>();
        // init the drone controller with the pose of the drone, when simulation starts
        desiredPosition = rb.transform.position;
        desiredOrientation = rb.transform.rotation.eulerAngles;

        
        // Initialize PID controllers for each axis
        rollPID = new PIDController(rollKp, rollKi, rollKd);
        PitchPID =  new PIDController(pitchKp, pitchKi, pitchKd);
        YawPID =  new PIDController(yawKp, yawKi, yawKd);
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);
        xPID = new PIDController(xKp, xKi, xKd);
        zPID = new PIDController(zKp, zKi, zKd);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;

    }

    void FixedUpdate()
    {
        UpdatePID();
        ApplyForces();
        //ClampVelocity();
    }

    void ApplyForces()
    {
        // Apply drag force
        Vector3 dragForce = -rb.velocity * drag;
        rb.AddForce(dragForce);

        // Apply gravity
        Vector3 gravityForce = Vector3.down * mass * Physics.gravity.magnitude;
        rb.AddForce(gravityForce);
    }

    void ClampVelocity()
    {
        // ´´NOT in use´´ //

        // Clamp linear velocity
        Vector3 clampedVelocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        rb.velocity = clampedVelocity;

        // Clamp angular velocity
        Vector3 clampedAngularVelocity = rb.angularVelocity;
        clampedAngularVelocity.y = Mathf.Clamp(clampedAngularVelocity.y, -maxAngularVelocity, maxAngularVelocity);
        rb.angularVelocity = clampedAngularVelocity;
    }

    void UpdatePID()
    {
        // get time since last update (NOT USED, using fixedDeltaTime in the PID itself)
        float deltaTime = Time.deltaTime;
           
        // Get current state from sensors 
        Vector3 currentPosition = transform.position;        
        Quaternion currentOrientation = transform.rotation;
        Vector3 currentEulerAngles = currentOrientation.eulerAngles;



        // Calculate control input for each axis using PID
        float rollControlInput = rollPID.UpdateAA(desiredOrientation.x, currentEulerAngles.x, deltaTime);
        //float rollControlInput = rollPID.UpdateAA(RollValue, currentEulerAngles.x, deltaTime);
        float pitchControlInput = PitchPID.UpdateAA(desiredOrientation.y, currentEulerAngles.y, deltaTime);
        //float pitchControlInput = PitchPID.UpdateAA(PitchValue, currentEulerAngles.y, deltaTime);
        float yawControlInput = YawPID.UpdateAA(desiredOrientation.z, currentEulerAngles.z, deltaTime);
        float altitudeControlInput = AltitudePID.UpdateAA(desiredPosition.y, currentPosition.y, deltaTime);
        float xControlInput = xPID.UpdateAA(desiredPosition.x, currentPosition.x, deltaTime);
        float zControlInput = zPID.UpdateAA(desiredPosition.z, currentPosition.z, deltaTime);

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput, xControlInput, zControlInput);
        // Apply torque for rotation
        //Vector3 torque = new Vector3(rollControlInput, pitchControlInput, yawControlInput);
        //rb.AddRelativeTorque(torque);


    }
    void ControlMotors(float roll, float pitch, float yaw, float altitude, float x, float z)
    {

        // Clamp the control signal. for force
        float y2 = Mathf.Clamp(altitude, -maxVelocity, maxVelocity);
        float x2 = Mathf.Clamp(x, -maxVelocity, maxVelocity);
        float z2 = Mathf.Clamp(z, -maxVelocity, maxVelocity);

        Vector3 force_vector = new Vector3(x2, y2, z2);
        rb.AddForce(force_vector, ForceMode.VelocityChange);
        
        // Clamp the control signal. for torque

        float clampPitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        float clampRoll = Mathf.Clamp(roll, -maxRoll, maxRoll);
        float clampYaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);

        Vector3 torque_vector = new Vector3(clampRoll, clampPitch, clampYaw);
        rb.AddTorque(torque_vector, ForceMode.VelocityChange);

        // Debug.Log("Drone pos: " + rb.position);
        // Debug.Log("Drone vel: " + rb.velocity);
        // Debug.Log("Drone rot: " + rb.transform.rotation);
        // Debug.Log("Drone ang: " + rb.angularVelocity);
        // Debug.Log("throttle: " + throttle);

    }

}

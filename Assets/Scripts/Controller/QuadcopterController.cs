using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuadcopterController: MonoBehaviour
{
    public float maxVelocity = 200f; // Maximum linear velocity
    public float maxAcceleration = 20f; // Maximum linear acceleration
    public float drag = 0.5f; // Drag coefficient
    public float mass = 5f; // Mass of the quadcopter
    public float maxAngularVelocity = 5f; // Maximum angular velocity
    public float maxAngularAcceleration = 2f; // Maximum angular acceleration
    //public Vector3 thrustForce = new Vector3(0f, 5f, 0f); // Thrust force
    public float speed = 5.0f;
    public float maxYaw = 80.0f;
    public float maxVelYaw = 20.0f;
    public float maxRoll = 10.0f;
    public float maxVelRoll = 20.0f;
    public float maxPitch = 10.0f; 
    public float maxVelPitch = 20.0f; 

    public float PitchValue = 0f;
    public float RollValue = 0f;
    public float YawValue = 0f;
    public float throttleValue = 0;
    public float baseAltitude;
    public float altitudeChangeRate = 10f; // Change in altitude per unit of throttle input

    public Rigidbody rb;
    private PIDController rollPID;
    private PIDController PitchPID;
    private PIDController YawPID;
    private PIDController AltitudePID;

    public UserInput inputController;

    // needs fine tuning! 
    public float rollKp = 1.0f, rollKi = 0.1f, rollKd = 0.01f;
    public float pitchKp = 1.0f, pitchKi = 0.1f, pitchKd = 0.01f;
    public float yawKp = 1.0f, yawKi = 0.1f, yawKd = 0.01f;
    public float altitudeKp = 1.0f, altitudeKi = 0.1f, altitudeKd = 0.01f;    

    // desired pose
    public Vector3 desiredPosition;
    public Quaternion desiredOrientation;

    // calc desired pose
    private void CalcDesiredPose(float rollChange, float pitchChange, float yawChange, float throttleChange)
    {
        // Sensitivity factors (determine how much each input affects the pose)
        float pitchSensitivity = 1.0f;
        float rollSensitivity = 1.0f;
        float yawSensitivity = 1.0f;
        float altitudeSensitivity = 3.0f;

        PitchValue = pitchChange;
        RollValue = rollChange;
        YawValue = yawChange;

        // Simple not taking the current pose into account!
        //desiredOrientation = Quaternion.Euler(new Vector3(pitchInput, yawInput, rollInput));
        //desiredAltitude = baseAltitude + (throttleInput * altitudeChangeRate);

        // Taking the current drone pose into account!
        Quaternion currentOrientation = transform.rotation;
        Vector3 currentEulerAngles = currentOrientation.eulerAngles;
        Vector3 newEulerAngles = new Vector3(
            currentEulerAngles.x + (pitchChange * pitchSensitivity * throttleChange),
            currentEulerAngles.y + (yawChange * yawSensitivity),  //* throttleChange),
            currentEulerAngles.z + (rollChange * rollSensitivity * throttleChange)
        );
        desiredOrientation = Quaternion.Euler(newEulerAngles);




        // current position should be the desired position, as its the starting position. 
        //Vector3 currentPosition = transform.position;
        Vector3 currentPosition = desiredPosition;
        Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement
        desiredPosition = currentPosition + newPositionChange;

    }

    private void Awake()
    {
        inputController = FindFirstObjectByType<UserInput>();
        desiredPosition = rb.transform.position;
        desiredOrientation = rb.transform.rotation;

        
        // Initialize PID controllers for each axis
        rollPID = new PIDController(rollKp, rollKi, rollKd);
        PitchPID =  new PIDController(pitchKp, pitchKi, pitchKd);
        YawPID =  new PIDController(yawKp, yawKi, yawKd);
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
        //baseAltitude;
        desiredPosition.y = transform.position.y;

      
    }

    void FixedUpdate()
    {
        UpdatePID();
        //ApplyForces();
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

        // Get current rotation and angular velocity
        //Vector3 currentRotation = transform.eulerAngles;
        //Vector3 currentAngularVelocity = rb.angularVelocity;

        //clamp desired parameters values.
        float desired_Orien_Roll = Mathf.Clamp(RollValue, -maxRoll, maxRoll);
        float desired_Orien_Pitch = Mathf.Clamp(PitchValue, -maxPitch, maxPitch);
        //desired_Orien_Roll = Mathf.Clamp(desiredOrientation.eulerAngles.z, -maxRoll, maxRoll);
        //desired_Orien_Pitch = Mathf.Clamp(desiredOrientation.eulerAngles.x, -maxPitch, maxPitch);
        float desired_Orien_Yaw = Mathf.Clamp(desiredOrientation.eulerAngles.y, -maxYaw, maxYaw);

        // don't calculate the desired orientation. the ones we get from the userInput is the desired!
        // keep desiredYaw. as its the rotation around z-axis.
        float rollControlInput = rollPID.UpdateAA(desired_Orien_Roll, currentEulerAngles.z, deltaTime);
        //float rollControlInput = rollPID.UpdateAA(RollValue, currentEulerAngles.x, deltaTime);
        float pitchControlInput = PitchPID.UpdateAA(desired_Orien_Pitch, currentEulerAngles.x, deltaTime);
        //float pitchControlInput = PitchPID.UpdateAA(PitchValue, currentEulerAngles.y, deltaTime);
        float yawControlInput = YawPID.UpdateAA(desiredOrientation.eulerAngles.y, currentEulerAngles.y, deltaTime);
        float altitudeControlInput = AltitudePID.UpdateAA(desiredPosition.y, currentPosition.y, deltaTime);

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput);
        ///ControlMotors(RollValue,PitchValue,YawValue,altitudeControlInput, throttleValue);
    }

    void ControlMotors(float roll, float pitch, float yaw, float lift2)
    {

        // Apply Clamp to simulate actuators that limits the control signal. 
        float throttle2 = Mathf.Clamp(lift2, -maxVelocity, maxVelocity);
        Debug.Log("throttle: " + throttle2);
        // Throttle, the upward force
        Vector3 lift = Vector3.up * throttle2;
        //Debug.Log("lift: " + lift);
        //rb.AddForce(lift, ForceMode.Acceleration);
        rb.AddForce(lift, ForceMode.VelocityChange);

        // pitch, forward and backward
        float clampPitch = Mathf.Clamp(pitch, -maxVelPitch, maxVelPitch);
        //rb.AddTorque(transform.right * clampPitch, ForceMode.Acceleration);
        // rb.AddTorque(transform.right * clampPitch * throttle, ForceMode.VelocityChange);

        // roll, left and right
        float clampRoll = Mathf.Clamp(roll, -maxVelRoll, maxVelRoll);
        //rb.AddTorque(-transform.forward * clampRoll, ForceMode.Acceleration);
        //rb.AddTorque(-transform.forward * clampRoll * throttle, ForceMode.VelocityChange);

        // yaw, left and right
        float clampYaw = Mathf.Clamp(yaw, -maxVelYaw, maxVelYaw);
        //rb.AddTorque(transform.up * clampYaw, ForceMode.Acceleration);
        //rb.AddTorque(transform.up * clampYaw * throttle, ForceMode.VelocityChange);

        // Debug.Log("Drone pos: " + rb.position);
        // Debug.Log("Drone vel: " + rb.velocity);
        // Debug.Log("Drone rot: " + rb.transform.rotation);
        // Debug.Log("Drone ang: " + rb.angularVelocity);
        // Debug.Log("throttle: " + throttle);

    }

    public void ApplyUserInput(float roll, float pitch, float yaw, float throttle)
    {
        // make a clamped input such that the system dosn't accumulate the user-inputs past the bounderies. 
        // make if it recieves a zero it clears the
        CalcDesiredPose(roll, pitch, yaw, throttle);
        throttleValue = throttle;
        //Debug.Log("drone r/p/y/th: " + roll + "/" + pitch + "/" + yaw + "/" + throttle);
 
    }
}

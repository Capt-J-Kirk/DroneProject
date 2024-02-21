using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuadcopterController: MonoBehaviour
{
    public float maxVelocity = 10f; // Maximum linear velocity
    public float maxAcceleration = 5f; // Maximum linear acceleration
    public float drag = 0.5f; // Drag coefficient
    public float mass = 5f; // Mass of the quadcopter
    public float maxAngularVelocity = 5f; // Maximum angular velocity
    public float maxAngularAcceleration = 2f; // Maximum angular acceleration
    public Vector3 thrustForce = new Vector3(0f, 5f, 0f); // Thrust force
    public float speed = 5.0f;

    public float baseAltitude;
    public float altitudeChangeRate = 10f; // Change in altitude per unit of throttle input

    private Rigidbody rb;
    private PIDController rollPID;
    private PIDController PitchPID;
    private PIDController YawPID;
    private PIDController AltitudePID;

    // needs fine tuning! 
    public float rollKp = 1.0f, rollKi = 0.1f, rollKd = 0.01f;
    public float pitchKp = 1.0f, pitchKi = 0.1f, pitchKd = 0.01f;
    public float yawKp = 1.0f, yawKi = 0.1f, yawKd = 0.01f;
    public float altitudeKp = 1.0f, altitudeKi = 0.1f, altitudeKd = 0.01f;    

    // desired pose
    public Vector3 desiredPosition;
    public Quaternion desiredOrientation;

    // calc desired pose
    private void CalcDesiredPose(float rollChange, float pitchChange, float yawChange, float altitudeChange)
    {
        // Sensitivity factors (determine how much each input affects the pose)
        float pitchSensitivity = 2.0f;
        float rollSensitivity = 2.0f;
        float yawSensitivity = 2.0f;
        float altitudeSensitivity = 1.0f;

        // Simple not taking the current pose into account!
        //desiredOrientation = Quaternion.Euler(new Vector3(pitchInput, yawInput, rollInput));
        //desiredAltitude = baseAltitude + (throttleInput * altitudeChangeRate);

        // Taking the current drone pose into account!
        Quaternion currentOrientation = transform.rotation;
        Vector3 currentEulerAngles = currentOrientation.eulerAngles;
        Vector3 newEulerAngles = new Vector3(
            currentEulerAngles.x + pitchChange * pitchSensitivity,
            currentEulerAngles.y + yawChange * yawSensitivity,
            currentEulerAngles.z + rollChange * rollSensitivity
        );
        desiredOrientation = Quaternion.Euler(newEulerAngles);

        Vector3 currentPosition = transform.position;
        Vector3 newPositionChange = Vector3.up * altitudeChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement
        desiredPosition = currentPosition + newPositionChange;

    }

    // set target values for roll, pitch and yaw
    //private float targetRoll = 0f;
    //private float targetPitch = 0f;
    //private float targetYaw = 0f;

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

    //   public Quaternion GetQuadcopterRotation()
    // {
    //     return transform.rotation;
    // }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
        baseAltitude = transform.position.y;

        // Initialize PID controllers for each axis
        rollPID = new PIDController(rollKp, rollKi, rollKd);
        PitchPID =  new PIDController(pitchKp, pitchKi, pitchKd);
        YawPID =  new PIDController(yawKp, yawKi, yawKd);
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);
      
    }

    void FixedUpdate()
    {
        UpdatePID();
        ApplyForces();
        ClampVelocity();
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


        // Calculate control input for each axis using PID
        // setPoint, ActualValue, timeFrame
        float rollControlInput = rollPID.Update(desiredOrientation.eulerAngles.x, currentEulerAngles.x, deltaTime);
        float pitchControlInput = PitchPID.Update(desiredOrientation.eulerAngles.y, currentEulerAngles.y, deltaTime);
        float yawControlInput = YawPID.Update(desiredOrientation.eulerAngles.z, currentEulerAngles.z, deltaTime);
        float altitudeControlInput = AltitudePID.Update(desiredPosition.y, currentPosition.y, deltaTime);

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput);


        // Apply torque for 
        //Vector3 torque = new Vector3(rollControlInput, pitchControlInput, yawControlInput);
        //rb.AddRelativeTorque(torque);
    }

    void ControlMotors(float roll, float pitch, float yaw, float throttle)
    {
        // Throttle, the upward force
        Vector3 lift = Vector3.up * throttle * speed;
        rb.AddForce(lift, ForceMode.Acceleration);

        // pitch, forward and backward
        rb.AddTorque(transform.right * pitch * speed, ForceMode.Acceleration);

        // roll, left and right
        rb.AddTorque(-transform.forward * roll * speed, ForceMode.Acceleration);

        // yaw, left and right
        rb.AddTorque(transform.up * yaw * speed, ForceMode.Acceleration);
    }



    public void ApplyUserInput(float roll, float pitch, float yaw, float throttle)
    {
        CalcDesiredPose(roll, pitch, yaw, throttle);
        // // Apply torque for rotation
        // Vector3 torque = new Vector3(0f, horizontalInput, 0f) * maxAngularAcceleration;
        // rb.AddRelativeTorque(torque);

        // // Apply thrust for linear movement
        // Vector3 thrust = transform.up * verticalInput * maxAcceleration;
        // rb.AddForce(thrustForce + thrust);

        // // Calculate desired pitch, roll, and yaw from user input (sensitivity)
        // float desiredPitch = verticalInput * 45.0f;  // Adjust as needed 
        // float desiredRoll = horizontalInput * 45.0f;  // Adjust as needed
        // float desiredYaw = 0f;  // You may adjust this based on user input

        // // Set the target pitch, roll, and yaw
        // SetTargetPitch(desiredPitch);
        // SetTargetRoll(desiredRoll);
        // SetTargetYaw(desiredYaw);

    }
}

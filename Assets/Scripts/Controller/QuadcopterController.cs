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

    private Rigidbody rb;
    private PIDController pidControllerRoll;
    private PIDController pidControllerPitch;
    private PIDController pidControllerYaw;


    // set target values for roll, pitch and yaw
    private float targetRoll = 0f;
    private float targetPitch = 0f;
    private float targetYaw = 0f;

    // Set target roll value
    public void SetTargetRoll(float value)
    {
        targetRoll = value;
    }

    // Set target pitch value
    public void SetTargetPitch(float value)
    {
        targetPitch = value;
    }

    // Set target yaw value
    public void SetTargetYaw(float value)
    {
        targetYaw = value;
    }

      public Quaternion GetQuadcopterRotation()
    {
        return transform.rotation;
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;

        // Initialize PID controllers for each axis
        pidControllerRoll = new PIDController(1f, 0.1f, 0.01f);
        pidControllerPitch = new PIDController(1f, 0.1f, 0.01f);
        pidControllerYaw = new PIDController(1f, 0.1f, 0.01f);
    }

    void FixedUpdate()
    {
        ApplyForces();
        ClampVelocity();
        UpdatePID();
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
        // Get current rotation and angular velocity
        Vector3 currentRotation = transform.eulerAngles;
        Vector3 currentAngularVelocity = rb.angularVelocity;

        // Calculate control input for each axis using PID
        float rollControlInput = pidControllerRoll.Update(currentRotation.x, currentAngularVelocity.x, targetRoll);
        float pitchControlInput = pidControllerPitch.Update(currentRotation.y, currentAngularVelocity.y, targetPitch);
        float yawControlInput = pidControllerYaw.Update(currentRotation.z, currentAngularVelocity.z, targetYaw);

        // Apply torque for rotation
        Vector3 torque = new Vector3(rollControlInput, pitchControlInput, yawControlInput);
        rb.AddRelativeTorque(torque);
    }

    public void ApplyUserInput(float horizontalInput, float verticalInput)
    {
        // Apply torque for rotation
        Vector3 torque = new Vector3(0f, horizontalInput, 0f) * maxAngularAcceleration;
        rb.AddRelativeTorque(torque);

        // Apply thrust for linear movement
        Vector3 thrust = transform.up * verticalInput * maxAcceleration;
        rb.AddForce(thrustForce + thrust);

        // Calculate desired pitch, roll, and yaw from user input (sensitivity)
        float desiredPitch = verticalInput * 45.0f;  // Adjust as needed 
        float desiredRoll = horizontalInput * 45.0f;  // Adjust as needed
        float desiredYaw = 0f;  // You may adjust this based on user input

        // Set the target pitch, roll, and yaw
        SetTargetPitch(desiredPitch);
        SetTargetRoll(desiredRoll);
        SetTargetYaw(desiredYaw);

    }
}

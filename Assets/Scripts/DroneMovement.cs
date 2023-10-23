using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovement : MonoBehaviour
{
    private Controls_1 input = null;
    private Rigidbody rb;
    private Vector2 throttleYawVector = Vector2.zero;
    private Vector2 throttleYawVector_prev = Vector2.zero;
    private Vector2 pitchRollVector = Vector2.zero;
    private float timer = 1f;
    private float levelTimer = 1f;

    public float maxSpeed = 10;         // Units per second.
    public float maxAngularSpeed = 180; // Degrees per second.
    public float maxDegPitchRoll = 30;  // Maximum degrees pitch/roll.

    private void Awake()
    {
        input = new Controls_1();
        rb = GetComponent<Rigidbody>();
    }

    // Subscribe
    private void OnEnable()
    {
        input.Enable();
        // Left stick
        input.DroneContol.ThrottleYaw.performed += OnThrottleYawPerformed;
        input.DroneContol.ThrottleYaw.canceled += OnThrottleYawCancelled;
        // Right stick
        input.DroneContol.PitchRoll.performed += OnPitchRollPerformed;
        input.DroneContol.PitchRoll.canceled += OnPitchRollCancelled;
    }

    // Unsubscribe
    private void OnDisable()
    {
        input.Disable();
        // Left stick
        input.DroneContol.ThrottleYaw.performed -= OnThrottleYawPerformed;
        input.DroneContol.ThrottleYaw.canceled -= OnThrottleYawCancelled;
        // Right stick
        input.DroneContol.PitchRoll.performed -= OnPitchRollPerformed;
        input.DroneContol.PitchRoll.canceled -= OnPitchRollCancelled;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Rotate();
        ChangeVelocity();
        ApplyDrag();
    }

    private void Rotate()
    {
        // Level the drone
        if (levelTimer <= 1f)
        {
            Quaternion currentRotation = transform.rotation;

            // Set the local forward vector parallel to the global water level.
            Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Quaternion targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);

            // Set the local up vector parallel to the global up vector.
            Quaternion toTargetUp = Quaternion.FromToRotation(currentRotation * Vector3.up, Vector3.up);

            // New rotation
            Quaternion newRotation = toTargetUp * targetRotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, levelTimer);
        }
        // Rotate the drone
        else
        {
            // Yaw
            if (Mathf.Abs(throttleYawVector.x) < 0.2f) throttleYawVector.x = 0f;
            transform.Rotate(new Vector3(0f, throttleYawVector.x * maxAngularSpeed, 0f) * Time.deltaTime, Space.Self);

            // Pitch
            if (Mathf.Abs(transform.rotation.eulerAngles.x) < maxDegPitchRoll || Mathf.Abs(transform.rotation.eulerAngles.x) > 360f - maxDegPitchRoll)
            {
                if (Mathf.Abs(pitchRollVector.y) < 0.2f) pitchRollVector.y = 0f;
                transform.Rotate(new Vector3(pitchRollVector.y * maxAngularSpeed / 6f, 0f, 0f) * Time.deltaTime, Space.Self);
            }

            // Roll
            if (Mathf.Abs(transform.rotation.eulerAngles.z) < maxDegPitchRoll || Mathf.Abs(transform.rotation.eulerAngles.z) > 360f - maxDegPitchRoll)
            {
                if (Mathf.Abs(pitchRollVector.x) < 0.2f) pitchRollVector.x = 0f;
                transform.Rotate(new Vector3(0f, 0f, -1f * pitchRollVector.x * maxAngularSpeed / 6f) * Time.deltaTime, Space.Self);
            }
        }
        levelTimer += 0.02f;
    }

    private void ChangeVelocity()
    {
        // New vertical velocity
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float local_y = Mathf.Lerp(throttleYawVector_prev.y * maxSpeed, throttleYawVector.y * maxSpeed, timer);
        localVelocity.y = local_y;

        // Cap speed
        if (Mathf.Abs(localVelocity.x) > maxSpeed) localVelocity = new Vector3(maxSpeed * (localVelocity.x / Mathf.Abs(localVelocity.x)), localVelocity.y, localVelocity.z);
        if (Mathf.Abs(localVelocity.z) > maxSpeed) localVelocity = new Vector3(localVelocity.x, localVelocity.y, maxSpeed * (localVelocity.z / Mathf.Abs(localVelocity.z)));
        if (localVelocity.y < -40f) localVelocity = new Vector3(localVelocity.x, -40f, localVelocity.z);
        if (localVelocity.y > maxSpeed) localVelocity = new Vector3(localVelocity.x, maxSpeed, localVelocity.z);

        // Apply new velocity
        rb.velocity = transform.TransformDirection(localVelocity);
        timer += 0.02f;
    }

    private void ApplyDrag()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            rb.AddForce(0.01f * -1f * rb.velocity, ForceMode.VelocityChange);
        }
    }



    // ########## Left stick callbacks ##########
    private void OnThrottleYawPerformed(InputAction.CallbackContext value)
    {
        throttleYawVector_prev = throttleYawVector;
        throttleYawVector = value.ReadValue<Vector2>();
        timer = 0f;
    }

    private void OnThrottleYawCancelled(InputAction.CallbackContext value)
    {
        throttleYawVector_prev = throttleYawVector;
        throttleYawVector = Vector2.zero;
        timer = 0f;
    }

    // ########## Right stick callbacks ##########
    private void OnPitchRollPerformed(InputAction.CallbackContext value)
    {
        pitchRollVector = value.ReadValue<Vector2>();
    }

    private void OnPitchRollCancelled(InputAction.CallbackContext value)
    {
        pitchRollVector = Vector2.zero;
        levelTimer = 0;
    }

}

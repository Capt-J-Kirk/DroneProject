using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneControl: MonoBehaviour
{
    private DPad_Control inputDPad = null;
    private XRI_Control_1 inputXRI = null;
    [SerializeField] public ParticleSystem droneSpray;
    [HideInInspector] public bool isSpraying = false;
    private Rigidbody rb;
    private Vector2 throttleYawVector = Vector2.zero;
    private Vector2 throttleYawVector_prev = Vector2.zero;
    private Vector2 pitchRollVector = Vector2.zero;
    public float speedTimer = 0;
    public float rotateTimer = 0;

    float maxSpeed = 2 ;                // Units per second.
    public float maxAngularSpeed = 10;  // Degrees per second.
    public float maxDegPitchRoll = 30;  // Maximum degrees pitch/roll.



    private void Awake()
    {
        inputDPad = new DPad_Control();
        inputXRI = new XRI_Control_1();
        rb = GetComponent<Rigidbody>();
        var main = droneSpray.main;
        main.loop = true;
    }


    // Subscribe
    private void OnEnable()
    {
        if (name == "Observing Drone")
        {
            inputDPad.Enable();
            // Left stick
            inputDPad.DPadContol.ThrottleYaw.performed += OnThrottleYawPerformed;
            inputDPad.DPadContol.ThrottleYaw.canceled += OnThrottleYawCancelled;
            // Right stick
            inputDPad.DPadContol.PitchRoll.performed += OnPitchRollPerformed;
            inputDPad.DPadContol.PitchRoll.canceled += OnPitchRollCancelled;
            // Right trigger (spray)
            inputDPad.DPadContol.Spray.performed += OnSprayPerformed;
            inputDPad.DPadContol.Spray.canceled += OnSprayCancelled;
        }
        if (name == "Washing Drone")
        {
            inputXRI.Enable();
            // Left stick
            inputXRI.XRILeftHandLocomotion.Move.performed += OnThrottleYawPerformed;
            inputXRI.XRILeftHandLocomotion.Move.canceled += OnThrottleYawCancelled;
            // Right stick
            inputXRI.XRIRightHandLocomotion.Move.performed += OnPitchRollPerformed;
            inputXRI.XRIRightHandLocomotion.Move.canceled += OnPitchRollCancelled;
            // Right trigger (spray)
            inputXRI.XRIRightHandInteraction.Activate.performed += OnSprayPerformed;
            inputXRI.XRIRightHandInteraction.Activate.canceled += OnSprayCancelled;
        }

    }

    // Unsubscribe
    private void OnDisable()
    {
        if (name == "Observing Drone")
        {
            inputDPad.Disable();
            // Left stick
            inputDPad.DPadContol.ThrottleYaw.performed -= OnThrottleYawPerformed;
            inputDPad.DPadContol.ThrottleYaw.canceled -= OnThrottleYawCancelled;
            // Right stick
            inputDPad.DPadContol.PitchRoll.performed -= OnPitchRollPerformed;
            inputDPad.DPadContol.PitchRoll.canceled -= OnPitchRollCancelled;
            // Right trigger (spray)
            inputDPad.DPadContol.Spray.performed -= OnSprayPerformed;
            inputDPad.DPadContol.Spray.canceled -= OnSprayCancelled;
        }
        if (name == "Washing Drone")
        {
            inputXRI.Disable();
            // Left stick
            inputXRI.XRILeftHandLocomotion.Move.performed -= OnThrottleYawPerformed;
            inputXRI.XRILeftHandLocomotion.Move.canceled -= OnThrottleYawCancelled;
            // Right stick
            inputXRI.XRIRightHandLocomotion.Move.performed -= OnPitchRollPerformed;
            inputXRI.XRIRightHandLocomotion.Move.canceled -= OnPitchRollCancelled;
            // Right trigger (spray)
            inputXRI.XRIRightHandInteraction.Activate.performed -= OnSprayPerformed;
            inputXRI.XRIRightHandInteraction.Activate.canceled -= OnSprayCancelled;
        }

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



    private void SpawnSpheres()
    { 
    
    }

    private void Rotate()
    {
        float maxRadPerSec = maxAngularSpeed * Mathf.PI / 180;
        float timeToMaxAngularSpeed = 1;

        // Level the drone
        if (throttleYawVector == Vector2.zero && pitchRollVector == Vector2.zero)
        {
            float speed = 0.6f;
            float singleStep = speed * Time.deltaTime;

            // Projecting local forward vector onto global level plane.
            Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            // Global up.
            Vector3 targetUp = Vector3.up;

            // Rotate the forward vector towards target direction by one step
            Vector3 newForward = Vector3.RotateTowards(transform.forward, targetForward, singleStep, 0.0f);

            // Rotate the up vector towards target direction by one step
            Vector3 newUp = Vector3.RotateTowards(transform.up, targetUp, singleStep, 0.0f);

            // Seting new rotation
            transform.rotation = Quaternion.LookRotation(newForward, newUp);

            // Slowing angular momentum
            rb.angularVelocity *= 0.95f;
        }

        // Rotate the drone
        else
        {
            // Yaw
            if (Mathf.Abs(throttleYawVector.x) < 0.2f) throttleYawVector.x = 0f;
            rb.AddRelativeTorque(new Vector3(0f, throttleYawVector.x * (rotateTimer / timeToMaxAngularSpeed), 0f),ForceMode.Impulse);

            // Capping angular y_velocity
            if (Mathf.Abs(rb.angularVelocity.y) > maxRadPerSec) rb.angularVelocity = new Vector3(rb.angularVelocity.x, maxRadPerSec * (rb.angularVelocity.y/ Mathf.Abs(rb.angularVelocity.y)), rb.angularVelocity.z);

            if (!(transform.localEulerAngles.x < 20 || transform.localEulerAngles.x > 340))
            {
            }
            if (!(transform.localEulerAngles.z < 20 || transform.localEulerAngles.z > 340))
            {
            }

            // Pitch
            if (Mathf.Abs(transform.rotation.eulerAngles.x) < maxDegPitchRoll || Mathf.Abs(transform.rotation.eulerAngles.x) > 360f - maxDegPitchRoll)
            {
                if (Mathf.Abs(pitchRollVector.y) < 0.5f) pitchRollVector.y = 0f;
                transform.Rotate(new Vector3(pitchRollVector.y * maxAngularSpeed / 5f, 0f, 0f) * Time.deltaTime, Space.Self);
            }

            // Roll
            if (Mathf.Abs(transform.rotation.eulerAngles.z) < maxDegPitchRoll || Mathf.Abs(transform.rotation.eulerAngles.z) > 360f - maxDegPitchRoll)
            {
                if (Mathf.Abs(pitchRollVector.x) < 0.5f) pitchRollVector.x = 0f;
                transform.Rotate(new Vector3(0f, 0f, -1f * pitchRollVector.x * maxAngularSpeed / 5f) * Time.deltaTime, Space.Self);
            }
        }

        rotateTimer += Time.deltaTime;
        if (rotateTimer > timeToMaxAngularSpeed) rotateTimer = timeToMaxAngularSpeed;
    }

    private void ChangeVelocity()
    {
        float speedChangeTime = 0.5f;
        if (Mathf.Sign(throttleYawVector_prev.y) != Mathf.Sign(throttleYawVector.y)) speedChangeTime *= 2;

        // Current velocity
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float local_y = Mathf.Lerp(throttleYawVector_prev.y * maxSpeed, throttleYawVector.y * maxSpeed, Mathf.Pow(speedTimer/speedChangeTime, 1.5f));
        localVelocity.y = local_y;

        // Cap speed
        if (Mathf.Abs(localVelocity.x) > maxSpeed) localVelocity = new Vector3(
            maxSpeed * (localVelocity.x / Mathf.Abs(localVelocity.x)),
            localVelocity.y,
            localVelocity.z
            );
        if (Mathf.Abs(localVelocity.y) > maxSpeed) localVelocity = new Vector3(
            localVelocity.x,
            maxSpeed * (localVelocity.y / Mathf.Abs(localVelocity.y)),
            localVelocity.z
            );
        if (Mathf.Abs(localVelocity.z) > maxSpeed) localVelocity = new Vector3(
            localVelocity.x,
            localVelocity.y,
            maxSpeed * (localVelocity.z / Mathf.Abs(localVelocity.z))
            );

        // Apply new velocity
        rb.velocity = transform.TransformDirection(localVelocity);

        speedTimer += Time.deltaTime;
        if (speedTimer > speedChangeTime) speedTimer = speedChangeTime;
    }

    private void ApplyDrag()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            rb.AddForce(0.01f * -rb.velocity, ForceMode.VelocityChange);
        }
    }



    // ########## Left stick callbacks ##########
    private void OnThrottleYawPerformed(InputAction.CallbackContext value)
    {
        if (throttleYawVector.x == 0) rotateTimer = 0; // Starting new rotation.

        throttleYawVector_prev = throttleYawVector;
        throttleYawVector = value.ReadValue<Vector2>();
    }

    private void OnThrottleYawCancelled(InputAction.CallbackContext value)
    {
        throttleYawVector_prev = throttleYawVector;
        throttleYawVector = Vector2.zero;
        speedTimer = 0f;
        rotateTimer = 0;
    }

    // ########## Right stick callbacks ##########
    private void OnPitchRollPerformed(InputAction.CallbackContext value)
    {
        pitchRollVector = value.ReadValue<Vector2>();
    }

    private void OnPitchRollCancelled(InputAction.CallbackContext value)
    {
        Vector2 newInput = value.ReadValue<Vector2>();
        float newX = newInput.x;
        float newY = newInput.y;
        if (newX == 0 && newY != 0) pitchRollVector = new Vector2(0, pitchRollVector.y);
        if (newX != 0 && newY == 0) pitchRollVector = new Vector2(pitchRollVector.x, 0);
        if (newX == 0 && newY == 0) pitchRollVector = Vector2.zero;
    }

    private void OnSprayPerformed(InputAction.CallbackContext value)
    {
        if (!droneSpray.isPlaying)
        {
            droneSpray.Play();
            isSpraying = true;
        }
        else
        {
            droneSpray.Stop();
            isSpraying = false;
        }
    }

    private void OnSprayCancelled(InputAction.CallbackContext value)
    {

    }


}

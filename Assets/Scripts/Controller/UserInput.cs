using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    private DPad_Control inputDPad = null;
    private XRI_Control inputXRI = null;

    //
    private QuadcopterController quadcopterController;
    //

    [SerializeField] private ParticleSystem droneSpray;
    [HideInInspector] public bool isSpraying = false;
    private Rigidbody rb;
    public Vector2 throttleYawVector = Vector2.zero;
    public Vector2 throttleYawVector_prev = Vector2.zero;
    public Vector2 pitchRollVector = Vector2.zero;
    public float speedTimer = 0;
    public float rotateTimer = 0;

    float maxSpeed = 2;                // Units per second.
    public float maxAngularSpeed = 90;  // Degrees per second.
    public float maxDegPitchRoll = 30;  // Maximum degrees pitch/roll.

    private void Awake()
    {
        inputDPad = new DPad_Control();
        inputXRI = new XRI_Control();
     
    }


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
            //inputDPad.DPadContol.Spray.performed += OnSprayPerformed;
            //inputDPad.DPadContol.Spray.canceled += OnSprayCancelled;
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
            //inputXRI.XRIRightHandInteraction.Activate.performed += OnSprayPerformed;
            //inputXRI.XRIRightHandInteraction.Activate.canceled += OnSprayCancelled;
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
            //inputDPad.DPadContol.Spray.performed -= OnSprayPerformed;
            //inputDPad.DPadContol.Spray.canceled -= OnSprayCancelled;
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
            //inputXRI.XRIRightHandInteraction.Activate.performed -= OnSprayPerformed;
            //inputXRI.XRIRightHandInteraction.Activate.canceled -= OnSprayCancelled;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        quadcopterController = FindFirstObjectByType<QuadcopterController>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // user input from UnityEngine.InputSystem
       
        // apply to main drone
        if (quadcopterController != null)
        {   // ApplyUserInput(float roll, float pitch, float yaw, float throttle)
            quadcopterController.ApplyUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
        }
        else
        {
            Debug.LogError("UserInputController: QuadcopterController reference is null.");
            // set custom parameter offsets for secondary drone
           
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

 

}

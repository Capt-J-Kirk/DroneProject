using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput GCInstance;
    private DPad_Control inputDPad = null;
    private XRI_Control inputXRI = null;

    //
    private QuadcopterController quadcopterController;
    private ObjectTransform Sec_quadcopterController;
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

    /// <Secondary drone>
    public bool AdjustOffsetParameter = false; // adjust the offset betwwen main and seondary drone
    public Vector2 Sec_throttleYawVector = Vector2.zero;
    public Vector2 Sec_throttleYawVector_prev = Vector2.zero;
    public Vector2 Sec_pitchRollVector = Vector2.zero;
    /// </Secondary drone>
    


    private void Awake()
    {
        inputDPad = new DPad_Control();
        inputXRI = new XRI_Control();

        GCInstance = this;
     
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
    void Awake()
    {
        quadcopterController = FindFirstObjectByType<QuadcopterController>();
        Sec_quadcopterController = FindFirstObjectByType<ObjectTransform>();
    }
    void Start()
    {
        //quadcopterController = FindFirstObjectByType<QuadcopterController>();
        //Sec_quadcopterController = FindFirstObjectByType<ObjectTransform>();
        //sec_ControlInput = FindFirstObjectByType<QuadcopterController_sec>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            AdjustOffsetParameter = !AdjustOffsetParameter;
        }
        HandleInput();


    }

    void HandleInput()
    {
        // user input from UnityEngine.InputSystem

        //Check that adjust offset is NOT active.
       if (!AdjustOffsetParameter)
       {
        // apply to main drone
        if (quadcopterController != null)
        {   // ApplyUserInput(float roll, float pitch, float yaw, float throttle)
            //quadcopterController.ApplyUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
            GetComponent<QuadcopterController>().ApplyUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
        }
        else
        {
            Debug.LogError("UserInputController: QuadcopterController reference is null.");
            // set custom parameter offsets for secondary drone
           
        }
       }
       else
       {
        // apply to sec drone 
        if (Sec_quadcopterController != null)
        {   
            // INSERT the rot and pos
            //GetComponent<ObjectTransform>().SetTransformationParameters();
            Sec_quadcopterController.SetUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
        }
        else
        {
            Debug.LogError("UserInputController: ObjectTransform reference is null.");
            // set custom parameter offsets for secondary drone
           
        }
       }
      



    }

        // ########## Left stick callbacks ##########
        private void OnThrottleYawPerformed(InputAction.CallbackContext value)
    {
        if (throttleYawVector.x == 0) rotateTimer = 0; // Starting new rotation.

        throttleYawVector_prev = throttleYawVector;
        throttleYawVector = value.ReadValue<Vector2>();
        Debug.Log("throttle/yaw " +throttleYawVector);
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

    // ########## Sec Drone parameter adjuster callbacks ##########
    private void OnXXXClick(InputAction.CallbackContext value)
    {
        // enable the adjustment of the secondary drones offset parameters

    }

}

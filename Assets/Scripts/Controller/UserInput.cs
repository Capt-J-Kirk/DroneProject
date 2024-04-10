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
   //private ObjectTransform Sec_quadcopterController;
    //

    [SerializeField] private ParticleSystem droneSpray;
    [HideInInspector] public bool isSpraying = false;
    private Rigidbody rb;
    public Vector2 throttleYawVector = Vector2.zero;
    public Vector2 throttleYawVector_prev = Vector2.zero;
    public Vector2 pitchRollVector = Vector2.zero;
    public float speedTimer = 0;
    public float rotateTimer = 0;

    float maxSpeed = 2;                 // Units per second.
    public float maxAngularSpeed = 90;  // Degrees per second.
    public float maxDegPitchRoll = 30;  // Maximum degrees pitch/roll.

    /// <Secondary drone>
    public bool AdjustOffsetParameter = false; // adjust the offset betwwen main and seondary drone
    public Vector2 Sec_throttleYawVector = Vector2.zero;
    public Vector2 Sec_throttleYawVector_prev = Vector2.zero;
    public Vector2 Sec_pitchRollVector = Vector2.zero;
    /// </Secondary drone>
    
    public bool ManualControl = false;
    public bool togglesecondarDrone = false;


    public GameObject secDrone;
    public GameObject mainDrone;
    ObjectTransform transformAdjustment;
    QuadcopterController_sec quadcopterController_sec;

    private void Awake()
    {
        inputDPad = new DPad_Control();
        inputXRI = new XRI_Control();

        //GCInstance = this;
     
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
        if ((name == "Washing Drone") || (name == "drone_main"))
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
            inputXRI.XRIRightHandInteraction.A.performed += OnAClick;

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
        //quadcopterController = FindFirstObjectByType<QuadcopterController>();
        //QuadcopterController_sec quadcopterController_sec = secDrone.GetComponent<QuadcopterController_sec>();
        //ObjectTransform transformAdjustment = secDrone.GetComponent<ObjectTransform>();
        
        //quadcopterController_sec = FindFirstObjectByType<QuadcopterController_sec>();
        //transformAdjustment = FindFirstObjectByType<ObjectTransform>();
        //sec_ControlInput = FindFirstObjectByType<QuadcopterController_sec>();
        if (mainDrone != null)
        {
            quadcopterController = mainDrone.GetComponent<QuadcopterController>();
            
        }
        else
        {
            Debug.Log("set ref to main Drone: ");
        }

        if (secDrone != null)
        {
            quadcopterController_sec = secDrone.GetComponent<QuadcopterController_sec>();
            transformAdjustment = secDrone.GetComponent<ObjectTransform>();
        }
        else
        {
            Debug.Log("didn't work: ");
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Switch between Main and Secondary drone
            togglesecondarDrone = !togglesecondarDrone;
            Debug.Log("toggled secondarDrone: " + togglesecondarDrone);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            // switch between manual or transform control
            ManualControl = !ManualControl;
            Debug.Log("Manual control secondarDrone: " + ManualControl);
        }

        HandleInput();
    }

    void HandleInput()
    {
        // user input from UnityEngine.InputSystem

        //if togglesecondarDrone is NOT active. pass userinput to main drone
       if (!togglesecondarDrone)
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
            // if manual control is active, control the secondary drone directly
            if (ManualControl)
            {
                if (quadcopterController_sec != null)
                {
                    quadcopterController_sec.ApplyUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
                }
                else
                {
                    Debug.LogError("UserInputController: QuadcopterController_sec reference is null.");
                }
            }
            else
            {   
                // if not active, adjust control scheme parameter
                if (transformAdjustment != null)
                {
                    transformAdjustment.SetUserInput(pitchRollVector.x, pitchRollVector.y, throttleYawVector.x, throttleYawVector.y);
                }
                else
                {
                    Debug.LogError("UserInputController: transformAdjustment reference is null.");
                }
            }



        // // apply to sec drone 
        // if (Sec_quadcopterController != null)
        // {   
        //     // INSERT the rot and pos
        //     //GetComponent<ObjectTransform>().SetTransformationParameters();
        //     Sec_quadcopterController.SetUserInput(pitchRollVector.y, pitchRollVector.x, throttleYawVector.y, throttleYawVector.x);
        // }
        // else
        // {
        //     Debug.LogError("UserInputController: ObjectTransform reference is null.");
        //     // set custom parameter offsets for secondary drone
           
        //}
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
    private void OnAClick(InputAction.CallbackContext value)
    {
        // enable the adjustment of the secondary drones offset parameters
        Debug.Log("A CLICKED!!!!");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro; 

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

    // defauld is scren 1 (main drone)
    public bool switchCamFeed = false;

    public GameObject secDrone;
    public GameObject mainDrone;
    ObjectTransform transformAdjustment;
    QuadcopterController_sec quadcopterController_sec;

    public GameObject ScreenTWO;
    public GameObject ScreenONE;
    bool switchScren = true;



    // UserInterface's infobars TWO SCREEN 
    public TMP_Text TWO_t_main_active;
    public Image TWO_i_main_active;
    public TMP_Text  TWO_t_sec_active;
    public Image TWO_i_sec_active;
    public TMP_Text TWO_t_follow;
    public Image TWO_i_follow;

    // UserInterface's infobars ONE SCREEN 
    public TMP_Text ONE_t_main_active;
    public Image ONE_i_main_active;
    public TMP_Text  ONE_t_sec_active;
    public Image ONE_i_sec_active;
    public TMP_Text ONE_t_follow;
    public Image ONE_i_follow;



    public string screen1 = ""; 

    private void Awake()
    {
        inputDPad = new DPad_Control();
        inputXRI = new XRI_Control();
        var main = droneSpray.main;
        main.loop = true;
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
            inputXRI.XRIRightHandInteraction.Activate.performed += OnSprayPerformed;
            inputXRI.XRIRightHandInteraction.Activate.canceled += OnSprayCancelled;
            // Righthand
            inputXRI.XRIRightHandInteraction.A.performed += OnAClick;
            inputXRI.XRIRightHandInteraction.B.performed += OnBClick;
            // Lefthand
            inputXRI.XRILeftHandInteraction.X.performed += OnXClick;
            inputXRI.XRILeftHandInteraction.Y.performed += OnYClick;

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
            inputXRI.XRIRightHandInteraction.Activate.performed -= OnSprayPerformed;
            inputXRI.XRIRightHandInteraction.Activate.canceled -= OnSprayCancelled;
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
        if (Input.GetKeyDown(KeyCode.C))
        {
            switchScren = !switchScren;
            //SetActive()
            if(true)
            {
                ScreenTWO.SetActive(switchScren);
                ScreenONE.SetActive(!switchScren);
            }
            
            Debug.Log("C CLICKED!!!!");
        }

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



        // update the text and color on the screens 
        if(togglesecondarDrone)
        {
            TWO_t_main_active.text = "Drone: "+ "\n" +" Master";
            TWO_i_main_active.color = Color.grey;
            TWO_t_sec_active.text = "Drone: "+"\n"+" Secondary";
            TWO_i_sec_active.color = Color.blue;

            ONE_t_main_active.text = "Drone: "+"\n"+" Master";
            ONE_i_main_active.color = Color.grey;
            ONE_t_sec_active.text = "Drone: "+"\n"+" Secondary";
            ONE_i_sec_active.color = Color.blue;
        }
        else{
            TWO_t_main_active.text = "Drone: "+"\n"+" Master";
            TWO_i_main_active.color = Color.blue;
            TWO_t_sec_active.text = "Drone: "+"\n"+" Secondary";
            TWO_i_sec_active.color = Color.grey;

            ONE_t_main_active.text = "Drone: "+"\n"+" Master";
            ONE_i_main_active.color = Color.blue;
            ONE_t_sec_active.text = "Drone: "+"\n"+" Secondary";
            ONE_i_sec_active.color = Color.grey;
        }
       

        if(transformAdjustment.toggleFollow)
        {
            TWO_t_follow.text = "Follow: "+"\n"+" ON";
            TWO_i_follow.color = Color.blue;

            ONE_t_follow.text = "Follow: "+"\n"+" ON";
            ONE_i_follow.color = Color.blue;
        }
        else{
            TWO_t_follow.text = "Follow: "+"\n"+" OFF";
            TWO_i_follow.color = Color.grey;

            ONE_t_follow.text = "Follow: "+"\n"+" OFF";
            ONE_i_follow.color = Color.grey;
        }

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
        if (togglesecondarDrone)
        {
            if (transformAdjustment.toggleFollow)
            {
                if (transformAdjustment.ControlScheme == 2)
                {
                    transformAdjustment.changeInPosition = true;
                }
                Debug.Log("changed position: " +  transformAdjustment.changeInPosition);
            }
            
        }
        // if (transformAdjustment.ControlScheme == 0)
        // {
        //     transformAdjustment.point = 1;
        // }
        // else
        // {
        //     transformAdjustment.ControlScheme = 0;
        // }
        // Debug.Log("changed position: " +  transformAdjustment.ControlScheme);
     
        Debug.Log("A CLICKED!!!!");
    }

    private void OnBClick(InputAction.CallbackContext value)
    {
        // Switch between Main and Secondary drone
        togglesecondarDrone = !togglesecondarDrone;
        Debug.Log("toggled secondarDrone: " + togglesecondarDrone);

        Debug.Log("B CLICKED!!!!");
    }

    private void OnXClick(InputAction.CallbackContext value)
    {
        // should be used to toggle between screens
        // enable the adjustment of the secondary drones offset parameters
        switchScren = !switchScren;
        //SetActive()
        if(screen1 == "1screen")
        {
            ScreenTWO.SetActive(switchScren);
            ScreenONE.SetActive(!switchScren);
        }
        
        Debug.Log("X CLICKED!!!!");
    }

    private void OnYClick(InputAction.CallbackContext value)
    {
        if(togglesecondarDrone)
        {
            transformAdjustment.toggleFollow = !transformAdjustment.toggleFollow; 
            // enable the adjustment of the secondary drones offset parameters
            if (transformAdjustment.toggleFollow)
            {
                Debug.Log("Drone: following!");
            }
            else
            {
                Debug.Log("Drone: NOT following!");
            }
        }
        // transformAdjustment.toggleFollow = !transformAdjustment.toggleFollow; 
        // // enable the adjustment of the secondary drones offset parameters
        // if (transformAdjustment.toggleFollow)
        // {
        //     Debug.Log("Drone: following!");
        // }
        // else
        // {
        //     Debug.Log("Drone: NOT following!");
        // }
        Debug.Log("Y CLICKED!!!!");
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

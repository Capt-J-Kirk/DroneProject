using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class QuadcopterController: MonoBehaviour
{
    private float maxVelocity = 200f; // Maximum linear velocity
    private float maxAcceleration = 20f; // Maximum linear acceleration
    private float drag = 0.5f; // Drag coefficient
    //private float mass = 5f; // Mass of the quadcopter
    private float maxAngularVelocity = 5f; // Maximum angular velocity
    //private float maxAngularAcceleration = 2f; // Maximum angular acceleration
    //public Vector3 thrustForce = new Vector3(0f, 5f, 0f); // Thrust force
    //private float speed = 5.0f;

    // Max angle
    public float maxYaw = 20.0f;
    public float maxRoll = 5.0f;
    public float maxPitch = 5.0f; 

    // Max velocity
    private float maxVelYaw = 100.0f;
    private float maxVelRoll = 100.0f;
    private float maxVelPitch = 100.0f; 
    private float gravityComp = 0f;
    private float prewYaw = 0f;
    public bool toggleDebug = false;


    public Rigidbody rb;
    private PIDController AltitudePID;
    private PIDController rollPIDQuaternion;
    private PIDController pitchPIDQuaternion;
    private PIDController yawPIDQuaternion;

    public UserInput inputController;
    
    // Tested to be good values
    private float rollKp = 5.0f, rollKi = 0.3f, rollKd = 0.08f;
    private float pitchKp = 5.0f, pitchKi = 0.3f, pitchKd = 0.08f;
    private float yawKp = 5.0f, yawKi = 0.3f, yawKd = 0.08f;
    private float altitudeKp = 5.0f, altitudeKi = 0.3f, altitudeKd = 0.08f; 
    // // needs fine tuning! 
    // public float rollKp = 1.0f, rollKi = 0.1f, rollKd = 0.01f;
    // public float pitchKp = 1.0f, pitchKi = 0.1f, pitchKd = 0.01f;
    // public float yawKp = 1.0f, yawKi = 0.1f, yawKd = 0.01f;
    // public float altitudeKp = 2.0f, altitudeKi = 0.1f, altitudeKd = 0.01f;    

    // desired pose
    public Vector3 desiredPosition;
    public Vector3 desiredEulerAngles; 
    public Quaternion desiredOrientation;
    // Neutral plane for calc max rotation limit (gyro)
    Quaternion neutralOrientation;


    int delay = 0;
    // calc desired pose

    public GameObject Visual_Quadcopter_main;
    

    // object avoidance
    public Transform windblade;
    private Collider targetCollider; // Collider of the target object
    private string currentState = "No Danger";
    public float distanceToObject = 0;


    // UserInterface's infobars TWO SCREEN 
    public TMP_Text TWO_t_main_dist;
    public Image TWO_i_main_dist;

    // UserInterface's infobars ONE SCREEN 
    public TMP_Text ONE_t_main_dist;
    public Image ONE_i_main_dist;


    private void TestingDesiredPose()
    {
        // used for PID tuning! 

        // Euler angles
        float newPitch = 0;
        float newRoll = 0;
        float newYaw = 0;
           
        //Debug.Log("newPitch: " + newPitch);
        //Debug.Log("newRoll: " + newRoll);
        //Debug.Log("newYaw: " + newYaw);
        //Debug.Log("newYaw: " + newYaw);
        Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;
        newPitch = Mathf.Clamp(neutralEulerAngles.x + newPitch, -maxPitch, maxPitch);
        newRoll = Mathf.Clamp(neutralEulerAngles.z + newRoll, -maxRoll, maxRoll);

        newYaw = neutralEulerAngles.y + newYaw; // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        newPitch = WrapAngle(newPitch);
        newRoll = WrapAngle(newRoll);
        newYaw = WrapAngle(newYaw);
        // update prewious Yew
        //prewYaw = newYaw;
        //Debug.Log("DesiredPitch: " + newPitch);
        //Debug.Log("DesiredYaw: " + newYaw);
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);
        Debug.Log("altitudeKp: " + altitudeKp);
        Debug.Log("altitudeKi: " + altitudeKi);
        Debug.Log("altitudeKd: " + altitudeKd);

        Vector3 currentDesiredPosition = new Vector3(0, 80f, 0);
        Debug.Log("currentDesiredPosition: " + currentDesiredPosition);
        Vector3 newPositionChange = Vector3.up * (currentDesiredPosition.y + 10f); // Assumes altitudeChange controls vertical movement
        Debug.Log("newPositionChange: " + newPositionChange);
        desiredPosition = newPositionChange;

    }

    private void CalcDesiredPose(float rollChange, float pitchChange, float yawChange, float throttleChange)
    {
        // Sensitivity factors (determine how much each input affects the pose)
        float pitchSensitivity = 10.0f;
        float rollSensitivity = 10.0f;
        float yawSensitivity = 5.0f;
        float altitudeSensitivity = 0.50f;
        float newPitch = 0;
        float newRoll = 0;
        

        // // Converting neutral orientation from Quaternion to euler angles
         Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;

        // // Calculate new angles by applying the userInput changes to the neutral orientation and limiting the angle 
        // float newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        // float newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
        // float newYaw = neutralEulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move

        if (pitchChange == 0)
        {
            // No input detected, set desiredOrientation to neutralOrientation
            newPitch = neutralOrientation.eulerAngles.x;

            // To cancel out the forward momentum, make a Impuls for a counter momentum 
            Vector3 counterTorque = new Vector3(-1*rb.angularVelocity.x,0,0);
            //ApplyCounterTorque(counterTorque);
           
        }
        else
        {
            // Calculate new angles directly within bounds
            newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
            //float newPitch = Mathf.Clamp(pitchChange * pitchSensitivity, -maxPitch, maxPitch);
            //newPitch = WrapAngle(newPitch);
        }

        if (rollChange == 0)
        {
            newRoll = neutralOrientation.eulerAngles.z;
            // To cancel out the left/right momentum, make a Impuls for a counter momentum
            Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
            //ApplyCounterTorque(counterTorque);
            
        }
        else
        {
            newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
            //float newRoll = Mathf.Clamp(rollChange * rollSensitivity, -maxRoll, maxRoll);
            //newRoll = WrapAngle(newRoll);
        }


        // 270 
        
        // may need to be the current orientation of the drone, to be able to spin 360
        float newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = transform.rotation.eulerAngles.y + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        //newYaw = WrapAngle(newYaw);
          
        if (toggleDebug)
        {
            Debug.Log("DesiredPitch: " + newPitch);
            Debug.Log("currentPitch: " + rb.transform.rotation.eulerAngles.x);
            Debug.Log("DesiredYaw: " + newYaw);
            // Debug.Log("PrewYaw: " + prewYaw);
            Debug.Log("DesiredRoll: " + newRoll);
            Debug.Log("currentRoll: " + rb.transform.rotation.eulerAngles.z);
        }

        // update prewious Yew
        prewYaw = newYaw;
        // only used for datacollector
        desiredEulerAngles = new Vector3(newPitch, newYaw, newRoll);
        //

        //Update desired orientation
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);
       
        //Debug.Log("desiredOrientation euler: " + desiredOrientation.eulerAngles);
    
        // current position should be the desired position, as its the starting position. 

        Vector3 currentDesiredPosition = desiredPosition;
        Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement

        // Update desired position
        desiredPosition = currentDesiredPosition + newPositionChange;

    }

    float WrapAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void Awake()
    {
        inputController = FindFirstObjectByType<UserInput>();
        desiredPosition = rb.transform.position;
        desiredOrientation = rb.transform.rotation;

        
        // Initialize PID controllers for each axis
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);

        rollPIDQuaternion = new PIDController(rollKp, rollKi, rollKd);
        pitchPIDQuaternion = new PIDController(pitchKp, pitchKi, pitchKd);
        yawPIDQuaternion = new PIDController(yawKp, yawKi, yawKd);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.maxAngularVelocity = maxAngularVelocity;
        // starting baseAltitude;
        desiredPosition.y = rb.transform.position.y;
        neutralOrientation = rb.transform.rotation;
        desiredOrientation = rb.transform.rotation;
        gravityComp = Mathf.Abs(Physics.gravity.y) * rb.mass;

        Visual_Quadcopter_main = GameObject.Find("Washing Drone");

        // object avoidance
        if(windblade != null)
        {
            targetCollider = windblade.GetComponent<MeshCollider>();
            //targetCollider.Convex = true;
            if (targetCollider == null)
            {
                Debug.LogError("windblade does not have a Collider!");
            }
        }
        else
        {
            Debug.LogError("windblade is not assigned!");
        }
      
    }

    void FixedUpdate()
    {
        // visual update the washing drones location
        Visual_Quadcopter_main.transform.position = transform.position;
        Visual_Quadcopter_main.transform.rotation = transform.rotation;
        //TestingDesiredPose();
        UpdatePID();
        Debug.DrawRay(rb.transform.position, Vector3.up, Color.red, duration: 5f);

        if (targetCollider != null)
        {
            // Use ClosestPoint to get the closest point on the target's surface to this GameObject
            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
            // Calculate the distance from this GameObject's position to the closest point
            distanceToObject = Vector3.Distance(transform.position, closestPoint);

            // Update the state based on the distance
            if (distanceToObject < 1f)
            {
                TWO_t_main_dist.text = "Dist Extreme Close";
                TWO_i_main_dist.color = Color.red;
                ONE_t_main_dist.text = "Dist Extreme Close";
                ONE_i_main_dist.color =  Color.red;
            }
            else if (distanceToObject < 3f)
            {
                TWO_t_main_dist.text = "Dist Close";
                TWO_i_main_dist.color = Color.yellow;
                ONE_t_main_dist.text = "Dist Close";
                ONE_i_main_dist.color =  Color.yellow;
            }
            else
            {
                TWO_t_main_dist.text = "Dist safe";
                TWO_i_main_dist.color = Color.green;
                ONE_t_main_dist.text = "Dist safe";
                ONE_i_main_dist.color =  Color.green;
            }

        }
      
    }


    void UpdatePID()
    {
    
        // get time since last update 
        float deltaTime = Time.fixedDeltaTime;

        // Get current state from sensors 
        Vector3 currentPosition = rb.transform.position;        
        //Vector3 currentEulerAngles = transform.eulerAngles;

        // Quaternion-based PID control
        Quaternion currentOrientation = rb.transform.rotation; // current orientation
        //Quaternion currentOrientation = transform.localRotation;
        //Quaternion desiredOrientation = Quaternion.Euler(desiredEulerAngles);
        
        //Debug.Log("desiredOrientation euler current: " + currentOrientation.eulerAngles);



        Vector3 angularVelocityError = AngularVelocities(currentOrientation, desiredOrientation, deltaTime);


        // Calc orientation delta
        Quaternion orientationDelta = Quaternion.Inverse(currentOrientation) * desiredOrientation;


        // Convert orientation delta to angular velocity vector (for linearity)
        //Vector3 angularVelocityError = slerp_OrientationDeltaToAngularVelocity(orientationDelta, deltaTime);
        //Vector3 angularVelocityError = OrientationDeltaToAngularVelocity(orientationDelta);
       

        // get current angularVelocity 
        Vector3 currentAngularVelocity = rb.angularVelocity;


        if (toggleDebug)
        {
            // //Debug.Log("UpdatePID called");
            Debug.Log("desiredOrientation: " + desiredOrientation);
            Debug.Log("currentOrientation: " + currentOrientation);
            Debug.Log("orientationDelta: " + orientationDelta);
            Debug.Log("angularVelocityError: " + angularVelocityError);
            // //Debug.DrawRay(transform.position, angularVelocityError * 200, Color.yellow);
            // Debug.Log("currentAngularVelocity: " + currentAngularVelocity);
            // //Debug.DrawRay(transform.position, rb.angularVelocity * 200, Color.black);
        }
       

        // PID control on the angular velocity error (closed feedback loop)

        float rollControlInput = rollPIDQuaternion.UpdateAA(angularVelocityError.z, currentAngularVelocity.z, deltaTime);
        float pitchControlInput = pitchPIDQuaternion.UpdateAA(angularVelocityError.x, currentAngularVelocity.x, deltaTime);
        float yawControlInput = yawPIDQuaternion.UpdateAA(angularVelocityError.y, currentAngularVelocity.y, deltaTime);
    
       
        float altitudeError = AltitudePID.UpdateAA(desiredPosition.y, currentPosition.y, deltaTime);
        
        // Gravity compensation 
        float altitudeControlInput = altitudeError + gravityComp;

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput);
 
    }
    // Singularity issue around 180 and 270!
    //  Vector3 OrientationDeltaToAngularVelocity(Quaternion delta)
    // {
    //     // Convert quaternion to angle-axis representation, then to angular velocity
    //     delta.ToAngleAxis(out float angle, out Vector3 axis);
    //     if (angle > 180) angle -= 360; // Convert to the shortest path
    //     Vector3 angularVelocity = axis.normalized * angle * Mathf.Deg2Rad; // Convert to radians
    //     return angularVelocity;
    // }


    // using spherical linear interpolation (slerp)
    // Vector3 slerp_OrientationDeltaToAngularVelocity(Quaternion delta, float deltaTime)
    // {
    //     // Ensure delta is normalized to avoid any scaling issues
    //     delta = Quaternion.Normalize(delta);

    //     // Use slerp to simulate the delta rotation over deltaTime
    //     // Start with identity quaternion, representing no rotation
    //     Quaternion noRotation = Quaternion.identity;
    //     // Interpolate towards delta using a very small step, assuming deltaTime is small
    //     Quaternion stepRotation = Quaternion.Slerp(noRotation, delta, deltaTime);

    //     // Compute angular displacement in quaternion form
    //     Quaternion angularDisplacement = stepRotation * Quaternion.Inverse(noRotation);

    //     // Convert angular displacement to angle-axis
    //     angularDisplacement.ToAngleAxis(out float angle, out Vector3 axis);
    //     // Ensure the axis is normalized (should already be, but just to be safe)
    //     axis = axis.normalized;

    //     // Convert angle from degrees to radians and normalize by deltaTime to get angular velocity
    //     Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / deltaTime);

    //     return angularVelocity;
    // }

    // // using the quaternion component directly 
    // Vector3 OrientationDeltaToAngularVelocity2(Quaternion delta)
    // {
    //     // Normalize delta to ensure it represents a pure rotation
    //     delta = Quaternion.Normalize(delta);

    //     // Directly work with quaternion components
    //     float w = delta.w;
    //     Vector3 xyz = new Vector3(delta.x, delta.y, delta.z);

    //     // The magnitude of xyz gives us the sin(angle/2), and w gives us cos(angle/2)
    //     float sinHalfAngle = xyz.magnitude;
    //     float cosHalfAngle = w;

    //     // Handling specifically for rotations near 180 degrees
    //     if (Mathf.Abs(w) < float.Epsilon)
    //     {
    //         // This is a special case for 180 degrees rotation
    //         // Axis is normalized xyz, angle is PI (180 degrees in radians)
    //         Vector3 axis = xyz.normalized;
    //         float angle = Mathf.PI; // 180 degrees in radians

    //         // Directly return the angular velocity for the 180-degree case
    //         Vector3 angularVelocity = axis * angle; // This assumes a very small deltaTime, effectively making it per second
    //         return angularVelocity;
    //     }
    //     else
    //     {
    //         // For general cases, calculate the full angle and normalize by deltaTime if necessary
    //         float angle = 2.0f * Mathf.Atan2(sinHalfAngle, cosHalfAngle);

    //         // Normalize the axis
    //         Vector3 axis = xyz.normalized;

    //         // Convert angle from radians to degrees and normalize by deltaTime to get angular velocity
    //         Vector3 angularVelocity = axis * angle; // This is in radians per second

    //         return angularVelocity;
    //     }
    // }

    // Vector3 OrientationDeltaToAngularVelocity(Quaternion delta)
    // {
    //     // Normalize delta to ensure it represents a pure rotation
    //     delta = Quaternion.Normalize(delta);

    //     float w = delta.w;
    //     Vector3 xyz = new Vector3(delta.x, delta.y, delta.z);

    //     // Handling specifically for rotations near 180 degrees
    //     if (Mathf.Abs(w) < float.Epsilon)
    //     {
    //         // This is a special case for 180 degrees rotation
    //         Vector3 axis = xyz.normalized;
    //         float angle = Mathf.PI; // 180 degrees in radians
    //         Vector3 angularVelocity = axis * angle; // Assumes deltaTime is very small, effectively making it per second
    //         return angularVelocity;
    //     }
    //     else
    //     {
    //         float sinHalfAngle = xyz.magnitude;
    //         float cosHalfAngle = w;
    //         float angle = 2.0f * Mathf.Atan2(sinHalfAngle, cosHalfAngle);

    //         if (angle > Mathf.PI)
    //         {
    //             angle -= 2.0f * Mathf.PI;
    //         }
    //         else if (angle < -Mathf.PI)
    //         {
    //             angle += 2.0f * Mathf.PI;
    //         }

    //         Vector3 axis = xyz.normalized;
    //         Vector3 angularVelocity = axis * angle; // This is in radians per second
    //         return angularVelocity;
    //     }
    // }


    
    public Vector3 AngularVelocities(Quaternion q1, Quaternion q2, float dt)
    {
        // Method to calculate angular velocity from two quaternions
        // Equation formula:
        // https://mariogc.com/post/angular-velocity-quaternions/?fbclid=IwAR1kXM70dvSfZLy7gwaALP_S9maYRiPM4jL5rcdO7ZJHg0Cu9uKEnkuepvE_aem_AStzX-28bYkrUS3ynU9xCW2W4pF7QOhyzEcDJQgidg3DdW5VZSaeVHdQyTHC2RA-b1oOOO4zi-uEjznNvuVJsFAI
        // converted from python to c#
        Vector3 angularVelocity = new Vector3(
            (2 / dt) * (q1.w * q2.x - q1.x * q2.w - q1.y * q2.z + q1.z * q2.y),
            (2 / dt) * (q1.w * q2.y + q1.x * q2.z - q1.y * q2.w - q1.z * q2.x),
            (2 / dt) * (q1.w * q2.z - q1.x * q2.y + q1.y * q2.x - q1.z * q2.w)
        );

        // Convert from rad/s to degrees/s
        //angularVelocity *= Mathf.Rad2Deg;

        return angularVelocity;
    }

    void ControlMotors(float roll, float pitch, float yaw, float lift)
    {
        // // new

        // // Spliting the thrust, to both the forward and lateral movement 
        // float pitchRad = Mathf.Deg2Rad * pitch;
        // float rollRad = Mathf.Deg2Rad * roll;
        // // calc thrust component
        // float forwardThrust = Mathf.Sin(pitchRad) * lift;
        // float lateralThrust = Mathf.Sin(rollRad) * lift;

        // // Apply forward and lateral thrust
        // rb.AddForce(transform.forward * forwardThrust, ForceMode.Force);
        // rb.AddForce(transform.right * lateralThrust, ForceMode.Force);

        // // calc vertical thrust component
        // float verticalAdjustment = Mathf.Sqrt(Mathf.Cos(pitchRad) * Mathf.Cos(pitchRad) + Mathf.Cos(rollRad) * Mathf.Cos(rollRad));
        // float adjustedLift = lift * verticalAdjustment;
        // rb.AddForce(Vector3.up * adjustedLift, ForceMode.Force);

        // old

        // Apply Clamp to simulate actuators that limits the control signal. 
        float throttle2 = Mathf.Clamp(lift, -maxVelocity, maxVelocity);
        // Throttle, the upward force 
        rb.AddForce(transform.up * throttle2, ForceMode.Force);

        // pitch, forward and backward
        float clampPitch = Mathf.Clamp(pitch, -maxVelPitch, maxVelPitch);
        rb.AddTorque(transform.right * clampPitch, ForceMode.Force);


        // roll, left and right
        float clampRoll = Mathf.Clamp(roll, -maxVelRoll, maxVelRoll);
        rb.AddTorque(transform.forward * clampRoll, ForceMode.Force); // might need to invert roll. 

        // yaw, left and right
        float clampYaw = Mathf.Clamp(yaw, -maxVelYaw, maxVelYaw);
        rb.AddTorque(transform.up * clampYaw, ForceMode.Force);
      

        if (toggleDebug)
        {
            //Debug.Log("lift: " + lift2);
            Debug.Log("clamplift: " + throttle2);
            //Debug.Log("Pitch: " + pitch);
            Debug.Log("clampPitch: " + clampPitch);
            //Debug.Log("Roll: " + roll);
            Debug.Log("clampRoll: " + clampRoll);
            //Debug.Log("Yaw: " + yaw);
            Debug.Log("clampYaw: " + clampYaw);

            // Debug.Log("Drone pos: " + rb.position);
            Debug.Log("Drone vel: " + rb.velocity);
            // Debug.Log("Drone rot: " + rb.transform.rotation);
            Debug.Log("Drone ang: " + rb.angularVelocity);
            // Debug.Log("throttle: " + throttle);
        }
    
    }

    public void ApplyUserInput(float roll, float pitch, float yaw, float throttle)
    {
        // yaw and throttle coming from the userInput should be swapted! 
        float temp = yaw;
        yaw = throttle;
        throttle = temp;

        // roll and pitch should be swapted aswell
        temp = roll;
        roll = pitch;
        pitch = temp;

        // filter out small noise values from joystick
        if (Mathf.Abs(roll) < 0.2f) roll = 0f;
        if (Mathf.Abs(pitch) < 0.2f) pitch = 0f;
        if (Mathf.Abs(yaw) < 0.2f) yaw = 0f;
        if (Mathf.Abs(throttle) < 0.2f) throttle = 0f;


        CalcDesiredPose(roll, pitch, yaw, throttle);
     
        if (toggleDebug)
        {
            //Debug.Log("drone r/p/y/th: " + roll + "/" + pitch + "/" + yaw + "/" + throttle);
        }
         
    }

}

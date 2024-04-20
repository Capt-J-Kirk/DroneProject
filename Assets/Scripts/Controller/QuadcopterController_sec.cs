using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class QuadcopterController_sec: MonoBehaviour
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
    private float maxVelYaw = .0f;
    private float maxVelRoll = 10.0f;
    private float maxVelPitch = 10.0f; 
    private float gravityComp = 0f;
    private float prewYaw = 0f;
    public bool toggleDebug = false;
   

    public Rigidbody rb;
    public PIDController AltitudePID;
    public PIDController rollPIDQuaternion;
    public PIDController pitchPIDQuaternion;
    public PIDController yawPIDQuaternion;

    public PIDController xPID;
    public PIDController zPID;

    public UserInput inputController;
    
    // Tested to be good values
    private float rollKp = 5.0f, rollKi = 0.3f, rollKd = 0.08f;
    private float pitchKp = 5.0f, pitchKi = 0.3f, pitchKd = 0.08f;
    private float yawKp = 5.0f, yawKi = 0.3f, yawKd = 0.08f;

    private float altitudeKp = 7.82403f, altitudeKi = 18.87807f, altitudeKd = 5f; 
    private float xKp = 7.82403f, xKi = 18.87807f, xKd = 5f; 
    private float zKp = 7.82403f, zKi = 18.87807f, zKd = 5f; 


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

    Vector3 targetPositionTEST;
    Quaternion rotTEST;

    public GameObject Visual_Quadcopter_secondary;


    // object avoidance

    // remenber to set the windblade in the inspector!
    public Transform windblade;
    private Collider targetCollider; // Collider of the target object
    private string currentState = "No Danger";
    public float distanceToObject = 0;
    Vector3 closestPoint;

    // UserInterface's infobars TWO SCREEN 
    public TMP_Text TWO_t_sec_dist;
    public Image TWO_i_sec_dist;

    // UserInterface's infobars ONE SCREEN 
    public TMP_Text ONE_t_sec_dist;
    public Image ONE_i_sec_dist;


    // add blinking functionality
    private bool shouldBlink = false;





    // calc desired pose
    public Quaternion getneutralOrientation()
    {
        return neutralOrientation;
    }

    // void MoveTowardsTarget2(Vector3 targetPosition, Quaternion rot)
    // {
    //     float speed = 0.5f;
    //     float turnSpeed = 0.5f;
    //     Vector3 currentPosition = transform.position;
    //     Vector3 directionToTarget = (targetPosition - currentPosition).normalized;

    //     // Calculate pitch. No changes needed here as your approach was correct.
    //     float targetPitch = Mathf.Asin(directionToTarget.x) * Mathf.Rad2Deg;

    //     // Extract yaw from the given rotation 'rot'.
    //     float targetYaw = rot.eulerAngles.y;

    //     // Roll might not be directly calculable in the context of just moving towards a point without more context
    //     // (e.g., desired bank angle during a turn, which would involve the object's current velocity and desired turn rate).
    //     // For a basic implementation that orients towards a target, roll is typically not adjusted based on target position.
    //     // Here, we'll set roll to zero for simplicity unless you have a specific use case requiring it.
    //     float targetRoll = 0f;

    //     // Clamp pitch values within allowed limits.
    //     targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);

    //     // For this implementation, we're assuming roll is not influenced by the target position, so it's not clamped.
    //     targetRoll = Mathf.Clamp(targetRoll, -maxRoll, maxRoll); // Uncomment if roll is determined differently.

    //     // Apply the calculated orientation and the target position.
    //     // Here, we set the object's orientation directly, but for smooth movement, consider using Quaternion.Lerp or Quaternion.Slerp.
    //     Quaternion desiredOrientation2 = Quaternion.Euler(targetPitch, targetYaw, targetRoll);
    //     Vector3 desiredPosition2 = targetPosition;

    //     // To apply the orientation, you might do something like this in Update or FixedUpdate:
    //     transform.position = Vector3.MoveTowards(transform.position, desiredPosition2, speed * Time.deltaTime);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, desiredOrientation2, turnSpeed * Time.deltaTime);
    // }

    // void MoveTowardsTarget(Vector3 targetPosition, Quaternion rot)
    // {
             
    //     // pitch should be x
    //     // roll = z
    //     // they should be controlling the horizontale plane movement
    //     // altitude is z 

    //     Vector3 directionToTarget = (targetPosition - transform.position).normalized;
    //     float horizontalDistance = new Vector3(directionToTarget.x, 0, directionToTarget.z).magnitude;
    //     Debug.Log("directionToTarget: " + directionToTarget);
    //     Debug.Log("horizontalDistance: " + horizontalDistance);

    //     directionToTarget = directionToTarget / horizontalDistance; // Normalize horizontal component
    //     Debug.Log("directionToTarget: " + directionToTarget);
    //     // Calculate pitch and roll based on the target's direction
    //     float targetPitch = Mathf.Asin(directionToTarget.y) * Mathf.Rad2Deg; // Asin gives the angle in radians, convert to degrees
    //     float targetRoll = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

    //     // clamp pitch and roll 
    //     targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);
    //     targetRoll = Mathf.Clamp(targetRoll, -maxRoll, maxRoll);
    //     Debug.Log("targetPitch: " + targetPitch);
    //     Debug.Log("targetRoll: " + targetRoll);

    //     // Apply desired orientation and position
    //     desiredOrientation = Quaternion.Euler(targetPitch, rot.y, targetRoll);
    //     desiredPosition = targetPosition;
       
    // }

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
        float pitchSensitivity = 5.0f;
        float pitch_x = 2.0f;
        float rollSensitivity = 10.0f;
        float roll_z = 2.0f;
        float yawSensitivity = 5.0f;
        float altitudeSensitivity = 0.50f;
        float newPitch = 0;
        float newRoll = 0;
        float xSensitivity = 0.50f;
        float ySensitivity = 0.50f;
        float zSensitivity = 0.50f;
        
        float xlocalChange = pitchChange;
        float ylocalChange = throttleChange;
        float zlocalChange = rollChange;


        
        Vector3 neutralEulerAngles = neutralOrientation.eulerAngles;

    
        if (pitchChange == 0)
        {
            // No input detected, set desiredOrientation to neutralOrientation
            newPitch = neutralOrientation.eulerAngles.x;
       
        }
        else
        {
            // Calculate new angles directly within bounds
            newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
        
        }

        if (rollChange == 0)
        {
            newRoll = neutralOrientation.eulerAngles.z;
       
        }
        else
        {
            newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
           
        }
        
        // may need to be the current orientation of the drone, to be able to spin 360
        float newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move

          
        if (toggleDebug)
        {
            Debug.Log("DesiredPitch: " + newPitch);
            Debug.Log("DesiredYaw: " + newYaw);
            Debug.Log("PrewYaw: " + prewYaw);
            Debug.Log("DesiredRoll: " + newRoll);
        }

        // update prewious Yew
        prewYaw = newYaw;

        // only used for datacollector
        desiredEulerAngles = new Vector3(newPitch, newYaw, newRoll);
        //

        //Update desired orientation
        desiredOrientation = Quaternion.Euler(newPitch, newYaw, newRoll);

        Vector3 currentDesiredPosition = desiredPosition;

        Vector3 newPositionChangeLocal = new Vector3(
            xlocalChange * xSensitivity,
            ylocalChange * ySensitivity,
            zlocalChange * zSensitivity);

        // Convert local position change to world space
        Vector3 newPositionChangeWorld = transform.TransformPoint(newPositionChangeLocal) - transform.position;

        // Update desired position
        desiredPosition = currentDesiredPosition + newPositionChangeWorld;

    }

    float WrapAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        //inputController = FindFirstObjectByType<UserInput>();
        desiredPosition = rb.transform.position;
        desiredOrientation = rb.transform.rotation;

        
        // Initialize PID controllers for each axis
        AltitudePID =  new PIDController(altitudeKp, altitudeKi, altitudeKd);

        rollPIDQuaternion = new PIDController(rollKp, rollKi, rollKd);
        pitchPIDQuaternion = new PIDController(pitchKp, pitchKi, pitchKd);
        yawPIDQuaternion = new PIDController(yawKp, yawKi, yawKd);

        xPID = new PIDController(xKp, xKi, xKd);
        zPID = new PIDController(zKp, zKi, zKd);
    }

    void Start()
    {
        // rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
        // starting baseAltitude;
        desiredPosition = transform.position;
        neutralOrientation = transform.rotation;
        desiredOrientation = transform.rotation;
        gravityComp = Mathf.Abs(Physics.gravity.y) * rb.mass;

        Visual_Quadcopter_secondary = GameObject.Find("Observing Drone");

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
        //TestingDesiredPose();
        Visual_Quadcopter_secondary.transform.position = transform.position;
        Visual_Quadcopter_secondary.transform.rotation = transform.rotation;

        //MoveTowardsTarget2(targetPositionTEST, rotTEST);
        UpdatePID();

        if (targetCollider != null)
        {
            // Use ClosestPoint to get the closest point on the target's surface to this GameObject
            closestPoint = targetCollider.ClosestPoint(transform.position);
            // Calculate the distance from this GameObject's position to the closest point
            distanceToObject = Vector3.Distance(transform.position, closestPoint);

            // Update the state based on the distance
            if (distanceToObject < 1.2f)
            {
                shouldBlink = true;
                StartBlinking(Color.red);
                TWO_t_sec_dist.text = "Dist Extreme Close";
                ONE_t_sec_dist.text = "Dist Extreme Close";
            }
            else if (distanceToObject < 4f)
            {
                shouldBlink = false;
                StopBlinking();
                TWO_t_sec_dist.text = "Dist Close";
                TWO_i_sec_dist.color = Color.yellow;
                ONE_t_sec_dist.text = "Dist Close";
                ONE_i_sec_dist.color = Color.yellow;
            }
            else
            {
                shouldBlink = false;
                StopBlinking();
                TWO_t_sec_dist.text = "Dist Safe";
                TWO_i_sec_dist.color = Color.green;
                ONE_t_sec_dist.text = "Dist Safe";
                ONE_i_sec_dist.color = Color.green;
            }

          
        }
    }

    IEnumerator Blink(Color blinkColor)
    {
        while (shouldBlink)
        {
            TWO_i_sec_dist.color = blinkColor;
            ONE_i_sec_dist.color = blinkColor;
            yield return new WaitForSeconds(0.5f); // Blink interval
            TWO_i_sec_dist.color = Color.clear; // Choose the off color, e.g., clear or white
            ONE_i_sec_dist.color = Color.clear;
            yield return new WaitForSeconds(0.5f);
        }
        // Reset color to current state
        TWO_i_sec_dist.color = blinkColor;
        ONE_i_sec_dist.color = blinkColor;
    }

    void StartBlinking(Color color)
    {
        StopCoroutine("Blink");
        StartCoroutine(Blink(color));
    }

    void StopBlinking()
    {
        StopCoroutine("Blink");
        TWO_i_sec_dist.color = TWO_i_sec_dist.color; // Reset to current non-blinking color
        ONE_i_sec_dist.color = ONE_i_sec_dist.color;
    }


    void UpdatePID()
    {
    
        // get time since last update 
        float deltaTime = Time.fixedDeltaTime;

        // Get current state from sensors 
        Vector3 currentPosition = transform.position;        
        //Vector3 currentEulerAngles = transform.eulerAngles;

        // Quaternion-based PID control
        Quaternion currentOrientation = transform.rotation; // current world orientation
   
       
        // ---- angularVelocity Error ----

        Vector3 angularVelocityError = AngularVelocities(currentOrientation, desiredOrientation, deltaTime);


        // get current angularVelocity 
        Vector3 currentAngularVelocity = rb.angularVelocity;

        
        // ---- Object avoidance ----

        float minDistance = 0.5f;
        if (distanceToObject < minDistance)
        {
            // Calculate the direction from the object to the GameObject
            Vector3 directionFromObject = currentPosition - closestPoint;
            directionFromObject.Normalize();  // Normalize the direction vector

            // Set the new desired position to maintain at least minDistance
            //desiredPosition = closestPoint + directionFromObject * minDistance;
        }


        // calculate the velocity 
        Vector3 vectorToTarget = desiredPosition - currentPosition;
        Vector3 Targetvelocity = vectorToTarget / deltaTime;

        // get current Velocity 
        Vector3 currentVelocity = rb.velocity;

        // PID control on the angular velocity error (closed feedback loop)

        float rollControlInput = rollPIDQuaternion.UpdateAA(angularVelocityError.z, currentAngularVelocity.z, deltaTime);
        float pitchControlInput = pitchPIDQuaternion.UpdateAA(angularVelocityError.x, currentAngularVelocity.x, deltaTime);
        float yawControlInput = yawPIDQuaternion.UpdateAA(angularVelocityError.y, currentAngularVelocity.y, deltaTime);
    
       
        float altitudeError = AltitudePID.UpdateAA(Targetvelocity.y, currentVelocity.y, deltaTime);
        float xControlInput = xPID.UpdateAA(Targetvelocity.x, currentVelocity.x, deltaTime);
        float zControlInput = zPID.UpdateAA(Targetvelocity.z, currentVelocity.z, deltaTime);
        // Gravity compensation 
        float altitudeControlInput = altitudeError + gravityComp;

        // Apply control input 
        ControlMotors(rollControlInput, pitchControlInput, yawControlInput, altitudeControlInput, xControlInput, zControlInput);
 
    }

     Vector3 OrientationDeltaToAngularVelocity(Quaternion delta)
    {
        // Convert quaternion to angle-axis representation, then to angular velocity
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180) angle -= 360; // Convert to the shortest path
        Vector3 angularVelocity = axis.normalized * angle * Mathf.Deg2Rad; // Convert to radians
        return angularVelocity;
    }

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

    void ControlMotors(float roll, float pitch, float yaw, float lift2, float x, float z)
    {
        // convert from world space to local space, then applying force
        Vector3 controlForceWorld = new Vector3(x, lift2, z);
        Vector3 controlForceLocal = transform.InverseTransformDirection(controlForceWorld);

        // apply force in x, y, and z
        controlForceLocal.x = Mathf.Clamp(controlForceLocal.x, -maxVelocity, maxVelocity);
        controlForceLocal.y = Mathf.Clamp(controlForceLocal.y, -maxVelocity, maxVelocity);
        controlForceLocal.z = Mathf.Clamp(controlForceLocal.z, -maxVelocity, maxVelocity);
        rb.AddForce(controlForceLocal, ForceMode.Force);



        // float xforce = Mathf.Clamp(x, -maxVelocity, maxVelocity);
        // rb.AddForce(transform.right * xforce, ForceMode.Force);

        // float zforce = Mathf.Clamp(z, -maxVelocity, maxVelocity);
        // rb.AddForce(transform.forward * zforce, ForceMode.Force);

        // // Apply Clamp to simulate actuators that limits the control signal. 
        // float throttle2 = Mathf.Clamp(lift2, -maxVelocity, maxVelocity);
        // // Throttle, the upward force 
        // rb.AddForce(transform.up * throttle2, ForceMode.Force);

        // pitch, forward and backward
        float clampPitch = Mathf.Clamp(pitch, -maxVelPitch, maxVelPitch);
        //rb.AddTorque(transform.right * clampPitch, ForceMode.Force);


        // roll, left and right
        float clampRoll = Mathf.Clamp(roll, -maxVelRoll, maxVelRoll);
        //rb.AddTorque(transform.forward * -clampRoll, ForceMode.Force); // might need to invert roll. 

        // yaw, left and right
        float clampYaw = Mathf.Clamp(yaw, -maxVelYaw, maxVelYaw);
        rb.AddTorque(transform.up * clampYaw, ForceMode.Force);
        //Debug.Log("lift: " + lift2);
        //Debug.Log("clamplift: " + throttle2);

        if (toggleDebug)
        {
            //Debug.Log("lift: " + lift2);
            //Debug.Log("clamplift: " + throttle2);
            //Debug.Log("Pitch: " + pitch);
            //Debug.Log("clampPitch: " + clampPitch);
            //Debug.Log("Roll: " + roll);
            //Debug.Log("clampRoll: " + clampRoll);
            //Debug.Log("Yaw: " + yaw);
            //Debug.Log("clampYaw: " + clampYaw);

            // Debug.Log("Drone pos: " + rb.position);
            // Debug.Log("Drone vel: " + rb.velocity);
            // Debug.Log("Drone rot: " + rb.transform.rotation);
            //Debug.Log("Drone ang: " + rb.angularVelocity);
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

    public void SetQuadcopterPose(Vector3 pos, Quaternion rot)
    {
        // transform.position = pos;
        // transform.rotation = rot;
        //MoveTowardsTarget(pos, rot);
        // Already calc by the transform script, for follow behaviour.
        desiredOrientation = rot;
        desiredPosition = pos;
        if (toggleDebug)
        {
            Debug.Log("pos modtaget: " + pos);
            Debug.Log("rot modtaget: " + rot);
        }
        
    }

    private void ApplyCounterTorque(Vector3 counterTorque)
    {
        //rb.AddTorque(counterTorque, ForceMode.Impulse);
    }

}

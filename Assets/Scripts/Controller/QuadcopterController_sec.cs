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
    private PIDController AltitudePID;
    private PIDController rollPIDQuaternion;
    private PIDController pitchPIDQuaternion;
    private PIDController yawPIDQuaternion;

    private PIDController xPID;
    private PIDController zPID;

    public UserInput inputController;
    
    // Tested to be good values
    private float rollKp = 5.0f, rollKi = 0.3f, rollKd = 0.08f;
    private float pitchKp = 5.0f, pitchKi = 0.3f, pitchKd = 0.08f;
    private float yawKp = 5.0f, yawKi = 0.3f, yawKd = 0.08f;
    private float altitudeKp = 5.0f, altitudeKi = 0.3f, altitudeKd = 0.08f; 

    private float xKp = 5.0f, xKi = 0.3f, xKd = 0.08f; 
    private float zKp = 5.0f, zKi = 0.3f, zKd = 0.08f; 
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


    // calc desired pose
    public Quaternion getneutralOrientation()
    {
        return neutralOrientation;
    }

    void MoveTowardsTarget2(Vector3 targetPosition, Quaternion rot)
    {
        float speed = 0.5f;
        float turnSpeed = 0.5f;
        Vector3 currentPosition = transform.position;
        Vector3 directionToTarget = (targetPosition - currentPosition).normalized;

        // Calculate pitch. No changes needed here as your approach was correct.
        float targetPitch = Mathf.Asin(directionToTarget.x) * Mathf.Rad2Deg;

        // Extract yaw from the given rotation 'rot'.
        float targetYaw = rot.eulerAngles.y;

        // Roll might not be directly calculable in the context of just moving towards a point without more context
        // (e.g., desired bank angle during a turn, which would involve the object's current velocity and desired turn rate).
        // For a basic implementation that orients towards a target, roll is typically not adjusted based on target position.
        // Here, we'll set roll to zero for simplicity unless you have a specific use case requiring it.
        float targetRoll = 0f;

        // Clamp pitch values within allowed limits.
        targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);

        // For this implementation, we're assuming roll is not influenced by the target position, so it's not clamped.
        targetRoll = Mathf.Clamp(targetRoll, -maxRoll, maxRoll); // Uncomment if roll is determined differently.

        // Apply the calculated orientation and the target position.
        // Here, we set the object's orientation directly, but for smooth movement, consider using Quaternion.Lerp or Quaternion.Slerp.
        Quaternion desiredOrientation2 = Quaternion.Euler(targetPitch, targetYaw, targetRoll);
        Vector3 desiredPosition2 = targetPosition;

        // To apply the orientation, you might do something like this in Update or FixedUpdate:
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition2, speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredOrientation2, turnSpeed * Time.deltaTime);
    }

    void MoveTowardsTarget(Vector3 targetPosition, Quaternion rot)
    {
             
        // pitch should be x
        // roll = z
        // they should be controlling the horizontale plane movement
        // altitude is z 

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float horizontalDistance = new Vector3(directionToTarget.x, 0, directionToTarget.z).magnitude;
        Debug.Log("directionToTarget: " + directionToTarget);
        Debug.Log("horizontalDistance: " + horizontalDistance);

        directionToTarget = directionToTarget / horizontalDistance; // Normalize horizontal component
        Debug.Log("directionToTarget: " + directionToTarget);
        // Calculate pitch and roll based on the target's direction
        float targetPitch = Mathf.Asin(directionToTarget.y) * Mathf.Rad2Deg; // Asin gives the angle in radians, convert to degrees
        float targetRoll = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        // clamp pitch and roll 
        targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);
        targetRoll = Mathf.Clamp(targetRoll, -maxRoll, maxRoll);
        Debug.Log("targetPitch: " + targetPitch);
        Debug.Log("targetRoll: " + targetRoll);

        // Apply desired orientation and position
        desiredOrientation = Quaternion.Euler(targetPitch, rot.y, targetRoll);
        desiredPosition = targetPosition;
       
    }

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
            Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
            ApplyCounterTorque(counterTorque);
        }
        else
        {
            // Calculate new angles directly within bounds
            newPitch = Mathf.Clamp(neutralEulerAngles.x + (pitchChange * pitchSensitivity), -maxPitch, maxPitch);
            //float newPitch = Mathf.Clamp(pitchChange * pitchSensitivity, -maxPitch, maxPitch);
            newPitch = WrapAngle(newPitch);
        }

        if (rollChange == 0)
        {
            newRoll = neutralOrientation.eulerAngles.z;
            // To cancel out the left/right momentum, make a Impuls for a counter momentum 
            Vector3 counterTorque = new Vector3(0,0,-1*rb.angularVelocity.z);
            ApplyCounterTorque(counterTorque);
        }
        else
        {
            newRoll = Mathf.Clamp(neutralEulerAngles.z + (rollChange * rollSensitivity), -maxRoll, maxRoll);
            //float newRoll = Mathf.Clamp(rollChange * rollSensitivity, -maxRoll, maxRoll);
            newRoll = WrapAngle(newRoll);
        }
        
        // may need to be the current orientation of the drone, to be able to spin 360
        float newYaw = neutralEulerAngles.y + prewYaw + (yawChange * yawSensitivity); // yaw can freely move
        //float newYaw = yawChange * yawSensitivity; // yaw can freely move
        newYaw = WrapAngle(newYaw);
          
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

        // x
        float newPositionChangeX = currentDesiredPosition.x + pitchChange * pitch_x;
        // y
        float newPositionChangeY = currentDesiredPosition.y * throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement
        // z 
        float newPositionChangeZ = currentDesiredPosition.z * rollChange * roll_z;

        //Vector3 newPositionChangeY = Vector3.up * throttleChange * altitudeSensitivity;
        // Update desired position
        //desiredPosition = currentDesiredPosition + newPositionChange;
        desiredPosition = new Vector3(newPositionChangeX, newPositionChangeY, newPositionChangeZ);
        // current position should be the desired position, as its the starting position. 

        // Vector3 currentDesiredPosition = desiredPosition;
        // Vector3 newPositionChange = Vector3.up * throttleChange * altitudeSensitivity; // Assumes altitudeChange controls vertical movement

        // // Update desired position
        // desiredPosition = currentDesiredPosition + newPositionChange;

    }

    float WrapAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void Awake()
    {
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
        rb = GetComponent<Rigidbody>();
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
            if (distanceToObject < 1f)
            {
                TWO_t_sec_dist.text = "Dist Extreme Close";
                TWO_i_sec_dist.color = Color.red;
                ONE_t_sec_dist.text = "Dist Extreme Close";
                ONE_i_sec_dist.color =  Color.red;
            }
            else if (distanceToObject < 3f)
            {
                TWO_t_sec_dist.text = "Dist Close";
                TWO_i_sec_dist.color = Color.yellow;
                ONE_t_sec_dist.text = "Dist Close";
                ONE_i_sec_dist.color =  Color.yellow;
            }
            else
            {
                TWO_t_sec_dist.text = "Dist safe";
                TWO_i_sec_dist.color = Color.green;
                ONE_t_sec_dist.text = "Dist safe";
                ONE_i_sec_dist.color =  Color.green;
            }

          
        }
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
        //Quaternion currentOrientation = transform.localRotation;
        //Quaternion desiredOrientation = Quaternion.Euler(desiredEulerAngles);
        
        // Calc orientation delta
        Quaternion orientationDelta = Quaternion.Inverse(currentOrientation) * desiredOrientation;


        // Convert orientation delta to angular velocity vector (for linearity)
        Vector3 angularVelocityError = OrientationDeltaToAngularVelocity(orientationDelta);
       

        // get current angularVelocity 
        Vector3 currentAngularVelocity = rb.angularVelocity;


        if (toggleDebug)
        {
            //Debug.Log("UpdatePID called");
            Debug.Log("desiredOrientation: " + desiredOrientation);
            Debug.Log("currentOrientation: " + currentOrientation);
            Debug.Log("orientationDelta: " + orientationDelta);
            Debug.Log("angularVelocityError: " + angularVelocityError);
            //Debug.DrawRay(transform.position, angularVelocityError * 200, Color.yellow);
            Debug.Log("currentAngularVelocity: " + currentAngularVelocity);
            //Debug.DrawRay(transform.position, rb.angularVelocity * 200, Color.black);
        }
        
        // Object avoidance 
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
        Vector3 velocity = vectorToTarget / deltaTime;

        // get current Velocity 
        Vector3 currentVelocity = rb.velocity;

        // PID control on the angular velocity error (closed feedback loop)

        float rollControlInput = rollPIDQuaternion.UpdateAA(angularVelocityError.z, currentAngularVelocity.z, deltaTime);
        float pitchControlInput = pitchPIDQuaternion.UpdateAA(angularVelocityError.x, currentAngularVelocity.x, deltaTime);
        float yawControlInput = yawPIDQuaternion.UpdateAA(angularVelocityError.y, currentAngularVelocity.y, deltaTime);
    
       
        float altitudeError = AltitudePID.UpdateAA(velocity.y, currentVelocity.y, deltaTime);
        float xControlInput = xPID.UpdateAA(velocity.x, currentVelocity.x, deltaTime);
        float zControlInput = zPID.UpdateAA(velocity.z, currentVelocity.z, deltaTime);
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

    void ControlMotors(float roll, float pitch, float yaw, float lift2, float x, float z)
    {

        float xforce = Mathf.Clamp(x, -maxVelocity, maxVelocity);
        rb.AddForce(transform.right * xforce, ForceMode.Force);

        float zforce = Mathf.Clamp(z, -maxVelocity, maxVelocity);
        rb.AddForce(transform.forward * zforce, ForceMode.Force);

        // Apply Clamp to simulate actuators that limits the control signal. 
        float throttle2 = Mathf.Clamp(lift2, -maxVelocity, maxVelocity);
        // Throttle, the upward force 
        rb.AddForce(transform.up * throttle2, ForceMode.Force);

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

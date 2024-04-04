using UnityEngine;
public class PIDController
{
    private float proportionalGain;
    private float integralGain;
    private float derivativeGain;

    private float integral = 0f; 
    private float minIntegral = -5f;
    private float maxIntegral = 5f;
    private float previousError = 0f;
    private float previousActualValue = 0f;

    private bool debug = false;

    public PIDController(float kp, float ki, float kd)
    {
        proportionalGain = kp;
        integralGain = ki;
        derivativeGain = kd;
    }

    public float UpdateAA(float setPoint, float actualValue, float timeFrame)
    {
        if (timeFrame <= 0f) timeFrame = 0.01f;  // Prevent division by zero or negative time

        float error = setPoint - actualValue;
        
        // Proportional term
        float proportional = proportionalGain * error;
        
        // Integral term
        //integral += error * Time.fixedDeltaTime;
        //integral += error * timeFrame;
        integral = Mathf.Clamp(integral + error * timeFrame, minIntegral, maxIntegral);
        float integralTerm = integralGain * integral;
        
        // Derivative term
        //float derivative = derivativeGain * ((error - previousError) / Time.fixedDeltaTime);
        //float derivative = derivativeGain * ((error - previousError) / timeFrame);
        float derivative = derivativeGain * ((error - previousError) / timeFrame);
        // should help against derivative kick, spike caused by the response to the sudden setpoint change
        // float derivative = derivativeGain * ((actualValue - previousActualValue) / timeFrame);
        // previousActualValue = actualValue;


        // Update previous error
        previousError = error;

        // Calculate and return the control input
        float output = proportional + integralTerm + derivative;
        
        if (debug)
        {
            Debug.Log("UpdateAA called:");
            Debug.Log("error: " + error);
            Debug.Log("proportional: " + proportional  + " /gain: " + proportionalGain);
            Debug.Log("integralTerm: " + integralTerm + " /gain: " + integralGain);
            Debug.Log("derivative: " + derivative + " /gain: " + derivativeGain);
            Debug.Log("output: " + output);
        }


        return output;
    }
}

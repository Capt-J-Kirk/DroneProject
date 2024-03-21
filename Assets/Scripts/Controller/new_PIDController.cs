using UnityEngine;
public class PIDController
{
    private float proportionalGain;
    private float integralGain;
    private float derivativeGain;

    private float integral = 0f; 
    private float previousError = 0f;

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
        integral += error * timeFrame;
        float integralTerm = integralGain * integral;

        // Derivative term
        //float derivative = derivativeGain * ((error - previousError) / Time.fixedDeltaTime);
        float derivative = derivativeGain * ((error - previousError) / timeFrame);

        // Update previous error
        previousError = error;

        // Calculate and return the control input
        float output = proportional + integralTerm + derivative;
        return output;
    }
}

using UnityEngine;
public class PIDController
{
    private float proportionalGain;
    private float integralGain;
    private float derivativeGain;

    private float integral = 0f; 
    private float previousError = 0f;

    public PIDController(float Kp, float ki, float kd)
    {
        proportionalGain = kp;
        integralGain = ki;
        derivativeGain = kd;
    }

    public float Update(float setPoint, float actualValue, float timeFrame)
    {
        float error = setPoint - actualValue;

        // Proportional term
        float proportional = proportionalGain * error;

        // Integral term
        integral += error * Time.fixedDeltaTime;
        float integralTerm = integralGain * integral;

        // Derivative term
        float derivative = derivativeGain * ((error - previousError) / Time.fixedDeltaTime);

        // Update previous error
        previousError = error;

        // Calculate and return the control input
        float output = proportional + integralTerm + derivative;
        return output;
    }
}

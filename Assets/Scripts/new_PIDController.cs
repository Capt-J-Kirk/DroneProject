using UnityEngine;
public class PIDController
{
    private float proportionalGain;
    private float integralGain;
    private float derivativeGain;

    private float integral;
    private float previousError;

    public PIDController(float pGain, float iGain, float dGain)
    {
        proportionalGain = pGain;
        integralGain = iGain;
        derivativeGain = dGain;
    }

    public float Update(float current, float currentVelocity, float target)
    {
        float error = target - current;

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
        return proportional + integralTerm + derivative;
    }
}

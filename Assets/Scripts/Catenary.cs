using UnityEngine;

public static class Catenary
{

    public static Vector3[] CatenaryPoint(Vector3 lowerEndPoint, Vector3 upperEndPoint, int resolution)
    {
        Vector3[] catenaryPoints = new Vector3[resolution + 1];

        // Length of the rope
        float ropeLength = Vector3.Distance(lowerEndPoint, upperEndPoint);

        // Parameters 'a' for catenary equation (concerns rope slack).
        float a = 0.75f * (ropeLength / Mathf.PI);

        // The increment length
        float incrementLength = ropeLength / resolution;

        // Direction vector of the rope
        Vector3 ropeDirection = (upperEndPoint - lowerEndPoint).normalized;

        // Difference in height between start and end points
        float heightDifference = upperEndPoint.y - lowerEndPoint.y;

        // Variables for iterating along the curve
        float currentLength = 0f;
        float yCorrection = 0;

        // Iterate through the resolution
        for (int i = 0; i <= resolution; i++)
        {
            // Calculate point along the rope - horizontal position. 
            Vector3 currentPoint = lowerEndPoint + ropeDirection * currentLength;

            // Calculate point along the rope - height.
            float t = (float)i / resolution;
            float x = 2f * a * (t - 0.5f); // x-coordinate for the catenary equation
            float c = lowerEndPoint.y + heightDifference * t;
            float currentY = a * (Cosh(x / a)) + c;

            // Correct for start point.
            if (i == 0) yCorrection = currentY - lowerEndPoint.y;
            currentY -= yCorrection;

            // Current point in the array
            catenaryPoints[i] = new Vector3(currentPoint.x, currentY, currentPoint.z);

            // Increment length
            currentLength += incrementLength;
        }

        return catenaryPoints;
    }


    private static float Cosh(float x)
    {
        float e = 2.718281828459045f;
        float result = (Mathf.Pow(e, x) + Mathf.Pow(e, -x)) / 2;
        return result;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectTransform: MonoBehaviour
{
    // Reference to the first GameObject
    public GameObject Quadcopter_main;

    // Reference to the second GameObject
    public GameObject Quadcopter_secondary;

    // Transformation matrix for rotation and translation
    private Matrix4x4 transformationMatrix;

    // Set up transformation parameters
    private Vector3 translationVector =  new Vector3(0.0f, 0.0f, 0.0f); 
    private Vector3 rotationVector =  new Vector3(0.0f, 0.0f, 0.0f); 

    void Start()
    {
        // assigning drone objects
        Quadcopter_main = GameObject.Find("Washing Drone");
        Quadcopter_secondary = GameObject.Find("Sec Drone");

        // Ensure that a GameObject is assigned
        if (Quadcopter_main == null || Quadcopter_secondary == null)
        {
            Debug.LogError("Please assign the drones in the inspector!");
            return;
        }

        // Get the pose of the Quadcopter_main
        Vector3 position1 = Quadcopter_main.transform.position;
        Quaternion rotation1 = Quadcopter_main.transform.rotation;

        // Create a transformation matrix for the Quadcopter_main
        Matrix4x4 poseMatrix1 = Matrix4x4.TRS(position1, rotation1, Vector3.one);

        // Create the transformation matrix from the desired offset
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotationVector));
        Matrix4x4 translationMatrix = Matrix4x4.Translate(translationVector);

        // Combine rotation and translation matrices
        transformationMatrix = translationMatrix * rotationMatrix;

        // Apply the transformation to get the pose of the second object
        Matrix4x4 poseMatrix2 = transformationMatrix * poseMatrix1;

        // Extract position and rotation from the resulting matrix
        Vector3 position2 = poseMatrix2.GetColumn(3);
        Quaternion rotation2 = Quaternion.LookRotation(poseMatrix2.GetColumn(2), poseMatrix2.GetColumn(1));

        // Find the QuadcopterController for the second drone
        //QuadcopterController_sec quadcopterController_2 = Quadcopter_secondary.GetComponent<QuadcopterController_sec>();
        QuadcopterController_sec quadcopterController_2 = Quadcopter_secondary.GetComponent<QuadcopterController_sec>();

        // Check if the script is attached
        if (quadcopterController_2 != null)
        {
            // Call the method to set desired position and rotation
            quadcopterController_2.SetQuadcopterPose(position2, rotation2);
        }
        else
        {
            Debug.LogError("QuadcopterController_sec script not found on the Quadcopter_secondary!");
        }
    }
    public Vector3 GetRotationVector()
    {
        return rotationVector;
    }

    public Vector3 GetTranslationVector()
    {
        return translationVector;
    }

    public void SetTransformationParameters(Vector3 rotation, Vector3 translation)
    {
        translationVector = translation;
        rotationVector = rotation;
    }
}

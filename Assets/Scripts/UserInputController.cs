using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// this script sends the user input to the main drone 
public class UserInputController: MonoBehaviour
{
    private QuadcopterController quadcopterController;
    private QuadcopterController_sec quadcopterController_2;

    private ObjectTransform objectTransform;

    // Parameters increment for rotation and translation
    private float rotationIncrement = 5.0f; 
    private float translationIncrement = 5.0f;


    void Start()
    {
        quadcopterController = GetComponent<QuadcopterController>();
        objectTransform = GetComponent<ObjectTransform>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        quadcopterController.ApplyUserInput(horizontalInput, verticalInput);
        AdjustParameters();
    }

     void AdjustParameters()
    {
        // Holding leftshift = decreases value

        // Rotation adjustments
        if (Input.GetKey(KeyCode.U))
        {
            // rotation around x-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector() + new Vector3((Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement), 0f, 0f),
                objectTransform.GetTranslationVector()
            );
        }

        if (Input.GetKey(KeyCode.I))
        {
            // rotation around y-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector(),
                objectTransform.GetTranslationVector() + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement), 0f)
            );
        }

        if (Input.GetKey(KeyCode.O))
        {
            // rotation around z-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector() + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement)),
                objectTransform.GetTranslationVector()
            );
        }

        // Translation adjustments
        
        if (Input.GetKey(KeyCode.J))
        {
            // translation along x-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector() + new Vector3((Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement), 0f, 0f),
                objectTransform.GetTranslationVector()
            );
        }

        if (Input.GetKey(KeyCode.K))
        {
            // translation along y-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector(),
                objectTransform.GetTranslationVector() + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement), 0f)
            );
        }

        if (Input.GetKey(KeyCode.L))
        {
            // translation along z-axis
            objectTransform.SetTransformationParameters(
                objectTransform.GetRotationVector(),
                objectTransform.GetTranslationVector() + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement))
            );
        }
    }
}

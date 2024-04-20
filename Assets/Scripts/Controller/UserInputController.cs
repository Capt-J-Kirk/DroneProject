// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.PlayerLoop;

// // This script sends the user input to the main drone 

// // NOTE // 
// // may remove QuadcopterController_sec, if not needed in the end?
// //
// //


// public class UserInputController: MonoBehaviour
// {
//     // Ref to class objects
//     private QuadcopterController quadcopterController;
//     private QuadcopterController_sec quadcopterController_2;

//     private ObjectTransform objectTransform;

//     // Parameters increment for rotation and translation
//     private float rotationIncrement = 5.0f; 
//     private float translationIncrement = 5.0f;


//     void Start()
//     {
//         // Initialization of game components (objects)
//         quadcopterController = GetComponent<QuadcopterController>();
//         quadcopterController_2 = GetComponent<QuadcopterController_sec>();
//         objectTransform = GetComponent<ObjectTransform>();

//         // Check if GameObjects are attached
//         if (quadcopterController == null)
//         {
//             Debug.LogError("UserInputController: QuadcopterController component not found.");
//         }
//          if (quadcopterController_2 == null)
//         {
//             Debug.LogError("UserInputController: QuadcopterController_2 component not found.");
//         }
//         if (objectTransform == null)
//         {
//             Debug.LogError("UserInputController: ObjectTransform component not found.");
    
//         }
//     }

//     void Update()
//     {
//         HandleInput();
//     }

//     void HandleInput()
//     {
//         // user input from UnityEngine.InputSystem
//         float horizontalInput = Input.GetAxis("Horizontal");
//         float verticalInput = Input.GetAxis("Vertical");
//         // apply to main drone
//         if (quadcopterController != null)
//         {
//             //quadcopterController.ApplyUserInput(horizontalInput, verticalInput);
//         }
//         else
//         {
//              Debug.LogError("UserInputController: QuadcopterController reference is null.");
//         // set custom parameter offsets for secondary drone
//         AdjustParameters();
//     }

//      void AdjustParameters()
//     {
//         // Holding leftshift = decreases value
//         // added Time.deltaTime to be frame rate independent 
//         if (objectTransform == null)
//         {
//             Debug.LogWarning("UserInputController: ObjectTransform reference is null.");
//             return; // Exit the function if objectTransform is null
//         }
//         // Rotation adjustments
//         if (Input.GetKey(KeyCode.U))
//         {
//             // rotation around x-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector() + new Vector3((Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement) * Time.deltaTime, 0f, 0f),
//                 objectTransform.GetTranslationVector()
//             );
//         }

//         if (Input.GetKey(KeyCode.I))
//         {
//             // rotation around y-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector() + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement) * Time.deltaTime, 0f),
//                 objectTransform.GetTranslationVector() 
//             );
//         }

//         if (Input.GetKey(KeyCode.O))
//         {
//             // rotation around z-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector() + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? -rotationIncrement : rotationIncrement) * Time.deltaTime),
//                 objectTransform.GetTranslationVector()
//             );
//         }

//         // Translation adjustments
        
//         if (Input.GetKey(KeyCode.J))
//         {
//             // translation along x-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector() ,
//                 objectTransform.GetTranslationVector() + new Vector3((Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement) * Time.deltaTime, 0f, 0f)
//             );
//         }

//         if (Input.GetKey(KeyCode.K))
//         {
//             // translation along y-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector(),
//                 objectTransform.GetTranslationVector() + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement) * Time.deltaTime, 0f)
//             );
//         }

//         if (Input.GetKey(KeyCode.L))
//         {
//             // translation along z-axis
//             objectTransform.SetTransformationParameters(
//                 objectTransform.GetRotationVector(),
//                 objectTransform.GetTranslationVector() + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? -translationIncrement : translationIncrement) * Time.deltaTime)
//             );
//         }
//     }
// }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class ToggleEffects : MonoBehaviour
{
    public PostProcessVolume volume; // Assign this in the inspector
    private Grain grain;
    private bool grainEnabled;
    private MotionBlur motionBlur;
    private bool motionBlurEnabled;

    void Start()
    {
        // Get the post-processing effects from the volume
        volume.profile.TryGetSettings(out grain);
        volume.profile.TryGetSettings(out motionBlur);
    }

    void Update()
    {
        // Toggle the effects when the user presses the 'G' and 'B' keys
        if (Input.GetKeyDown(KeyCode.G))
        {
            grainEnabled = !grainEnabled;
            grain.active = grainEnabled;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            motionBlurEnabled = !motionBlurEnabled;
            motionBlur.active = motionBlurEnabled;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


[System.Serializable]
public class DPadMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public void Map(bool mapPos, bool mapRot)
    {
        if (mapPos) ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        if (mapRot) ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class DPadPlayer_Animator : MonoBehaviour
{
    [Range(0, 1)]
    public float turnSmoothness = 0.1f;
    public DPadMap head;
    public DPadMap leftHand;
    public DPadMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;


    private void Start()
    {
        leftHand.Map(true, true);
        rightHand.Map(true, true);
    }


    void LateUpdate()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        head.Map(true, true);
        leftHand.Map(true, false);
        rightHand.Map(true, false);
    }


}

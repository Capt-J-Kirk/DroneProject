using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Pivot:" + transform.InverseTransformPoint(transform.position));
        Debug.Log("Position:" + transform.position);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckForGround : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private GameObject[] hoseSegmentPrefabs;
    private GameObject chosenPrefab;

    Vector3 vanHosePoint = Vector3.zero;
    Vector3 droneHosePoint = Vector3.zero;
    private GameObject droneHoseGO;

    List<GameObject> hoseSegments = default;
    Vector3[] catenaryHosePoints = default;

    private List<Vector3> newPosList = new();
    private Vector3 newPos;

    // Debugging
    bool debug = false;


    private void Awake()
    {
        chosenPrefab = hoseSegmentPrefabs[1];
        vanHosePoint = transform.position;
        droneHoseGO = GameObject.FindGameObjectWithTag("DroneHosePoint");
        droneHosePoint = droneHoseGO.transform.position;
        hoseSegments = new();
        Physics.queriesHitBackfaces = true;
    }

    private void Start()
    {
        catenaryHosePoints = Catenary.CatenaryPoint(vanHosePoint, droneHosePoint, 100);

        for (int i = 1; i < catenaryHosePoints.Length; i++)
        {
            Vector3 segmentVector = (catenaryHosePoints[i] - catenaryHosePoints[i - 1]);
            Vector3 positionVector = catenaryHosePoints[i - 1] + 0.5f * segmentVector;
            GameObject segment = Instantiate(chosenPrefab, positionVector, Quaternion.identity);
            segment.transform.up = new Vector3(segmentVector.x, segmentVector.y, segmentVector.z);
            segment.transform.localScale = new Vector3(segment.transform.localScale.x, segmentVector.magnitude * 0.5f, segment.transform.localScale.z);
            segment.transform.parent = transform;
            hoseSegments.Add(segment);

        
        }

        /**
        foreach (Vector3 itr in catenaryHosePoints)
        {
            GameObject segment = Instantiate(chosenPrefab, itr, Quaternion.identity);
            segment.transform.parent = transform;
            hoseSegments.Add(segment);
        }
        **/
    }


    private void FixedUpdate()
    {
        MoveHose();
    }


    private void MoveHose()
    {
        droneHosePoint = droneHoseGO.transform.position;
        catenaryHosePoints = Catenary.CatenaryPoint(vanHosePoint, droneHosePoint, 100);

        for (int i = 1; i < catenaryHosePoints.Length; i++)
        {
            Vector3 segmentVector = (catenaryHosePoints[i] - catenaryHosePoints[i - 1]);
            Vector3 positionVector = catenaryHosePoints[i - 1] + 0.5f * segmentVector;
            GameObject segment = hoseSegments[i - 1];
            segment.transform.position = positionVector;
            segment.transform.up = new Vector3(segmentVector.x, segmentVector.y, segmentVector.z);
            segment.transform.localScale = new Vector3(segment.transform.localScale.x, segmentVector.magnitude * 0.5f, segment.transform.localScale.z);

            segment.transform.position = Check(segment);
        }
    }


    Vector3 Check(GameObject obj)
    {
        newPosList.Clear();
        newPosList.Add(obj.transform.position);
        newPos = newPosList[0];

        // Raycast upwards
        RaycastHit[] hitUp = Physics.RaycastAll(obj.transform.position, Vector3.up, Mathf.Infinity, layerMask);

        if (hitUp.Length > 0)
        {
            Array.Sort(hitUp, (a, b) => b.point.y.CompareTo(a.point.y));
            RaycastHit hit = hitUp[0];

            if (debug) Debug.DrawRay(obj.transform.position, Vector3.up * hit.distance, Color.green);
            newPosList.Add(hit.point);

            //Debug.Log("I hit up: " + hit.transform.name);
        }
        else
        {
            if (debug) Debug.DrawRay(obj.transform.position, Vector3.up * 100, Color.red); // Draw a debug line indicating no hit
        }

        // Raycast downwards
        RaycastHit[] hitDown = Physics.RaycastAll(obj.transform.position, Vector3.down, Mathf.Infinity, layerMask);

        if (hitDown.Length > 0)
        {
            Array.Sort(hitDown, (a, b) => b.point.y.CompareTo(a.point.y));
            RaycastHit hit = hitDown[0];

            if (debug) Debug.DrawRay(obj.transform.position, Vector3.down * hit.distance, Color.blue);
            newPosList.Add(hit.point);

            //Debug.Log("I hit up: " + hit.transform.name);
        }
        else
        {
            if (debug) Debug.DrawRay(obj.transform.position, Vector3.down * 100, Color.yellow); // Draw a debug line indicating no hit
        }

        float yPos = Mathf.NegativeInfinity;
        foreach (Vector3 itr in newPosList)
        {
            if (itr.y > yPos)
            {
                newPos = itr;
                yPos = itr.y;
            }
        }

        return newPos;
    }
}

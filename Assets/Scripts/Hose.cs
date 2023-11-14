using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : MonoBehaviour
{
    [SerializeField]
    GameObject hoseParent;

    [SerializeField]
    GameObject hoseSegment;

    Vector3 startPoint;
    public float hoseLength = 20;
    float segmentLength = 0.2f;


    private void Awake()
    {
        startPoint = new Vector3(100, 0.01f, 100);
        SetupHose();
    }




    void SetupHose()
    {
        Vector3 spawnPoint = startPoint;
        GameObject parent = Instantiate(hoseSegment, spawnPoint, Quaternion.Euler(0,0,-90));
        parent.name = "0";

        parent.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Destroy(parent.GetComponent<HingeJoint>());

        GameObject child;
        int segmentCount = (int)(hoseLength/segmentLength);
        

        for (int i=1; i<(segmentCount ); i++)
        {
            spawnPoint = new Vector3(startPoint.x + (segmentLength*i), startPoint.y, startPoint.z);
            child = Instantiate(hoseSegment, spawnPoint, Quaternion.Euler(0, 0, -90));
            child.GetComponent<HingeJoint>().connectedBody = parent.GetComponent<Rigidbody>();
            child.name = i.ToString();
            parent = child;
        }
    }


}

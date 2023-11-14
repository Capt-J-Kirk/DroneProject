using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHose : MonoBehaviour
{
    [SerializeField]
    GameObject parentObject;
    [SerializeField]
    GameObject segmentPrefab;

    [SerializeField]
    [Range(1, 1000)]
    int hoseLength = 1;

    [SerializeField]
    bool reset, spawn, snapFirst, snapLast;
    private readonly float segmentDist = 0.21f;

    // Hose snap points.
    string drone_HosePointTag = "DroneHosePoint";
    string van_HosePointTag = "VanHosePoint";
    GameObject drone_HosePointGO;
    GameObject van_HosePointGO;


    private void Awake()
    {
        drone_HosePointGO = GameObject.FindWithTag(drone_HosePointTag);
        van_HosePointGO = GameObject.FindWithTag(van_HosePointTag);
    }

    private void Start()
    {
        Spawn();
    }


    void Update()
    {
        if (reset)
        {
            foreach (GameObject itr in GameObject.FindGameObjectsWithTag("Player"))
            {
                Destroy(itr);
            }
        }

        if (spawn)
        {
            Spawn();
            spawn = false;
        }

             
       
        
    }

    void Spawn()
    {
        int segments = (int)(hoseLength / segmentDist);

        for (int i = 0; i < segments; i++)
        {
            GameObject newSegment;
            newSegment = Instantiate(
                segmentPrefab,
                new Vector3(
                        transform.position.x,
                        transform.position.y + segmentDist * (i + 1),
                        transform.position.z
                ),
                Quaternion.identity,
                parentObject.transform
            );

            //newSegment.transform.eulerAngles = new Vector3(180f, 0f, 0f);
            newSegment.name = parentObject.transform.childCount.ToString();

            if (i == 0)
            {
                Destroy(newSegment.GetComponent<CharacterJoint>());
                if (snapFirst)
                {
                    newSegment.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }


            }
            else {
                newSegment.GetComponent<CharacterJoint>().connectedBody =
                    parentObject.transform.Find((parentObject.transform.childCount - 1).ToString()).GetComponent<Rigidbody>();

            }
        }

        if (snapLast)
            parentObject.transform.Find(parentObject.transform.childCount.ToString()).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //newSegment.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

    }

    void SnapHose()
    { 
        

    }

}

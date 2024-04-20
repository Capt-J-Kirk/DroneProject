using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public bool spawnBoxGrid = true;
    public GameObject boxPrefab;
    private Vector3 boxScale;
    private Vector3 bounds;


    private void Awake()
    {
        if (spawnBoxGrid) InitAndSpawn();
    }

    void InitAndSpawn()
    {
        // Init
        bounds = GetComponent<Renderer>().localBounds.extents * 2;
        // Debug.Log("Bounds: " + bounds);
        boxScale.x = bounds.x / 2f;
        float yRes = Mathf.Round(bounds.y / boxScale.x);
        boxScale.y = bounds.y / yRes;
        float zRes = Mathf.Round(bounds.z / boxScale.x);
        boxScale.z = bounds.z / zRes;
        //
        SpawnBoxes(boxScale);
    }


    void SpawnBoxes(Vector3 boxScale)
    {
        float x = - boxScale.x / 2f;

        for (float z = boxScale.z/2f; z <= bounds.z; z += boxScale.z)
        {
            for (float y = boxScale.y / 2f; y <= bounds.y; y += boxScale.y)
            {
                GameObject box = Instantiate(boxPrefab);
                box.transform.parent = transform;
                box.transform.localPosition = new(x, y, z);
                box.transform.localRotation = Quaternion.identity;
                box.transform.localScale = boxScale;
            }
        }

        x = boxScale.x / 2f;

        for (float z = boxScale.z / 2f; z <= bounds.z; z += boxScale.z)
        {
            for (float y = boxScale.y / 2f; y <= bounds.y; y += boxScale.y)
            {
                GameObject box = Instantiate(boxPrefab);
                box.transform.parent = transform;
                box.transform.localPosition = new(x, y, z);
                box.transform.localRotation = Quaternion.identity;
                box.transform.localScale = boxScale;
            }
        }
    }


}

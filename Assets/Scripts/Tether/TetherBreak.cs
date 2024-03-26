using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherBreak : MonoBehaviour
{
    public GameObject char1;

    public GameObject char2;

    public HingeJoint joint1;

    public HingeJoint joint2;

    public Rigidbody b1;
    public Rigidbody b2;
    public Rigidbody b3;
    public Rigidbody b4;
    public Rigidbody b5;
    public Rigidbody b6;
    public Rigidbody b7;
    public Rigidbody b8;
    public Rigidbody b9;
    public Rigidbody b10;
    public Rigidbody bend;
    
    // Start is called before the first frame update
    void Start()
    {
        float dist = Vector3.Distance(char1.transform.position, char2.transform.position);
        Debug.Log("Distance is: " + dist);
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(char1.transform.position, char2.transform.position);
        if (dist >= 4)
        {
            Debug.Log("Distance is: " + dist);
        }

        if (dist >= 5.5)
        {
            joint1.connectedBody = null;
            joint2.connectedBody = null;
            Destroy(joint1);
            Destroy(joint2);
            addMass();
        }
    }
    
    private void addMass()
    {
        b1.mass = 1;
        b1.useGravity = true;
        b2.mass = 1;
        b2.useGravity = true;
        b3.mass = 1;
        b3.useGravity = true;
        b4.mass = 1;
        b4.useGravity = true;
        b6.mass = 1;
        b6.useGravity = true;
        b5.mass = 1;
        b5.useGravity = true;
        b7.mass = 1;
        b7.useGravity = true;
        b8.mass = 1;
        b8.useGravity = true;
        b9.mass = 1;
        b9.useGravity = true;
        b10.mass = 1;
        b10.useGravity = true;
        bend.mass = 1;
        bend.useGravity = true;
        
    }
}

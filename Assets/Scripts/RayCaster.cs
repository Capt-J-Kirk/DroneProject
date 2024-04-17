using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Es.InkPainter;
//using UnityEditor.PackageManager;

public class RayCaster : MonoBehaviour
{
    public GameObject[] turbineObjects;
    public GameObject testObject;

    public GameObject spawn;
    public GameObject spawner;

    // Drone data
    private DroneControl myDrone;
    public GameObject raySenderObject;

    // Ray variables
    public float rayFrequency = 0.1f;
    public int rayCount = 18;
    public float beamRadius = 0.25f;
    private List<GameObject> rayCasters;

    // Target object data
    private string targetTag = "WindTurbine";


    //private Mesh mesh;
    //private Color[] originalColors;
    //private Material material;

    // Inkpainter
    [SerializeField]
    Brush brush;

    private void Awake()
    {
        myDrone = GetComponent<DroneControl>();
        rayCasters = new();
    }

    void Start()
    {
        // Assuming your object has a MeshFilter and MeshRenderer
        //mesh = GetComponent<MeshFilter>().mesh;
        //originalColors = mesh.colors; // Store original colors
        //material = GetComponent<Renderer>().material;

        SpawnRaycasters();
        //foreach (GameObject itr in turbineObjects) InitMesh(itr);
        //InitMesh(testObject);
    }

    private void Update()
    {
        //if (myDrone.isSpraying && !IsInvoking(nameof(SendRays))) InvokeRepeating(nameof(SendRays), 0, rayFrequency);
        //if (!myDrone.isSpraying) CancelInvoke(nameof(SendRays));

        if (myDrone.isSpraying) SendRays();
    }

    void SpawnRaycasters()
    {
        // Spawn centerbeam
        GameObject rayCaster = Instantiate(spawn, spawner.transform.position, spawner.transform.rotation, spawner.transform);
        rayCasters.Add(rayCaster);
        rayCaster.transform.rotation = Quaternion.Euler(3,0,0);

        return;

        int casterCircleCount = 4;
        float angleIncrement = 360f / rayCount;

        for (int i = 0; i < casterCircleCount; i++)
        {
            float radius = (beamRadius / casterCircleCount) * (i + 1);

            for (int j = 0; j < rayCount; j++)
            {
                float angle = j * angleIncrement;

                // Calculate positions
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                Vector3 circlePoint = spawner.transform.position + new Vector3(x, y, 0f);

                rayCaster = Instantiate(spawn, circlePoint, spawner.transform.rotation, spawner.transform);
                rayCasters.Add(rayCaster);
            }
        }
    }



    void SendRays()
    {
        // Cast rays from raycasters
        for (int i = 0; i < rayCasters.Count; i++)
        {
            Vector3 position = rayCasters[i].transform.position;
            Vector3 forwardDirection = rayCasters[i].transform.forward;

            // Forward raycast
            Ray ray = new Ray(position, forwardDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.CompareTag(targetTag))
            {
                Debug.Log("I hit: " + hit.collider.name);
                var paintObject = hit.transform.GetComponent<InkCanvas>();
                if (paintObject != null)
                    paintObject.Paint(brush, hit);
                /**
                // Get object data
                GameObject hitObject = hit.collider.gameObject;
                Mesh mesh = hitObject.GetComponent<MeshFilter>().mesh;
                Color[] colors = mesh.colors;

                // Get the triangle index from RaycastHit
                int triangleIndex = hit.triangleIndex;
                Debug.Log("I hit tri: " + triangleIndex + ", on: " + hitObject.name);

                // Modify color
                colors[triangleIndex*3] = Color.red;
                colors[triangleIndex*3 + 1] = Color.red;
                colors[triangleIndex*3 + 2] = Color.red;

                // Apply modified colours
                mesh.colors = colors;
                **/
            }

            // Visualize ray
            Debug.DrawRay(position, forwardDirection * 10, Color.yellow);
        }
    }


    void InitMesh(GameObject meshObject)
    {
        Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
        mesh = GenerateUVCoordinates(mesh);

        Color[] colors = new Color[mesh.vertexCount];


        for (int k = 0; k < colors.Length; k++) colors[k] = Color.cyan;
        mesh.colors = colors;


        Debug.Log("Name: " + meshObject.name + ", Triangles:" + mesh.triangles.Length + ", Vertices: " + mesh.vertexCount);
    }

    Mesh GenerateUVCoordinates(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = new Vector2[vertices.Length];

        // Simple UV mapping: assign UV coordinates based on X and Z positions
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        // Assign the UV coordinates to the mesh
        mesh.uv = uv;
        return mesh;
    }


}



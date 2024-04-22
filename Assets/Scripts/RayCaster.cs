using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Es.InkPainter;


public class RayCaster : MonoBehaviour
{
    // Ref. to performance.
    PerformanceCalc performanceCalc;

    public GameObject spawn;
    public GameObject spawner;

    // Drone data
    private DroneControl thisDrone;

    // Ray variables
    float rayFrequency = 0.1f;
    int rayCount = 18;
    float beamRadius = 0.25f;
    List<GameObject> rayCasters;
    float maxCleaningDistance = 9f;

    // Target object data
    private string tagTurbine = "WindTurbine";
    private string tagBoundingBox = "BoundingBox";
    private string tagTarget = "";

    // Inkpainter
    [SerializeField]
    Brush brush;


    private void Awake()
    {
        performanceCalc = FindObjectOfType<PerformanceCalc>();
        if (!performanceCalc) Debug.Log("PerformanceCalc was not found!");
        tagTarget = tagTurbine;
        thisDrone = GetComponent<DroneControl>();
        rayCasters = new();

        // Testing
        // thisDrone.isSpraying = true;
        // thisDrone.droneSpray.Play();
    }


    void Start()
    {
        SpawnRaycasters();
    }


    private void FixedUpdate()
    {
        if (thisDrone.isSpraying) SendRays();
    }


    void SpawnRaycasters()
    {
        // Spawn centerbeam
        GameObject rayCaster = Instantiate(spawn, spawner.transform.position, spawner.transform.rotation, spawner.transform);
        rayCasters.Add(rayCaster);
        rayCaster.transform.rotation = Quaternion.Euler(3,0,0);

        return; // Using just 1, for now.

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

            if (Physics.Raycast(ray, out hit) &&
                hit.collider.gameObject.CompareTag(tagTarget) &&
                hit.distance < maxCleaningDistance
                )
            {
                foreach (PerformanceCalc.TurbinePart itr in performanceCalc.parts)
                {
                    if (hit.collider.gameObject.name == itr.paint_GO.name) itr.wasHit = true;
                }


                //Debug.Log("I hit: " + hit.collider.name);
                var paintObject = hit.transform.GetComponent<InkCanvas>();
                if (paintObject != null)
                    paintObject.Paint(brush, hit);

                // #################################
                // GameObject hitGO = hit.collider.gameObject;
                // Debug.Log("Extends: " + hitGO.GetComponent<Renderer>().bounds.extents);
                // Vector3 localHitPoint = hitGO.transform.InverseTransformPoint(hit.point) * hitGO.transform.parent.localScale.x;
                // Vector3 boundingBox_BlenderAxes_Hit = new Vector3(localHitPoint.y, localHitPoint.z, localHitPoint.x);
                // Debug.Log("Hit point. Object coordinate system: " + boundingBox_BlenderAxes_Hit);
                // #################################
            }

            // Visualize ray
            Debug.DrawRay(position, forwardDirection * maxCleaningDistance, Color.yellow);
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



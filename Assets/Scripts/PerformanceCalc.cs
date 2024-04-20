using Es.InkPainter;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TurbinePart
{
    public GameObject paint_GO;
    public InkCanvas inkCanvas;
    public Material material;

    public bool assigned = false;
    public bool updatePerformance = false;
    public float timer = 0;
    public MeshFilter meshFilter;
    public Texture2D startTexture;
    public Texture2D currentTexture;

    // Dirt info containers
    public Dictionary<Vector3, float> coordDirt_Dict = new();
    public List<Vector3> coordDirt_List = new();
    public List<Vector2Int> textureDirt_List = new();
    public Dictionary<Vector2Int, float> textureDirt_Dict = new();

    // Overall measure for turbine part.
    public float startDirt = 0;
    public float currDirt = 0;

    // Hit by raycast
    public bool wasHit = false;
}

public class PerformanceCalc : MonoBehaviour
{
    public List<TurbinePart> parts;

    private void Awake()
    {
        Vector3 resolution = new Vector3(2f, 3f, 3f);

        foreach (TurbinePart itr in parts)
        {
            Vector3 bounds = itr.paint_GO.GetComponent<Renderer>().bounds.extents * 2f;
            //for(int i=0; )
        }
    }

    private void FixedUpdate()
    {
        foreach (TurbinePart itr in parts)
        {
            itr.timer += Time.deltaTime;

            if (!itr.assigned && itr.timer > 3)
            {
                // Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture BEGINNING assign.");
                Assign(itr);
                // Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture SUCCESSFULLY assigned.");
                itr.timer = 0;
            }

            if (itr.assigned && itr.timer > 1f && itr.wasHit)
            {
                UpdatePerformance(itr);
                Debug.Log("Updating performance");
                itr.timer = 0;
                itr.wasHit = false;
            }
        }
    }


    void UpdatePerformance(TurbinePart itr)
    {
        itr.inkCanvas = itr.paint_GO.GetComponent<InkCanvas>();
        RenderTexture renderTex = itr.inkCanvas.GetPaintMainTexture(itr.material.name + " (Instance)");
        RenderTexture.active = renderTex;

        itr.currentTexture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
        if (itr.currentTexture == null)
        {
            Debug.LogError("Instanced texure not found");
            return;
        }
        itr.currentTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        itr.currentTexture.Apply();
        RenderTexture.active = null;

        // Find current dirt.
        itr.currDirt = 0;
        foreach (Vector2Int texCoord in itr.textureDirt_List)
        {
            Color color = itr.currentTexture.GetPixel(texCoord.x, texCoord.y);
            //float dirt_OLD = (color.r + color.g + color.b) / 3f;
            float dirt = 1 - color.grayscale;
            itr.currDirt += dirt;
        }

    }


    void Assign(TurbinePart itr)
    {
        itr.assigned = true;
        itr.meshFilter = itr.paint_GO.GetComponent<MeshFilter>();
        if (itr.meshFilter == null)
        {
            Debug.LogError("Mesh filter not found");
            return;
        }

        itr.inkCanvas = itr.paint_GO.GetComponent<InkCanvas>();
        RenderTexture renderTex = itr.inkCanvas.GetPaintMainTexture(itr.material.name + " (Instance)");
        RenderTexture.active = renderTex;

        itr.startTexture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
        if (itr.startTexture == null)
        {
            Debug.LogError("Instanced texure not found");
            return;
        }
        itr.startTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        itr.startTexture.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = null;

        //Debug.Log("Start texture w/h: " + itr.startTexture.width + "/" + itr.startTexture.height);

        for (int i = 0; i < itr.startTexture.width; i++)
        {
            for (int j = 0; j < itr.startTexture.height; j++)
            {
                Color color = itr.startTexture.GetPixel(i, j);

                if (color.grayscale != 1)
                {
                    Vector2 uvCoordinate = new Vector2((float)i / itr.startTexture.width, (float)j / itr.startTexture.height);
                    Vector3 coordinate = GetLocalCoordinateFromUV(itr, uvCoordinate);
                    if (coordinate != Vector3.zero)
                    {
                        //float dirt_OLD = (color.r + color.g + color.b) / 3f;
                        float dirt = 1 - color.grayscale;
                        itr.startDirt += dirt;
                        itr.currDirt += dirt;

                        // Dirt to texture
                        itr.textureDirt_List.Add(new Vector2Int(i, j));
                        itr.textureDirt_Dict.Add(new Vector2Int(i, j), dirt);
                        // Dirt to coordinate
                        itr.coordDirt_List.Add(coordinate);
                        itr.coordDirt_Dict.Add(coordinate, dirt);
                    }
                }
            }
        }
        itr.currentTexture = itr.startTexture;
    }


    Vector3 GetLocalCoordinateFromUV(TurbinePart itr, Vector2 uvCoordinate)
    {
        // Mesh data
        Vector3[] vertices = itr.meshFilter.sharedMesh.vertices;
        Vector2[] uv = itr.meshFilter.sharedMesh.uv;
        int[] triangles = itr.meshFilter.sharedMesh.triangles;

        //Debug.Log("Triangle count: " + triangles.Length / 3);

        // Iterate through triangles to find corresponding 3D local coordinate
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv0 = uv[triangles[i]];
            Vector2 uv1 = uv[triangles[i + 1]];
            Vector2 uv2 = uv[triangles[i + 2]];

            // Calculate barycentric coordinates
            Vector3 barycentric = GetBarycentric(uvCoordinate, uv0, uv1, uv2);

            // Check if the UV coordinate lies within the triangle.
            if (barycentric.x >= 0f && barycentric.y >= 0f && barycentric.z >= 0f)
            {
                // Coordinates of triangle vertices.
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                // Local coordinate from barycentric coordinates
                Vector3 localCoordinate = v0 * barycentric.x + v1 * barycentric.y + v2 * barycentric.z;

                //Debug.Log("Local coordinate: " + localCoordinate);

                return localCoordinate;
            }
        }
        // Return zero if UV coordinate not within any triangle.
        return Vector3.zero;
    }


    Vector3 GetBarycentric(Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        Vector3 B = new();
        B.x = ((v2.y - v3.y) * (p.x - v3.x) + (v3.x - v2.x) * (p.y - v3.y)) /
            ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));
        B.y = ((v3.y - v1.y) * (p.x - v3.x) + (v1.x - v3.x) * (p.y - v3.y)) /
            ((v3.y - v1.y) * (v2.x - v3.x) + (v1.x - v3.x) * (v2.y - v3.y));
        B.z = 1 - B.x - B.y;
        return B;
    }

}
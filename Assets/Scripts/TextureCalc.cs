using Es.InkPainter;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureCalc : MonoBehaviour
{
    public List<TurbinePart> parts;

    [Serializable]
    public class TurbinePart
    {
        public GameObject paintObject;
        public Material material;

        public bool assigned = false;
        public bool updatePerformance = false;
        public float timer = 0;
        public MeshFilter meshFilter;
        public Texture2D startTexture;
        public Texture2D currentTexture;
        public List<Vector2Int> activePixelList = new();
        public Dictionary<Vector2Int, Color> activePixelDict = new();

        public Dictionary<Vector3Int, float> startDirtDict = new();
        public Dictionary<Vector3Int, float> startDirtList = new();
        public float startDirt = 0;
        public Dictionary<Vector3Int, float> currDirtDict = new();
        public float currDirt = 0;
    }



    private void Awake()
    {
        Vector3 resolution = new Vector3(2f, 3f, 3f);

        foreach (TurbinePart itr in parts)
        {
            Vector3 bounds = itr.paintObject.GetComponent<Renderer>().bounds.extents * 2f;
            //for(int i=0; )

        }
    }

    private void FixedUpdate()
    {
        foreach (TurbinePart itr in parts)
        {
            if (!itr.assigned)
            {
                if (itr.timer > 3)
                {
                    Debug.Log("Part " + itr.material + " beginning assig.");
                    Assign(itr);
                    Debug.Log("Part " + itr.material + " assigned.");
                    itr.timer = 0;
                }
                else itr.timer += Time.deltaTime;
            }
            else if (itr.timer > 1f)
            {
                itr.timer = 0;
                UpdatePerformance(itr);
            }
        }
    }


    void UpdatePerformance(TurbinePart itr)
    {

    }


    void Assign(TurbinePart itr)
    {
        itr.assigned = true;
        itr.meshFilter = itr.paintObject.GetComponent<MeshFilter>();
        if (itr.meshFilter == null)
        {
            Debug.LogError("Mesh filter not found");
            return;
        }

        InkCanvas inkCanvas = itr.paintObject.GetComponent<InkCanvas>();
        RenderTexture renderTex = inkCanvas.GetPaintMainTexture(itr.material.name + " (Instance)");
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

        Debug.Log("Start texture w/h: " + itr.startTexture.width + "/" + itr.startTexture.height);

        for (int i = 0; i < itr.startTexture.width; i++)
        {
            for (int j = 0; j < itr.startTexture.height; j++)
            {
                Color color = itr.startTexture.GetPixel(i, j);

                if (color.grayscale != 1)
                {
                    itr.activePixelList.Add(new Vector2Int(i, j));
                    itr.activePixelDict.Add(new Vector2Int(i, j), color);


                    // #####################
                    Vector2 uvCoordinate = new Vector2((float)i / itr.startTexture.width, (float)j / itr.startTexture.height);
                    Vector3 coordinate = GetLocalCoordinateFromUV(itr, uvCoordinate);
                    if (coordinate != Vector3.zero)
                    {
                        itr.startDirt += (color.r + color.g + color.b) / 3f;
                        Debug.Log("Start dirt: " + itr.startDirt);
                        Debug.Log("Start dirt");
                    }
                    // #####################
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
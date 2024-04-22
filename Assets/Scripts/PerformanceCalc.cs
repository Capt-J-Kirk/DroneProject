using Es.InkPainter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.FilePathAttribute;


public class PerformanceCalc : MonoBehaviour
{
    public float totalVolume = 0;
    public float wingDirtImportanceRatio = 0.7f;
    public Vector3Int resolution = new Vector3Int(3, 3, 2);
    public List<TurbinePart> parts;


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
        public List<Vector2Int> texCoord_List = new();
        public Dictionary<Vector2Int, Vector3Int> texCoordToZoneCoord_Dict = new();
        //
        public List<Vector3Int> zoneStartDirt_List = new();
        public Dictionary<Vector3Int, float> zoneStartDirt_Dict = new();
        public Dictionary<Vector3Int, float> zoneCurrDirt_Dict = new();

        // Overall measure for turbine part.
        public float startDirt = 0;
        public float currDirt = 0;

        // Hit by raycast
        public bool wasHit = false;

        // Normalize performance
        public Vector3 bounds;
        public float boundsVolume;
        public float normalizedStartDirt;
    }



    private void Awake()
    {
        foreach (TurbinePart itr in parts)
        {
            string boundingBoxName = itr.paint_GO.name + "_Bounding_Box";
            GameObject boundsGO = GameObject.Find(boundingBoxName);
            Vector3 bounds = boundsGO.GetComponent<Renderer>().localBounds.extents * 2;
            itr.bounds = new Vector3(bounds.y, bounds.z, bounds.x);  // Rearranging from blender axes.
            itr.boundsVolume = itr.bounds.x * itr.bounds.y * itr.bounds.z;
            totalVolume += itr.boundsVolume;
            boundsGO.SetActive(false);
        }
        Debug.Log("Total vol: " +  " = " + totalVolume);
    }

    private void FixedUpdate()
    {
        foreach (TurbinePart itr in parts)
        {
            itr.timer += Time.deltaTime;

            if (!itr.assigned && itr.timer > 3)
            {
                Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture BEGINNING assign.");
                Assign(itr);
                Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture SUCCESSFULLY assigned.");
                itr.timer = 0;
            }

            if (itr.assigned && itr.timer > 1f && itr.wasHit)
            {
                Debug.Log("Performance running.");
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
        // ----------------------------------------------------------------------------------------
        itr.currDirt = 0;                                                                   // Resetting.
        foreach (Vector3Int zone in itr.zoneStartDirt_List) itr.zoneCurrDirt_Dict[zone] = 0; // Resetting.
        // Updating 
        foreach (Vector2Int texCoord in itr.texCoord_List)
        {
            Color color = itr.currentTexture.GetPixel(texCoord.x, texCoord.y);
            float dirt = 1 - color.grayscale;
            itr.currDirt += dirt;

            // Updating zone dirt
            Vector3Int zone = itr.texCoordToZoneCoord_Dict[texCoord];
            itr.zoneCurrDirt_Dict[zone] += dirt;
        }

        // Debugging
        Debug.Log("=================================================================================");
        foreach (Vector3Int zone in itr.zoneStartDirt_List) Debug.Log("Dirt in zone " + zone + " = " + itr.zoneCurrDirt_Dict[zone]);
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
                        float dirt = 1 - color.grayscale;
                        itr.startDirt += dirt;
                        itr.currDirt += dirt;

                        // Texture coordinates and local coordinates.
                        Vector2Int texCoord = new Vector2Int(i, j);
                        itr.texCoord_List.Add(texCoord);
                        Vector3 localCoord = new Vector3(coordinate.y, coordinate.z, coordinate.x);  // Rearranging from blender axes.

                        // Zone dirt --------------------------------------------------------------------------------------------
                        Vector3Int zoneCoord = new();
                        // x coord
                        for (int r=0; r<resolution.x; r++) {
                            float coord = localCoord.x;
                            if (coord >= r * itr.bounds.x / resolution.x && coord < (r + 1) * itr.bounds.x / resolution.x)
                            {
                                zoneCoord.x = r;
                            }
                        }
                        // y coord
                        for (int r = 0; r < resolution.y; r++)
                        {
                            float coord = localCoord.y;
                            if (coord >= r * itr.bounds.y / resolution.y && coord < (r + 1) * itr.bounds.y / resolution.y)
                            {
                                zoneCoord.y = r;
                            }
                        }
                        // z coord
                        float coord_z = localCoord.z;
                        if (coord_z <= 0) zoneCoord.z = 0;
                        else zoneCoord.z = 1;

                        if (!itr.zoneStartDirt_Dict.ContainsKey(zoneCoord))
                        {
                            itr.zoneStartDirt_List.Add(zoneCoord);
                            itr.zoneStartDirt_Dict.Add(zoneCoord, dirt);
                            itr.zoneCurrDirt_Dict.Add(zoneCoord, dirt);
                        }
                        else
                        {
                            itr.zoneStartDirt_Dict[zoneCoord] += dirt;
                            itr.zoneCurrDirt_Dict[zoneCoord] += dirt;
                        }
                        // ------------------------------------------------------------------------------------------------------
                        itr.texCoordToZoneCoord_Dict.Add(texCoord, zoneCoord);  // Find object zones from texure coordinates.
                    }
                }
            }
        }
        itr.currentTexture = itr.startTexture;

        // Ordering list of zones.
        itr.zoneStartDirt_List = itr.zoneStartDirt_List.OrderBy(v => v.x).ToList();
        itr.zoneStartDirt_List = itr.zoneStartDirt_List.OrderBy(v => v.y).ToList();
        itr.zoneStartDirt_List = itr.zoneStartDirt_List.OrderBy(v => v.z).ToList();
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
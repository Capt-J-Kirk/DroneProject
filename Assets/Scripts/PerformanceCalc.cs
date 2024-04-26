using Es.InkPainter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.FilePathAttribute;


public class PerformanceCalc : MonoBehaviour
{
    public float totalVolume = 0;
    public float wingDirtImportanceRatio = 0.7f;
    public Vector3Int resolution = new Vector3Int(3, 3, 2);
    public List<TurbinePart> parts;

    // Data saving.
    public float dataSaveTimer = 0;
    public int partsReady = 0;
    public bool writingFirstTime = true;
    private readonly string dataFolderPath = Application.dataPath + "/Data";
    private readonly string dataFilePrefix = "Game_";
    private string filePath = "";


    [Serializable]
    public class TurbinePart
    {
        public GameObject paint_GO;
        public InkCanvas inkCanvas;
        public Material material;

        public bool assigned = false;
        public bool updatePerformance = false;
        public float timer = 0;
        public float globalTimer = 0;
        public MeshFilter meshFilter;
        public Texture2D startTexture;
        public Texture2D currentTexture;

        // Hit by raycast
        public bool wasHit = false;
        public bool wasHitOnce = false;

        // Dirt info containers
        public List<Vector2Int> texCoord_List = new();
        public Dictionary<Vector2Int, Vector3Int> texCoordToZoneCoord_Dict = new();
        // - Overall measure for turbine part.
        public float startNormTotDirt = 0;
        public float currNormTotDirt = 0;
        // - Measure for zones in part.
        public List<Vector3Int> zoneStartDirt_List = new();
        public Dictionary<Vector3Int, float> zoneStartDirt_Dict = new();
        public Dictionary<Vector3Int, float> zoneCurrDirt_Dict = new();

        // Normalize performance
        public Vector3 bounds;
        public float boundsVolume;
        public int pixelCount;
        public float surfacePixelRatio;
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
            //Debug.Log("bounds:" + itr.bounds);
        }
        //Debug.Log("Total vol: " +  " = " + totalVolume);
    }


    private void FixedUpdate()
    {
        dataSaveTimer += Time.deltaTime;
        
        foreach (TurbinePart itr in parts)
        {
            itr.timer += Time.deltaTime;

            if (!itr.assigned && itr.timer > 3)
            {
                Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture BEGINNING assign.");
                Assign(itr);
                NormalizePart(itr);
                Debug.Log("Wind turbine, " + itr.paint_GO.name + " texture SUCCESSFULLY assigned.");
                itr.timer = 0;
                partsReady++;
            }

            if (itr.assigned && itr.timer > 1f && itr.wasHit)
            {
                Debug.Log("Updating performance, " + itr.paint_GO.name + " ==============================================================");
                UpdatePerformance(itr);
                itr.timer = 0;  // Will not be updated again, even if hit before 1 sec.
                itr.wasHit = false;
                itr.wasHitOnce = true;
                partsReady++;
            }

            // The part performance timer starts now.
            if (itr.wasHitOnce) itr.globalTimer += Time.deltaTime;
        }


        if (writingFirstTime && partsReady == parts.Count)
        {
            SaveData();
        }

        // If something was hit and timer expired we save.
        if (partsReady > parts.Count && dataSaveTimer > 1f)
        {
            partsReady = parts.Count;
            dataSaveTimer = 0;
            SaveData();
        }
    }


    void NormalizePart(TurbinePart itr)
    {
        // Esthablish
        itr.surfacePixelRatio =
            (2 * itr.bounds.x * itr.bounds.y +
             2 * itr.bounds.y * itr.bounds.z +
             2 * itr.bounds.x * itr.bounds.z
            ) / itr.pixelCount;

        itr.startNormTotDirt *= itr.surfacePixelRatio;
        itr.currNormTotDirt *= itr.surfacePixelRatio;
        foreach (Vector3Int zone in itr.zoneStartDirt_List)
        {
            itr.zoneStartDirt_Dict[zone] *= itr.surfacePixelRatio;
            itr.zoneCurrDirt_Dict[zone] *= itr.surfacePixelRatio;
            Debug.Log(itr.paint_GO.name + ": " + zone + " = " + itr.zoneCurrDirt_Dict[zone]);
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
        itr.currNormTotDirt = 0;                                                               // Resetting.
        foreach (Vector3Int zone in itr.zoneStartDirt_List) itr.zoneCurrDirt_Dict[zone] = 0;   // Resetting.
        // Updating 
        foreach (Vector2Int texCoord in itr.texCoord_List)
        {
            Color color = itr.currentTexture.GetPixel(texCoord.x, texCoord.y);
            float dirt = (1 - color.grayscale) * itr.surfacePixelRatio;  // White = 1, so 1 - white is no dirt.
            itr.currNormTotDirt += dirt;

            // Updating zone dirt
            Vector3Int zone = itr.texCoordToZoneCoord_Dict[texCoord];
            itr.zoneCurrDirt_Dict[zone] += dirt;
        }

        // Debug.Log("=================================================================================");
        // foreach (Vector3Int zone in itr.zoneStartDirt_List) Debug.Log("Dirt in zone " + zone + " = " + itr.zoneCurrDirt_Dict[zone]);
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

        // Iterate over the entire texture.
        int pixelCounter = 0;
        for (int i = 0; i < itr.startTexture.width; i++)
        {
            for (int j = 0; j < itr.startTexture.height; j++)
            {
                Vector2 uvCoordinate = new Vector2((float)i / itr.startTexture.width, (float)j / itr.startTexture.height);
                Vector3 coordinate = GetLocalCoordinateFromUV(itr, uvCoordinate);

                if (coordinate != Vector3.zero)  // The uv-coordinate is mapped to the mesh.
                {
                    pixelCounter++;
                    Color color = itr.startTexture.GetPixel(i, j);
                    if (color.grayscale != 1)   // Is not completely white.
                    {
                        float dirt = 1 - color.grayscale;
                        itr.startNormTotDirt += dirt;
                        itr.currNormTotDirt += dirt;

                        // Texture coordinates and local coordinates.
                        Vector2Int texCoord = new Vector2Int(i, j);
                        itr.texCoord_List.Add(texCoord);
                        Vector3 localCoord = new Vector3(coordinate.y, coordinate.z, coordinate.x);  // Rearranging from blender axes.

                        // Zone dirt --------------------------------------------------------------------------------------------
                        Vector3Int zoneCoord = new();
                        // x coord
                        for (int r = 0; r < resolution.x; r++)
                        {
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
        itr.pixelCount = pixelCounter;
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


    void SaveData()
    {
        // Create data root folder if not existing.
        if (writingFirstTime)
        {
            writingFirstTime = false;
            if (!Directory.Exists(dataFolderPath)) Directory.CreateDirectory(dataFolderPath);
            // Refresh the AssetDatabase to make sure Unity detects the newly created folder.
            UnityEditor.AssetDatabase.Refresh();

            // Constructing file path.
            DateTime now = DateTime.Now;
            string timeString = "";
            timeString += now.Year.ToString() + '-';
            timeString += now.Month.ToString("D2") + '-';
            timeString += now.Day.ToString("D2") + '_';
            timeString += now.Hour.ToString("D2") + '-';
            timeString += now.Minute.ToString("D2") + '-';
            timeString += now.Second.ToString("D2");
            filePath = Path.Combine(dataFolderPath, dataFilePrefix + timeString + ".csv");

            // Creating the empty file.
            File.WriteAllText(filePath, "");
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        // Constructing file content.
        string dat = "";
        foreach (TurbinePart part in parts)
        {
            dat += part.paint_GO.name + ',' + part.globalTimer.ToString("0.00") + ',';
            foreach (Vector3Int itr in part.zoneStartDirt_List)
            {
                dat += itr.x.ToString() + ',' + itr.y.ToString() + ',' + itr.z.ToString() + ',';
                dat += part.zoneCurrDirt_Dict[itr].ToString("0.00") + ',';
            }
            dat = dat.Remove(dat.Length-1);
            dat += "\n";
        }
        File.AppendAllText(filePath, dat);
    }


}
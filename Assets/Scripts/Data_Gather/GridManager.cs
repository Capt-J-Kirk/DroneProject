using UnityEngine;
using System.Collections.Generic;

// create the tile data type
public struct TileData
{
    public Vector3 Position;
    public float Intensity;
    // Add other properties as needed

    public TileData(Vector3 position, float intensity)
    {
        Position = position;
        Intensity = intensity;
      
    }

}

// create and manage the tile grid
public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab; // Assign a prefab for visual representation
    public int width, height; // defines the grid width and hight
    private TileData[,] grid;

    void Start()
    {
        grid = new TileData[width, height];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, 0, y); // Adjust as needed for your grid layout
                grid[x, y] = new TileData(position, 0); // Initialize with default intensity of 0

                // Instantiate the tile prefab and position it
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObj.name = $"Tile {x} {y}";
                /
            }
        }
    }

    // simple impl. 
    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         RaycastHit hit;
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             // Assuming your tiles are named "Tile X Y", where X and Y are coordinates
    //             string[] parts = hit.transform.name.Split(' ');
    //             int x = int.Parse(parts[1]);
    //             int y = int.Parse(parts[2]);

    //             // Update the intensity
    //             TileData tile = grid[x, y];
    //             tile.Intensity = Mathf.Clamp01(tile.Intensity + 0.1f);
    //             grid[x, y] = tile;

    //             // Update the tile's visual appearance
    //             hit.transform.GetComponent<Renderer>().material.color = new Color(tile.Intensity, tile.Intensity, tile.Intensity);
    //         }
    //     }
    // }



    // complex implementation of a cone like spray detection on multiple tiles with varying intensity

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f)) // 100f is the max distance
            {
                ApplyConeEffect(hit.point, ray.direction);
            }
        }
    }

    void ApplyConeEffect(Vector3 hitPoint, Vector3 hitDirection)
    {
        public float radius = 5f; // Radius of the effect
        public float maxAngle = 45f; // Max angle for the cone
        public float maxIntensityIncrease = 0.5f; // Max increase in intensity

        Collider[] hitColliders = Physics.OverlapSphere(hitPoint, radius);
        foreach (var collider in hitColliders)
        {
            // Check if the collider is part of a tile
            if (collider.gameObject.CompareTag("Tile")) // Make sure your tiles have the "Tile" tag
            {
                Vector3 toCollider = collider.transform.position - hitPoint;
                float angle = Vector3.Angle(hitDirection, toCollider);
                if (angle <= maxAngle) // Check if within cone
                {
                    float distance = toCollider.magnitude;
                    float intensityFactor = 1 - (distance / radius); // Decreases with distance
                    float angleFactor = 1 - (angle / maxAngle); // Decreases with angle deviation
                    float intensityIncrease = maxIntensityIncrease * intensityFactor * angleFactor;

                    // Assuming your tiles are named "Tile X Y", where X and Y are coordinates
                    string[] parts = collider.gameObject.name.Split(' ');
                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);

                    // Update the intensity
                    TileData tile = grid[x, y];
                    tile.Intensity = Mathf.Clamp01(tile.Intensity + intensityIncrease);
                    grid[x, y] = tile;

                    // Update the tile's visual appearance
                    collider.gameObject.GetComponent<Renderer>().material.color = new Color(tile.Intensity, tile.Intensity, tile.Intensity);
                }
            }
        }
    }


}

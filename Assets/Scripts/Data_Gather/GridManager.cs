using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class GridManager : MonoBehaviour
{
    public GameObject boxPrefab;
    public Transform startPosition;
    public int width = 10;
    public int height = 10;
    public float boxSize = 0.1f;

    //private List<BoxData> boxList = new List<BoxData>();
    private List<BoxData> BoxList = new List<BoxData>();
    public List<List<BoxData>> allGrids = new List<List<BoxData>>(); 

    void Start()
    {
        GenerateGrid();
    }

 
    void GenerateGrid()
    {
       
        Vector3 basePosition = startPosition.position;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 spawnPosition = new Vector3(basePosition.x + x * boxSize, basePosition.y, basePosition.z + z * boxSize);
                GameObject newBox = Instantiate(boxPrefab, spawnPosition, Quaternion.identity, startPosition);
                newBox.name = $"Box_{x}_{z}";
                newBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);

                BoxData boxData = newBox.AddComponent<BoxData>();
                BoxList.Add(boxData);
                
                // Initialize properties
                boxData.value = Random.Range(0, 100); // Example initialization
                boxData.intensity = Random.Range(0.0f, 1.0f); // Example initialization
                boxData.flag = Random.value > 0.5f; // Example initialization
            }
        }

        allGrids.Add(BoxList);
    }


    public void SaveToCSV()
    {
        StringBuilder csvBuilder = new StringBuilder();

        // Add the top headers only once, at the start
        for (int i = 0; i < width * height; i++)
        {
            csvBuilder.Append($",Box_{i}");
        }
        csvBuilder.AppendLine();

        // Write the data of each grid in blocks of three rows per grid
        foreach (var grid in allGrids)
        {
            // Values Row with its property name
            csvBuilder.Append("Value");
            foreach (var box in grid)
            {
                csvBuilder.Append($", {box.value}");
            }
            csvBuilder.AppendLine();

            // Intensity Row with its property name
            csvBuilder.Append("Intensity");
            foreach (var box in grid)
            {
                csvBuilder.Append($", {box.intensity:F2}");
            }
            csvBuilder.AppendLine();

            // Flag Row with its property name
            csvBuilder.Append("Flag");
            foreach (var box in grid)
            {
                csvBuilder.Append($", {box.flag}");
            }
            csvBuilder.AppendLine();
        }

        string filePath = Path.Combine(Application.persistentDataPath, "box_data.csv");
        File.WriteAllText(filePath, csvBuilder.ToString());

        Debug.Log($"Data saved to {filePath}");
    }


    public void UpdateAndSaveData()
    {
        UpdateBoxValues();
        SaveToCSV();
    }

    void Update()
    {
        UpdateBoxValues();
        if (Input.GetKeyDown(KeyCode.W))
        {
            UpdateBoxValues();
            SaveToCSV();
        }
    }
    

    void UpdateBoxValues()
    {
        foreach (var box in BoxList)
        {
            // Increment or update properties as an example
            box.value++;
            box.intensity += 0.1f;
            box.flag = !box.flag; // Toggle for example
        }
        allGrids.Add(BoxList);
    }
}

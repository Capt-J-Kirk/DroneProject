using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class GridManager : MonoBehaviour
{
    public GameObject boxPrefab;
    public Transform startPosition;
    public GameObject Nozzle;
    public int width = 10;
    public int height = 10;
    public float boxSize = 0.1f;


    
    // filename data !
    public string type;
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;


    // public summerized values for datacollector
    public float cleaningPercent = 0;
    public float maxCleanValuePossible = 0;
    public float currentCleanValue = 0;
    public float cleaningPerSecond = 0;



    //private List<BoxData> boxList = new List<BoxData>();
    private List<BoxData> BoxList = new List<BoxData>();
    public List<List<BoxData>> allGrids = new List<List<BoxData>>(); 

    // USED for raycasting
    public float maxDistance = 5.0f;
    public float maxRadius = 0.20f;


    void Start()
    {
       // GenerateGrid();
    }

    // make sure to setup the tag "cleaning" in the tags and layers setting!
 
    public void GenerateGrid()
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
                newBox.tag = "cleaning";

                BoxData boxData = newBox.AddComponent<BoxData>();
                BoxList.Add(boxData);
                
                // Initialize properties
                boxData.value = 0.0f;//Random.Range(0, 100); // Example initialization
                boxData.intensity = 0.0f; //Random.Range(0.0f, 1.0f); // Example initialization
                boxData.flag = false; //Random.value > 0.5f; // Example initialization
                // Set the color based on intensity
                Renderer boxRenderer = newBox.GetComponent<Renderer>();
                boxRenderer.material = new Material(boxRenderer.material); // This is necessary if you're not using shared materials
                boxRenderer.material.color = Color.Lerp(Color.black, Color.white, boxData.intensity);
                // Add a BoxCollider to the box
                //newBox.AddComponent<BoxCollider>();
            
                maxCleanValuePossible += 1.0f; 

            }
        }

        allGrids.Add(BoxList);
    }
    public void SaveToCSV()
    {
        StringBuilder csvBuilder = new StringBuilder();

        // Start with an empty string for the first cell if you want headers to start from the second column
        csvBuilder.Append("Box"); // Or leave this empty if you want an empty first cell
        
        // Add the top headers
        for (int i = 0; i < width * height; i++)
        {
            csvBuilder.Append($",Box_{i}");
        }
        csvBuilder.AppendLine();

        // Write the data of each grid
        foreach (var grid in allGrids)
        {
            // Values Row with its property name
            csvBuilder.Append("Value"); // This will go into the first column of the new row
            foreach (var box in grid)
            {
                csvBuilder.Append($",{box.value}"); // Ensure to start with a comma
            }
            csvBuilder.AppendLine();

            // Intensity Row with its property name
            csvBuilder.Append("Intensity");
            foreach (var box in grid)
            {
                csvBuilder.Append($",{box.intensity:F2}"); // Ensure to start with a comma
            }
            csvBuilder.AppendLine();

            // Flag Row with its property name
            csvBuilder.Append("Flag");
            foreach (var box in grid)
            {
                csvBuilder.Append($",{box.flag}"); // Ensure to start with a comma
            }
            csvBuilder.AppendLine();
        }

        string filePath = Path.Combine(Application.persistentDataPath, "box_data.csv");
        File.WriteAllText(filePath, csvBuilder.ToString());

        Debug.Log($"Data saved to {filePath}");
    }


    // public void SaveToCSV()
    // {
    //     StringBuilder csvBuilder = new StringBuilder();

    //     // Add the top headers only once, at the start
    //     for (int i = 0; i < width * height; i++)
    //     {
    //         csvBuilder.Append($",Box_{i}");
    //     }
    //     csvBuilder.AppendLine();

    //     // Write the data of each grid in blocks of three rows per grid
    //     foreach (var grid in allGrids)
    //     {
    //         // Values Row with its property name
    //         csvBuilder.Append("Value");
    //         foreach (var box in grid)
    //         {
    //             csvBuilder.Append($", {box.value}");
    //         }
    //         csvBuilder.AppendLine();

    //         // Intensity Row with its property name
    //         csvBuilder.Append("Intensity");
    //         foreach (var box in grid)
    //         {
    //             csvBuilder.Append($", {box.intensity:F2}");
    //         }
    //         csvBuilder.AppendLine();

    //         // Flag Row with its property name
    //         csvBuilder.Append("Flag");
    //         foreach (var box in grid)
    //         {
    //             csvBuilder.Append($", {box.flag}");
    //         }
    //         csvBuilder.AppendLine();
    //     }

    //     string filePath = Path.Combine(Application.persistentDataPath, "box_data.csv");
    //     File.WriteAllText(filePath, csvBuilder.ToString());

    //     Debug.Log($"Data saved to {filePath}");
    // }


    public void UpdateAndSaveData()
    {
        UpdateBoxValues();
        SaveToCSV();
    }

    // void Update()
    // {
    //     UpdateBoxValues();
    //     if (Input.GetKeyDown(KeyCode.W))
    //     {
    //         UpdateBoxValues();
    //         SaveToCSV();
    //     }
    // }
    

    public void UpdateBoxValues()
    {
        // used to update the values
        foreach (var box in BoxList)
        {
            // Increment or update properties as an example
            box.value +=0.01f;
            box.intensity += 0.1f;
            box.intensity = Mathf.Clamp(box.intensity, 0.0f, 1.0f);
            box.flag = !box.flag; // Toggle for example

            // Update the color based on the new intensity
            Renderer boxRenderer = box.GetComponent<Renderer>();
            boxRenderer.material.color = Color.Lerp(Color.black, Color.white, box.intensity);
   
        }
        // allGrids.Add(BoxList);


        // used to take a snapshot and store it
        List<BoxData> currentGridState = new List<BoxData>();
        foreach (var box in BoxList)
        {
            // Clone the box data to keep a snapshot of its current state
            BoxData boxStateSnapshot = new BoxData()
            {
                value = box.value,
                intensity = box.intensity,
                flag = box.flag
            };
            currentGridState.Add(boxStateSnapshot);
            //temp += box.intensity;
        }
        //currentCleanValue = temp;
        //cleaningPercent = (maxCleanValuePossible / 100.0f) * currentCleanValue;

        // Append the snapshot of the current grid state to allGrids
        allGrids.Add(currentGridState);


    }

    public void UpdateBoxValuesWithRayCast()
    {
        Debug.Log("inside raycast");
        // needs thr transform to be the nozzle of the watersprayer
        Ray ray = new Ray(Nozzle.transform.position, transform.forward* -1);
        RaycastHit hit;
        Debug.DrawRay(Nozzle.transform.position, transform.forward* -1 * 10, Color.blue);

        float SumCleaningsFactor = 0.0f;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Calculate factors for decreasing value based on distance
            float distanceFromRayOrigin = Vector3.Distance(Nozzle.transform.position, hit.point);
            float valueFactor = 1 - (distanceFromRayOrigin / maxDistance);

            // Perform an overlapping sphere check to simulate a cone spread
            float radiusAtHit = Mathf.Lerp(0, maxRadius, hit.distance / maxDistance);
            Collider[] hitColliders = Physics.OverlapSphere(hit.point, radiusAtHit);


            foreach (Collider collider in hitColliders)
            {

                // Calculate radial factors for value decrement based on distance
                float radialDistance = Vector3.Distance(hit.point, collider.transform.position);
                float radialFactor = 1 - (radialDistance / radiusAtHit);
                float cleaningsFactor = valueFactor * radialFactor;

                // sum the cleaning effort for this time frame.
                SumCleaningsFactor += cleaningsFactor;

                Debug.Log("cleaning value: " + cleaningsFactor);

                BoxData boxData = collider.GetComponent<BoxData>(); 
                if (boxData != null) //&& BoxList.Contains(collider.gameObject))
                {
                    Debug.Log("box data:");
                    // Update the box cleanings status
                    // using the value as the total amount of cleaning applied, there intencity if the real cleanliness of the box
                    boxData.value += cleaningsFactor;
                    boxData.intensity = Mathf.Clamp(boxData.intensity + cleaningsFactor, 0.0f, 1.0f);
                    // set the flag, if box is 100p cleaned
                    if (boxData.intensity == 1.0f)
                    {
                        boxData.flag = true;
                    }
                    // Update the color based on the new intensity
                    Renderer boxRenderer = boxData.gameObject.GetComponent<Renderer>();
                    boxRenderer.material.color = Color.Lerp(Color.black, Color.white, boxData.intensity);
           
                }
            }
        }
        // get states of each box 
        cleaningPerSecond = SumCleaningsFactor;
        float temp = 0.0f;
        // foreach (var box in BoxList)
        // {
           
        //     temp += box.intensity;
            
        // }

        
        // append the boxlist current states to the list
        //allGrids.Add(BoxList);

        foreach (var box in BoxList)
        {
            // Increment or update properties as an example
            box.value++;
            box.intensity += 0.1f;
            box.intensity = Mathf.Clamp(box.intensity, 0.0f, 1.0f);
            box.flag = !box.flag; // Toggle for example

            // Update the color based on the new intensity
            Renderer boxRenderer = box.GetComponent<Renderer>();
            boxRenderer.material.color = Color.Lerp(Color.black, Color.white, box.intensity);
   
        }
        allGrids.Add(BoxList);



        // List<BoxData> currentGridState = new List<BoxData>();
        // foreach (var box in BoxList)
        // {
        //     // Clone the box data to keep a snapshot of its current state
        //     BoxData boxStateSnapshot = new BoxData()
        //     {
        //         value = box.value,
        //         intensity = box.intensity,
        //         flag = box.flag
        //     };
        //     currentGridState.Add(boxStateSnapshot);
        //     temp += box.intensity;
        // }
        // currentCleanValue = temp;
        // cleaningPercent = (maxCleanValuePossible / 100.0f) * currentCleanValue;

        // // Append the snapshot of the current grid state to allGrids
        // allGrids.Add(currentGridState);

        
    }

 
}

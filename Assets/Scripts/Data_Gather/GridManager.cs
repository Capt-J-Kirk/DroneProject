using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
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

    public GameObject grid1;
    public GameObject grid2;
    
    public GameObject old_grid1;
    public GameObject old_grid2;
    // filename data !
    public int type;
    public string name;
    public string controlScheme;
    public string startPose;
    public string gridLocation;
    public string userInterface;

    public UserInput userInput;
    // public summerized values for datacollector
    public float cleaningPercent = 0;
    public float maxCleanValuePossible = 0;
    public float currentCleanValue = 0;
    public float cleaningPerSecond = 0;



    //private List<BoxData> boxList = new List<BoxData>();
    private List<BoxData> BoxList = new List<BoxData>();
    public List<List<BoxData>> allGrids = new List<List<BoxData>>(); 

    // USED for raycasting
    public float maxDistance = 8.0f;
    public float maxRadius = 0.25f;


    void Start()
    {
       // GenerateGrid();
       old_grid1.SetActive(false);
       old_grid2.SetActive(false);
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
        //for (int i = 0; i < width * height; i++)
        for (int i = 0; i < BoxList.Count; i++)
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
                csvBuilder.Append($",{box.value.ToString(CultureInfo.InvariantCulture)}"); // Ensure to start with a comma
            }
            csvBuilder.AppendLine();

            // Intensity Row with its property name
            csvBuilder.Append("Intensity");
            foreach (var box in grid)
            {
                csvBuilder.Append($",{box.intensity.ToString(CultureInfo.InvariantCulture):F2}"); // Ensure to start with a comma
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

        string fileNamePart = ("performance" + "_" + type.ToString() + "_" + name + "_" + controlScheme + "_" + startPose + "_" + gridLocation + "_" + userInterface); 
        string fileName = $"{fileNamePart}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);


        //string filePath = Path.Combine(Application.persistentDataPath, "box_data.csv");
        File.WriteAllText(filePath, csvBuilder.ToString());

        Debug.Log($"Data saved to {filePath}");
    }


    // This method will repopulate the BoxList with BoxData components from existing grid children
    public void PopulateBoxListFromExistingGrid()
    {
        // Clear the existing list to avoid duplicates
        BoxList.Clear();

        // Iterate through all children of the 'Grid' GameObject (this.transform assumes this script is attached to 'Grid')

        // make a check to select which grid should be loaded

        if(gridLocation == "grid1")
        {
            foreach (Transform child in grid1.transform)
            {
                // Check if the child has a BoxData component
                BoxData boxData = child.GetComponent<BoxData>();
                if (boxData != null)
                {
                    // Add the BoxData component to the BoxList
                    BoxList.Add(boxData);
                }
            }
        }  
        else
        {
            foreach (Transform child in grid2.transform)
            {
                // Check if the child has a BoxData component
                BoxData boxData = child.GetComponent<BoxData>();
                if (boxData != null)
                {
                    // Add the BoxData component to the BoxList
                    BoxList.Add(boxData);
                }
            }
        }
        
   
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
            box.intensity += 0.01f;
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
        //Debug.Log("inside raycast");
        // needs thr transform to be the nozzle of the watersprayer
        //Vector3 castFromPosition = Nozzle.transform.position + new Vector3(0, -2,0);

        Ray ray = new Ray(Nozzle.transform.position, transform.forward* -1);
        RaycastHit hit;
        Debug.DrawRay(Nozzle.transform.position, transform.forward* -1 * maxDistance, Color.blue);

        float SumCleaningsFactor = 0.0f;
        if(true)//userInput.isSpraying)
        {
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
                    float cleaningsFactor = (valueFactor * radialFactor)/2f; // scale down the cleaning factor by 10

                    // sum the cleaning effort for this time frame.
                    SumCleaningsFactor += cleaningsFactor;

                    //Debug.Log("cleaning value: " + cleaningsFactor);

                    BoxData boxData = collider.GetComponent<BoxData>(); 
                    if (boxData != null) //&& BoxList.Contains(collider.gameObject))
                    {
                        //Debug.Log("box data:");
                        // Update the box cleanings status
                        // using the value as the total amount of cleaning applied, there intencity if the real cleanliness of the box
                        boxData.value += cleaningsFactor;
                        boxData.intensity += cleaningsFactor;
                        boxData.intensity = Mathf.Clamp(boxData.intensity, 0.0f, 1.0f);
                        Debug.Log("Intensitet " + boxData.intensity);
                        // set the flag, if box is 100p cleaned
                        if (boxData.intensity == 1.0f)
                        {
                            boxData.flag = true;
                        }
                        // // Update the color based on the new intensity
                        Renderer boxRenderer = boxData.gameObject.GetComponent<Renderer>();
                        boxRenderer.material.color = Color.Lerp(Color.black, Color.white, boxData.intensity);
            
                    }
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
            temp += box.intensity;
            // Update the color based on the new intensity
            // Renderer boxRenderer = boxData.gameObject.GetComponent<Renderer>();
            // boxRenderer.material.color = Color.Lerp(Color.black, Color.white, boxData.intensity);
        }
        currentCleanValue = temp;
        cleaningPercent = (maxCleanValuePossible / 100.0f) * currentCleanValue;
        Debug.Log("currentGridState " + currentGridState);
        // Append the snapshot of the current grid state to allGrids
        allGrids.Add(currentGridState);
        
    }


    public void ClearGridData()
    {
        allGrids.Clear();

        foreach (var box in BoxList)
        {
            box.value = 0.0f;
            box.intensity = 0.0f;
            box.intensity = Mathf.Clamp(box.intensity, 0.0f, 1.0f);
            box.flag = false; // Toggle for example

            // Update the color based on the new intensity
            Renderer boxRenderer = box.GetComponent<Renderer>();
            boxRenderer.material.color = Color.Lerp(Color.black, Color.white, box.intensity);
   
        }
        Debug.Log("Grid list cleared.");

    }
 
    public void ClearboxList()
    {
        BoxList.Clear(); 
        allGrids.Clear();
        maxCleanValuePossible = 0;
    }

    public void ClearGeneratedGrid()
    {
        // Iterate through the list of box data objects
        foreach (var boxData in BoxList)
        {
            if (boxData.gameObject != null)
            {
                Destroy(boxData.gameObject); // Destroy the GameObject
            }
        }
        // Clear the lista
        BoxList.Clear(); 
        allGrids.Clear();
        maxCleanValuePossible = 0; 
        Debug.Log("All generated grid boxes have been removed.");
    }

}

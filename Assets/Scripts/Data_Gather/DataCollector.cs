using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    // start of data collection
    public string type = "controller";
    public bool startDataCollection = false;

    // allocation of each list, limit memory reallocation during testing. 
    //private int frequency = 20; // how often is the data logged
    //private int Duration  = 240; // 4 minuts (buffer) total planned mission time
    private int expectedSize = 20 * 240;
    // Lists to hold data
    private List<DroneData> dataList = new List<DroneData>(20*240);





    // Method to add data to lists
    public void CollectData(string name, string controlScheme, string startPose, string gridLocation, 
                            string userInterface, Vector3 main_pos, Quaternion main_rot, Vector3 sec_pos, 
                            Quaternion sec_rot, bool inFlight, bool inCleaning, float cleaningPercent, 
                            float maxCleanValuePossible, float currentCleanValue, float cleaningPerSecond, 
                            float throttle1, float pitch1, float yaw1, float roll1, 
                            float throttle2, float pitch2, float yaw2, float roll2, float radius, float theta, 
                            float phi, bool followMode, bool switchDrone, bool controlMainDrone, 
                            bool switchCamFeed, bool isSpraying, float distanceToObject1, float distanceToObject2)
    {
        DroneData newData = new DroneData(name, controlScheme, startPose, gridLocation, userInterface,
                                          main_pos, main_rot, sec_pos, sec_rot, inFlight, inCleaning,
                                          cleaningPercent, maxCleanValuePossible, currentCleanValue, 
                                          cleaningPerSecond, throttle1, pitch1, yaw1, 
                                          roll1, throttle2, pitch2, yaw2, 
                                          roll2, radius, theta, 
                                          phi, followMode, switchDrone, controlMainDrone, switchCamFeed, isSpraying, 
                                          distanceToObject1, distanceToObject2);

        dataList.Add(newData);
    }

   
     public void SaveDataToCSV()
    {
        // Example: Use the first data entry to generate part of the file name
        // Ensure dataList is not empty to avoid errors
        if (dataList.Count == 0)
        {
            Debug.LogError("Data list is empty. No data to save.");
            return;
        }

        string fileNamePart = type + "_" + dataList[0].name + "_" + dataList[0].controlScheme + "_" + dataList[0].startPose + "_" + dataList[0].gridLocation + "_" + dataList[0].userInterface; // Customize as needed
        string fileName = $"{fileNamePart}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder csvContent = new StringBuilder();
        // CSV header
        csvContent.AppendLine("Name,ControlScheme,StartPose,GridLocation,UserInterface," +
                      "Main_Pos_X,Main_Pos_Y,Main_Pos_Z," +
                      "Main_Rot_X,Main_Rot_Y,Main_Rot_Z,Main_Rot_W," +
                      "Sec_Pos_X,Sec_Pos_Y,Sec_Pos_Z," +
                      "Sec_Rot_X,Sec_Rot_Y,Sec_Rot_Z,Sec_Rot_W," +
                      "InFlight,InCleaning," +
                      "CleaningPercent,MaxCleanValuePossible,CurrentCleanValue,CleaningPerSecond," +
                      "throttle1,pitch1,yaw1,roll1,throttle2,pitch2,yaw2,roll2," +
                      "Radius,Theta,Phi," +
                      "FollowMode,SwitchDrone,ControlMainDrone,SwitchCamFeed," +
                      "isSpraying,DistanceToObject1,DistanceToObject2");
        // Convert each DroneData instance to a CSV row and append
        foreach (var data in dataList)
        {
            string csvLine = $"{data.name},{data.controlScheme},{data.startPose},{data.gridLocation},{data.userInterface}," +
                    $"{data.main_pos.x},{data.main_pos.y},{data.main_pos.z}," +
                    $"{data.main_rot.x},{data.main_rot.y},{data.main_rot.z},{data.main_rot.w}," +
                    $"{data.sec_pos.x},{data.sec_pos.y},{data.sec_pos.z}," +
                    $"{data.sec_rot.x},{data.sec_rot.y},{data.sec_rot.z},{data.sec_rot.w}," +
                    $"{data.inFlight},{data.inCleaning}," +
                    $"{data.cleaningPercent},{data.maxCleanValuePossible},{data.currentCleanValue},{data.cleaningPerSecond}," +
                    $"{data.throttle1},{data.pitch1},{data.yaw1},{data.roll1}," +
                    $"{data.throttle2},{data.pitch2},{data.yaw2},{data.roll2}," +
                    $"{data.radius},{data.theta},{data.phi}," +
                    $"{data.followMode},{data.switchDrone},{data.controlMainDrone},{data.switchCamFeed}," +
                    $"{data.isSpraying},{data.distanceToObject1},{data.distanceToObject2}";
            csvContent.AppendLine(csvLine);
        }

        // Write to file
        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log($"Data saved to {filePath}");
    }

    public void ClearDataList()
    {
        dataList.Clear();
        Debug.Log("Data list cleared.");
    }
}

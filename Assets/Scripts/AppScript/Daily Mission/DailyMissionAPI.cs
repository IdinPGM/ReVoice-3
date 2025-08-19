using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class DailyMissionAPI : MonoBehaviour
{
    [Header("API Settings")]
    public string baseURL = "https://api.mystrokeapi.uk";
    public string getMissionsEndpoint = "/api/daily-missions";
    public string updateMissionEndpoint = "/api/daily-missions/update";

    private DailyMissionManager missionManager;

    private void Start()
    {
        missionManager = FindObjectOfType<DailyMissionManager>();
    }

    public void LoadMissionsFromServer()
    {
        StartCoroutine(GetMissionsFromServer());
    }

    public void UpdateMissionOnServer(DailyMissionData mission)
    {
        StartCoroutine(UpdateMissionProgress(mission));
    }

    private IEnumerator GetMissionsFromServer()
    {
        string url = baseURL + getMissionsEndpoint;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Add headers if needed (authentication, etc.)
            // webRequest.SetRequestHeader("Authorization", "Bearer " + authToken);
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Missions loaded from server: " + jsonResponse);
                
                try
                {
                    // Parse the response and update missions
                    if (missionManager != null)
                    {
                        missionManager.LoadMissionsFromJSON(jsonResponse);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse missions JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Failed to load missions from server: " + webRequest.error);
                // Fall back to local missions
                LoadLocalMissions();
            }
        }
    }

    private IEnumerator UpdateMissionProgress(DailyMissionData mission)
    {
        string url = baseURL + updateMissionEndpoint;
        
        // Create JSON payload
        string jsonPayload = JsonUtility.ToJson(mission);
        
        using (UnityWebRequest webRequest = UnityWebRequest.Put(url, jsonPayload))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            // Add headers if needed (authentication, etc.)
            // webRequest.SetRequestHeader("Authorization", "Bearer " + authToken);
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Mission progress updated on server");
            }
            else
            {
                Debug.LogError("Failed to update mission on server: " + webRequest.error);
            }
        }
    }

    private void LoadLocalMissions()
    {
        // Create sample JSON data as fallback
        string sampleJSON = @"{
            ""dailyMission"": [
                {
                    ""id"": ""facial_detection_001"",
                    ""questType"": ""facial_detection"",
                    ""questDate"": null,
                    ""targetCount"": 2,
                    ""currentCount"": 1,
                    ""isCompleted"": false,
                    ""points"": 10
                },
                {
                    ""id"": ""minigame_001"",
                    ""questType"": ""minigame"",
                    ""questDate"": null,
                    ""targetCount"": 4,
                    ""currentCount"": 0,
                    ""isCompleted"": false,
                    ""points"": 15
                },
                {
                    ""id"": ""productivity_001"",
                    ""questType"": ""productivity"",
                    ""questDate"": null,
                    ""targetCount"": 15,
                    ""currentCount"": 0,
                    ""isCompleted"": false,
                    ""points"": 20
                }
            ]
        }";

        if (missionManager != null)
        {
            missionManager.LoadMissionsFromJSON(sampleJSON);
        }
    }

    public void TestWithSampleData()
    {
        LoadLocalMissions();
    }
}

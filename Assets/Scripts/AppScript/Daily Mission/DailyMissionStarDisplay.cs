using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DailyMissionStarDisplay : MonoBehaviour
{
    [Header("Star UI References")]
    public Image star1;
    public Image star2;
    public Image star3;

    [Header("Star Sprites")]
    public Sprite emptyStar;
    public Sprite filledStar;

    [Header("API Settings")]
    public string baseURL = "https://api.mystrokeapi.uk";
    public string getMissionsEndpoint = "/user/daily-mission";

    private void Start()
    {
        // Initialize with empty stars
        UpdateStarDisplay(0);
        
        // Load mission data to update stars
        LoadAndUpdateStars();
    }

    public void LoadAndUpdateStars()
    {
        string url = baseURL + getMissionsEndpoint;

        // Prepare headers with authorization
        var headers = new System.Collections.Generic.Dictionary<string, string>();
        string authToken = PlayerPrefs.GetString("authToken", "");
        if (!string.IsNullOrEmpty(authToken))
        {
            headers.Add("Authorization", "Bearer " + authToken);
        }

        StartCoroutine(HttpHelper.GetRequestCoroutine<DailyMissionList>(
            url,
            onSuccess: (response) =>
            {
                Debug.Log("Mission star data loaded successfully");
                int completedCount = CountCompletedMissions(response.dailyMission);
                UpdateStarDisplay(completedCount);
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Failed to load mission star data: {error} (Code: {code})");
                // Use local fallback data
                LoadLocalStarData();
            },
            additionalHeaders: headers
        ));
    }

    private void LoadLocalStarData()
    {
        // Sample data for testing - you can modify this based on your needs
        string sampleJSON = @"{
            ""dailyMission"": [
                {
                    ""id"": ""play_time_001"",
                    ""questType"": ""play_time"",
                    ""questDate"": null,
                    ""targetCount"": 30,
                    ""currentCount"": 15,
                    ""isCompleted"": false,
                    ""points"": 10
                },
                {
                    ""id"": ""facial_detection_001"",
                    ""questType"": ""facial_detection"",
                    ""questDate"": null,
                    ""targetCount"": 5,
                    ""currentCount"": 2,
                    ""isCompleted"": false,
                    ""points"": 15
                },
                {
                    ""id"": ""functional_speech_001"",
                    ""questType"": ""functional_speech"",
                    ""questDate"": null,
                    ""targetCount"": 3,
                    ""currentCount"": 3,
                    ""isCompleted"": true,
                    ""points"": 20
                }
            ]
        }";

        try
        {
            DailyMissionList missionList = JsonUtility.FromJson<DailyMissionList>(sampleJSON);
            int completedCount = CountCompletedMissions(missionList.dailyMission);
            UpdateStarDisplay(completedCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load local star data: " + e.Message);
            UpdateStarDisplay(0); // Show empty stars as fallback
        }
    }

    private int CountCompletedMissions(DailyMissionData[] missions)
    {
        if (missions == null) return 0;
        return missions.Count(m => m.isCompleted);
    }

    public void UpdateStarDisplay(int completedMissions)
    {
        // Update 3-star system based on completed missions count
        if (star1 != null)
            star1.sprite = completedMissions >= 1 ? filledStar : emptyStar;
        
        if (star2 != null)
            star2.sprite = completedMissions >= 2 ? filledStar : emptyStar;
        
        if (star3 != null)
            star3.sprite = completedMissions >= 3 ? filledStar : emptyStar;

        Debug.Log($"Stars updated: {completedMissions}/3 missions completed");
    }

    // Public method to manually refresh stars (can be called from other scripts)
    public void RefreshStars()
    {
        LoadAndUpdateStars();
    }

    // Public method to set stars manually (useful for testing or manual control)
    public void SetStars(int completedCount)
    {
        UpdateStarDisplay(completedCount);
    }
}

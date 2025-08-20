using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class DailyMissionManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform missionContainer;
    public GameObject missionItemPrefab;
    public TextMeshProUGUI titleText;

    [Header("API Settings")]
    public string baseURL = "https://api.mystrokeapi.uk";
    public string getMissionsEndpoint = "/user/daily-mission";

    private List<DailyMissionData> currentMissions;
    private List<DailyMissionItem> missionItems;

    // Events for UI feedback only
    public static event System.Action OnPlayTimeUsed;
    public static event System.Action OnFacialDetectionUsed;
    public static event System.Action OnFunctionalSpeechUsed;

    [Header("Star System (3 Stars)")]
    public Image star1;
    public Image star2;
    public Image star3;

    [Header("Star Sprites")]
    public Sprite emptyStar;
    public Sprite filledStar;

    private void Awake()
    {
        currentMissions = new List<DailyMissionData>();
        missionItems = new List<DailyMissionItem>();
    }

    private void Start()
    {
        SetupUI();
        LoadMissionsFromServer();
        SetupEventHandlers();
        UpdateStarDisplay(0);
    }

    private void SetupEventHandlers()
    {
        // Subscribe to events for UI feedback
        OnPlayTimeUsed += HandlePlayTimeFeedback;
        OnFacialDetectionUsed += HandleFacialDetectionFeedback;
        OnFunctionalSpeechUsed += HandleFunctionalSpeechFeedback;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        OnPlayTimeUsed -= HandlePlayTimeFeedback;
        OnFacialDetectionUsed -= HandleFacialDetectionFeedback;
        OnFunctionalSpeechUsed -= HandleFunctionalSpeechFeedback;
    }

    // ===== API Methods (merged from DailyMissionAPI) =====

    public void LoadMissionsFromServer()
    {
        string url = baseURL + getMissionsEndpoint;

        // Prepare headers with authorization
        var headers = new Dictionary<string, string>();
        string authToken = PlayerPrefs.GetString("authToken", "");
        if (!string.IsNullOrEmpty(authToken))
        {
            headers.Add("Authorization", "Bearer " + authToken);
        }

        StartCoroutine(HttpHelper.GetRequestCoroutine<DailyMissionList>(
            url,
            onSuccess: (response) =>
            {
                Debug.Log("Missions loaded from server successfully");
                LoadMissionsFromJSON(JsonUtility.ToJson(response));
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Failed to load missions from server: {error} (Code: {code})");
                LoadLocalMissions();
            },
            additionalHeaders: headers
        ));
    }

    private void LoadLocalMissions()
    {
        // Create sample JSON data as fallback with correct questTypes
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

        LoadMissionsFromJSON(sampleJSON);
    }

    // ===== Event Feedback Methods (merged from DailyMissionTracker) =====

    public static void TriggerPlayTime()
    {
        OnPlayTimeUsed?.Invoke();
        Debug.Log("Daily Mission Event: Play Time Used (UI feedback only)");
    }

    public static void TriggerFacialDetection()
    {
        OnFacialDetectionUsed?.Invoke();
        Debug.Log("Daily Mission Event: Facial Detection Used (UI feedback only)");
    }

    public static void TriggerFunctionalSpeech()
    {
        OnFunctionalSpeechUsed?.Invoke();
        Debug.Log("Daily Mission Event: Functional Speech Used (UI feedback only)");
    }

    private void HandlePlayTimeFeedback()
    {
        Debug.Log("UI Feedback: Play time tracked");
        // Add UI animations, notifications, etc. here
        RefreshMissionDisplay();
    }

    private void HandleFacialDetectionFeedback()
    {
        Debug.Log("UI Feedback: Facial detection used");
        // Add UI animations, notifications, etc. here
        RefreshMissionDisplay();
    }

    private void HandleFunctionalSpeechFeedback()
    {
        Debug.Log("UI Feedback: Functional speech used");
        // Add UI animations, notifications, etc. here
        RefreshMissionDisplay();
    }

    private void SetupUI()
    {
        if (titleText != null)
            titleText.text = "Daily Mission";
    }

    private void CreateDefaultMissions()
    {
        currentMissions.Clear();

        // Create default missions with correct questTypes
        currentMissions.Add(new DailyMissionData("play_time_001", "play_time", 30, 10));
        currentMissions.Add(new DailyMissionData("facial_detection_001", "facial_detection", 5, 15));
        currentMissions.Add(new DailyMissionData("functional_speech_001", "functional_speech", 3, 20));

        CreateMissionUI();
    }

    private void CreateMissionUI()
    {
        // Clear existing mission items
        foreach (var item in missionItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        missionItems.Clear();

        // Create UI for each mission
        foreach (var mission in currentMissions)
        {
            CreateMissionItem(mission);
        }

        // Update star display after creating UI
        int completedCount = GetCompletedMissionsCount();
        UpdateStarDisplay(completedCount);
    }

    private void CreateMissionItem(DailyMissionData missionData)
    {
        if (missionItemPrefab == null || missionContainer == null) return;

        GameObject itemObj = Instantiate(missionItemPrefab, missionContainer);
        DailyMissionItem missionItem = itemObj.GetComponent<DailyMissionItem>();

        if (missionItem != null)
        {
            missionItem.SetupMission(missionData); // No claim callback needed
            missionItems.Add(missionItem);
        }
    }

    public void LoadMissionsFromJSON(string jsonData)
    {
        try
        {
            DailyMissionList missionList = JsonUtility.FromJson<DailyMissionList>(jsonData);
            currentMissions = new List<DailyMissionData>(missionList.dailyMission);
            CreateMissionUI();
            
            // Update star display based on completed missions
            int completedCount = GetCompletedMissionsCount();
            UpdateStarDisplay(completedCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load missions from JSON: " + e.Message);
        }
    }

    public string GetMissionsAsJSON()
    {
        DailyMissionList missionList = new DailyMissionList();
        missionList.dailyMission = currentMissions.ToArray();
        return JsonUtility.ToJson(missionList, true);
    }

    public List<DailyMissionData> GetCurrentMissions()
    {
        return new List<DailyMissionData>(currentMissions);
    }

    public int GetTotalClaimedPoints()
    {
        return currentMissions.Where(m => m.isCompleted).Sum(m => m.points);
    }

    public int GetTotalAvailablePoints()
    {
        return currentMissions.Sum(m => m.points);
    }

    public int GetCompletedMissionsCount()
    {
        return currentMissions.Count(m => m.isCompleted);
    }
    
    public void RefreshMissionDisplay()
    {
        // Refresh all mission items UI
        for (int i = 0; i < missionItems.Count && i < currentMissions.Count; i++)
        {
            if (missionItems[i] != null)
            {
                missionItems[i].SetupMission(currentMissions[i]);
            }
        }

        // Update star display
        int completedCount = GetCompletedMissionsCount();
        UpdateStarDisplay(completedCount);
        
        Debug.Log($"Mission display refreshed. Completed missions: {completedCount}/3");
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
    }

}

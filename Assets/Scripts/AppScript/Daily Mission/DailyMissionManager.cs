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
    public Button backButton;
    public Button settingsButton;
    public TextMeshProUGUI titleText;

    [Header("Mission Settings")]
    public DailyMissionData[] defaultMissions;

    private List<DailyMissionData> currentMissions;
    private List<DailyMissionItem> missionItems;

    // Events
    public static event System.Action<DailyMissionData> OnMissionCompleted;
    public static event System.Action<DailyMissionData> OnMissionClaimed;
    public static event System.Action<int> OnPointsEarned;

    private void Awake()
    {
        currentMissions = new List<DailyMissionData>();
        missionItems = new List<DailyMissionItem>();
    }

    private void Start()
    {
        InitializeMissions();
        SetupUI();
        LoadMissionProgress();
    }

    private void SetupUI()
    {
        if (titleText != null)
            titleText.text = "Daily Mission";

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
    }

    private void InitializeMissions()
    {
        // Load missions from JSON or create default missions
        string savedMissions = PlayerPrefs.GetString("DailyMissions", "");
        
        if (!string.IsNullOrEmpty(savedMissions))
        {
            try
            {
                DailyMissionList missionList = JsonUtility.FromJson<DailyMissionList>(savedMissions);
                currentMissions = new List<DailyMissionData>(missionList.dailyMission);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load saved missions: " + e.Message);
                CreateDefaultMissions();
            }
        }
        else
        {
            CreateDefaultMissions();
        }

        // Check if it's a new day and reset missions if needed
        CheckAndResetDailyMissions();
        
        CreateMissionUI();
    }

    private void CreateDefaultMissions()
    {
        currentMissions.Clear();
        
        // Create default missions based on the image
        currentMissions.Add(new DailyMissionData("facial_detection", "facial_detection", 2, 10));
        currentMissions.Add(new DailyMissionData("minigame_play", "minigame", 4, 15));
        currentMissions.Add(new DailyMissionData("productivity", "productivity", 15, 20));
        
        SaveMissionProgress();
    }

    private void CheckAndResetDailyMissions()
    {
        string lastResetDate = PlayerPrefs.GetString("LastMissionReset", "");
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastResetDate != todayDate)
        {
            // Reset all missions for new day
            foreach (var mission in currentMissions)
            {
                mission.ResetProgress();
                mission.questDate = DateTime.Now;
            }
            
            PlayerPrefs.SetString("LastMissionReset", todayDate);
            SaveMissionProgress();
        }
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
    }

    private void CreateMissionItem(DailyMissionData missionData)
    {
        if (missionItemPrefab == null || missionContainer == null) return;

        GameObject itemObj = Instantiate(missionItemPrefab, missionContainer);
        DailyMissionItem missionItem = itemObj.GetComponent<DailyMissionItem>();

        if (missionItem != null)
        {
            missionItem.SetupMission(missionData, OnMissionClaimedCallback);
            missionItems.Add(missionItem);
        }
    }

    public void UpdateMissionProgress(string questType, int amount = 1)
    {
        var mission = currentMissions.FirstOrDefault(m => m.questType == questType);
        if (mission != null)
        {
            bool wasCompleted = mission.isCompleted;
            mission.AddProgress(amount);
            
            // Update UI
            var missionItem = missionItems.FirstOrDefault(m => 
                m.transform.GetComponent<DailyMissionItem>() != null);
            
            // Find the correct mission item and update it
            foreach (var item in missionItems)
            {
                // Update all items to refresh UI
                item.UpdateProgress(mission.currentCount);
            }

            // Check if mission just completed
            if (!wasCompleted && mission.isCompleted)
            {
                OnMissionCompleted?.Invoke(mission);
                Debug.Log($"Mission {mission.questType} completed!");
            }

            SaveMissionProgress();
        }
    }

    private void OnMissionClaimedCallback(DailyMissionData mission)
    {
        if (mission.CanClaim())
        {
            OnMissionClaimed?.Invoke(mission);
            OnPointsEarned?.Invoke(mission.points);
            
            Debug.Log($"Mission claimed: {mission.questType}, Points earned: {mission.points}");
            
            // Here you can add logic to give rewards to player
            // For example: PlayerManager.Instance.AddPoints(mission.points);
        }
    }

    public void LoadMissionProgress()
    {
        // This method can be called to reload mission data from server
        // For now, it loads from PlayerPrefs
        string savedMissions = PlayerPrefs.GetString("DailyMissions", "");
        if (!string.IsNullOrEmpty(savedMissions))
        {
            try
            {
                DailyMissionList missionList = JsonUtility.FromJson<DailyMissionList>(savedMissions);
                currentMissions = new List<DailyMissionData>(missionList.dailyMission);
                CreateMissionUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load mission progress: " + e.Message);
            }
        }
    }

    public void SaveMissionProgress()
    {
        DailyMissionList missionList = new DailyMissionList();
        missionList.dailyMission = currentMissions.ToArray();
        
        string json = JsonUtility.ToJson(missionList, true);
        PlayerPrefs.SetString("DailyMissions", json);
        PlayerPrefs.Save();
    }

    public void LoadMissionsFromJSON(string jsonData)
    {
        try
        {
            DailyMissionList missionList = JsonUtility.FromJson<DailyMissionList>(jsonData);
            currentMissions = new List<DailyMissionData>(missionList.dailyMission);
            CreateMissionUI();
            SaveMissionProgress();
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

    private void GoBack()
    {
        // Add your back navigation logic here
        Debug.Log("Going back from Daily Mission");
        // For example: SceneManager.LoadScene("MainMenu");
    }

    private void OpenSettings()
    {
        // Add your settings logic here
        Debug.Log("Opening settings");
    }

    private void OnDestroy()
    {
        SaveMissionProgress();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveMissionProgress();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveMissionProgress();
        }
    }
}

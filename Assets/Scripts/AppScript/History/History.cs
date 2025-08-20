using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;

public class History : MonoBehaviour
{
    [Header("UI References")]
    public Transform historyContainer; // Parent object สำหรับ history items
    public GameObject historyItemPrefab; // Prefab ของ history item
    public ScrollRect scrollRect; // ScrollRect สำหรับ scroll
    public TextMeshProUGUI loadingText; // Text แสดงสถานะการโหลด
    public Button refreshButton; // ปุ่มรีเฟรช
    
    [Header("Settings")]
    public int maxHistoryItems = 10; // จำนวนสูงสุดของ history items
    public float itemSpacing = 200f; // เพิ่มระยะห่างระหว่าง items (เปลี่ยนจาก 10f เป็น 20f)
    
    private List<HistoryData> historyDataList = new List<HistoryData>();
    private List<HistoryItem> historyItemsList = new List<HistoryItem>();
    private bool isLoading = false;
    
    private void Start()
    {
        InitializeHistory();
        LoadHistoryFromServer();
    }
    
    private void InitializeHistory()
    {
        // Setup refresh button
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshHistory);
        
        // Setup container spacing
        if (historyContainer != null)
        {
            VerticalLayoutGroup layoutGroup = historyContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = itemSpacing;
                layoutGroup.childForceExpandHeight = false; // เพิ่มเพื่อไม่ให้ขยายเต็ม
            }
        }
        
        ShowLoadingState(false);
    }
    
    public void LoadHistoryFromServer()
    {
        if (isLoading) return;
        
        StartCoroutine(LoadHistoryCoroutine());
    }
    
    private IEnumerator LoadHistoryCoroutine()
    {
        isLoading = true;
        ShowLoadingState(true);
        
        // Call actual API
        FetchHistory(); // No yield return needed
        
        // Loading state will be managed in the callbacks
        yield break; // Required for IEnumerator
    }
    
    private List<HistoryData> GetSampleHistoryData()
    {
        // Sample data ตามรูปแบบใหม่
        List<HistoryData> sampleData = new List<HistoryData>
        {
            new HistoryData("Facial Detection", "Level 1", "20/04/25", "Great Job!", "Main Game", 5),
            new HistoryData("Functional Speech", "Level 2", "20/04/25", "Great!", "Main Game", 4),
            new HistoryData("Phoneme Practice", "Level 3", "20/04/25", "Excellent!", "Main Game", 5),
            new HistoryData("Language Therapy", "Custom Level", "19/04/25", "Amazing!", "Custom Game", 5),
            new HistoryData("Facial Detection", "Level 5", "18/04/25", "Good work!", "Main Game", 3)
        };
        
        return sampleData;
    }
    
    // Method สำหรับเรียก API จริง - now using void
    private void FetchHistory()
    {
        string authToken = PlayerPrefs.GetString("authToken", string.Empty);
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("No authentication token found. Using sample data.");
            LoadSampleData();
            FinishLoading(); // Handle loading state here
            return;
        }

        var headers = new Dictionary<string, string>
        {
            {"Authorization", "Bearer " + authToken }
        };

        StartCoroutine(HttpHelper.GetRequestCoroutine<ApiHistoryResponse>(
            "https://api.mystrokeapi.uk/user/history?page=1&limit=10",
            onSuccess: (response) => {
                Debug.Log("History data received successfully.");
                List<HistoryData> historyDataList = ConvertApiResponseToHistoryData(response.history);
                UpdateHistoryDisplay(historyDataList);
                FinishLoading(); // Handle loading state here
            },
            onError: (error, code) => {
                Debug.LogError($"Failed to fetch history data: {error} (Code: {code})");
                LoadSampleData(); // Fallback to sample data
                FinishLoading(); // Handle loading state here
            },
            additionalHeaders: headers
        ));
    }
    
    private void FinishLoading()
    {
        isLoading = false;
        ShowLoadingState(false);
    }
    
    private void LoadSampleData()
    {
        List<HistoryData> sampleData = GetSampleHistoryData();
        UpdateHistoryDisplay(sampleData);
    }
    
    public void UpdateHistoryDisplay(List<HistoryData> newHistoryData)
    {
        // Clear existing items
        ClearHistoryItems();
        
        // Store new data
        historyDataList = new List<HistoryData>(newHistoryData);
        
        // Limit items if needed
        if (historyDataList.Count > maxHistoryItems)
        {
            historyDataList = historyDataList.GetRange(0, maxHistoryItems);
        }
        
        // Create new items
        CreateHistoryItems();
        
        // Update spacing after creating items
        UpdateItemSpacing();
        
        // Scroll to top
        if (scrollRect != null)
            StartCoroutine(ScrollToTop());
    }
    
    private void CreateHistoryItems()
    {
        if (historyItemPrefab == null || historyContainer == null) return;
        
        foreach (HistoryData data in historyDataList)
        {
            GameObject itemObject = Instantiate(historyItemPrefab, historyContainer);
            HistoryItem historyItem = itemObject.GetComponent<HistoryItem>();
            
            if (historyItem != null)
            {
                historyItem.SetupHistoryItem(data);
                historyItemsList.Add(historyItem);
            }
        }
    }
    
    private void ClearHistoryItems()
    {
        historyItemsList.Clear();
        
        if (historyContainer != null)
        {
            for (int i = historyContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(historyContainer.GetChild(i).gameObject);
            }
        }
    }
    
    private IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void ShowLoadingState(bool show)
    {
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(show);
            loadingText.text = show ? "Loading History ..." : "";
        }
        
        if (refreshButton != null)
            refreshButton.interactable = !show;
    }
    
    public void RefreshHistory()
    {
        LoadHistoryFromServer();
    }
    
    // Method เพื่อแก้ไขระยะห่างแบบทันที (เรียกใช้จาก Inspector หรือ script อื่น)
    [ContextMenu("Update Item Spacing")]
    public void UpdateItemSpacing()
    {
        if (historyContainer != null)
        {
            VerticalLayoutGroup layoutGroup = historyContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = itemSpacing;
                // บังคับให้ layout group อัปเดตทันที
                LayoutRebuilder.ForceRebuildLayoutImmediate(historyContainer.GetComponent<RectTransform>());
            }
        }
    }
    
    // Method เพื่อทดสอบสร้าง history item (เรียกใช้จาก Inspector)
    [ContextMenu("Test Create Sample Data")]
    public void TestCreateSampleData()
    {
        LoadSampleData();
    }
    
    public void AddNewHistoryItem(HistoryData newData)
    {
        historyDataList.Insert(0, newData); // เพิ่มที่ด้านบน
        
        // Remove excess items
        if (historyDataList.Count > maxHistoryItems)
            historyDataList.RemoveAt(historyDataList.Count - 1);
        
        UpdateHistoryDisplay(historyDataList);
    }
    
    // Helper method สำหรับเพิ่ม history item แบบง่าย
    public void AddNewHistoryItem(string gameType, string levelName, int score, bool isCustom = false)
    {
        string gameCategory = HistoryData.GetGameCategoryFromType(gameType);
        string date = DateTime.Now.ToString("dd/MM/yy");
        string compliment = GetComplimentFromScore(score);
        string type = isCustom ? "Custom Game" : "Main Game";
        
        HistoryData newData = new HistoryData(gameCategory, levelName, date, compliment, type, score);
        AddNewHistoryItem(newData);
    }
    
    public int GetHistoryCount()
    {
        return historyDataList.Count;
    }
    
    public List<HistoryData> GetHistoryData()
    {
        return new List<HistoryData>(historyDataList);
    }

    private List<HistoryData> ConvertApiResponseToHistoryData(ApiHistoryItem[] apiItems)
    {
        List<HistoryData> historyDataList = new List<HistoryData>();
        
        foreach (var item in apiItems)
        {
            // Convert API response to HistoryData format
            string gameCategory = HistoryData.GetGameCategoryFromType(item.type);
            string levelName = item.name ?? "Unknown Level";
            string date = FormatDate(item.completedAt);
            string complimentText = GetComplimentFromScore(item.score);
            string gameType = item.isCustom ? "Custom Game" : "Main Game";
            int starCount = Mathf.Clamp(item.score, 0, 5);
            
            HistoryData historyData = new HistoryData(gameCategory, levelName, date, complimentText, gameType, starCount);
            historyDataList.Add(historyData);
        }
        
        return historyDataList;
    }

    private string FormatDate(string completedAt)
    {
        if (string.IsNullOrEmpty(completedAt))
            return DateTime.Now.ToString("dd/MM/yy");
        
        try
        {
            DateTime dateTime = DateTime.Parse(completedAt);
            return dateTime.ToString("dd/MM/yy");
        }
        catch
        {
            return DateTime.Now.ToString("dd/MM/yy");
        }
    }

    private string GetComplimentFromScore(int score)
    {
        string[] compliments = {
            "Excellent!", "Outstanding!", "Superb!", "Fantastic!", "Brilliant!",
            "Great Job!", "Well done!", "Impressive!", "Nice work!", "Very good!",
            "Good work!", "Solid effort!", "Keep it up!", "Nice progress!", "Well played!",
            "Nice try!", "Getting better!", "Keep practicing!", "Don't give up!", "Almost there!",
            "Keep trying!", "You can do it!", "Stay motivated!", "Perfect!", "Keep going!",
            "Good effort!", "Keep learning!", "Stay positive!", "Try again!", "Keep improving!"
        };

        System.Random rand = new System.Random();
        return compliments[rand.Next(compliments.Length)];
    }
}

// Class สำหรับ JSON response จาก API
[Serializable]
public class ApiHistoryResponse
{
    public ApiHistoryItem[] history;
    public int rowCount;
}

[Serializable]
public class ApiHistoryItem
{
    public string id;
    public string name;           // ชื่อด่าน/level
    public string description;    // ไม่ใช้
    public string levelId;
    public string type;           // ประเภทเกม (facial, functional, phoneme, language)
    public string subtype;        // ไม่ใช้
    public bool isCustom;         // เป็น custom game หรือไม่
    public int score;             // คะแนน (1-5)
    public string completedAt;    // วันที่เล่น
}

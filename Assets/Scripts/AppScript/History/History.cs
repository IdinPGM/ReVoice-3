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
    public float itemSpacing = 10f; // ระยะห่างระหว่าง items
    
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
                layoutGroup.spacing = itemSpacing;
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
        // Sample data ตามรูปที่แนบมา
        List<HistoryData> sampleData = new List<HistoryData>
        {
            new HistoryData("Minigame - A", "20/04/25", "Great Job!", false, 5),
            new HistoryData("Minigame - A", "20/04/25", "Great!", false, 4),
            new HistoryData("Minigame - A", "20/04/25", "Excellent!", false, 5),
            new HistoryData("Custom Game", "19/04/25", "Amazing!", true, 5),
            new HistoryData("Minigame - B", "18/04/25", "Good work!", false, 3)
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
            loadingText.text = show ? "Loading..." : "";
        }
        
        if (refreshButton != null)
            refreshButton.interactable = !show;
    }
    
    public void RefreshHistory()
    {
        LoadHistoryFromServer();
    }
    
    public void AddNewHistoryItem(HistoryData newData)
    {
        historyDataList.Insert(0, newData); // เพิ่มที่ด้านบน
        
        // Remove excess items
        if (historyDataList.Count > maxHistoryItems)
            historyDataList.RemoveAt(historyDataList.Count - 1);
        
        UpdateHistoryDisplay(historyDataList);
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
            string gameCategory = item.name ?? "Unknown Game";
            string date = FormatDate(item.completedAt);
            string complimentText = GetComplimentFromScore(item.score);
            bool isCustom = item.isCustom;
            int starCount = Mathf.Clamp(item.score, 0, 5);
            
            HistoryData historyData = new HistoryData(gameCategory, date, complimentText, isCustom, starCount);
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
        switch (score)
        {
            case 5: return "Excellent!";
            case 4: return "Great Job!";
            case 3: return "Good work!";
            case 2: return "Nice try!";
            case 1: return "Keep trying!";
            default: return "Good effort!";
        }
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
    public string name;
    public string description;
    public string levelId;
    public string type;
    public string subtype;
    public bool isCustom;
    public int score;
    public string completedAt;
}

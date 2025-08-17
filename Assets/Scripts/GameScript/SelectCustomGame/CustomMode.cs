// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// using TMPro;
// using System;

// public class CustomMode : MonoBehaviour
// {
//     [Header("UI References")]
//     public Transform gamesContainer; // Parent object สำหรับ custom game items
//     public GameObject customGameItemPrefab; // Prefab ของ custom game item
//     public ScrollRect scrollRect; // ScrollRect สำหรับ scroll
//     public TextMeshProUGUI loadingText; // Text แสดงสถานะการโหลด
//     public Button refreshButton; // ปุ่มรีเฟรช
//     public TextMeshProUGUI titleText; // Text หัวข้อ "Custom Games"
//     public TextMeshProUGUI noGamesText; // Text แสดงเมื่อไม่มีเกม
    
//     [Header("Settings")]
//     public int maxGamesPerPage = 10; // จำนวนสูงสุดของเกมต่อหน้า
//     public float itemSpacing = 10f; // ระยะห่างระหว่าง items
//     public string apiUrl = "https://api.mystrokeapi.uk/game/custom-levels"; // URL ของ API
    
//     [Header("Filter Settings")]
//     public TMP_Dropdown typeDropdown; // Dropdown สำหรับเลือก type
//     public TMP_Dropdown subtypeDropdown; // Dropdown สำหรับเลือก subtype
//     public Button filterButton; // ปุ่มกรอง
    
//     private List<CustomGameData> gamesDataList = new List<CustomGameData>();
//     private List<CustomGameItem> gameItemsList = new List<CustomGameItem>();
//     private bool isLoading = false;
//     private int currentPage = 1;
//     private string currentType = "";
//     private string currentSubtype = "";
    
//     private void Start()
//     {
//         InitializeCustomMode();
//         LoadCustomGamesFromServer();
//     }
    
//     private void InitializeCustomMode()
//     {
//         // Setup refresh button
//         if (refreshButton != null)
//             refreshButton.onClick.AddListener(RefreshCustomGames);
        
//         // Setup filter button
//         if (filterButton != null)
//             filterButton.onClick.AddListener(ApplyFilters);
        
//         // Setup container spacing
//         if (gamesContainer != null)
//         {
//             VerticalLayoutGroup layoutGroup = gamesContainer.GetComponent<VerticalLayoutGroup>();
//             if (layoutGroup != null)
//                 layoutGroup.spacing = itemSpacing;
//         }
        
//         // Setup dropdowns
//         InitializeDropdowns();
        
//         // Set title
//         if (titleText != null)
//             titleText.text = "Custom Games";
        
//         ShowLoadingState(false);
//         ShowNoGamesMessage(false);
//     }
    
//     private void InitializeDropdowns()
//     {
//         // Initialize type dropdown
//         if (typeDropdown != null)
//         {
//             typeDropdown.ClearOptions();
//             List<string> typeOptions = new List<string>
//             {
//                 "All Types",
//                 "Facial Detection",
//                 "Language Therapy", 
//                 "Phoneme Practice",
//                 "Functional Speech"
//             };
//             typeDropdown.AddOptions(typeOptions);
//             typeDropdown.onValueChanged.AddListener(OnTypeChanged);
//         }
        
//         // Initialize subtype dropdown
//         if (subtypeDropdown != null)
//         {
//             subtypeDropdown.ClearOptions();
//             List<string> subtypeOptions = new List<string>
//             {
//                 "All Subtypes",
//                 "A",
//                 "B"
//             };
//             subtypeDropdown.AddOptions(subtypeOptions);
//             subtypeDropdown.onValueChanged.AddListener(OnSubtypeChanged);
//         }
//     }
    
//     private void OnTypeChanged(int index)
//     {
//         if (typeDropdown != null && index >= 0 && index < typeDropdown.options.Count)
//         {
//             string selectedType = typeDropdown.options[index].text;
//             currentType = selectedType == "All Types" ? "" : selectedType;
//         }
//     }
    
//     private void OnSubtypeChanged(int index)
//     {
//         if (subtypeDropdown != null && index >= 0 && index < subtypeDropdown.options.Count)
//         {
//             string selectedSubtype = subtypeDropdown.options[index].text;
//             currentSubtype = selectedSubtype == "All Subtypes" ? "" : selectedSubtype;
//         }
//     }
    
//     private void ApplyFilters()
//     {
//         currentPage = 1; // Reset to first page when applying filters
//         LoadCustomGamesFromServer();
//     }
    
//     public void LoadCustomGamesFromServer()
//     {
//         if (isLoading) return;
        
//         StartCoroutine(LoadCustomGamesCoroutine());
//     }
    
//     private IEnumerator LoadCustomGamesCoroutine()
//     {
//         isLoading = true;
//         ShowLoadingState(true);
//         ShowNoGamesMessage(false);
        
//         // Call actual API
//         FetchCustomGames();
        
//         yield break; // Required for IEnumerator
//     }
    
//     private void FetchCustomGames()
//     {
//         string authToken = PlayerPrefs.GetString("authToken", string.Empty);
//         if (string.IsNullOrEmpty(authToken))
//         {
//             Debug.LogWarning("No authentication token found. Using sample data.");
//             LoadSampleData();
//             FinishLoading();
//             return;
//         }
        
//         // Build API URL with parameters
//         string url = BuildApiUrl();
        
//         var headers = new Dictionary<string, string>
//         {
//             {"Authorization", "Bearer " + authToken }
//         };
        
//         Debug.Log($"Fetching custom games from: {url}");
        
//         StartCoroutine(HttpHelper.GetRequestCoroutine<ApiCustomGameResponse>(
//             url,
//             onSuccess: (response) => {
//                 Debug.Log("Custom games data received successfully.");
//                 List<CustomGameData> gamesData = ConvertApiResponseToCustomGameData(response.data);
//                 UpdateGamesDisplay(gamesData);
//                 FinishLoading();
//             },
//             onError: (error, code) => {
//                 Debug.LogError($"Failed to fetch custom games data: {error} (Code: {code})");
//                 LoadSampleData(); // Fallback to sample data
//                 FinishLoading();
//             },
//             additionalHeaders: headers
//         ));
//     }
    
//     private string BuildApiUrl()
//     {
//         string url = $"{apiUrl}?page={currentPage}&limit={maxGamesPerPage}";
        
//         if (!string.IsNullOrEmpty(currentType))
//             url += $"&type={UnityWebRequest.EscapeURL(currentType)}";
            
//         if (!string.IsNullOrEmpty(currentSubtype))
//             url += $"&subtype={UnityWebRequest.EscapeURL(currentSubtype)}";
        
//         return url;
//     }
    
//     private List<CustomGameData> ConvertApiResponseToCustomGameData(CustomGameApiData[] apiData)
//     {
//         List<CustomGameData> gamesList = new List<CustomGameData>();
        
//         if (apiData != null)
//         {
//             foreach (var apiGame in apiData)
//             {
//                 CustomGameData gameData = new CustomGameData(
//                     apiGame.id,
//                     apiGame.name,
//                     apiGame.created_at,
//                     apiGame.type,
//                     apiGame.subtype,
//                     apiGame.description,
//                     apiGame.image_url
//                 );
                
//                 gamesList.Add(gameData);
//             }
//         }
        
//         return gamesList;
//     }
    
//     private void LoadSampleData()
//     {
//         List<CustomGameData> sampleData = GetSampleCustomGamesData();
//         UpdateGamesDisplay(sampleData);
//     }
    
//     private List<CustomGameData> GetSampleCustomGamesData()
//     {
//         // Sample data ตามรูปที่แนบมา
//         List<CustomGameData> sampleData = new List<CustomGameData>
//         {
//             new CustomGameData("1", "มินิเกมเฉพาะ - B", "2025-04-21", "Facial Detection", "B"),
//             new CustomGameData("2", "มินิเกมเฉพาะ - A", "2025-04-18", "Language Therapy", "A")
//         };
        
//         return sampleData;
//     }
    
//     public void UpdateGamesDisplay(List<CustomGameData> newGamesData)
//     {
//         // Clear existing items
//         ClearGameItems();
        
//         // Store new data
//         gamesDataList = new List<CustomGameData>(newGamesData);
        
//         // Create new items
//         CreateGameItems();
        
//         // Show no games message if empty
//         ShowNoGamesMessage(gamesDataList.Count == 0);
        
//         // Scroll to top
//         if (scrollRect != null)
//             StartCoroutine(ScrollToTop());
//     }
    
//     private void CreateGameItems()
//     {
//         if (customGameItemPrefab == null || gamesContainer == null)
//         {
//             Debug.LogError("Custom game item prefab or games container is not assigned!");
//             return;
//         }
        
//         foreach (CustomGameData gameData in gamesDataList)
//         {
//             GameObject itemObj = Instantiate(customGameItemPrefab, gamesContainer);
//             CustomGameItem gameItem = itemObj.GetComponent<CustomGameItem>();
            
//             if (gameItem != null)
//             {
//                 gameItem.SetupCustomGameItem(gameData, OnGameSelected);
//                 gameItemsList.Add(gameItem);
//             }
//             else
//             {
//                 Debug.LogError("CustomGameItem component not found on prefab!");
//             }
//         }
//     }
    
//     private void ClearGameItems()
//     {
//         foreach (CustomGameItem item in gameItemsList)
//         {
//             if (item != null && item.gameObject != null)
//                 DestroyImmediate(item.gameObject);
//         }
        
//         gameItemsList.Clear();
//     }
    
//     private void OnGameSelected(CustomGameData gameData)
//     {
//         Debug.Log($"Game selected: {gameData.name} (Type: {gameData.type}, Subtype: {gameData.subtype})");
        
//         // Additional logic when a game is selected
//         // This method is called from CustomGameItem when play button is clicked
//     }
    
//     private IEnumerator ScrollToTop()
//     {
//         yield return new WaitForEndOfFrame();
//         if (scrollRect != null)
//             scrollRect.verticalNormalizedPosition = 1f;
//     }
    
//     private void ShowLoadingState(bool show)
//     {
//         isLoading = show;
        
//         if (loadingText != null)
//         {
//             loadingText.gameObject.SetActive(show);
//             if (show)
//                 loadingText.text = "Loading custom games...";
//         }
        
//         if (refreshButton != null)
//             refreshButton.interactable = !show;
            
//         if (filterButton != null)
//             filterButton.interactable = !show;
//     }
    
//     private void ShowNoGamesMessage(bool show)
//     {
//         if (noGamesText != null)
//         {
//             noGamesText.gameObject.SetActive(show);
//             if (show)
//                 noGamesText.text = "No custom games found.\nTry adjusting your filters or create a new custom game.";
//         }
//     }
    
//     private void FinishLoading()
//     {
//         isLoading = false;
//         ShowLoadingState(false);
//     }
    
//     public void RefreshCustomGames()
//     {
//         currentPage = 1; // Reset to first page
//         LoadCustomGamesFromServer();
//     }
    
//     // Method สำหรับโหลดหน้าถัดไป (สามารถเรียกใช้จาก UI)
//     public void LoadNextPage()
//     {
//         if (!isLoading)
//         {
//             currentPage++;
//             LoadCustomGamesFromServer();
//         }
//     }
    
//     // Method สำหรับโหลดหน้าก่อนหน้า (สามารถเรียกใช้จาก UI)
//     public void LoadPreviousPage()
//     {
//         if (!isLoading && currentPage > 1)
//         {
//             currentPage--;
//             LoadCustomGamesFromServer();
//         }
//     }
    
//     private void OnDestroy()
//     {
//         // Clean up event listeners
//         if (refreshButton != null)
//             refreshButton.onClick.RemoveListener(RefreshCustomGames);
            
//         if (filterButton != null)
//             filterButton.onClick.RemoveListener(ApplyFilters);
            
//         if (typeDropdown != null)
//             typeDropdown.onValueChanged.RemoveListener(OnTypeChanged);
            
//         if (subtypeDropdown != null)
//             subtypeDropdown.onValueChanged.RemoveListener(OnSubtypeChanged);
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;

public class CustomGameItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI gameNameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI gameTypeText;
    public TextMeshProUGUI gameSubtypeText;
    
    [Header("Visual Settings")]
    public Sprite defaultBackgroundSprite;
    public Color defaultBackgroundColor = Color.white;
    
    [Header("Click Feedback (Optional)")]
    public Color clickHighlightColor = new Color(1f, 1f, 1f, 0.8f);
    
    [Header("Game Type Background Images")]
    public Sprite facialDetectionBackground;
    public Sprite languageTherapyBackground;
    public Sprite phonemePracticeBackground;
    public Sprite functionalSpeechBackground;
    
    private CustomGameData gameData;
    private Action<CustomGameData> onPlayCallback;

    [System.Serializable]
    public class Stage
    {
        public int number;
        public string target;
        public string description;
        public string image;
    }

    [System.Serializable]
    public class StageDataWrapper
    {
        public Stage[] stages;
    }

    [System.Serializable]
    public class SessionStartResponse
    {
        public string sessionId;
        public Stage[] stage;
    }

    [System.Serializable]
    public class SessionStartRequest
    {
        public string levelId;
        public bool isCustom;
    }

    [System.Serializable]
    public class Level
    {
        public string id;
        public string type;
        public string subtype;
        public string name;
        public string description;
    }

    [System.Serializable]
    public class GetLevelResponse
    {
        public Level[] levels;

    }
    
    public void SetupCustomGameItem(CustomGameData data, Action<CustomGameData> playCallback = null)
    {
        gameData = data;
        onPlayCallback = playCallback;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (gameData == null) return;
        
        // Set text values
        if (gameNameText != null)
            gameNameText.text = gameData.name;
            
        if (dateText != null)
            dateText.text = gameData.GetFormattedDate();
        
        // แสดง Game Type แยกต่างหาก
        if (gameTypeText != null)
            gameTypeText.text = gameData.GetDisplayType();
            
        // แสดง Game Subtype แยกต่างหาก
        if (gameSubtypeText != null)
        {
            string subtypeDisplay = gameData.GetDisplaySubtype();
            gameSubtypeText.text = string.IsNullOrEmpty(subtypeDisplay) ? "" : subtypeDisplay;
            // ซ่อน subtype text ถ้าไม่มีข้อมูล
            gameSubtypeText.gameObject.SetActive(!string.IsNullOrEmpty(subtypeDisplay));
        }
        
        // Set background based on game type
        if (backgroundImage != null)
        {
            Sprite backgroundToUse = GetBackgroundSpriteByType(gameData.type);
            if (backgroundToUse != null)
            {
                backgroundImage.sprite = backgroundToUse;
            }
            else if (defaultBackgroundSprite != null)
            {
                backgroundImage.sprite = defaultBackgroundSprite;
            }
            
            backgroundImage.color = defaultBackgroundColor;
        }
    }
    
    private Sprite GetBackgroundSpriteByType(string gameType)
    {
        if (string.IsNullOrEmpty(gameType)) return null;
        
        switch (gameType.ToLower())
        {
            case "facial_detection":
            case "facial detection":
            case "facialdetection":
                return facialDetectionBackground;
                
            case "language_therapy":
            case "language therapy":
            case "languagetherapy":
                return languageTherapyBackground;

            case "phoneme_practice":
            case "phoneme practice":
            case "phonemepractice":
                return phonemePracticeBackground;

            case "functional_speech":
            case "functional speech":
            case "functionalspeech":
                return functionalSpeechBackground;
                
            default:
                return null;
        }
    }
    
    private IEnumerator LoadBackgroundImageFromUrl(string imageUrl)
    {
        if (backgroundImage == null || string.IsNullOrEmpty(imageUrl))
            yield break;
            
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    if (texture != null)
                    {
                        // สร้าง Sprite จาก Texture2D
                        Sprite sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f)
                        );
                        
                        // Only apply if no game-type-specific background is available
                        if (GetBackgroundSpriteByType(gameData?.type) == null)
                        {
                            backgroundImage.sprite = sprite;
                            backgroundImage.color = Color.white; // Reset color to show image properly
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to create sprite from downloaded image: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load background image: {www.error}");
            }
        }
    }
    
    private void OnPlayButtonClicked()
    {
        if (gameData != null)
        {
            Debug.Log($"Playing custom game: {gameData.name} (ID: {gameData.id})");
            
            // Call the callback if provided
            onPlayCallback?.Invoke(gameData);

            // Start the level with the game type
            if (!string.IsNullOrEmpty(gameData.id) && !string.IsNullOrEmpty(gameData.type))
            {
                StartCustomLevel(gameData.id, gameData.type);
            }
            else
            {
                Debug.LogError("Game ID or type is null or empty. Cannot start level.");
            }
        }
    }
    
    private void StartCustomLevel(string levelId, string gameType)
    {
        PlayerPrefs.SetString("levelId", levelId);
        PlayerPrefs.Save();
        Debug.Log($"Level ID set to: {levelId}");

        // Prepare request data
        var requestData = new SessionStartRequest
        {
            levelId = levelId,
            isCustom = true // Assuming this is a custom level
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<SessionStartRequest, SessionStartResponse>(
            "https://api.mystrokeapi.uk/game/session/start",
            requestData,
            onSuccess: (response) =>
            {
                // Save sessionId to PlayerPrefs
                PlayerPrefs.SetString("sessionId", response.sessionId);
                PlayerPrefs.Save();
                Debug.Log($"Session started with ID: {response.sessionId}");

                // save stage data as JSON
                string stageDataJson = JsonUtility.ToJson(new StageDataWrapper { stages = response.stage });
                PlayerPrefs.SetString("stageData", stageDataJson);
                PlayerPrefs.Save();
                Debug.Log($"Stage data saved as JSON: {stageDataJson}");

                // Set stage number in PlayerPrefs
                if (response.stage.Length > 0)
                {
                    PlayerPrefs.SetInt("stageNumber", response.stage[0].number);
                    PlayerPrefs.Save();
                    Debug.Log($"Stage number set to: {response.stage[0].number}");
                }

                // Load the game scene
                string sceneName = GetSceneNameFromGameType(gameType);
                Debug.Log($"Loading scene: {sceneName} for game type: {gameType}");
                SceneManager.LoadScene(sceneName);
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Failed to start session: {error} (Code: {code})");
            },
            additionalHeaders: new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + PlayerPrefs.GetString("authToken", string.Empty) }
            }
        ));
    }

    private Dictionary<string, string> gameTypeToSceneMap = new Dictionary<string, string>
    {   
        {"facial_detection", "Facial Detection"},
        { "functional_speech", "Functional Speech"},
        { "phoneme_practice", "Phoneme Practice"},
        {"language_therapy", "Language Therapy"},
    };

    private string GetSceneNameFromGameType(string gameType)
    {
        string lowerGameType = gameType.ToLower();
        
        if (gameTypeToSceneMap.ContainsKey(lowerGameType))
        {
            return gameTypeToSceneMap[lowerGameType];
        }
        
        Debug.LogWarning($"Unknown game type: {gameType}. Defaulting to Home.");
        return "Home"; // Default scene if game type is unknown
    }

    // Implementation สำหรับ IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"=== CustomGameItem OnPointerClick triggered ===");
        Debug.Log($"Game item clicked: {gameData?.name} (ID: {gameData?.id})");
        Debug.Log($"Game type: {gameData?.type}, Subtype: {gameData?.subtype}");
        
        if (gameData == null)
        {
            Debug.LogError("gameData is null! Cannot navigate to game.");
            return;
        }
        
        // เพิ่ม visual feedback (optional)
        StartCoroutine(ClickFeedback());

        // เรียก game logic
        Debug.Log("Calling OnPlayButtonClicked()...");
        OnPlayButtonClicked();
    }
    
    // Coroutine สำหรับ visual feedback เมื่อกด
    private IEnumerator ClickFeedback()
    {
        if (backgroundImage != null)
        {
            Color originalColor = backgroundImage.color;
            backgroundImage.color = clickHighlightColor;
            yield return new WaitForSeconds(0.1f);
            backgroundImage.color = originalColor;
        }
    }
}

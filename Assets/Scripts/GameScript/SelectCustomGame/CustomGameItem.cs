// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// using TMPro;
// using System.Collections;
// using System;

// public class CustomGameItem : MonoBehaviour
// {
//     [Header("UI References")]
//     public Image backgroundImage;
//     public TextMeshProUGUI gameNameText;
//     public TextMeshProUGUI dateText;
//     public TextMeshProUGUI categoryText;
//     public Button playButton;
    
//     [Header("Visual Settings")]
//     public Sprite defaultBackgroundSprite;
//     public Color defaultBackgroundColor = Color.white;
    
//     private CustomGameData gameData;
//     private Action<CustomGameData> onPlayCallback;
    
//     private void Awake()
//     {
//         // Setup play button
//         if (playButton != null)
//             playButton.onClick.AddListener(OnPlayButtonClicked);
//     }
    
//     public void SetupCustomGameItem(CustomGameData data, Action<CustomGameData> playCallback = null)
//     {
//         gameData = data;
//         onPlayCallback = playCallback;
//         UpdateUI();
        
//         // Load background image if URL is provided
//         if (!string.IsNullOrEmpty(data.backgroundImageUrl))
//         {
//             StartCoroutine(LoadBackgroundImage(data.backgroundImageUrl));
//         }
//     }
    
//     private void UpdateUI()
//     {
//         if (gameData == null) return;
        
//         // Set text values
//         if (gameNameText != null)
//             gameNameText.text = gameData.name;
            
//         if (dateText != null)
//             dateText.text = gameData.GetFormattedDate();
            
//         if (categoryText != null)
//             categoryText.text = gameData.category;
        
//         // Set default background
//         if (backgroundImage != null)
//         {
//             if (defaultBackgroundSprite != null)
//                 backgroundImage.sprite = defaultBackgroundSprite;
//             backgroundImage.color = defaultBackgroundColor;
//         }
//     }
    
//     private IEnumerator LoadBackgroundImage(string imageUrl)
//     {
//         if (backgroundImage == null || string.IsNullOrEmpty(imageUrl))
//             yield break;
            
//         using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
//         {
//             yield return www.SendWebRequest();
            
//             if (www.result == UnityWebRequest.Result.Success)
//             {
//                 try
//                 {
//                     Texture2D texture = DownloadHandlerTexture.GetContent(www);
//                     if (texture != null)
//                     {
//                         // สร้าง Sprite จาก Texture2D
//                         Sprite sprite = Sprite.Create(
//                             texture,
//                             new Rect(0, 0, texture.width, texture.height),
//                             new Vector2(0.5f, 0.5f)
//                         );
                        
//                         backgroundImage.sprite = sprite;
//                         backgroundImage.color = Color.white; // Reset color to show image properly
//                     }
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogWarning($"Failed to create sprite from downloaded image: {e.Message}");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning($"Failed to load background image: {www.error}");
//             }
//         }
//     }
    
//     private void OnPlayButtonClicked()
//     {
//         if (gameData != null)
//         {
//             Debug.Log($"Playing custom game: {gameData.name} (ID: {gameData.id})");
            
//             // Call the callback if provided
//             onPlayCallback?.Invoke(gameData);
            
//             // Navigate to the appropriate game based on type/subtype
//             NavigateToGame();
//         }
//     }
    
//     private void NavigateToGame()
//     {
//         if (gameData == null) return;
        
//         // Navigate based on game type and subtype
//         switch (gameData.type.ToLower())
//         {
//             case "facial detection":
//             case "facialdetection":
//                 NavigateToFacialDetection();
//                 break;
                
//             case "language therapy":
//             case "languagetherapy":
//                 NavigateToLanguageTherapy();
//                 break;
                
//             case "phoneme practice":
//             case "phonemepractice":
//                 NavigateToPhonemePractice();
//                 break;
                
//             case "functional speech":
//             case "functionalspeech":
//                 NavigateToFunctionalSpeech();
//                 break;
                
//             default:
//                 Debug.LogWarning($"Unknown game type: {gameData.type}");
//                 NavigateToDefaultGame();
//                 break;
//         }
//     }
    
//     private void NavigateToFacialDetection()
//     {
//         Debug.Log("Navigating to Facial Detection game...");
//         // Store custom game data for the game to use
//         PlayerPrefs.SetString("CustomGameId", gameData.id);
//         PlayerPrefs.SetString("CustomGameName", gameData.name);
//         PlayerPrefs.SetString("CustomGameType", gameData.type);
//         PlayerPrefs.SetString("CustomGameSubtype", gameData.subtype);
        
//         // Load the Facial Detection scene or activate the appropriate game object
//         // Example: UnityEngine.SceneManagement.SceneManager.LoadScene("FacialDetectionScene");
        
//         // หรือถ้าใช้ระบบ GameObject activation
//         FacialDetection facialDetection = FindObjectOfType<FacialDetection>();
//         if (facialDetection != null)
//         {
//             facialDetection.StartCustomGame(gameData);
//         }
//     }
    
//     private void NavigateToLanguageTherapy()
//     {
//         Debug.Log("Navigating to Language Therapy game...");
//         PlayerPrefs.SetString("CustomGameId", gameData.id);
//         PlayerPrefs.SetString("CustomGameName", gameData.name);
//         PlayerPrefs.SetString("CustomGameType", gameData.type);
//         PlayerPrefs.SetString("CustomGameSubtype", gameData.subtype);
        
//         LanguageTherapy languageTherapy = FindObjectOfType<LanguageTherapy>();
//         if (languageTherapy != null)
//         {
//             languageTherapy.StartCustomGame(gameData);
//         }
//     }
    
//     private void NavigateToPhonemePractice()
//     {
//         Debug.Log("Navigating to Phoneme Practice game...");
//         PlayerPrefs.SetString("CustomGameId", gameData.id);
//         PlayerPrefs.SetString("CustomGameName", gameData.name);
//         PlayerPrefs.SetString("CustomGameType", gameData.type);
//         PlayerPrefs.SetString("CustomGameSubtype", gameData.subtype);

//         PhonemePractice phonemePractice = FindObjectOfType<PhonemePractice>();
//         if (phonemePractice != null)
//         {
//             phonemePractice.StartCustomGame(gameData);
//         }
//     }
    
//     private void NavigateToFunctionalSpeech()
//     {
//         Debug.Log("Navigating to Functional Speech game...");
//         PlayerPrefs.SetString("CustomGameId", gameData.id);
//         PlayerPrefs.SetString("CustomGameName", gameData.name);
//         PlayerPrefs.SetString("CustomGameType", gameData.type);
//         PlayerPrefs.SetString("CustomGameSubtype", gameData.subtype);
        
//         FunctionalSpeech functionalSpeech = FindObjectOfType<FunctionalSpeech>();
//         if (functionalSpeech != null)
//         {
//             functionalSpeech.StartCustomGame(gameData);
//         }
//     }
    
//     private void NavigateToDefaultGame()
//     {
//         Debug.Log("Navigating to default custom game...");
//         // Default behavior for unknown game types
//         PlayerPrefs.SetString("CustomGameId", gameData.id);
//         PlayerPrefs.SetString("CustomGameName", gameData.name);
//         PlayerPrefs.SetString("CustomGameType", gameData.type);
//         PlayerPrefs.SetString("CustomGameSubtype", gameData.subtype);
//     }
    
//     private void OnDestroy()
//     {
//         // Clean up button listener
//         if (playButton != null)
//             playButton.onClick.RemoveListener(OnPlayButtonClicked);
//     }
// }

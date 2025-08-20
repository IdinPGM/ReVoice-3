using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class HistoryItem : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI gameCategoryText;
    public TextMeshProUGUI levelNameText; // เพิ่มสำหรับชื่อด่าน
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI complimentText;
    public TextMeshProUGUI gameTypeText; // เปลี่ยนจาก customText
    public TextMeshProUGUI score;
    public Image starImage; // เปลี่ยนจาก array เป็น Image เดียว
    
    [Header("Background Images for 4 Games")]
    [Tooltip("Assign background sprites for each game type")]
    public Sprite facialDetectionBackground;     // Background สำหรับ Facial Detection
    public Sprite functionalSpeechBackground;    // Background สำหรับ Functional Speech
    public Sprite phonemePracticeBackground;     // Background สำหรับ Phoneme Practice
    public Sprite languageTherapyBackground;     // Background สำหรับ Language Therapy
    
    [Header("Star Settings")]
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;
    public Color starActiveColor = Color.yellow;
    public Color starInactiveColor = Color.gray;
    
    private HistoryData historyData;
    
    public void SetupHistoryItem(HistoryData data)
    {
        historyData = data;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (historyData == null) return;
        
        // Set text values
        if (gameCategoryText != null)
            gameCategoryText.text = historyData.gameCategory;
            
        if (levelNameText != null)
            levelNameText.text = $"Level: {historyData.levelName}";
            
        if (dateText != null)
            dateText.text = $"Date: {historyData.date}";
            
        if (complimentText != null)
            complimentText.text = historyData.complimentText;
            
        if (score != null)
            score.text = historyData.score.ToString();

        // Set game type (Main Game หรือ Custom Game)
        if (gameTypeText != null)
        {
            gameTypeText.text = historyData.gameType;
            gameTypeText.gameObject.SetActive(!string.IsNullOrEmpty(historyData.gameType));
        }
        
        // Set background image based on game category
        SetBackgroundImage();
        
        // Set star display
        UpdateStarDisplay();
        
        // Load background image from URL if needed (optional, จะใช้ local backgrounds แทน)
        if (!string.IsNullOrEmpty(historyData.backgroundImageUrl))
            StartCoroutine(LoadBackgroundFromURL(historyData.backgroundImageUrl));
    }
    
    private void SetBackgroundImage()
    {
        if (backgroundImage == null) return;
        
        // เลือก background sprite ตาม game category
        Sprite selectedBackground = GetBackgroundSpriteByCategory(historyData.gameCategory);
        
        if (selectedBackground != null)
        {
            backgroundImage.sprite = selectedBackground;
            historyData.backgroundImage = selectedBackground;
            Debug.Log($"Background set for {historyData.gameCategory}");
        }
        else
        {
            Debug.LogWarning($"No background sprite assigned for category: {historyData.gameCategory}. Please assign sprites in Inspector.");
        }
    }
    
    private Sprite GetBackgroundSpriteByCategory(string gameCategory)
    {
        if (string.IsNullOrEmpty(gameCategory)) return null;
        
        string category = gameCategory.ToLower();
        
        if (category.Contains("facial"))
            return facialDetectionBackground;
        else if (category.Contains("functional"))
            return functionalSpeechBackground;
        else if (category.Contains("phoneme"))
            return phonemePracticeBackground;
        else if (category.Contains("language"))
            return languageTherapyBackground;
        
        // Default fallback
        return facialDetectionBackground;
    }
    
    private void UpdateStarDisplay()
    {
        if (starImage == null) return;
        
        bool showFilledStar = historyData.score > 0;
        
        // Set sprite
        if (filledStarSprite != null && emptyStarSprite != null)
            starImage.sprite = showFilledStar ? filledStarSprite : emptyStarSprite;
        
        // Set color
        starImage.color = showFilledStar ? starActiveColor : starInactiveColor;
    }
    
    private IEnumerator LoadBackgroundFromURL(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success && backgroundImage != null)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    backgroundImage.sprite = sprite;
                    historyData.backgroundImage = sprite;
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load background image from URL: {url}. Error: {request.error}");
            }
        }
    }
    
    // Method สำหรับ click event (ถ้าต้องการ)
    public void OnItemClicked()
    {
        Debug.Log($"History item clicked: {historyData.gameCategory} - Level: {historyData.levelName} - {historyData.complimentText}");
        // Add your click handling logic here
    }
}

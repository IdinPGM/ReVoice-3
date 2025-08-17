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
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI complimentText;
    public TextMeshProUGUI customText;
    public TextMeshProUGUI starCountText;
    public Image[] starImages; // Array ของรูปดาว
    public GameObject customIndicator; // GameObject สำหรับแสดง custom indicator
    
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
            
        if (dateText != null)
            dateText.text = historyData.date;
            
        if (complimentText != null)
            complimentText.text = historyData.complimentText;
            
        if (starCountText != null)
            starCountText.text = historyData.starCount.ToString();
        
        // Set custom indicator
        if (customIndicator != null)
            customIndicator.SetActive(historyData.isCustom);
            
        if (customText != null)
        {
            customText.text = historyData.isCustom ? "Custom" : "";
            customText.gameObject.SetActive(historyData.isCustom);
        }
        
        // Set background image
        if (backgroundImage != null && historyData.backgroundImage != null)
            backgroundImage.sprite = historyData.backgroundImage;
        
        // Set stars
        UpdateStarDisplay();
        
        // Load background image from URL if needed
        if (!string.IsNullOrEmpty(historyData.backgroundImageUrl))
            StartCoroutine(LoadBackgroundFromURL(historyData.backgroundImageUrl));
    }
    
    private void UpdateStarDisplay()
    {
        if (starImages == null) return;
        
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                bool isActive = i < historyData.starCount;
                
                // Set sprite
                if (filledStarSprite != null && emptyStarSprite != null)
                    starImages[i].sprite = isActive ? filledStarSprite : emptyStarSprite;
                
                // Set color
                starImages[i].color = isActive ? starActiveColor : starInactiveColor;
            }
        }
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
        Debug.Log($"History item clicked: {historyData.gameCategory} - {historyData.complimentText}");
        // Add your click handling logic here
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyMissionItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI missionTypeText;
    public TextMeshProUGUI missionDescriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;
    public Image starIcon;
    public Image backgroundImage;

    [Header("Star Sprites")]
    public Sprite emptyStar;
    public Sprite filledStar;

    [Header("Background Colors")]
    public Color completedColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Blue for completed
    public Color inProgressColor = new Color(0.3f, 0.5f, 0.7f, 1f); // Darker blue for in progress
    public Color defaultColor = new Color(0.4f, 0.6f, 0.8f, 1f); // Light blue for default

    private DailyMissionData missionData;
    private System.Action<DailyMissionData> onClaimCallback;

    public void SetupMission(DailyMissionData data, System.Action<DailyMissionData> claimCallback = null)
    {
        missionData = data;
        onClaimCallback = claimCallback;

        UpdateUI();
        
        // Setup claim button
        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(ClaimMission);
        }
    }

    private void UpdateUI()
    {
        if (missionData == null) return;

        // Update mission type and description
        if (missionTypeText != null)
            missionTypeText.text = GetMissionTypeDisplayName(missionData.questType);

        if (missionDescriptionText != null)
            missionDescriptionText.text = GetMissionDescription(missionData.questType);

        // Update progress
        if (progressText != null)
            progressText.text = $"{missionData.currentCount}/{missionData.targetCount}";

        if (progressBar != null)
        {
            progressBar.value = missionData.GetProgress();
        }

        // Update star icon
        if (starIcon != null)
        {
            starIcon.sprite = missionData.isCompleted ? filledStar : emptyStar;
        }

        // Update background color
        if (backgroundImage != null)
        {
            if (missionData.isCompleted)
                backgroundImage.color = completedColor;
            else if (missionData.currentCount > 0)
                backgroundImage.color = inProgressColor;
            else
                backgroundImage.color = defaultColor;
        }

        // Update claim button
        if (claimButton != null)
        {
            claimButton.interactable = missionData.CanClaim();
            claimButton.gameObject.SetActive(missionData.isCompleted);
        }
    }

    private string GetMissionTypeDisplayName(string questType)
    {
        switch (questType.ToLower())
        {
            case "facial_detection":
                return "Facial Detection";
            case "minigame":
                return "Minigame";
            case "productivity":
                return "Productivity";
            default:
                return questType;
        }
    }

    private string GetMissionDescription(string questType)
    {
        switch (questType.ToLower())
        {
            case "facial_detection":
                return "ใช้ระบบการตรวจจับใบหน้าในวันนี้";
            case "minigame":
                return "เล่น Minigame\nจำนวนการเกมที่เล่นรวมต่อวัน";
            case "productivity":
                return "เล่น Minigame\nโดยได้แต้มความเป็นมืออาชีพต่อวัน";
            default:
                return "เสร็จสิ้นภารกิจประจำวัน";
        }
    }

    private void ClaimMission()
    {
        if (missionData != null && missionData.CanClaim())
        {
            onClaimCallback?.Invoke(missionData);
            
            // Disable claim button after claiming
            if (claimButton != null)
                claimButton.interactable = false;
        }
    }

    public void UpdateProgress(int newProgress)
    {
        if (missionData != null)
        {
            missionData.currentCount = newProgress;
            if (missionData.currentCount >= missionData.targetCount)
            {
                missionData.currentCount = missionData.targetCount;
                missionData.isCompleted = true;
            }
            UpdateUI();
        }
    }
}

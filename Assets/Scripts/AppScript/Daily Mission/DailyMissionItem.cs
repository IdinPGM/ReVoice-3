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
    public Image starIcon;
    public Image backgroundImage;
    public Image completeImage;

    [Header("Star Sprites")]
    public Sprite emptyStar;
    public Sprite filledStar;


    private DailyMissionData missionData;

    private void Start()
    {
        // // Initialize complete image to be hidden by default
        // if (completeImage != null)
        // {
        //     completeImage.gameObject.SetActive(false);
        // }
    }

    public void SetupMission(DailyMissionData data)
    {
        missionData = data;
        UpdateUI();
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

        // Update complete image overlay
        if (completeImage != null)
        {
            // Debug.Log($"Mission {missionData.questType}: isCompleted = {missionData.isCompleted}");
            completeImage.gameObject.SetActive(missionData.isCompleted);
        }
    }

    private string GetMissionTypeDisplayName(string questType)
    {
        switch (questType.ToLower())
        {
            case "play_time":
                return "Play Time";
            case "facial_detection":
                return "Facial Detection";
            case "functional_speech":
                return "Functional Speech";
            default:
                return questType;
        }
    }

    private string GetMissionDescription(string questType)
    {
        switch (questType.ToLower())
        {
            case "play_time":
                return "Play games for specified minutes";
            case "facial_detection":
                return "Use facial detection feature";
            case "functional_speech":
                return "Complete functional speech exercises";
            default:
                return "Complete daily mission";
        }
    }
}

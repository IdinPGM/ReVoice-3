using UnityEngine;

public static class DailyMissionEvents
{
    // Events for mission tracking
    public static event System.Action OnFacialDetectionUsed;
    public static event System.Action OnMinigameCompleted;
    public static event System.Action<int> OnProductivityPointsEarned;

    // Trigger methods to be called from other systems
    public static void TriggerFacialDetection()
    {
        OnFacialDetectionUsed?.Invoke();
        Debug.Log("Daily Mission Event: Facial Detection Used");
    }

    public static void TriggerMinigameCompleted()
    {
        OnMinigameCompleted?.Invoke();
        Debug.Log("Daily Mission Event: Minigame Completed");
    }

    public static void TriggerProductivityPoints(int points)
    {
        OnProductivityPointsEarned?.Invoke(points);
        Debug.Log($"Daily Mission Event: Productivity Points Earned: {points}");
    }
}

public class DailyMissionTracker : MonoBehaviour
{
    private DailyMissionManager missionManager;

    private void Start()
    {
        missionManager = FindObjectOfType<DailyMissionManager>();
        
        // Subscribe to mission events
        DailyMissionEvents.OnFacialDetectionUsed += HandleFacialDetection;
        DailyMissionEvents.OnMinigameCompleted += HandleMinigameCompleted;
        DailyMissionEvents.OnProductivityPointsEarned += HandleProductivityPoints;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        DailyMissionEvents.OnFacialDetectionUsed -= HandleFacialDetection;
        DailyMissionEvents.OnMinigameCompleted -= HandleMinigameCompleted;
        DailyMissionEvents.OnProductivityPointsEarned -= HandleProductivityPoints;
    }

    private void HandleFacialDetection()
    {
        if (missionManager != null)
        {
            missionManager.UpdateMissionProgress("facial_detection", 1);
        }
    }

    private void HandleMinigameCompleted()
    {
        if (missionManager != null)
        {
            missionManager.UpdateMissionProgress("minigame", 1);
        }
    }

    private void HandleProductivityPoints(int points)
    {
        if (missionManager != null)
        {
            missionManager.UpdateMissionProgress("productivity", points);
        }
    }
}

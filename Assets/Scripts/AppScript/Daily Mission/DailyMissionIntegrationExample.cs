using UnityEngine;

public class DailyMissionIntegrationExample : MonoBehaviour
{
    // ตัวอย่างการใช้งานระบบ Daily Mission

    private void Start()
    {
        // Subscribe to mission events
        DailyMissionManager.OnMissionCompleted += OnMissionCompleted;
        DailyMissionManager.OnMissionClaimed += OnMissionClaimed;
        DailyMissionManager.OnPointsEarned += OnPointsEarned;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        DailyMissionManager.OnMissionCompleted -= OnMissionCompleted;
        DailyMissionManager.OnMissionClaimed -= OnMissionClaimed;
        DailyMissionManager.OnPointsEarned -= OnPointsEarned;
    }

    // ===== ตัวอย่างการเรียกใช้งานจากระบบอื่นๆ =====

    // เรียกใช้เมื่อผู้เล่นใช้ Facial Detection
    public void OnFacialDetectionUsed()
    {
        DailyMissionEvents.TriggerFacialDetection();
    }

    // เรียกใช้เมื่อผู้เล่นเล่น Minigame เสร็จ
    public void OnMinigameFinished()
    {
        DailyMissionEvents.TriggerMinigameCompleted();
    }

    // เรียกใช้เมื่อผู้เล่นได้แต้ม Productivity
    public void OnProductivityScored(int points)
    {
        DailyMissionEvents.TriggerProductivityPoints(points);
    }

    // ===== Event Handlers =====

    private void OnMissionCompleted(DailyMissionData mission)
    {
        Debug.Log($"🎉 Mission Completed: {mission.questType}");
        
        // แสดง UI notification
        ShowMissionCompletedNotification(mission);
        
        // เล่นเสียงเอฟเฟกต์
        // AudioManager.Instance.PlaySFX("mission_completed");
        
        // แสดงพาร์ทิเคิล
        // ParticleManager.Instance.PlayEffect("star_burst");
    }

    private void OnMissionClaimed(DailyMissionData mission)
    {
        Debug.Log($"💰 Mission Claimed: {mission.questType}, Points: {mission.points}");
        
        // อัพเดทคะแนนผู้เล่น
        // PlayerManager.Instance.AddPoints(mission.points);
        
        // แสดง UI การได้รับรางวัล
        ShowRewardUI(mission.points);
    }

    private void OnPointsEarned(int points)
    {
        Debug.Log($"⭐ Points Earned: {points}");
        
        // อัพเดท UI คะแนนรวม
        // UIManager.Instance.UpdateTotalPoints();
    }

    // ===== UI Methods =====

    private void ShowMissionCompletedNotification(DailyMissionData mission)
    {
        // แสดง popup แจ้งเตือนว่าเควสเสร็จแล้ว
        // NotificationManager.Instance.ShowNotification(
        //     "Mission Completed!", 
        //     $"You have completed {mission.questType}!"
        // );
    }

    private void ShowRewardUI(int points)
    {
        // แสดง UI รางวัลที่ได้รับ
        // RewardUI.Instance.ShowReward(points);
    }

    // ===== Testing Methods (สำหรับทดสอบ) =====

    [ContextMenu("Test Facial Detection")]
    public void TestFacialDetection()
    {
        OnFacialDetectionUsed();
    }

    [ContextMenu("Test Minigame")]
    public void TestMinigame()
    {
        OnMinigameFinished();
    }

    [ContextMenu("Test Productivity")]
    public void TestProductivity()
    {
        OnProductivityScored(5);
    }

    [ContextMenu("Load Sample Missions")]
    public void LoadSampleMissions()
    {
        DailyMissionAPI api = FindObjectOfType<DailyMissionAPI>();
        if (api != null)
        {
            api.TestWithSampleData();
        }
    }
}

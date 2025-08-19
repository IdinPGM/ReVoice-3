using System;
using UnityEngine;

[System.Serializable]
public class DailyMissionData
{
    public string id;
    public string questType;
    public DateTime? questDate;
    public int targetCount;
    public int currentCount;
    public bool isCompleted;
    public int points;

    public DailyMissionData()
    {
        id = "";
        questType = "";
        questDate = null;
        targetCount = 1;
        currentCount = 0;
        isCompleted = false;
        points = 0;
    }

    public DailyMissionData(string id, string questType, int targetCount, int points)
    {
        this.id = id;
        this.questType = questType;
        this.questDate = DateTime.Now;
        this.targetCount = targetCount;
        this.currentCount = 0;
        this.isCompleted = false;
        this.points = points;
    }

    public float GetProgress()
    {
        if (targetCount == 0) return 0f;
        return (float)currentCount / targetCount;
    }

    public void AddProgress(int amount = 1)
    {
        if (isCompleted) return;
        
        currentCount += amount;
        if (currentCount >= targetCount)
        {
            currentCount = targetCount;
            isCompleted = true;
        }
    }

    public void ResetProgress()
    {
        currentCount = 0;
        isCompleted = false;
    }

    public bool CanClaim()
    {
        return isCompleted && currentCount >= targetCount;
    }
}

[System.Serializable]
public class DailyMissionList
{
    public DailyMissionData[] dailyMission;

    public DailyMissionList()
    {
        dailyMission = new DailyMissionData[0];
    }
}

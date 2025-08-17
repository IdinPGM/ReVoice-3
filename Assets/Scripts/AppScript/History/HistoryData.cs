using System;
using UnityEngine;

[Serializable]
public class HistoryData
{
    [Header("Game Information")]
    public string gameCategory;
    public string date;
    public string complimentText;
    public bool isCustom;
    public int starCount;
    
    [Header("Visual")]
    public Sprite backgroundImage;
    public string backgroundImageUrl; // สำหรับโหลดจาก server
    
    public HistoryData()
    {
        gameCategory = "";
        date = "";
        complimentText = "";
        isCustom = false;
        starCount = 0;
        backgroundImage = null;
        backgroundImageUrl = "";
    }
    
    public HistoryData(string category, string gameDate, string compliment, bool custom, int stars, string bgImageUrl = "")
    {
        gameCategory = category;
        date = gameDate;
        complimentText = compliment;
        isCustom = custom;
        starCount = Mathf.Clamp(stars, 0, 5); // จำกัดดาวไม่เกิน 5 ดวง
        backgroundImageUrl = bgImageUrl;
    }
}

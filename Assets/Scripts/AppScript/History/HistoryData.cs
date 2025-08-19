using System;
using UnityEngine;

[Serializable]
public class HistoryData
{
    [Header("Game Information")]
    public string gameCategory;
    public string levelName; // เพิ่มชื่อด่าน
    public string date;
    public string complimentText;
    public string gameType; // Main Game หรือ Custom Game
    public int starCount;
    
    [Header("Visual")]
    public Sprite backgroundImage;
    public string backgroundImageUrl; // สำหรับโหลดจาก server
    
    public HistoryData()
    {
        gameCategory = "";
        levelName = "";
        date = "";
        complimentText = "";
        gameType = "Main Game";
        starCount = 0;
        backgroundImage = null;
        backgroundImageUrl = "";
    }
    
    public HistoryData(string category, string level, string gameDate, string compliment, string type, int stars, string bgImageUrl = "")
    {
        gameCategory = category;
        levelName = level;
        date = gameDate;
        complimentText = compliment;
        gameType = type;
        starCount = Mathf.Clamp(stars, 0, 5); // จำกัดดาวไม่เกิน 5 ดวง
        backgroundImageUrl = bgImageUrl;
    }
    
    // Method สำหรับแปลง type จาก API
    public static string GetGameCategoryFromType(string type)
    {
        switch (type?.ToLower())
        {
            case "facial":
            case "facial_detection":
                return "Facial Detection";
            case "functional":
            case "functional_speech":
                return "Functional Speech";
            case "phoneme":
            case "phoneme_practice":
                return "Phoneme Practice";
            case "language":
            case "language_therapy":
                return "Language Therapy";
            default:
                return "Unknown Game";
        }
    }
    
    // Method สำหรับกำหนด background image ตาม game type
    public static int GetBackgroundIndexFromType(string type)
    {
        switch (type?.ToLower())
        {
            case "facial":
            case "facial_detection":
                return 0;
            case "functional":
            case "functional_speech":
                return 1;
            case "phoneme":
            case "phoneme_practice":
                return 2;
            case "language":
            case "language_therapy":
                return 3;
            default:
                return 0;
        }
    }
}

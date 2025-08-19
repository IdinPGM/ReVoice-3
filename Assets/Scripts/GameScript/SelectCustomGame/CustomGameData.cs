using System;
using UnityEngine;

[Serializable]
public class CustomGameData
{
    [Header("Game Information")]
    public string id;
    public string name;
    public string createdDate;
    public string category; // หมวดหมู่มินิเกม (type + subtype)
    public string type;
    public string subtype;
    public string description;
    
    [Header("Visual")]
    public Sprite backgroundImage;
    public string backgroundImageUrl; // สำหรับโหลดจาก server
    
    public CustomGameData()
    {
        id = "";
        name = "";
        createdDate = "";
        category = "";
        type = "";
        subtype = "";
        description = "";
        backgroundImage = null;
        backgroundImageUrl = "";
    }
    
    public CustomGameData(string gameId, string gameName, string date, string gameType, string gameSubtype, string desc = "", string bgImageUrl = "")
    {
        id = gameId;
        name = gameName;
        createdDate = date;
        type = gameType;
        subtype = gameSubtype;
        category = GetCategoryDisplay(gameType, gameSubtype);
        description = desc;
        backgroundImageUrl = bgImageUrl;
    }
    
    // Method สำหรับสร้างชื่อหมวดหมู่ที่แสดงผล
    private string GetCategoryDisplay(string gameType, string gameSubtype)
    {
        if (string.IsNullOrEmpty(gameType))
            return "Custom Game";
            
        // แปลง underscore format เป็น display format
        string displayType = ConvertTypeToDisplayFormat(gameType);
        
        if (string.IsNullOrEmpty(gameSubtype))
            return displayType;
            
        return $"{displayType} - {gameSubtype}";
    }
    
    // Method สำหรับแปลง type จาก API format เป็น display format
    public static string ConvertTypeToDisplayFormat(string type)
    {
        if (string.IsNullOrEmpty(type)) return "";
        
        switch (type.ToLower())
        {
            case "facial_detection":
                return "Facial Detection";
            case "language_therapy":
                return "Language Therapy";
            case "phoneme_practice":
                return "Phoneme Practice";
            case "functional_speech":
                return "Functional Speech";
            default:
                // ถ้าไม่เจอใน mapping ให้แปลง underscore เป็น space และ capitalize แต่ละคำ
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(type.Replace("_", " ").ToLower());
        }
    }
    
    // Method สำหรับ get display type (แยกจาก subtype)
    public string GetDisplayType()
    {
        return ConvertTypeToDisplayFormat(type);
    }
    
    // Method สำหรับ get display subtype
    public string GetDisplaySubtype()
    {
        return string.IsNullOrEmpty(subtype) ? "" : subtype;
    }
    
    // Method สำหรับ format วันที่
    public string GetFormattedDate()
    {
        if (string.IsNullOrEmpty(createdDate))
            return "";
            
        try
        {
            // ถ้าเป็นรูปแบบ ISO date (YYYY-MM-DD) แปลงเป็น DD/MM/YY
            if (DateTime.TryParse(createdDate, out DateTime date))
            {
                return date.ToString("dd/MM/yy");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse date: {createdDate}, Error: {e.Message}");
        }
        
        return createdDate; // ถ้าแปลงไม่ได้ให้ return ค่าเดิม
    }
}

// API Response Models
[Serializable]
public class ApiCustomGameResponse
{
    public CustomGameApiData[] levels;
    public int page;
    public int limit;
    public int total;
    public bool hasMore;
}

[Serializable]
public class CustomGameApiData
{
    public string id;
    public string name;
    public string description;
    public string type;
    public string subtype;
}

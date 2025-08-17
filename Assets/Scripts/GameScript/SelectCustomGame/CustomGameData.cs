// using System;
// using UnityEngine;

// [Serializable]
// public class CustomGameData
// {
//     [Header("Game Information")]
//     public string id;
//     public string name;
//     public string createdDate;
//     public string category; // หมวดหมู่มินิเกม (type + subtype)
//     public string type;
//     public string subtype;
//     public string description;
    
//     [Header("Visual")]
//     public Sprite backgroundImage;
//     public string backgroundImageUrl; // สำหรับโหลดจาก server
    
//     public CustomGameData()
//     {
//         id = "";
//         name = "";
//         createdDate = "";
//         category = "";
//         type = "";
//         subtype = "";
//         description = "";
//         backgroundImage = null;
//         backgroundImageUrl = "";
//     }
    
//     public CustomGameData(string gameId, string gameName, string date, string gameType, string gameSubtype, string desc = "", string bgImageUrl = "")
//     {
//         id = gameId;
//         name = gameName;
//         createdDate = date;
//         type = gameType;
//         subtype = gameSubtype;
//         category = GetCategoryDisplay(gameType, gameSubtype);
//         description = desc;
//         backgroundImageUrl = bgImageUrl;
//     }
    
//     // Method สำหรับสร้างชื่อหมวดหมู่ที่แสดงผล
//     private string GetCategoryDisplay(string gameType, string gameSubtype)
//     {
//         if (string.IsNullOrEmpty(gameType))
//             return "Custom Game";
            
//         if (string.IsNullOrEmpty(gameSubtype))
//             return gameType;
            
//         return $"{gameType} - {gameSubtype}";
//     }
    
//     // Method สำหรับ format วันที่
//     public string GetFormattedDate()
//     {
//         if (string.IsNullOrEmpty(createdDate))
//             return "";
            
//         try
//         {
//             // ถ้าเป็นรูปแบบ ISO date (YYYY-MM-DD) แปลงเป็น DD/MM/YY
//             if (DateTime.TryParse(createdDate, out DateTime date))
//             {
//                 return date.ToString("dd/MM/yy");
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogWarning($"Failed to parse date: {createdDate}, Error: {e.Message}");
//         }
        
//         return createdDate; // ถ้าแปลงไม่ได้ให้ return ค่าเดิม
//     }
// }

// // API Response Models
// [Serializable]
// public class ApiCustomGameResponse
// {
//     public CustomGameApiData[] data;
//     public ApiPagination pagination;
// }

// [Serializable]
// public class CustomGameApiData
// {
//     public string id;
//     public string name;
//     public string created_at;
//     public string type;
//     public string subtype;
//     public string description;
//     public string image_url;
// }

// [Serializable]
// public class ApiPagination
// {
//     public int page;
//     public int limit;
//     public int total;
//     public int totalPages;
// }

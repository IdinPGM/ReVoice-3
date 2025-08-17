# History System Documentation

## ไฟล์ที่สร้างขึ้น

### 1. HistoryData.cs
- Class สำหรับเก็บข้อมูลประวัติเกม
- รองรับ: game category, date, compliment text, custom flag, star count, background image

### 2. HistoryItem.cs  
- Component สำหรับแสดงผลแต่ละ item ใน history list
- รองรับการโหลดรูปภาพจาก URL
- จัดการการแสดงดาวตามจำนวนที่ได้รับ

### 3. History.cs
- Main controller สำหรับจัดการ History page
- รองรับการเรียก API และแสดง sample data
- มีระบบ refresh และ scroll

## การตั้งค่าใน Unity

### 1. สร้าง History Page UI

#### Canvas Structure:
```
Canvas
└── HistoryPage
    ├── Header
    │   ├── Title (Text: "Lastest History")
    │   └── RefreshButton
    ├── ScrollView
    │   └── Viewport
    │       └── Content (HistoryContainer)
    └── LoadingText
```

### 2. สร้าง History Item Prefab

#### Prefab Structure:
```
HistoryItemPrefab
├── BackgroundImage (Image)
├── GameCategoryText (TextMeshPro)
├── DateText (TextMeshPro) 
├── ComplimentText (TextMeshPro)
├── CustomIndicator (GameObject)
│   └── CustomText (TextMeshPro)
├── StarContainer
│   ├── Star1 (Image)
│   ├── Star2 (Image)
│   ├── Star3 (Image)
│   ├── Star4 (Image)
│   └── Star5 (Image)
└── StarCountText (TextMeshPro)
```

### 3. Component Assignment

#### History.cs:
- historyContainer: Content GameObject ใน ScrollView
- historyItemPrefab: HistoryItem Prefab
- scrollRect: ScrollRect component
- loadingText: Loading Text
- refreshButton: Refresh Button

#### HistoryItem.cs:
- backgroundImage: Background Image
- gameCategoryText: Game Category Text
- dateText: Date Text  
- complimentText: Compliment Text
- customText: Custom Text
- starCountText: Star Count Text
- starImages: Array ของ Star Images (5 ตัว)
- customIndicator: Custom Indicator GameObject
- filledStarSprite: Sprite สำหรับดาวที่เต็ม
- emptyStarSprite: Sprite สำหรับดาวที่ว่าง

## การใช้งาน

### 1. เรียกใช้ในสคริปต์อื่น:
```csharp
// เพิ่ม history item ใหม่
HistoryData newData = new HistoryData("Minigame - A", "21/04/25", "Perfect!", false, 5);
historyScript.AddNewHistoryItem(newData);

// รีเฟรช history
historyScript.RefreshHistory();

// ดูจำนวน history
int count = historyScript.GetHistoryCount();
```

### 2. การเชื่อมต่อ API:
- แก้ไขใน method `CallHistoryAPI()` ใน History.cs
- เปลี่ยน URL และปรับแต่ง JSON parsing ตาม API ของคุณ

### 3. Sample Data Format:
```json
{
  "historyList": [
    {
      "gameCategory": "Minigame - A",
      "date": "20/04/25", 
      "complimentText": "Great Job!",
      "isCustom": false,
      "starCount": 5,
      "backgroundImageUrl": "https://example.com/image.jpg"
    }
  ],
  "success": true,
  "message": "Success"
}
```

## Features

✅ แสดงข้อมูลประวัติตามรูปแบบที่กำหนด
✅ รองรับการโหลดรูปภาพจาก URL  
✅ ระบบดาวแบบ dynamic (1-5 ดวง)
✅ แสดง Custom indicator
✅ Scroll ได้และมี refresh
✅ ใช้งานง่าย ไม่ซับซ้อน
✅ รองรับ sample data และ API call
✅ Error handling และ fallback

## ปรับแต่งเพิ่มเติม

1. **สีและ Theme**: แก้ไขสีใน HistoryItem.cs
2. **ขนาดและ Spacing**: แก้ไขใน History.cs  
3. **Animation**: เพิ่ม animation ใน HistoryItem.cs
4. **API URL**: แก้ไขใน CallHistoryAPI() method

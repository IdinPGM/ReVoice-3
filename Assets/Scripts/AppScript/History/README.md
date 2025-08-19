# History System Documentation (Updated)

## การเปลี่ยนแปลงใหม่

### 1. Background Images
- รองรับ 4 แบบสำหรับ 4 มินิเกม:
  - **Index 0**: Facial Detection
  - **Index 1**: Functional Speech  
  - **Index 2**: Phoneme Practice
  - **Index 3**: Language Therapy

### 2. Star Display
- เปลี่ยนจาก Array เป็น **Image เดียว**
- แสดงดาวเต็ม/ดาวว่าง ตาม score > 0 หรือไม่

### 3. Game Type Display
- เปลี่ยนจาก Custom Indicator เป็น **Game Type Text**
- แสดง: "Main Game" หรือ "Custom Game"

### 4. Level Name
- เพิ่มการแสดง **ชื่อด่าน** จากข้อมูล API

## ไฟล์ที่อัปเดต

### 1. HistoryData.cs
```csharp
// เพิ่ม fields ใหม่
public string levelName;      // ชื่อด่าน
public string gameType;       // Main Game/Custom Game

// Helper methods
GetGameCategoryFromType()     // แปลง type เป็น category
GetBackgroundIndexFromType()  // เลือก background image
```

### 2. HistoryItem.cs
```csharp
// UI References ใหม่
public TextMeshProUGUI levelNameText;  // ชื่อด่าน
public TextMeshProUGUI gameTypeText;   // Main/Custom Game
public Image starImage;                // ดาวเดียว (ไม่ใช่ array)
public Sprite[] gameBackgrounds;       // 4 backgrounds

// Methods ใหม่
SetBackgroundImage()          // เลือก background ตาม game type
UpdateStarDisplay()           // แสดงดาวเดียว
```

### 3. History.cs
```csharp
// ปรับ API conversion
ConvertApiResponseToHistoryData()     // ใช้ข้อมูลใหม่จาก API
AddNewHistoryItem()                   // เพิ่ม overload method

// Sample data ใหม่
GetSampleHistoryData()                // ข้อมูลทดสอบแบบใหม่
```

## การตั้งค่าใน Unity (อัปเดต)

### 1. History Item Prefab Structure ใหม่:
```
HistoryItemPrefab
├── BackgroundImage (Image)
├── GameCategoryText (TextMeshPro)      // "Facial Detection" etc.
├── LevelNameText (TextMeshPro)         // "Level 1", "Custom Level" etc.
├── DateText (TextMeshPro)              // "20/04/25"
├── ComplimentText (TextMeshPro)        // "Great Job!"
├── GameTypeText (TextMeshPro)          // "Main Game" / "Custom Game"
├── StarImage (Image)                   // ดาวเดียว
└── StarCountText (TextMeshPro)         // "5"
```

### 2. Component Assignment (อัปเดต)

#### HistoryItem.cs:
```csharp
// Required assignments
backgroundImage: Background Image
gameCategoryText: Game Category Text
levelNameText: Level Name Text        // ใหม่
dateText: Date Text  
complimentText: Compliment Text
gameTypeText: Game Type Text          // เปลี่ยนจาก customText
starCountText: Star Count Text
starImage: Star Image                 // เปลี่ยนจาก starImages array

// Background Images (ต้องลากใส่ 4 sprites)
gameBackgrounds[0]: Facial Detection Background
gameBackgrounds[1]: Functional Speech Background  
gameBackgrounds[2]: Phoneme Practice Background
gameBackgrounds[3]: Language Therapy Background

// Star sprites
filledStarSprite: Filled Star Sprite
emptyStarSprite: Empty Star Sprite
```

## API Data Format

### Input จาก Server:
```json
{
  "history": [
    {
      "id": "string",
      "name": "Level 1",              // ชื่อด่าน
      "description": "string",        // ไม่ใช้
      "levelId": "string",
      "type": "facial",               // ประเภทเกม
      "subtype": "string",            // ไม่ใช้
      "isCustom": false,              // Main/Custom
      "score": 5,                     // คะแนน 1-5
      "completedAt": "2025-04-20"     // วันที่
    }
  ]
}
```

### Output ที่แสดง:
- **Game Category**: "Facial Detection" (แปลงจาก type)
- **Level Name**: "Level 1" (จาก name)
- **Date**: "20/04/25" (แปลงจาก completedAt)
- **Compliment**: "Excellent!" (แปลงจาก score)
- **Game Type**: "Main Game" (แปลงจาก isCustom)
- **Star**: แสดงดาวเต็ม/ว่าง ตาม score > 0
- **Background**: เลือกจาก 4 แบบตาม type

## การใช้งาน (อัปเดต)

### 1. เพิ่ม History Item ใหม่:
```csharp
// แบบง่าย
historyScript.AddNewHistoryItem("facial", "Level 1", 5, false);

// แบบเต็ม
HistoryData newData = new HistoryData(
    "Facial Detection", 
    "Level 1", 
    "20/04/25", 
    "Perfect!", 
    "Main Game", 
    5
);
historyScript.AddNewHistoryItem(newData);
```

### 2. แปลง Game Type:
```csharp
string category = HistoryData.GetGameCategoryFromType("facial");
// Result: "Facial Detection"

int bgIndex = HistoryData.GetBackgroundIndexFromType("functional");  
// Result: 1 (Functional Speech background)
```

## สิ่งที่ต้องทำใน Unity

1. **สร้าง 4 Background Images** สำหรับแต่ละเกม
2. **อัปเดต History Item Prefab** ตาม structure ใหม่
3. **ลาก Sprites** ใส่ใน gameBackgrounds array
4. **ลบ starImages array** และใช้ starImage เดียว
5. **เพิ่ม levelNameText และ gameTypeText**
6. **ลบ customIndicator** (ถ้ามี)

## Game Type Mapping

| API Type | Display Category |
|----------|------------------|
| facial | Facial Detection |
| functional | Functional Speech |
| phoneme | Phoneme Practice |
| language | Language Therapy |

## ข้อดีของการอัปเดต

✅ รองรับ 4 มินิเกมด้วย background แยกกัน  
✅ แสดงชื่อด่านชัดเจน  
✅ ระบบดาวเรียบง่าย  
✅ แสดง Main/Custom Game แทน true/false  
✅ ตรงกับข้อมูล API ที่กำหนด  
✅ ใช้งานง่าย ไม่ซับซ้อน

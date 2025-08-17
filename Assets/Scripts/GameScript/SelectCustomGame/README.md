# Custom Mode Scripts

สคริปต์สำหรับจัดการ Custom Mode ในแอปพลิเคชัน ReVoice-3 ที่ดึงข้อมูลเกม custom จาก API และแสดงในรูปแบบปุ่มเพื่อให้ผู้ใช้เลือกเล่น

## ไฟล์ที่เกี่ยวข้อง

### 1. CustomGameData.cs
- **หน้าที่**: เก็บข้อมูลของเกม custom แต่ละเกม
- **ข้อมูลที่เก็บ**:
  - ชื่อเกม (name)
  - วันที่สร้าง (createdDate) 
  - หมวดหมู่มินิเกม (category)
  - ประเภทเกม (type, subtype)
  - รูปภาพพื้นหลัง (backgroundImage, backgroundImageUrl)

### 2. CustomGameItem.cs
- **หน้าที่**: จัดการการแสดงผลของเกม custom แต่ละรายการ
- **ฟีเจอร์**:
  - แสดงชื่อเกม, วันที่, หมวดหมู่
  - โหลดรูปภาพพื้นหลังจาก URL
  - ปุ่มเล่นเกม
  - นำทางไปยังเกมที่เหมาะสมตาม type/subtype

### 3. CustomMode.cs
- **หน้าที่**: จัดการหน้า Custom Mode หลัก
- **ฟีเจอร์**:
  - เรียก API เพื่อดึงข้อมูลเกม custom
  - แสดงรายการเกมในรูปแบบ scrollable list
  - ระบบกรองตาม type และ subtype
  - ระบบ pagination
  - การรีเฟรชข้อมูล

## API Endpoint

```
GET https://api.mystrokeapi.uk/game/custom-levels?page=1&limit=10&type=&subtype=
```

### Parameters:
- `page`: หน้าที่ต้องการ (default: 1)
- `limit`: จำนวนรายการต่อหน้า (default: 10)
- `type`: ประเภทเกม (optional)
- `subtype`: ประเภทย่อย (optional)

### Response Format:
```json
{
  "data": [
    {
      "id": "string",
      "name": "string",
      "created_at": "string",
      "type": "string", 
      "subtype": "string",
      "description": "string",
      "image_url": "string"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 100,
    "totalPages": 10
  }
}
```

## การติดตั้งและใช้งาน

### 1. สร้าง UI Components
ต้องสร้าง UI elements ใน Unity:

#### สำหรับ CustomMode:
- `gamesContainer`: Empty GameObject สำหรับใส่ game items (ต้องมี VerticalLayoutGroup component)
- `customGameItemPrefab`: Prefab ของ CustomGameItem
- `scrollRect`: ScrollRect component (สำหรับ scroll ในรายการเกม)
- `loadingText`: TextMeshPro สำหรับแสดงสถานะโหลด
- `refreshButton`: Button สำหรับรีเฟรช
- `titleText`: TextMeshPro สำหรับหัวข้อ "Custom Games"
- `noGamesText`: TextMeshPro สำหรับแสดงเมื่อไม่มีเกม
- `typeDropdown`, `subtypeDropdown`: TMP_Dropdown สำหรับกรอง
- `filterButton`: Button สำหรับใช้กรอง

#### สำหรับ CustomGameItem Prefab:
- `backgroundImage`: Image สำหรับพื้นหลัง
- `gameNameText`: TextMeshPro สำหรับชื่อเกม
- `dateText`: TextMeshPro สำหรับวันที่
- `categoryText`: TextMeshPro สำหรับหมวดหมู่
- `playButton`: Button สำหรับเล่นเกม

### 2. Setup ใน Unity Editor
1. สร้าง Empty GameObject และแนบ script `CustomMode`
2. Assign UI components ที่สร้างไว้เข้ากับตัวแปรใน script
3. สร้าง Prefab สำหรับ CustomGameItem และ assign เข้ากับ `customGameItemPrefab`
4. ตั้งค่า Layout:
   - ใส่ `VerticalLayoutGroup` component ใน `gamesContainer`
   - ตั้งค่า `spacing` ตามต้องการ
   - ใส่ `ContentSizeFitter` component ใน `gamesContainer` (Vertical Fit: Preferred Size)

### 3. สร้าง Sample UI Layout

#### Hierarchy ตัวอย่าง:
```
CustomModePanel
├── Header
│   ├── TitleText (CustomMode.titleText)
│   └── RefreshButton (CustomMode.refreshButton)
├── FilterPanel
│   ├── TypeDropdown (CustomMode.typeDropdown)
│   ├── SubtypeDropdown (CustomMode.subtypeDropdown)
│   └── FilterButton (CustomMode.filterButton)
├── ScrollView (CustomMode.scrollRect)
│   └── Viewport
│       └── Content (CustomMode.gamesContainer)
│           └── CustomGameItem (Prefab)
├── LoadingText (CustomMode.loadingText)
└── NoGamesText (CustomMode.noGamesText)
```

#### CustomGameItem Prefab Layout:
```
CustomGameItem (with Button component)
├── BackgroundImage (CustomGameItem.backgroundImage)
├── ContentPanel
│   ├── GameNameText (CustomGameItem.gameNameText)
│   ├── DateText (CustomGameItem.dateText)
│   ├── CategoryText (CustomGameItem.categoryText)
│   └── PlayButton (CustomGameItem.playButton)
```

### 4. เชื่อมต่อกับเกมอื่น ๆ
Script จะเรียกไปยังเกมต่าง ๆ ตาม type:
- `FacialDetection`: สำหรับ Facial Detection games
- `LanguageTherapy`: สำหรับ Language Therapy games  
- `PhonemePracticeNew`: สำหรับ Phoneme Practice games
- `FunctionalSpeech`: สำหรับ Functional Speech games

เกมเหล่านี้ได้เพิ่ม method `StartCustomGame(object customGameData)` แล้ว เพื่อรับข้อมูลเกม custom

### 5. Authentication Setup
- ตรวจสอบว่ามี `authToken` ใน PlayerPrefs
- ถ้าไม่มี จะใช้ sample data แทน
- Token จะถูกส่งใน Authorization header เป็น Bearer token

## การใช้งาน

1. เมื่อเปิดหน้า Custom Mode จะเรียก API อัตโนมัติ
2. ผู้ใช้สามารถใช้ dropdown เพื่อกรองตาม type และ subtype
3. กดปุ่ม "Apply Filter" เพื่อใช้กรอง
4. กดปุ่ม "Refresh" เพื่อโหลดข้อมูลใหม่
5. กดปุ่ม "Play" บนเกมที่ต้องการเพื่อเริ่มเล่น

## หมายเหตุ

- Script ใช้ `HttpHelper` class ที่มีอยู่แล้วในโปรเจ็กต์
- ถ้าไม่มี authentication token จะแสดง sample data
- รองรับการโหลดรูปภาพจาก URL
- ใช้ PlayerPrefs เพื่อส่งข้อมูลเกมไปยังเกมอื่น ๆ

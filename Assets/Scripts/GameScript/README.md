# การปรับปรุงระบบเกม ReVoice

## การเปลี่ยนแปลงที่สำคัญ

### 1. สร้าง Base Class สำหรับเกมที่ใช้เสียง

สร้าง `BaseSpeechGame.cs` เพื่อรวมฟังก์ชันที่ใช้ร่วมกันระหว่าง:
- FunctionalSpeech 
- PhonemePractice

### 2. ลดการเขียนโค้ดซ้ำ (DRY Principle)

#### ฟังก์ชันที่รวมใน Base Class:
- การจัดการ Webcam
- การโหลดข้อมูล Stage จาก PlayerPrefs
- การตั้งค่า UI เริ่มต้น
- การบันทึกและส่งเสียง
- การจัดการ Response และ Feedback
- การข้ามและไปด่านต่อไป

#### Abstract Properties ที่ Child Class ต้องกำหนด:
```csharp
protected abstract float ThresholdValue { get; }     // ค่า threshold สำหรับการตรวจสอบ
protected abstract string GameName { get; }          // ชื่อเกม
protected abstract string CorrectMessage { get; }    // ข้อความเมื่อถูก
protected abstract string RetryMessagePrefix { get; } // ข้อความเมื่อผิด
```

### 3. ไฟล์ที่สร้างใหม่

- `BaseSpeechGame.cs` - Base class สำหรับเกมที่ใช้เสียง
- `FunctionalSpeechNew.cs` - เวอร์ชันใหม่ของ FunctionalSpeech ที่ใช้ Base Class
- `PhonemePracticeNew.cs` - เวอร์ชันใหม่ของ PhonemePractice ที่ใช้ Base Class

### 4. ประโยชน์ของการปรับปรุง

1. **ลดโค้ดซ้ำ**: จาก ~280 บรรทัดต่อไฟล์ เหลือเพียง ~15 บรรทัด
2. **ง่ายต่อการดูแล**: แก้ไขใน Base Class จะส่งผลต่อทุกเกม
3. **เพิ่มเกมใหม่ง่าย**: เพียงสืบทอด Base Class และกำหนด Properties
4. **ลดข้อผิดพลาด**: Logic หลักอยู่ในที่เดียว

### 5. ตัวอย่างการเพิ่มเกมใหม่

```csharp
public class NewSpeechGame : BaseSpeechGame
{
    protected override float ThresholdValue => 0.7f;
    protected override string GameName => "New Game";
    protected override string CorrectMessage => "ยอดเยี่ยม!";
    protected override string RetryMessagePrefix => "ลองใหม่: ";
}
```

### 6. การใช้งาน

1. ใช้ `FunctionalSpeechNew.cs` แทน `FunctionalSpeech.cs`
2. ใช้ `PhonemePracticeNew.cs` แทน `PhonemePractice.cs`
3. `LanguageTherapy.cs` และ `FacialDetection.cs` ใช้ระบบเดิมเนื่องจากมีความซับซ้อนเพิ่มเติม

### 7. โครงสร้างโฟลเดอร์

```
Assets/Scripts/GameScript/
├── BaseSpeechGame.cs          (ใหม่)
├── FunctionalSpeech.cs        (เก่า - สามารถลบได้)
├── FunctionalSpeechNew.cs     (ใหม่)
├── PhonemePractice.cs         (เก่า - สามารถลบได้)
├── PhonemePracticeNew.cs      (ใหม่)
├── LanguageTherapy.cs         (ยังใช้ระบบเดิม)
└── FacialDetection.cs         (ยังใช้ระบบเดิม)
```

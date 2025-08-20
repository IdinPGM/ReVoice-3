using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private bool animateProgress = true;
    [SerializeField] private float animationSpeed = 2f;
    
    private int totalStages;
    private int currentStage;
    private float targetProgress;
    
    private void Start()
    {
        // ถ้าไม่ได้กำหนด Slider ให้หาจาก component เดียวกัน
        if (progressSlider == null)
        {
            progressSlider = GetComponent<Slider>();
        }
        
        if (progressSlider == null)
        {
            Debug.LogError("ProgressBar: No Slider component found!");
            return;
        }
        
        // ตั้งค่าเริ่มต้นของ Slider
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = 0f;
        
        // โหลดข้อมูล stage
        LoadStageData();
        
        // อัพเดทความคืบหน้าเริ่มต้น
        UpdateProgress();
    }
    
    private void LoadStageData()
    {
        // โหลดข้อมูล stage จาก PlayerPrefs
        string stageDataJson = PlayerPrefs.GetString("stageData", "");
        
        if (!string.IsNullOrEmpty(stageDataJson))
        {
            try
            {
                StageDataWrapper wrapper = JsonUtility.FromJson<StageDataWrapper>(stageDataJson);
                totalStages = wrapper.stages.Length;
                Debug.Log($"ProgressBar: Total stages loaded: {totalStages}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ProgressBar: Failed to parse stage data: {e.Message}");
                totalStages = 1; // Default value
            }
        }
        else
        {
            Debug.LogWarning("ProgressBar: No stage data found in PlayerPrefs");
            totalStages = 1; // Default value
        }
        
        // โหลด stage ปัจจุบัน
        currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        Debug.Log($"ProgressBar: Current stage: {currentStage}, Total stages: {totalStages}");
    }
    
    public void UpdateProgress()
    {
        // โหลด stage ปัจจุบันใหม่
        currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        
        if (totalStages <= 0)
        {
            targetProgress = 0f;
            return;
        }
        
        // คำนวณความคืบหน้า (0.0 - 1.0)
        // เมื่อเริ่มต้น stage 0 = 0%
        // เมื่อจบ stage สุดท้าย = 100%
        targetProgress = (float)currentStage / (float)totalStages;
        
        // จำกัดค่าไว้ระหว่าง 0-1
        targetProgress = Mathf.Clamp01(targetProgress);
        
        Debug.Log($"ProgressBar: Progress updated - Stage {currentStage}/{totalStages} = {targetProgress:P1}");
        
        if (animateProgress)
        {
            // ใช้ animation เพื่อเปลี่ยนค่าอย่างนุ่มนวล
            StartCoroutine(AnimateToProgress(targetProgress));
        }
        else
        {
            // เปลี่ยนค่าทันที
            progressSlider.value = targetProgress;
        }
    }
    
    private System.Collections.IEnumerator AnimateToProgress(float target)
    {
        float startValue = progressSlider.value;
        float elapsedTime = 0f;
        float duration = Mathf.Abs(target - startValue) / animationSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // ใช้ smooth curve สำหรับ animation
            t = Mathf.SmoothStep(0f, 1f, t);
            
            progressSlider.value = Mathf.Lerp(startValue, target, t);
            yield return null;
        }
        
        // ตั้งค่าสุดท้ายให้แน่ใจ
        progressSlider.value = target;
    }
    
    // Method สำหรับรีเซ็ตความคืบหน้า
    public void ResetProgress()
    {
        currentStage = 0;
        targetProgress = 0f;
        
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
        
        Debug.Log("ProgressBar: Progress reset to 0%");
    }
    
    // Method สำหรับตั้งค่าความคืบหน้าแบบกำหนดเอง
    public void SetProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
        
        if (animateProgress)
        {
            StartCoroutine(AnimateToProgress(targetProgress));
        }
        else
        {
            progressSlider.value = targetProgress;
        }
    }
    
    // Method สำหรับตั้งค่า stage และ total stages แบบกำหนดเอง
    public void SetStageInfo(int stage, int total)
    {
        currentStage = stage;
        totalStages = total;
        UpdateProgress();
    }
    
    // Properties สำหรับการเข้าถึงข้อมูล
    public float CurrentProgress => progressSlider != null ? progressSlider.value : 0f;
    public int CurrentStage => currentStage;
    public int TotalStages => totalStages;
    public bool IsComplete => currentStage >= totalStages;
    
    // Class สำหรับ stage data (copy from BaseSpeechGame)
    [System.Serializable]
    public class Stage
    {
        public int number;
        public string target;
        public string description;
        public string image;
    }
    
    [System.Serializable]
    public class StageDataWrapper
    {
        public Stage[] stages;
    }
    
    private void OnValidate()
    {
        // ตรวจสอบใน Editor
        if (progressSlider == null)
        {
            progressSlider = GetComponent<Slider>();
        }
    }
}

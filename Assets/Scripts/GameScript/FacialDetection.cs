using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class FacialDetection : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text targetText, descriptionText, feedbackText, detectedFaceText;
    [SerializeField] private RawImage targetFaceImage, feedbackBox;

    [Header("Webcam Settings")]
    private WebCamTexture webCam;
    private CameraSwitcher cameraSwitcher;

    [Header("Sound Effects")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip sfxGameStart, sfxCorrectAnswer, sfxShowSkip, sfxSkip, sfxGameComplete;

    [System.Serializable]
    public class forwardResponse
    {
        public bool isPassed;
        public string inputValue;
        public string feedback;
    }

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

    [System.Serializable]
    public class EndSessionRequest
    {
        public string sessionId;
    }

    [System.Serializable]
    public class EndSessionResponse
    {
        public string message;
    }

    private Stage[] stages;
    private int currentStageIndex = 0;
    private bool isDetecting = false;
    private readonly float detectionInterval = 2f; // Detect every 2 seconds
    private Coroutine detectionCoroutine;

    private void Start()
    {
        PlaySFX(sfxGameStart);
       // หา CameraSwitcher
        cameraSwitcher = FindFirstObjectByType<CameraSwitcher>();
        if (cameraSwitcher == null)
        {
            Debug.LogError("CameraSwitcher not found! Please make sure it exists in the scene.");
            return;
        }

        // Subscribe to camera change events
        CameraSwitcher.OnCameraChanged += OnCameraChanged;

        // รอให้ CameraSwitcher เริ่มทำงานก่อน
        StartCoroutine(WaitForCameraInitialization());

        // Load stage data from PlayerPrefs
        string json = PlayerPrefs.GetString("stageData", "");
        if (!string.IsNullOrEmpty(json))
        {
            StageDataWrapper wrapper = JsonUtility.FromJson<StageDataWrapper>(json);
            stages = wrapper.stages;
        }

        // Setup UI
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);
        feedbackBox.raycastTarget = false;
        targetFaceImage.raycastTarget = false;
    }

    private IEnumerator WaitForCameraInitialization()
    {
        // รอให้ CameraSwitcher เริ่มต้นกล้อง
        yield return new WaitForSeconds(0.5f);
        
        // ใช้กล้องจาก CameraSwitcher
        webCam = cameraSwitcher.GetCurrentCamera();
        if (webCam != null)
        {
            // ใช้ RawImage เดียวกัน (ไม่ต้องสร้าง texture ใหม่)
            Debug.Log("Using camera from CameraSwitcher: " + webCam.deviceName);
        }
        
        LoadCurrentStage();
    }

    private void OnCameraChanged(WebCamTexture newCamera)
    {
        // อัพเดทกล้องเมื่อ CameraSwitcher สลับกล้อง
        webCam = newCamera;
        Debug.Log("Camera switched to: " + (newCamera != null ? newCamera.deviceName : "null"));
    }

    private void LoadCurrentStage()
    {
        if (stages == null || stages.Length == 0) return;

        // Read current stage number
        int stageNumber = PlayerPrefs.GetInt("stageNumber", 0);
        currentStageIndex = System.Array.FindIndex(stages, s => s.number == stageNumber);
        if (currentStageIndex < 0) currentStageIndex = 0;

        // Update UI
        targetText.text = stages[currentStageIndex].target;
        descriptionText.text = stages[currentStageIndex].description;
        // โหลดรูปเป้าหมาย
        StartCoroutine(LoadTargetImage(stages[currentStageIndex].image));

        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);

        // Start facial detection
        StartFacialDetection();

        // Show skip button after 10 seconds
        Invoke("ShowSkip", 10f);
    }

    // โหลดรูปเป้าหมายจาก URL
    private IEnumerator LoadTargetImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                targetFaceImage.texture = tex;
                targetFaceImage.raycastTarget = false;
            }
            else Debug.LogError("Failed to load target image: " + uwr.error);
        }
    }

    private void StartFacialDetection()
    {
        isDetecting = true;
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
        }
        detectedFaceText.text = "ตอนนี้คุณกำลังทำหน้า: " + stages[currentStageIndex].target;
        detectionCoroutine = StartCoroutine(DetectionLoop());
    }

    private void StopFacialDetection()
    {
        isDetecting = false;
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
            detectionCoroutine = null;
        }
    }

    private IEnumerator DetectionLoop()
    {
        while (isDetecting)
        {
            yield return new WaitForSeconds(detectionInterval);

            if (isDetecting)
            {
                CaptureAndSendFrame();
            }
        }
    }

    private Texture2D snapshotTexture;

    private void CaptureAndSendFrame()
    {
        // ตรวจสอบว่ามีกล้องและทำงานอยู่หรือไม่
        if (webCam == null || !webCam.isPlaying)
        {
            Debug.LogWarning("WebCam is not available or not playing");
            return;
        }

        // Reuse Texture2D to avoid allocations
        if (snapshotTexture == null || snapshotTexture.width != webCam.width || snapshotTexture.height != webCam.height)
        {
            if (snapshotTexture != null)
                Destroy(snapshotTexture);
            snapshotTexture = new Texture2D(webCam.width, webCam.height, TextureFormat.RGB24, false);
        }

        snapshotTexture.SetPixels(webCam.GetPixels());
        snapshotTexture.Apply();

        // Convert to byte array
        byte[] imageData = snapshotTexture.EncodeToJPG();

        // Prepare form data
        var formData = new Dictionary<string, object>
        {
            { "sessionId", PlayerPrefs.GetString("sessionId", string.Empty) },
            { "stageNumber", PlayerPrefs.GetInt("stageNumber", 0) },
            { "threshold", 0.7f }
        };

        // Send to server
        StartCoroutine(HttpHelper.PostImageCoroutine<forwardResponse>(
            "https://api.mystrokeapi.uk/game/session/forward",
            formData: formData,
            onSuccess: (response) =>
            {
                CheckFacialExpression(response);
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Facial detection error: {error}, Code: {code}");
            },
            imageData: imageData,
            imageFieldName: "value"
        ));
    }

    private void CheckFacialExpression(forwardResponse response)
    {
        if (response.isPassed)
        {
            PlaySFX(sfxCorrectAnswer);
            Debug.Log("Facial expression correct!");
            StopFacialDetection();
            // แสดงโค้ดที่ตรวจจับได้
            detectedFaceText.text = response.inputValue;
            // แสดง feedback
            feedbackBox.gameObject.SetActive(true);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "เยี่ยมเลย! คุณทำได้ดีมาก";
            CancelInvoke("ShowSkip"); // Cancel skip button if passed
            // รอ 2 วิ แล้วไป stage ถัดไป
            StartCoroutine(ProceedToNextStage());
        }
        else
        {
            Debug.Log("Facial expression incorrect: " + response.feedback);
        }
    }

    // หน่วงเวลา 2 วิ แล้วเรียกไป stage ถัดไป
    private IEnumerator ProceedToNextStage()
    {
        yield return new WaitForSeconds(2f);
        NextStage();
    }

    private void NextStage()
    {
        StopFacialDetection();

        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;

        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();

        Debug.Log($"Next button clicked. Stage number updated to: {nextStage}");

        // Check if this is the last stage
        if (nextStage >= stages.Length)
        {
            Debug.Log("Game completed!");
            OnGameCompleted();
            SceneManager.LoadScene("GameComplete");
            return;
        }

        LoadCurrentStage();
    }

    private void OnSkipButtonClicked()
    {
        PlaySFX(sfxSkip);
        StopFacialDetection();

        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;

        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();

        Debug.Log($"Skip button clicked. Stage number updated to: {nextStage}");

        // Check if this is the last stage
        if (nextStage >= stages.Length)
        {
            Debug.Log("Game completed!");
            OnGameCompleted();
            SceneManager.LoadScene("Home");
            return;
        }

        LoadCurrentStage();
    }

    private void ShowSkip()
    {
        PlaySFX(sfxShowSkip);
        skipButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        CameraSwitcher.OnCameraChanged -= OnCameraChanged;

        StopFacialDetection();
        
        if (snapshotTexture != null)
        {
            Destroy(snapshotTexture);
            snapshotTexture = null;
        }
    }

    protected virtual void EndGameSession()
    {
        string sessionId = PlayerPrefs.GetString("sessionId", string.Empty);
        string authToken = PlayerPrefs.GetString("authToken", string.Empty);
        
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogWarning("EndGameSession: No session ID found");
            return;
        }

        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("EndGameSession: No auth token found");
            return;
        }

        EndSessionRequest requestData = new EndSessionRequest
        {
            sessionId = sessionId
        };

        var headers = new Dictionary<string, string>
        {
            {"Authorization", "Bearer " + authToken }
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<EndSessionRequest, EndSessionResponse>(
            "https://api.mystrokeapi.uk/game/session/end",
            requestData,
            onSuccess: (response) => {
                Debug.Log($"Game session ended successfully: {response.message}");
            },
            onError: (error, code) => {
                Debug.LogError($"Failed to end game session: {error}, Code: {code}");
            },
            additionalHeaders: headers
        ));
    }

    protected virtual void OnGameCompleted()
    {
        PlaySFX(sfxGameComplete);

        EndGameSession();
        Debug.Log($"{gameObject.name} game completed. Stopping all cameras.");
        // หยุดการทำงานของกล้องทั้งหมด
        StopAllCameras();
    }

    private void StopAllCameras()
    {
        // หยุดกล้องจาก CameraSwitcher
        if (cameraSwitcher != null)
        {
            cameraSwitcher.StopCamera();
        }

        // หยุดกล้องอื่น ๆ ที่อาจยังทำงานอยู่
        foreach (var rawImage in Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None))
        {
            if (rawImage.texture is WebCamTexture cam && cam.isPlaying)
            {
                cam.Stop();
                Debug.Log($"Stopped camera: {cam.deviceName}");
            }
        }

        // เคลียร์ reference
        webCam = null;
        Debug.Log("All cameras stopped.");
    }
    
    protected void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
    
    // Method สำหรับเริ่มเกม custom
    public void StartCustomGame(object customGameData)
    {
        Debug.Log($"Starting Facial Detection custom game");
        
        // Store custom game information from PlayerPrefs (set by CustomGameItem)
        string customGameId = PlayerPrefs.GetString("CustomGameId", "");
        string customGameName = PlayerPrefs.GetString("CustomGameName", "");
        string customGameType = PlayerPrefs.GetString("CustomGameType", "");
        string customGameSubtype = PlayerPrefs.GetString("CustomGameSubtype", "");
        
        Debug.Log($"Custom Game Info - ID: {customGameId}, Name: {customGameName}, Type: {customGameType}, Subtype: {customGameSubtype}");
        
        // เริ่มเกมโดยใช้การทำงานเหมือน Start() method
        // แต่สามารถปรับแต่งได้ตามข้อมูล custom game
        PlaySFX(sfxGameStart);
        
        // รอให้ระบบเริ่มต้นเสร็จก่อน
        StartCoroutine(WaitForCameraInitialization());
    }
}

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
    [SerializeField] private RawImage webcamImage, targetFaceImage, feedbackBox;

    private WebCamTexture webCam;

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

    private Stage[] stages;
    private int currentStageIndex = 0;
    private bool isDetecting = false;
    private readonly float detectionInterval = 2f; // Detect every 2 seconds
    private Coroutine detectionCoroutine;

    private void Start()
    {
        PlaySFX(sfxGameStart);
        // Initialize webcam with specific device
        var camDevices = WebCamTexture.devices;
        if (camDevices.Length > 0)
            webCam = new WebCamTexture(camDevices[0].name);
        else
            webCam = new WebCamTexture();
        webCam.Play();
        webcamImage.texture = webCam;
        webcamImage.raycastTarget = false;

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

        LoadCurrentStage();
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
        if (webCam == null || !webCam.isPlaying) return;

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
            SceneManager.LoadScene("Home");
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
        StopFacialDetection();
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
        }
        if (snapshotTexture != null)
        {
            Destroy(snapshotTexture);
            snapshotTexture = null;
        }
    }

    protected virtual void OnGameCompleted()
    {
        PlaySFX(sfxGameComplete);
        // Stop webcam when game completes
        foreach (var raw in Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None))
        {
            if (raw.texture is WebCamTexture cam && cam.isPlaying)
                cam.Stop();
        }
    }
    
    protected void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

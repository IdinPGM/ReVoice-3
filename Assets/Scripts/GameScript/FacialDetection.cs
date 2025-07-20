using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FacialDetection : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button nextButton, skipButton;
    [SerializeField] private TMP_Text targetText, descriptionText, feedbackText, detectedFaceImage;
    [SerializeField] private RawImage webcamImage, targetFaceImage, feedbackBox;

    private WebCamTexture webCam;

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
    private float detectionInterval = 2f; // ตรวจจับทุก 2 วินาที
    private Coroutine detectionCoroutine;

    private void Start()
    {
        // Initialize webcam
        webCam = new WebCamTexture();
        if (!webCam.isPlaying) webCam.Play();
        webcamImage.texture = webCam;

        // Load stage data from PlayerPrefs
        string json = PlayerPrefs.GetString("stageData", "");
        if (!string.IsNullOrEmpty(json))
        {
            StageDataWrapper wrapper = JsonUtility.FromJson<StageDataWrapper>(json);
            stages = wrapper.stages;
        }

        // Setup UI
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);

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

        // Reset UI state
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);

        // Start facial detection
        StartFacialDetection();

        // Show skip button after 10 seconds
        Invoke("ShowSkip", 10f);
    }

    private void StartFacialDetection()
    {
        isDetecting = true;
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
        }
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

    private void CaptureAndSendFrame()
    {
        if (webCam == null || !webCam.isPlaying) return;

        // Capture current frame
        Texture2D snapshot = new Texture2D(webCam.width, webCam.height);
        snapshot.SetPixels(webCam.GetPixels());
        snapshot.Apply();

        // Convert to byte array
        byte[] imageData = snapshot.EncodeToJPG();
        Destroy(snapshot);

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
            onSuccess: (response) => {
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
            Debug.Log("Facial expression correct!");
            StopFacialDetection();
            
            feedbackBox.gameObject.SetActive(true);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "ถูกต้อง! คุณทำใบหน้าได้ดีมาก";
            
            nextButton.gameObject.SetActive(true);
            CancelInvoke("ShowSkip"); // Cancel skip button if passed
        }
        else
        {
            Debug.Log("Facial expression incorrect: " + response.feedback);
            // Continue detecting, don't show feedback for wrong attempts
            // to avoid overwhelming the user
        }
    }

    private void OnNextButtonClicked()
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
            // Return to history or main menu
            // You can add scene transition logic here
            return;
        }
        
        LoadCurrentStage();
    }

    private void OnSkipButtonClicked()
    {
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
            // Return to history or main menu
            return;
        }
        
        LoadCurrentStage();
    }

    private void ShowSkip()
    {
        if (!nextButton.gameObject.activeSelf) // Only show skip if not passed yet
        {
            skipButton.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        StopFacialDetection();
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
        }
    }
}

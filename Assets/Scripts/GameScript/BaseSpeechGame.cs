using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public abstract class BaseSpeechGame : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] protected RawImage webcamImage;
    [SerializeField] protected RawImage feedbackBox;
    [SerializeField] protected Button recordButton, nextButton, skipButton, voiceButton;
    [SerializeField] protected TMP_Text targetText, descriptionText, feedbackText;

    protected WebCamTexture webCam;
    protected AudioClip recordedClip;

    [Header("Sound Effects")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip sfxGameStart, sfxStartRecording, sfxStopRecording, sfxCorrectAnswer, sfxWrongAnswer, sfxShowSkip, sfxNext, sfxSkip, sfxGameComplete;
    protected float startTime;
    protected float recordingLength = 8f;

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

    protected Stage[] stages;
    protected int currentStageIndex = 0;

    // Abstract properties that child classes must implement
    protected abstract float ThresholdValue { get; }
    protected abstract string GameName { get; }
    protected abstract string CorrectMessage { get; }
    protected abstract string RetryMessagePrefix { get; }

    protected virtual void Start()
    {
        PlaySFX(sfxGameStart);
        InitializeWebcam();
        LoadStageData();
        SetupUI();
        LoadCurrentStage();
    }

    protected virtual void InitializeWebcam()
    {
        webCam = new WebCamTexture();
        if (!webCam.isPlaying) webCam.Play();
        webcamImage.texture = webCam;
    }

    protected virtual void LoadStageData()
    {
        string json = PlayerPrefs.GetString("stageData", "");
        if (!string.IsNullOrEmpty(json))
        {
            StageDataWrapper wrapper = JsonUtility.FromJson<StageDataWrapper>(json);
            stages = wrapper.stages;
        }
    }

    protected virtual void SetupUI()
    {
        recordButton.onClick.AddListener(StartRecording);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        voiceButton.onClick.AddListener(OnVoiceButtonClicked);

        // Initial UI state
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);
    }

    protected virtual void LoadCurrentStage()
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
        ResetUIState();

        // Show skip button after 10 seconds
        Invoke("ShowSkip", 10f);
    }

    protected virtual void ResetUIState()
    {
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);
    }

    public virtual void StartRecording()
    {
        PlaySFX(sfxStartRecording);
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected.");
            return;
        }

        string deviceMicrophone = Microphone.devices[0];
        int sampleRate = 16000;
        int lengthSeconds = 8;

        recordedClip = Microphone.Start(deviceMicrophone, false, lengthSeconds, sampleRate);
        startTime = Time.realtimeSinceStartup;
        Debug.Log($"{GameName} recording started.");

        // Change button to stop recording
        recordButton.onClick.RemoveListener(StartRecording);
        recordButton.onClick.AddListener(StopRecording);

        // Update button text
        UpdateRecordButtonText("หยุดบันทึก");
    }

    public virtual void StopRecording()
    {
        PlaySFX(sfxStopRecording);
        Microphone.End(null);
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        Debug.Log($"{GameName} recording stopped.");

        // Update button text back to record
        UpdateRecordButtonText("บันทึกเสียง");

        // Send to server
        SendAudioToServer();

        // Change button back to start recording
        recordButton.onClick.RemoveListener(StopRecording);
        recordButton.onClick.AddListener(StartRecording);
    }

    protected virtual void UpdateRecordButtonText(string text)
    {
        var recordButtonText = recordButton.GetComponentInChildren<TMP_Text>();
        if (recordButtonText != null)
        {
            recordButtonText.text = text;
        }
    }

    protected virtual void SendAudioToServer()
    {
        var formData = new Dictionary<string, object>
        {
            { "sessionId", PlayerPrefs.GetString("sessionId", string.Empty) },
            { "stageNumber", PlayerPrefs.GetInt("stageNumber", 0) },
            { "threshold", ThresholdValue },
        };

        StartCoroutine(HttpHelper.PostAudioCoroutine<forwardResponse>(
            "https://api.mystrokeapi.uk/game/session/forward",
            formData: formData,
            onSuccess: (response) => {
                CheckAnswer(response);
            },
            onError: (error, code) =>
            {
                Debug.LogError($"{GameName} analysis error: {error}, Code: {code}");
                ShowErrorFeedback();
            },
            audioClip: recordedClip,
            audioFieldName: "value"
        ));
    }

    protected virtual void ShowErrorFeedback()
    {
        feedbackBox.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "เกิดข้อผิดพลาดในการประมวลผล กรุณาลองใหม่อีกครั้ง";
    }

    private AudioClip TrimClip(AudioClip clip, float length)
    {
        int samples = (int)(clip.frequency * length);
        float[] data = new float[samples];
        clip.GetData(data, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, samples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);

        return trimmedClip;
    }

    protected virtual void CheckAnswer(forwardResponse response)
    {
        feedbackBox.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);

        if (response.isPassed)
        {
            PlaySFX(sfxCorrectAnswer);
            // เปลี่ยนสีให้เป็นเขียวเมื่อผ่าน
            feedbackText.color = new Color(0.077f, 0.783f, 0.236f);
            Debug.Log($"{GameName} answer is correct");
            feedbackText.text = CorrectMessage + "\nคุณพูดคำว่า " + response.inputValue + "!";
            nextButton.gameObject.SetActive(true);
            CancelInvoke("ShowSkip");
        }
        else
        {
            PlaySFX(sfxWrongAnswer);
            // เปลี่ยนสีให้เป็นแดงเมื่อไม่ผ่าน
            feedbackText.color = Color.red;
            Debug.Log($"{GameName} answer is incorrect: " + response.feedback);
            feedbackText.text = RetryMessagePrefix + response.feedback + "\nคุณพูดคำว่า " + response.inputValue + "!";
            Invoke("HideFeedback", 3f);
        }
    }

    protected virtual void HideFeedback()
    {
        feedbackBox.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }

    protected virtual void OnVoiceButtonClicked()
    {
        Debug.Log($"Playing {GameName} example audio");
        
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    protected virtual void ShowSkip()
    {
        if (!nextButton.gameObject.activeSelf) // Only show skip if not passed yet
        {
            PlaySFX(sfxShowSkip);
            skipButton.gameObject.SetActive(true);
        }
    }

    protected virtual void OnNextButtonClicked()
    {
        PlaySFX(sfxNext);
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Next button clicked. Stage number updated to: {nextStage}");
        
        // Check if this is the last stage
        if (nextStage >= 3)
        {
            Debug.Log($"{GameName} completed!");
            OnGameCompleted();
            // Stop webcam and navigate to
            SceneManager.LoadScene("Home");
            return;
        }
        
        LoadCurrentStage();
    }

    protected virtual void OnSkipButtonClicked()
    {
        PlaySFX(sfxSkip);
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Skip button clicked. Stage number updated to: {nextStage}");
        
        // Check if this is the last stage
        if (nextStage >= stages.Length)
        {
            Debug.Log($"{GameName} completed!");
            OnGameCompleted();
            // Stop webcam and navigate to Home
            SceneManager.LoadScene("Home");
            return;
        }
        
        LoadCurrentStage();
    }

    protected virtual void OnGameCompleted()
    {
        PlaySFX(sfxGameComplete);
        // Stop webcam when game completes
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
            webcamImage.texture = null; // Clear webcam texture
        }
        // Override for additional completion logic if needed
    }

    // Helper method สำหรับเล่น SFX
    protected void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

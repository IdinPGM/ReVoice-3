using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionalSpeech : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button voiceButton;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RawImage WebcamImage;
    [SerializeField] private TMP_Text feedbackText;

    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    private float startTime;
    private float recordingLength = 8f; // Duration of recording in seconds

    private WebCamTexture webCam;

    [System.Serializable]
    public class forwardResponse
    {
        public bool isPassed;
        public string inputValue;
        public string feedback;
    }

    private void Start()
    {
        webCam = new WebCamTexture();
        if (!webCam.isPlaying) webCam.Play();
        WebcamImage.texture = webCam;

        recordButton.onClick.AddListener(StartRecording);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        voiceButton.onClick.AddListener(OnVoiceButtonClicked);
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        Invoke("Show", 15f);
    }

    public void StartRecording()
    {
        string deviceMicrophone = Microphone.devices[0];
        int sampleRate = 16000; // Sample rate
        int lenghtSeconds = 8; // Duration of recording in seconds

        if (Microphone.devices.Length > 0)
        {
            recordedClip = Microphone.Start(deviceMicrophone, false, lenghtSeconds, sampleRate);
            startTime = Time.realtimeSinceStartup;
            Debug.Log("Recording started.");

            // Change button to stop recording
            recordButton.onClick.RemoveListener(StartRecording);
            recordButton.onClick.AddListener(StopRecording);


        }
        else
        {
            Debug.LogError("No microphone detected.");
        }
    }

    public void StopRecording()
    {
        Microphone.End(null);
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        Debug.Log("Recording stopped.");


        // Make the record button inactive
        recordButton.gameObject.SetActive(false);

        var formData = new Dictionary<string, object>
        {
            { "sessionId", PlayerPrefs.GetString("sessionId", string.Empty) },
            { "stageNumber", PlayerPrefs.GetInt("stageNumber", 0) },
            { "threshold", 0.5f }, // Example threshold value
        };

        // Send data to server
        StartCoroutine(HttpHelper.PostAudioCoroutine<forwardResponse>(
            "https://api.mystrokeapi.uk/game/session/forward",
            formData: formData,
            onSuccess: (response) => {
                checkAnswer(response);
            },
            onError: (error, code) => {
                Debug.LogError($"Error: {error}, Code: {code}");
                feedbackText.text = "An error occurred while processing your response.";
            },
            audioClip: recordedClip,
            audioFieldName: "value"
        ));

        // Change button to start recording again
        recordButton.onClick.RemoveListener(StopRecording);
        recordButton.onClick.AddListener(StartRecording);
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

    private void checkAnswer(forwardResponse response)
    {
        if (response.isPassed)
        {
            Debug.Log("Answer is correct");
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Correct!";
            nextButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Answer is incorrect");
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Incorrect. " + response.feedback;
        }
    }

    private void OnVoiceButtonClicked()
    {
        // Logic to handle voice button click
        Debug.Log("Voice button clicked");
    }

    private void OnNextButtonClicked()
    {
        // แก้ไขจาก PlayerPrefs.GetInt("stageNumber", + 1); ที่ผิด
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Next button clicked. Stage number updated to: {nextStage}");
        
        // เพิ่มการตรวจสอบว่าเป็น stage สุดท้ายหรือไม่
        // (ถ้าจำเป็น สามารถเพิ่ม logic การจบเกมที่นี่)
        
        // อัปเดต UI หรือโหลด stage ถัดไป
        // LoadNextStage();
    }

    private void OnSkipButtonClicked()
    {
        // เพิ่ม stage number เมื่อข้าม
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Skip button clicked. Stage number updated to: {nextStage}");
        
        // โหลด stage ถัดไป
        // LoadNextStage();
    }

    private void Show()
    {
        skipButton.gameObject.SetActive(true);
    }
    
    private void OnDestroyCamera()
    {
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class LanguageTherapy : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button recordButton, nextButton, skipButton, voiceButton;
    [SerializeField] private TMP_Text questionText, descriptionText, feedbackText;
    [SerializeField] private RawImage webcamImage, feedbackBox;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    private WebCamTexture webCam;
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    private float startTime;
    private float recordingLength = 8f;
    private string selectedChoice = "";

    [System.Serializable]
    public class forwardResponse
    {
        public bool isPassed;
        public string inputValue;
        public string feedback;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
        public string value;
    }

    [System.Serializable]
    public class Stage
    {
        public int number;
        public string target; // The question
        public string description;
        public string image;
        public Choice[] choices; // Multiple choice options
    }

    [System.Serializable]
    public class StageDataWrapper
    {
        public Stage[] stages;
    }

    private Stage[] stages;
    private int currentStageIndex = 0;
    private List<Button> choiceButtons = new List<Button>();

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

        // Setup buttons
        recordButton.onClick.AddListener(StartRecording);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        voiceButton.onClick.AddListener(OnVoiceButtonClicked);

        // Initial UI state
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);
        recordButton.interactable = false; // Disable until choice is selected

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
        questionText.text = stages[currentStageIndex].target;
        descriptionText.text = stages[currentStageIndex].description;

        // Reset UI state
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        feedbackBox.gameObject.SetActive(false);
        recordButton.interactable = false;
        selectedChoice = "";

        // Create choice buttons
        CreateChoiceButtons();

        // เล่นเสียง questionText โดยอัตโนมัติเมื่อโหลด stage
        StartCoroutine(PlayQuestionTextWithDelay(1f));

        // Show skip button after 10 seconds
        Invoke("ShowSkip", 10f);
    }

    private void CreateChoiceButtons()
    {
        // Clear existing buttons
        foreach (Button btn in choiceButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        choiceButtons.Clear();

        // Create new choice buttons
        if (stages[currentStageIndex].choices != null)
        {
            foreach (Choice choice in stages[currentStageIndex].choices)
            {
                GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer);
                Button choiceBtn = choiceObj.GetComponent<Button>();
                TMP_Text choiceText = choiceObj.GetComponentInChildren<TMP_Text>();

                if (choiceText != null)
                {
                    choiceText.text = choice.text;
                }

                // Add click listener
                string choiceValue = choice.value;
                choiceBtn.onClick.AddListener(() => OnChoiceSelected(choiceValue, choiceBtn));

                choiceButtons.Add(choiceBtn);
            }
        }
    }

    private void OnChoiceSelected(string choiceValue, Button selectedButton)
    {
        selectedChoice = choiceValue;
        
        // Update button visuals to show selection
        foreach (Button btn in choiceButtons)
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }

        // Highlight selected button
        var selectedColors = selectedButton.colors;
        selectedColors.normalColor = Color.yellow;
        selectedButton.colors = selectedColors;

        // Enable record button
        recordButton.interactable = true;

        Debug.Log($"Choice selected: {choiceValue}");
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(selectedChoice))
        {
            Debug.LogWarning("Please select a choice before recording.");
            return;
        }

        string deviceMicrophone = Microphone.devices[0];
        int sampleRate = 16000;
        int lengthSeconds = 8;

        if (Microphone.devices.Length > 0)
        {
            recordedClip = Microphone.Start(deviceMicrophone, false, lengthSeconds, sampleRate);
            startTime = Time.realtimeSinceStartup;
            Debug.Log("Recording language therapy response started.");

            // Change button to stop recording
            recordButton.onClick.RemoveListener(StartRecording);
            recordButton.onClick.AddListener(StopRecording);

            // Update button text
            var recordButtonText = recordButton.GetComponentInChildren<TMP_Text>();
            if (recordButtonText != null)
            {
                recordButtonText.text = "หยุดบันทึก";
            }
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
        Debug.Log("Language therapy recording stopped.");

        // Update button text back to record
        var recordButtonText = recordButton.GetComponentInChildren<TMP_Text>();
        if (recordButtonText != null)
        {
            recordButtonText.text = "บันทึกเสียง";
        }

        var formData = new Dictionary<string, object>
        {
            { "sessionId", PlayerPrefs.GetString("sessionId", string.Empty) },
            { "stageNumber", PlayerPrefs.GetInt("stageNumber", 0) },
            { "threshold", 0.5f },
            { "selectedChoice", selectedChoice } // Include selected choice for validation
        };

        // Send data to server for language therapy analysis
        StartCoroutine(HttpHelper.PostAudioCoroutine<forwardResponse>(
            "https://api.mystrokeapi.uk/game/session/forward",
            formData: formData,
            onSuccess: (response) => {
                CheckLanguageResponse(response);
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Language therapy analysis error: {error}, Code: {code}");
                feedbackBox.gameObject.SetActive(true);
                feedbackText.gameObject.SetActive(true);
                feedbackText.text = "เกิดข้อผิดพลาดในการประมวลผล กรุณาลองใหม่อีกครั้ง";
            },
            audioClip: recordedClip,
            audioFieldName: "value"
        ));

        // Change button back to start recording
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

    private void CheckLanguageResponse(forwardResponse response)
    {
        feedbackBox.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);

        if (response.isPassed)
        {
            Debug.Log("Language therapy response correct!");
            feedbackText.text = "ยอดเยี่ยม! คุณตอบและพูดได้ถูกต้อง";
            nextButton.gameObject.SetActive(true);
            CancelInvoke("ShowSkip"); // Cancel skip button if passed
        }
        else
        {
            Debug.Log("Language therapy response incorrect: " + response.feedback);
            feedbackText.text = "ลองใหม่อีกครั้ง: " + response.feedback;
            // Hide feedback after a few seconds to allow retry
            Invoke("HideFeedback", 4f);
        }
    }

    private void HideFeedback()
    {
        feedbackBox.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }

    private void OnVoiceButtonClicked()
    {
        // ใช้ TTS เพื่อเล่นเสียงคำถาม (questionText)
        Debug.Log("Playing question using TTS");
        
        if (questionText != null && !string.IsNullOrEmpty(questionText.text))
        {
            StartCoroutine(SpeakText(questionText.text));
        }
        else
        {
            Debug.LogWarning("Question text is empty or null");
            
            // Fallback: เล่นเสียงจาก AudioSource หากมี
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }

    // Coroutine สำหรับ Text-to-Speech
    private IEnumerator SpeakText(string text)
    {
        Debug.Log($"TTS: Speaking text - {text}");
        
        // URL encode ข้อความ
        string encodedText = UnityEngine.Networking.UnityWebRequest.EscapeURL(text);
        
        // ใช้ Google Translate TTS API
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={encodedText}&tl=th&client=tw-ob";
        
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            // เพิ่ม headers เพื่อหลีกเลี่ยงการถูกบล็อก
            www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                if (clip != null && audioSource != null)
                {
                    // หยุดเสียงปัจจุบันก่อน
                    if (audioSource.isPlaying)
                        audioSource.Stop();
                        
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log($"TTS: Successfully played text - {text}");
                }
                else
                {
                    Debug.LogError("TTS: Failed to create audio clip");
                }
            }
            else
            {
                Debug.LogError($"TTS: Failed to get audio: {www.error}");
                
                // Fallback: แสดงข้อความในคอนโซลแทน
                Debug.Log($"TTS Fallback: {text}");
            }
        }
    }

    // Coroutine สำหรับเล่นเสียง questionText หลังจากหน่วงเวลา
    private IEnumerator PlayQuestionTextWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (questionText != null && !string.IsNullOrEmpty(questionText.text))
        {
            Debug.Log($"Auto-playing question text: {questionText.text}");
            yield return StartCoroutine(SpeakText(questionText.text));
        }
    }

    private void OnNextButtonClicked()
    {
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Next button clicked. Stage number updated to: {nextStage}");
        
        // Check if this is the last stage
        if (nextStage >= stages.Length)
        {
            Debug.Log("Language Therapy completed!");
            // Return to history or main menu
            return;
        }
        
        LoadCurrentStage();
    }

    private void OnSkipButtonClicked()
    {
        int currentStage = PlayerPrefs.GetInt("stageNumber", 0);
        int nextStage = currentStage + 1;
        
        PlayerPrefs.SetInt("stageNumber", nextStage);
        PlayerPrefs.Save();
        
        Debug.Log($"Skip button clicked. Stage number updated to: {nextStage}");
        
        // Check if this is the last stage
        if (nextStage >= stages.Length)
        {
            Debug.Log("Language Therapy completed!");
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
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
        }
    }
}

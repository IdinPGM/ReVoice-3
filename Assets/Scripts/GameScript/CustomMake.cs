using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CustomMake : MonoBehaviour
{
    [SerializeField] Button customButtons;

    private void Start()
    {
        // กำหนดให้ปุ่มไปที่ level 3 ของ functional speech
        customButtons.onClick.AddListener(GoToFunctionalSpeechLevel3);
    }

    private void GoToFunctionalSpeechLevel3()
    {
        Debug.Log("Going to Functional Speech Level 3");

        // ตั้งค่าสำหรับ functional speech
        PlayerPrefs.SetString("gameType", "functional_speech");
        PlayerPrefs.SetFloat("difficulty", 0.9f);
        
        // ใช้ Level ID ที่กำหนดไว้แล้ว
        string levelId = "f117cf03-7323-43db-9979-d50265c25628";
        PlayerPrefs.SetString("levelId", levelId);
        PlayerPrefs.Save();

        // เริ่ม session
        StartLevel(levelId);
    }

    private void StartLevel(string levelId)
    {
        Debug.Log($"Starting level: {levelId}");

        // ข้อมูลสำหรับส่งไป server
        var requestData = new SessionStartRequest
        {
            levelId = levelId,
            isCustom = false
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<SessionStartRequest, SessionStartResponse>(
            "https://api.mystrokeapi.uk/game/session/start",
            requestData,
            onSuccess: (response) =>
            {
                // บันทึก session ID
                PlayerPrefs.SetString("sessionId", response.sessionId);
                
                // บันทึกข้อมูล stage
                string stageDataJson = JsonUtility.ToJson(new StageDataWrapper { stages = response.stage });
                PlayerPrefs.SetString("stageData", stageDataJson);
                
                // เริ่มที่ stage แรก
                if (response.stage.Length > 0)
                {
                    PlayerPrefs.SetInt("stageNumber", response.stage[0].number);
                }
                
                PlayerPrefs.Save();
                Debug.Log($"Session started: {response.sessionId}");

                // ไปที่เกม Functional Speech
                SceneManager.LoadScene("Functional Speech");
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Failed to start session: {error} (Code: {code})");
            },
            additionalHeaders: new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + PlayerPrefs.GetString("authToken", string.Empty) }
            }
        ));
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
    public class SessionStartResponse
    {
        public string sessionId;
        public Stage[] stage;
    }

    [System.Serializable]
    public class SessionStartRequest
    {
        public string levelId;
        public bool isCustom;
    }
}
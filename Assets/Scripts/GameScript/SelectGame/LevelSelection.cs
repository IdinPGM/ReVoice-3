using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.ComponentModel.Design;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] Button[] levelButtons; // Array of buttons for levels 1-15
    private string gameType;
    private float difficulty;

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

    [System.Serializable]
    public class Level
    {
        public string id;
        public string type;
        public string subtype;
        public string name;
        public string description;
    }

    [System.Serializable]
    public class GetLevelResponse
    {
        public Level[] levels;
    }

    private void Start()
    {
        // Assign click events to level buttons
        for (int i = 0; i < levelButtons.Length && i < 15; i++)
        {
            int level = i + 1; // Levels start from 1
            levelButtons[i].gameObject.SetActive(false);
        }


        // Retrieve Game ID and Difficulty ID from PlayerPrefs
        gameType = PlayerPrefs.GetString("gameType", "default_game");

        if (gameType == "default_game")
        {
            // If no game type is set, return to the Game Menu
            Debug.LogWarning("No game type set. Returning to Game Menu.");
            SceneManager.LoadScene("Home");
            return; // Exit early to avoid further processing
        }

        difficulty = PlayerPrefs.GetFloat("difficulty", 0.6f);
        Debug.Log($"Loaded Game ID: {gameType}, Difficulty ID: {difficulty}");

        // Load levels from the server
        StartCoroutine(HttpHelper.GetRequestCoroutine<GetLevelResponse>(
            $"https://api.mystrokeapi.uk/game/main-levels?page=1&limit=15&type={gameType}",
            onSuccess: (response) =>
            {
                Debug.Log($"Received {response.levels.Length} levels for game type: {gameType}");
                for (int i = 0; i < response.levels.Length && i < levelButtons.Length; i++)
                {
                    Level level = response.levels[i];
                    levelButtons[i].gameObject.SetActive(true);
                    levelButtons[i].onClick.AddListener(() => StartLevel(level.id));
                }
            },
            onError: (error, code) =>
            {
                Debug.LogError($"Failed to load levels: {error} (Code: {code})");
            }
        ));

    }

    private void StartLevel(string levelId)
    {
        PlayerPrefs.SetString("levelId", levelId);
        PlayerPrefs.Save();
        Debug.Log($"Level ID set to: {levelId}");

        // Prepare request data
        var requestData = new SessionStartRequest
        {
            levelId = levelId,
            isCustom = false // Assuming this is not a custom level
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<SessionStartRequest, SessionStartResponse>(
            "https://api.mystrokeapi.uk/game/session/start",
            requestData,
            onSuccess: (response) =>
            {
                // Save sessionId to PlayerPrefs
                PlayerPrefs.SetString("sessionId", response.sessionId);
                PlayerPrefs.Save();
                Debug.Log($"Session started with ID: {response.sessionId}");

                // save stage data as JSON
                string stageDataJson = JsonUtility.ToJson(new StageDataWrapper { stages = response.stage });
                PlayerPrefs.SetString("stageData", stageDataJson);
                PlayerPrefs.Save();
                Debug.Log($"Stage data saved as JSON: {stageDataJson}");

                // Set stage number in PlayerPrefs
                if (response.stage.Length > 0)
                {
                    PlayerPrefs.SetInt("stageNumber", response.stage[0].number);
                    PlayerPrefs.Save();
                    Debug.Log($"Stage number set to: {response.stage[0].number}");
                }

                // Load the game scene
                string sceneName = GetSceneNameFromGameType(gameType);
                Debug.Log($"Loading scene: {sceneName} for game type: {gameType}");
                SceneManager.LoadScene(sceneName);
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

    private Dictionary<string, string> gameTypeToSceneMap = new Dictionary<string, string>
    {   
        {"facial_detection", "Facial Detection"},
        { "functional_speech", "Functional Speech"},
        { "phoneme_practice", "Phoneme Practice"},
        {"language_therapy", "Language Therapy"},
    };

    private string GetSceneNameFromGameType(string gameType)
    {
        string lowerGameType = gameType.ToLower();
        
        if (gameTypeToSceneMap.ContainsKey(lowerGameType))
        {
            return gameTypeToSceneMap[lowerGameType];
        }
        
        Debug.LogWarning($"Unknown game type: {gameType}. Defaulting to Home.");
        return "Home"; // Default scene if game type is unknown
    }
}
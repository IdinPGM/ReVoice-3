using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Streak : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI streakText;

    [System.Serializable]
    public class StreakData
    {
        public int currentStreak;
        public int longestStreak;
    }

    [System.Serializable]
    public class StreakResponse
    {
        public StreakData streak;
    }

    private void Start()
    {   
        streakText.text = "...";

        string authToken = PlayerPrefs.GetString("authToken", string.Empty);
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("No authentication token found. User may not be logged in.");
            return;
        }

        var headers = new Dictionary<string, string>
        {
            {"Authorization", "Bearer " + authToken }
        };

        StartCoroutine(HttpHelper.GetRequestCoroutine<StreakResponse>(
            "https://api.mystrokeapi.uk/user/streak",
            onSuccess: (response) => {
                Debug.Log("Streak data received successfully.");
                streakText.text = $"{response.streak.currentStreak}";
            },
            onError: (error, code) => {
                Debug.LogError($"Failed to fetch streak data: {error} (Code: {code})");
                streakText.text = "???";
            },
            additionalHeaders: headers
        ));
    }
}


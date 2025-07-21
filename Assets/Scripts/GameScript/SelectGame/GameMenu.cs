using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [SerializeField] Button facialDetectionButton;
    [SerializeField] Button functionalSpeechButton;
    [SerializeField] Button phonemePracticeButton;
    // [SerializeField] Button languageTherapyButton;

    // Define Game IDs for each game mode
    private const string FACIAL_DETECTION_ID = "facial_detection";
    private const string FUNCTIONAL_SPEECH_ID = "functional_speech";
    private const string PHONEME_PRACTICE_ID = "phoneme_practice";
    // private const string LANGUAGE_THERAPY_ID = "language_therapy";

    private void Start()
    {
        // Check if the user is logged in
        string authToken = PlayerPrefs.GetString("authToken", string.Empty);
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("No authentication token found. User may not be logged in.");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        // Assign button click events
        facialDetectionButton.onClick.AddListener(() => GoToDifficultySelection(FACIAL_DETECTION_ID));
        functionalSpeechButton.onClick.AddListener(() => GoToDifficultySelection(FUNCTIONAL_SPEECH_ID));
        phonemePracticeButton.onClick.AddListener(() => GoToDifficultySelection(PHONEME_PRACTICE_ID));
        // languageTherapyButton.onClick.AddListener(() => GoToDifficultySelection(LANGUAGE_THERAPY_ID));
    }

    private void GoToDifficultySelection(string gameType)
    {
        // Save Game ID to PlayerPrefs
        PlayerPrefs.SetString("gameType", gameType);
        PlayerPrefs.Save(); // Ensure the data is saved
        Debug.Log($"Game Type set to: {gameType}");

        // Load the difficulty selection scene
        SceneManager.LoadScene("Difficulty Selection");
    }
}
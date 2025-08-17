using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DifficultySelection : MonoBehaviour
{
    [SerializeField] Button easyButton;
    [SerializeField] Button normalButton;
    [SerializeField] Button hardButton;

    private string gameType;
    private const float EASY_DIFFICULTY = 0.5f;
    private const float NORMAL_DIFFICULTY = 0.7f;
    private const float HARD_DIFFICULTY = 0.9f;

    private void Start()
    {
        // Retrieve Game ID from PlayerPrefs
        gameType = PlayerPrefs.GetString("gameType", "default_game");
        Debug.Log($"Loaded Game Type: {gameType}");

        if (gameType == "default_game")
        {
            // GO back to Game Menu if no game type is set
            Debug.LogWarning("No game type set. Returning to Game Menu.");
            SceneManager.LoadScene("Home");
            return; // Exit early to avoid further processing
        }

        // Assign button click events
        easyButton.onClick.AddListener(() => GoToLevelSelection(EASY_DIFFICULTY));
        normalButton.onClick.AddListener(() => GoToLevelSelection(NORMAL_DIFFICULTY));
        hardButton.onClick.AddListener(() => GoToLevelSelection(HARD_DIFFICULTY));
    }

    private void GoToLevelSelection(float difficulty)
    {
        // Save Difficulty ID to PlayerPrefs
        PlayerPrefs.SetFloat("difficulty", difficulty);
        PlayerPrefs.Save();
        Debug.Log($"Difficulty set to: {difficulty}");

        // Load the level selection scene
        SceneManager.LoadScene("Select Level");
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI errorMessage;

    [System.Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string token;
    }
    
    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        errorMessage.gameObject.SetActive(false);
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Username and password cannot be empty.");
            return;
        }

        var loginData = new LoginRequest
        {
            email = email,
            password = password
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<LoginRequest, LoginResponse>(
            "https://api.mystrokeapi.uk/auth/api/sign-in/email",
            loginData,
            onSuccess: (response) => {
                PlayerPrefs.SetString("authToken", response.token);
                PlayerPrefs.Save();
                Debug.Log("Login successful, token saved.");
                SceneManager.LoadScene("Home");
            },
            onError: (error, code) => {
                Debug.LogError($"Login failed: {error} (Code: {code})");
                ShowError($"Login failed: {error}");
            }
        ));
    }

    private void ShowError(string message)
    {
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);
    }
}
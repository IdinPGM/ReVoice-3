using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



public class Register : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    public Button registerButton;
    public TextMeshProUGUI errorMessage;

    [System.Serializable]
    private class RegisterRequest
    {
        public string name;
        public string email;
        public string password;
    }

    [System.Serializable]
    private class RegisterResponse
    {
        public string token;
    }
    
     private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        errorMessage.gameObject.SetActive(false);
    }

    public void OnRegisterButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();
        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            ShowError("Username, password, and email cannot be empty.");
            return;
        }

        if (password.Length < 7)
        {
            ShowError("Password must be at least 7 characters long.");
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowError("Please enter a valid email address.");
            return;
        }

        if (username.Length < 3)
        {
            ShowError("Username must be at least 3 characters long.");
            return;
        }

        var registerData = new RegisterRequest
        {
            name = username,
            email = email,
            password = password,
        };

        StartCoroutine(HttpHelper.PostRequestCoroutine<RegisterRequest, RegisterResponse>(
            "https://api.mystrokeapi.uk/auth/api/sign-up/email",
            registerData,
            onSuccess: (response) => {
                PlayerPrefs.SetString("authToken", response.token);
                PlayerPrefs.Save();
                Debug.Log("Registration successful, token saved.");
                SceneManager.LoadScene("Home");
            },
            onError: (error, code) => {
                Debug.LogError($"Registration failed: {error} (Code: {code})");
                ShowError($"Registration failed: {error}");
            }
        ));
    }

    private void ShowError(string message)
    {
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);
    }
}

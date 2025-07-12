using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using System;

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

        // StartCoroutine(LoginCoroutine(loginData,
        //     onSuccess: (response) => {
        //         PlayerPrefs.SetString("authToken", response.token);
        //         PlayerPrefs.Save();
        //         Debug.Log("Login successful, token saved.");
        //         SceneManager.LoadScene("Home");
        //     },
        //     onError: (error, code) => {
        //         Debug.LogError($"Login failed: {error} (Code: {code})");
        //         ShowError($"Login failed: {error}");
        //     }
        // ));

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

    // private IEnumerator LoginCoroutine(LoginRequest loginData,
    //     Action<LoginResponse> onSuccess,
    //     Action<string, long> onError
    // ) 
    // {
    //     string endpoint = "https://api.mystrokeapi.uk/auth/api/sign-in/email";

    //     string jsonData = JsonUtility.ToJson(loginData);

    //     byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

    //     using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
    //     {
    //         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //         request.downloadHandler = new DownloadHandlerBuffer();
    //         request.SetRequestHeader("Content-Type", "application/json");

    //         yield return request.SendWebRequest();

    //         if (request.result == UnityWebRequest.Result.Success)
    //         {
    //             Debug.Log($"Request Success: {request.downloadHandler.text}");

    //             try
    //             {   
    //                 // Change here!!
    //                 LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
    //                 Debug.Log($"Login Success: {response.token}");
    //                 onSuccess?.Invoke(response);
    //             }
    //             catch (Exception ex)
    //             {
    //                 Debug.LogError($"JSON Parsing Error: {ex.Message}");
    //                 onError?.Invoke($"Failed to parse login response.: {ex.Message}", request.responseCode);
    //             }
    //         }
    //         else
    //         {
    //             string errorMessage = request.error;
    //             Debug.LogError($"Request Error: {errorMessage}");
    //             onError?.Invoke(errorMessage, request.responseCode);
    //         }
    //     }
    // }

    private void ShowError(string message)
    {
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);
    }
}
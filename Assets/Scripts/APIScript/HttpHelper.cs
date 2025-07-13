using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

public static class HttpHelper
{
    public static IEnumerator PostRequestCoroutine<TRequest, TResponse>(
        string endpoint,
        TRequest requestData,
        Action<TResponse> onSuccess,
        Action<string, long> onError,
        Dictionary<string, string> additionalHeaders = null
    )
    {
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request Success: {request.downloadHandler.text}");

                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    Debug.Log($"Response: {response}");
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    onError?.Invoke($"Failed to parse response: {ex.Message}", request.responseCode);
                }
            }
            else
            {
                string errorMessage = request.error;
                Debug.LogError($"Request Error: {errorMessage}");
                onError?.Invoke(errorMessage, request.responseCode);
            }
        }
    }

    public static IEnumerator GetRequestCoroutine<TResponse>(
        string endpoint,
        Action<TResponse> onSuccess,
        Action<string, long> onError,
        Dictionary<string, string> additionalHeaders = null
    )
    {
        using (UnityWebRequest request = new UnityWebRequest(endpoint, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request Success: {request.downloadHandler.text}");

                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    Debug.Log($"Response: {response}");
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    onError?.Invoke($"Failed to parse response: {ex.Message}", request.responseCode);
                }
            }
            else
            {
                string errorMessage = request.error;
                Debug.LogError($"Request Error: {errorMessage}");
                onError?.Invoke(errorMessage, request.responseCode);
            }
        }
    }

    public static IEnumerator PostAudioCoroutine<TResponse>(
        string endpoint,
        Dictionary<string, object> formData,
        Action<TResponse> onSuccess,
        Action<string, long> onError,
        AudioClip audioClip = null,
        string audioFieldName = "audio",
        Dictionary<string, string> additionalHeaders = null
    )
    {
        WWWForm form = new WWWForm();

        if (audioClip != null)
        {
            byte[] audioData = WavUtility.ConvertAudioClipToWav(audioClip);
            form.AddBinaryData(audioFieldName, audioData, $"{audioFieldName}.wav", "audio/wav");
        }

        if (formData != null)
        {
            foreach (var kvp in formData)
            {
                if (kvp.Value != null)
                {
                    form.AddField(kvp.Key, kvp.Value.ToString());
                }
            }
        }

        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Form Upload Success: {request.downloadHandler.text}");

                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    Debug.Log($"Response: {response}");
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    onError?.Invoke($"Failed to parse response: {ex.Message}", request.responseCode);
                }
            }
            else
            {
                string errorMessage = request.error;
                Debug.LogError($"Form Upload Error: {errorMessage}");
                onError?.Invoke(errorMessage, request.responseCode);
            }
        }
        
    }

    // public static IEnumerator PostAudioFileCoroutine<TResponse>(
    //     string endpoint,
    //     string filePath,
    //     string sessionId,
    //     int stageNumber,
    //     Action<TResponse> onSuccess,
    //     Action<string, long> onError,
    //     Dictionary<string, string> additionalHeaders = null)
    // {
    //     if (!File.Exists(filePath))
    //     {
    //         onError?.Invoke("Audio file not found.", 0);
    //         yield break;
    //     }

    //     byte[] fileData = File.ReadAllBytes(filePath);
    //     WWWForm form = new WWWForm();
    //     form.AddBinaryData("value", fileData, Path.GetFileName(filePath), "audio/wav");
    //     form.AddField("sessionId", sessionId);
    //     form.AddField("stageNumber", stageNumber.ToString());

    //     using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form))
    //     {
    //         if (additionalHeaders != null)
    //         {
    //             foreach (var header in additionalHeaders)
    //             {
    //                 request.SetRequestHeader(header.Key, header.Value);
    //             }
    //         }

    //         yield return request.SendWebRequest();

    //         if (request.result == UnityWebRequest.Result.Success)
    //         {
    //             Debug.Log($"Audio Upload Success: {request.downloadHandler.text}");

    //             try
    //             {
    //                 TResponse response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
    //                 Debug.Log($"Response: {response}");
    //                 onSuccess?.Invoke(response);
    //             }
    //             catch (Exception ex)
    //             {
    //                 Debug.LogError($"JSON Parsing Error: {ex.Message}");
    //                 onError?.Invoke($"Failed to parse response: {ex.Message}", request.responseCode);
    //             }
    //         }
    //         else
    //         {
    //             string errorMessage = request.error;
    //             Debug.LogError($"Audio Upload Error: {errorMessage}");
    //             onError?.Invoke(errorMessage, request.responseCode);
    //         }
    //     }
    // }
}





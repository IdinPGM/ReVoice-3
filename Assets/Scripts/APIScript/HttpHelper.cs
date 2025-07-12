using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

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
        using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
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
}

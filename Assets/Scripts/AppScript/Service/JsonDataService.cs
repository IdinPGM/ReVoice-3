using System;
using System.IO;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string key, T data, bool encrypted)
    {
        string path = Path.Combine(Application.persistentDataPath, key + ".json");

        try
        {
            if (File.Exists(path))
            {
                Debug.Log("Data exists, overwriting!");
                File.Delete(path);
            }
            else
            {
                Debug.Log("Data does not exist, creating new file!");
            }
            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonUtility.ToJson(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message} {e.StackTrace}");
            return false;
        }
    }

    public T LoadData<T>(string key, bool encrypted)
    {
        string path = Path.Combine(Application.persistentDataPath, key + ".json");
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("Data file not found!");
                return default(T);
            }
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message} {e.StackTrace}");
            return default(T);
        }
    }
}
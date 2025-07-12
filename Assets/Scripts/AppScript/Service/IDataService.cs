using UnityEngine;

public interface IDataService
{
    bool SaveData<T>(string key, T data, bool encrypted);

    T LoadData<T>(string key, bool encrypted);
}
using UnityEngine;
using System.IO;

public class RecordingAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    private string filePath = "recordedAudio.wav";
    private string directoryPath = "Recordings";
    private float startTime;
    private float recordingLength = 8f; // Duration of recording in seconds

    private void Awake()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void StartRecording()
    {
        string deviceMicrophone = Microphone.devices[0];
        int sampleRate = 16000; // Sample rate
        int lenghtSeconds = 8; // Duration of recording in seconds

        if (Microphone.devices.Length > 0)
        {
            recordedClip = Microphone.Start(deviceMicrophone, false, lenghtSeconds, sampleRate);
            startTime = Time.realtimeSinceStartup;
            Debug.Log("Recording started.");
        }
        else
        {
            Debug.LogError("No microphone detected.");
        }
    }

    public void StopRecording()
    {
        Microphone.End(null);
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        SaveRecording();
        Debug.Log("Recording stopped.");
    }

    public void SaveRecording()
    {
        if (recordedClip != null)
        {
            string fullPath = Path.Combine(directoryPath, filePath);
            // WavUtility.Save(fullPath, recordedClip);
            // Debug.Log($"Recording saved to {fullPath}");
        }
        else
        {
            Debug.LogWarning("No audio clip found to save.");
        }
    }

    private AudioClip TrimClip(AudioClip clip, float length)
    {
        int samples = (int)(clip.frequency * length);
        float[] data = new float[samples];
        clip.GetData(data, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, samples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);

        return trimmedClip;
    }

    // public void PlayRecordedAudio()
    // {
    //     if (recordedClip != null)
    //     {
    //         audioSource.clip = recordedClip;
    //         audioSource.Play();
    //         Debug.Log("Playing recorded audio.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No audio clip to play.");
    //     }
    // }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionalSpeech : BaseSpeechGame
{
    // Override abstract properties
    protected override float ThresholdValue => PlayerPrefs.GetFloat("difficulty", 0.6f);
    protected override string GameName => "functional_speech";
    protected override string CorrectMessage => "เก่งมากกกก! พูดถูกต้องเลย";
    protected override string RetryMessagePrefix => "เอาใหม่นะๆ: ";
    
    // Method สำหรับเริ่มเกม custom
    public void StartCustomGame(object customGameData)
    {
        Debug.Log($"Starting Functional Speech custom game");
        
        // Store custom game information from PlayerPrefs (set by CustomGameItem)
        string customGameId = PlayerPrefs.GetString("CustomGameId", "");
        string customGameName = PlayerPrefs.GetString("CustomGameName", "");
        string customGameType = PlayerPrefs.GetString("CustomGameType", "");
        string customGameSubtype = PlayerPrefs.GetString("CustomGameSubtype", "");
        
        Debug.Log($"Custom Game Info - ID: {customGameId}, Name: {customGameName}, Type: {customGameType}, Subtype: {customGameSubtype}");
        
        // เริ่มเกมโดยใช้การทำงานเหมือน Start() method
        base.Start();
    }
}

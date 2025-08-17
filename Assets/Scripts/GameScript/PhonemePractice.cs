using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhonemePractice : BaseSpeechGame
{
    // Override abstract properties
    protected override float ThresholdValue => PlayerPrefs.GetFloat("difficulty", 0.6f);
    protected override string GameName => "phoneme_practice";
    protected override string CorrectMessage => "สุดยอดเลย! ออกเสียงดีมาก";
    protected override string RetryMessagePrefix => "ลองอีกทีนึงนะ: ";
    
    // Method สำหรับเริ่มเกม custom
    public void StartCustomGame(object customGameData)
    {
        Debug.Log($"Starting Phoneme Practice custom game");
        
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

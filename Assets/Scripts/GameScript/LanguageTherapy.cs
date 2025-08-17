using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class LanguageTherapy : BaseSpeechGame
{
    // Override abstract properties
    protected override float ThresholdValue => PlayerPrefs.GetFloat("difficulty", 0.6f);
    protected override string GameName => "Language Therapy";
    protected override string CorrectMessage => "เยี่ยมมาก! คุณพูดถูกต้องแล้ว";
    protected override string RetryMessagePrefix => "ลองใหม่อีกครั้ง: ";

    private bool isCustomGameMode = false;
    
    // Override Start method เพื่อให้เกมปกติทำงานได้
    protected override void Start()
    {
        if (!isCustomGameMode)
        {
            Debug.Log("Starting Language Therapy main game");
            base.Start();
        }
    }
    
    // Method สำหรับเริ่มเกม custom
    public void StartCustomGame(object customGameData)
    {
        Debug.Log($"Starting Language Therapy custom game");
        
        // Set flag เพื่อไม่ให้ Start() ทำงานซ้ำ
        isCustomGameMode = true;
        
        // Store custom game information from PlayerPrefs (set by CustomGameItem)
        string customGameId = PlayerPrefs.GetString("CustomGameId", "");
        string customGameName = PlayerPrefs.GetString("CustomGameName", "");
        string customGameType = PlayerPrefs.GetString("CustomGameType", "");
        string customGameSubtype = PlayerPrefs.GetString("CustomGameSubtype", "");
        
        Debug.Log($"Custom Game Info - ID: {customGameId}, Name: {customGameName}, Type: {customGameType}, Subtype: {customGameSubtype}");
        
        // Reset stage number เพื่อเริ่มจากด่านแรก
        PlayerPrefs.SetInt("stageNumber", 0);
        PlayerPrefs.Save();
        Debug.Log("Stage number reset to 0 for custom game");
        
        // เริ่มเกมโดยใช้การทำงานเหมือน Start() method
        base.Start();
    }
}

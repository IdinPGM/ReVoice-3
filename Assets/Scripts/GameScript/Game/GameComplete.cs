using System.Collections;
using UnityEngine;
using TMPro;

public class GameComplete : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text scoreValue;
    // [SerializeField] private TMP_Text pointValue;

    [System.Serializable]
    public class EndSessionResponse
    {
        public string gameSessionId;
        public int score;
        // public int point;
    }

    private void Start()
    {
        // แสดง score ที่ได้จาก EndGameSession
        DisplayScore();
    }

    private void DisplayScore()
    {
        // อ่าน score จาก PlayerPrefs ที่เก็บไว้จาก EndGameSession response
        // int points = PlayerPrefs.GetInt("GamePoints", 0);
        int finalScore = PlayerPrefs.GetInt("GameScore", 0);

        // if (pointValue != null)
        // {
        //     pointValue.text = points.ToString();
        // }

        if (scoreValue != null)
        {
            scoreValue.text = finalScore.ToString();
        }
        
        Debug.Log($"Game Complete - Final Score: {finalScore}");
    }
}

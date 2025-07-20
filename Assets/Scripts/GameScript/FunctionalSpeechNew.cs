using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionalSpeechNew : BaseSpeechGame
{
    // Override abstract properties
    protected override float ThresholdValue => 0.5f;
    protected override string GameName => "functional_speech";
    protected override string CorrectMessage => "เก่งมากกกก! พูดถูกต้องเลย";
    protected override string RetryMessagePrefix => "เอาใหม่นะๆ: ";

    // FunctionalSpeech specific implementation can be added here if needed
    // Most functionality is now inherited from BaseSpeechGame
}

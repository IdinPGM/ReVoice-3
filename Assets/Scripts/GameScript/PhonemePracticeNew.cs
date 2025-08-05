using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhonemePracticeNew : BaseSpeechGame
{
    // Override abstract properties
    protected override float ThresholdValue => 0.5f;
    protected override string GameName => "phoneme_practice";
    protected override string CorrectMessage => "สุดยอดเลย! ออกเสียงดีมาก";
    protected override string RetryMessagePrefix => "ลองอีกทีนึงนะ: ";

    // PhonemePractice specific implementation can be added here if needed
    // Most functionality is now inherited from BaseSpeechGame
}

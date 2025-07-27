using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [Header("Sound Effect")]
    [SerializeField] private AudioClip sfx;
    [Header("Audio Source (optional)")]
    [SerializeField] private AudioSource audioSource;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (audioSource == null)
        {
            // สร้าง AudioSource อัตโนมัติถ้ายังไม่มี
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        button.onClick.AddListener(PlaySFX);
    }

    private void PlaySFX()
    {
        if (sfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(sfx);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Camera : MonoBehaviour
{
    [SerializeField] private RawImage img = default;

    private WebCamTexture webCam;

    private void Start()
    {
        webCam = new WebCamTexture();
        if (!webCam.isPlaying) webCam.Play();
        img.texture = webCam;
    }
}
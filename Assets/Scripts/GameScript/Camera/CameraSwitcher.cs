using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraSwitcher : MonoBehaviour
{
    [Header("UI Display for Webcam")]
    [SerializeField] private RawImage display;

    private WebCamTexture webCam;
    private WebCamDevice[] devices;
    private int currentCameraIndex = 0;

    void Awake()
    {
        if (display == null)
            display = GetComponent<RawImage>();
    }

    void Start()
    {
        devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No camera device found.");
            return;
        }
        StartCamera(currentCameraIndex);
    }

    private void StartCamera(int index)
    {
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
            webCam = null;
        }

        string deviceName = devices[index].name;
        webCam = new WebCamTexture(deviceName);
        display.texture = webCam;
        webCam.Play();
        UpdateDisplayOrientation();
    }

    private void UpdateDisplayOrientation()
    {
        if (webCam == null) return;

        // หมุนภาพให้ถูกทิศทาง
        int rotation = -webCam.videoRotationAngle;
        display.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

        // mirror หากเป็นกล้องหน้า
        bool isFront = devices[currentCameraIndex].isFrontFacing;
        display.uvRect = isFront
            ? new Rect(1, 0, -1, 1)
            : new Rect(0, 0, 1, 1);
    }

    // ผูกเมธอดนี้กับปุ่มใน Inspector
    public void SwitchCamera()
    {
        if (devices.Length <= 1) return;
        currentCameraIndex = (currentCameraIndex + 1) % devices.Length;
        StartCamera(currentCameraIndex);
    }

    void OnDisable()
    {
        if (webCam != null && webCam.isPlaying)
            webCam.Stop();
    }
}
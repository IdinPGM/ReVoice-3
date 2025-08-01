using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RawImage))]
public class CameraSwitcher : MonoBehaviour
{
    [Header("UI Display for Webcam")]
    [SerializeField] private RawImage display;

    private WebCamTexture webCam;
    private WebCamDevice[] devices;
    private int currentCameraIndex = 0;

    public static event Action<WebCamTexture> OnCameraChanged;

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
        
        // รอ 1 frame เพื่อให้กล้องได้ข้อมูลครบถ้วน
        StartCoroutine(UpdateDisplayAfterStart());
    }

    private System.Collections.IEnumerator UpdateDisplayAfterStart()
    {
        yield return null;
        UpdateDisplayOrientation();

        // แจ้ง FacialDetection ว่ากล้องเปลี่ยนแล้ว
        OnCameraChanged?.Invoke(webCam);
    }

    private void UpdateDisplayOrientation()
    {
        if (webCam == null) return;

        bool isFront = devices[currentCameraIndex].isFrontFacing;
        int rotation = webCam.videoRotationAngle;
        bool verticallyMirrored = webCam.videoVerticallyMirrored;

        // Debug information
        Debug.Log($"Camera: {devices[currentCameraIndex].name}");
        Debug.Log($"IsFront: {isFront}, VerticallyMirrored: {verticallyMirrored}, Rotation: {rotation}");

        if (isFront)
        {
            // กล้องหน้า - ลองหลายวิธีการแก้ไข
            
            // วิธีที่ 1: หมุน + mirror แนวตั้ง (สำหรับกล้องที่หัวกลับ)
            if (verticallyMirrored)
            {
                display.rectTransform.localEulerAngles = new Vector3(0, 0, -rotation);
                display.uvRect = new Rect(1, 1, -1, -1); // mirror ทั้งสองแกน
            }
            // วิธีที่ 2: หมุนเพิ่ม 180 องศา + mirror แนวนอน
            else
            {
                // display.rectTransform.localEulerAngles = new Vector3(0, 0, -rotation + 180);
                // display.uvRect = new Rect(1, 0, -1, 1); // mirror แนวนอนอย่างเดียว
            }
        }
        else
        {
            // กล้องหลัง - ใช้การตั้งค่าปกติ
            display.rectTransform.localEulerAngles = new Vector3(0, 0, -rotation);
            
            if (verticallyMirrored)
            {
                display.uvRect = new Rect(0, 1, 1, -1); // mirror แนวตั้ง
            }
            else
            {
                display.uvRect = new Rect(0, 0, 1, 1); // ปกติ
            }
        }

        Debug.Log($"Applied Rotation: {display.rectTransform.localEulerAngles.z}");
        Debug.Log($"Applied UVRect: {display.uvRect}");
    }

    // ผูกเมธอดนี้กับปุ่มใน Inspector
    public void SwitchCamera()
    {
        if (devices.Length <= 1) return;
        currentCameraIndex = (currentCameraIndex + 1) % devices.Length;
        StartCamera(currentCameraIndex);
    }

    // Method สำหรับ FacialDetection เรียกใช้
    public WebCamTexture GetCurrentCamera()
    {
        return webCam;
    }

    public bool IsCurrentCameraFrontFacing()
    {
        if (devices.Length == 0) return false;
        return devices[currentCameraIndex].isFrontFacing;
    }

    // Method สำหรับหยุดกล้อง
    public void StopCamera()
    {
        if (webCam != null && webCam.isPlaying)
        {
            webCam.Stop();
            Debug.Log($"Camera stopped: {webCam.deviceName}");
        }
        
        // เคลียร์ texture จาก display
        if (display != null)
        {
            display.texture = null;
        }
    }

    void OnDisable()
    {
        StopCamera();
    }

    void OnDestroy()
    {
        StopCamera();
    }
}
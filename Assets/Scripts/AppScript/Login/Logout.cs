using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Logout : MonoBehaviour
{
    [Header("UI Elements")]
    public Button logoutButton;
    public TMP_Text statusMessage;
    public RawImage transparentBackground;

    [Header("Confirmation Dialog")]
    public GameObject confirmationDialog;
    public Button confirmButton;
    public Button cancelButton;

    private void Start()
    {
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmLogout);
        cancelButton.onClick.AddListener(OnCancelLogout);

        statusMessage.gameObject.SetActive(false);
        transparentBackground.gameObject.SetActive(false);
        confirmationDialog.SetActive(false);
    }

    private void OnLogoutButtonClicked()
    {
        // แสดง confirmation dialog
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            transparentBackground.gameObject.SetActive(true);
            Debug.Log("Logout button clicked, showing confirmation dialog.");
        }
            
    }

    private void OnConfirmLogout()
    {
        // ซ่อน dialog และทำการ logout
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
        
        StartCoroutine(LogoutCoroutine());
    }

    private void OnCancelLogout()
    {
        // ซ่อน dialog และยกเลิกการ logout
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
        transparentBackground.gameObject.SetActive(false);
    }

    private IEnumerator LogoutCoroutine()
    {
        string authToken = PlayerPrefs.GetString("authToken", "");
        
        if (!string.IsNullOrEmpty(authToken))
        {
            // เรียก API logout เพื่อลบ token จากเซิร์ฟเวอร์
            yield return StartCoroutine(HttpHelper.PostRequestCoroutine<object, object>(
                "https://api.mystrokeapi.uk/auth/api/sign-out",
                null,
                onSuccess: (response) => {
                    Debug.Log("Successfully logged out from server.");
                    statusMessage.text = "Logged out successfully.";
                    statusMessage.gameObject.SetActive(true);
                    CompleteLogout();
                },
                onError: (error, code) => {
                    Debug.LogWarning($"Logout API failed: {error} (Code: {code})");
                    statusMessage.text = $"Logout failed: {error}";
                    statusMessage.gameObject.SetActive(true);
                    // ถึงแม้ API จะ fail ก็ยังลบข้อมูล local ต่อไป
                    CompleteLogout();
                }
            ));
        }
        else
        {
            // ไม่มี token ให้ลบข้อมูล local เลย
            CompleteLogout();
        }
    }

    private void CompleteLogout()
    {
        // ลบข้อมูลที่เก็บไว้ทั้งหมด
        ClearUserData();
        
        ShowStatus("Logged out successfully.");
        Debug.Log("User logged out successfully.");
        
        // หน่วงเวลาเล็กน้อยแล้วกลับไปหน้า Login
        StartCoroutine(RedirectToLogin());
    }

    private void ClearUserData()
    {
        // ลบ auth token
        PlayerPrefs.DeleteKey("authToken");
        
        // ลบข้อมูลเกมที่เก็บไว้
        PlayerPrefs.DeleteKey("sessionId");
        PlayerPrefs.DeleteKey("stageNumber");
        PlayerPrefs.DeleteKey("stageData");
        
        // บันทึกการเปลี่ยนแปลง
        PlayerPrefs.Save();
        
        Debug.Log("All user data cleared.");
    }

    private void ShowStatus(string message)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
            statusMessage.gameObject.SetActive(true);
        }
    }

    private IEnumerator RedirectToLogin()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Login");
    }

    // Method สำหรับเรียกจากสคริปต์อื่น
    public void LogoutUser()
    {
        OnLogoutButtonClicked();
    }
}
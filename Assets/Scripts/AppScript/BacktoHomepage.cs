using UnityEngine;

public class BacktoHomepage : MonoBehaviour
{
    public GameObject Backtoward;
    public GameObject HomePage;
    public GameObject HistoryPage;
    public GameObject CustomModePage;
    public GameObject GamePage;
    public GameObject AccountSettingPage;

    // เรียกใช้เมธอดนี้เมื่อกดปุ่ม backtoward
    public void BackTowardToHome()
    {
        HomePage.SetActive(true);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);
    }
}

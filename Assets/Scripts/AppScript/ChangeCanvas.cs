using UnityEngine;

public class ChangeCanvas : MonoBehaviour
{
    public GameObject Button;
    public GameObject HomePage;
    public GameObject HistoryPage;
    public GameObject CustomModePage;
    public GameObject GamePage;
    public GameObject AccountSettingPage;

    // เรียกใช้เมธอดนี้เมื่อกดปุ่ม backtoward
    public void ToHome()
    {
        HomePage.SetActive(true);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);
    }

    public void ToCustom()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(true);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);
    }

    public void ToHistory()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(true);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);
    }

    public void ToGame()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(true);
        AccountSettingPage.SetActive(false);
    }

    public void ToAccountSetting()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(true);
    }
}

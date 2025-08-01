using UnityEngine;
using System.Collections;

public class ChangeCanvas : MonoBehaviour
{
    [Tooltip("Delay in seconds before switching canvas")]
    public float switchDelay = 0.1f;

    public GameObject Button;
    public GameObject HomePage;
    public GameObject HistoryPage;
    public GameObject CustomModePage;
    public GameObject GamePage;
    public GameObject AccountSettingPage;

    // เรียกใช้เมธอดนี้เมื่อกดปุ่ม backtoward
    public void ToHome()
    {
        StartCoroutine(SwitchCanvasAfterDelay(HomePage));
    }

    public void ToCustom()
    {
        StartCoroutine(SwitchCanvasAfterDelay(CustomModePage));
    }

    public void ToHistory()
    {
        StartCoroutine(SwitchCanvasAfterDelay(HistoryPage));
    }

    public void ToGame()
    {
        StartCoroutine(SwitchCanvasAfterDelay(GamePage));
    }

    public void ToAccountSetting()
    {
        StartCoroutine(SwitchCanvasAfterDelay(AccountSettingPage));
    }

    // Coroutine ช่วยหน่วงเวลาก่อนสลับหน้า
    private IEnumerator SwitchCanvasAfterDelay(GameObject activePage)
    {
        yield return new WaitForSeconds(switchDelay);
        HomePage.SetActive(activePage == HomePage);
        HistoryPage.SetActive(activePage == HistoryPage);
        CustomModePage.SetActive(activePage == CustomModePage);
        GamePage.SetActive(activePage == GamePage);
        AccountSettingPage.SetActive(activePage == AccountSettingPage);
    }
}
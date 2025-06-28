using UnityEngine;
using DG.Tweening;

public class NavigationBar : MonoBehaviour
{
    public GameObject HomeButton;
    public GameObject HistoryButton;
    public GameObject CustomModeButton;
    public GameObject GameButton;
    public GameObject AccountSettingButton;
    public GameObject HomePage;
    public GameObject HistoryPage;
    public GameObject CustomModePage;
    public GameObject GamePage;
    public GameObject AccountSettingPage;
    private Vector3 DefaultScale = new Vector3(0.87f, 0.87f, 0.87f);
    private Vector3 Scaled = new Vector3(1.07f, 1.07f, 1.07f);

    void Awake()
    {
        HomeButton.transform.DOScale(Scaled, 0.5f);
    }

    public void OpenHomePage()
    {
        HomePage.SetActive(true);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);

        HomeButton.transform.DOScale(Scaled, 0.5f);
        HistoryButton.transform.DOScale(DefaultScale, 0.5f);
        CustomModeButton.transform.DOScale(DefaultScale, 0.5f);
        GameButton.transform.DOScale(DefaultScale, 0.5f);
        AccountSettingButton.transform.DOScale(DefaultScale, 0.5f);

    }
    public void OpenHistoryPage()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(true);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);

        HomeButton.transform.DOScale(DefaultScale, 0.5f);
        HistoryButton.transform.DOScale(Scaled, 0.5f);
        CustomModeButton.transform.DOScale(DefaultScale, 0.5f);
        GameButton.transform.DOScale(DefaultScale, 0.5f);
        AccountSettingButton.transform.DOScale(DefaultScale, 0.5f);

    }
    public void OpenCustomModePage()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(true);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(false);

        HomeButton.transform.DOScale(DefaultScale, 0.5f);
        HistoryButton.transform.DOScale(DefaultScale, 0.5f);
        CustomModeButton.transform.DOScale(Scaled, 0.5f);
        GameButton.transform.DOScale(DefaultScale, 0.5f);
        AccountSettingButton.transform.DOScale(DefaultScale, 0.5f);

    }
    public void OpenGamePage()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(true);
        AccountSettingPage.SetActive(false);

        HomeButton.transform.DOScale(DefaultScale, 0.5f);
        HistoryButton.transform.DOScale(DefaultScale, 0.5f);
        CustomModeButton.transform.DOScale(DefaultScale, 0.5f);
        GameButton.transform.DOScale(Scaled, 0.5f);
        AccountSettingButton.transform.DOScale(DefaultScale, 0.5f);

    }
    public void OpenAccountSettingPage()
    {
        HomePage.SetActive(false);
        HistoryPage.SetActive(false);
        CustomModePage.SetActive(false);
        GamePage.SetActive(false);
        AccountSettingPage.SetActive(true);

        HomeButton.transform.DOScale(DefaultScale, 0.5f);
        HistoryButton.transform.DOScale(DefaultScale, 0.5f);
        CustomModeButton.transform.DOScale(DefaultScale, 0.5f);
        GameButton.transform.DOScale(DefaultScale, 0.5f);
        AccountSettingButton.transform.DOScale(Scaled, 0.5f);

    }
}

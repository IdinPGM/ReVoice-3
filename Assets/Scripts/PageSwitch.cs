using UnityEngine;
using UnityEngine.UI;

public class PageSwitch : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button[] tabButtons; // ปุ่มแต่ละแท็บ
    [Header("Tab Pages")]
    [SerializeField] private GameObject[] tabPages; // หน้าแต่ละแท็บ
    [Header("Tab Button Size")]
    [SerializeField] private Vector2 normalTabSize = new Vector2(100, 100);
    [SerializeField] private Vector2 activeTabSize = new Vector2(120, 120);

    private int currentTab = 0;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        ShowTab(currentTab);
    }

    public void SwitchTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabPages.Length) return;
        currentTab = tabIndex;
        ShowTab(currentTab);
    }

    private void ShowTab(int tabIndex)
    {
        for (int i = 0; i < tabPages.Length; i++)
        {
            // แสดงเฉพาะหน้า tab ที่เลือก
            tabPages[i].SetActive(i == tabIndex);

            // ปรับขนาดปุ่ม
            var rect = tabButtons[i].GetComponent<RectTransform>();
            rect.sizeDelta = (i == tabIndex) ? activeTabSize : normalTabSize;
        }
    }
}
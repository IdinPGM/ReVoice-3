using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    public GameObject panel;
    public GameObject image;

    private bool isToggled = false;

    public void ToggleObjects()
    {
        isToggled = !isToggled;

        panel.SetActive(!isToggled);
        image.SetActive(isToggled);
    }
}

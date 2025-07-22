using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    // private WebCamTexture webCam;
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        // webCam.Stop();
    }
}

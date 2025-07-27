using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
public class SwitchScene : MonoBehaviour
{
    private void Awake()
    {
        Invoke("LoadScene", 0.4f);
    }

    public void LoadScene(string sceneName)
    {
        // Stop any active webcam textures
        foreach (var raw in Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None))
        {
            if (raw.texture is WebCamTexture cam && cam.isPlaying)
            {
                cam.Stop();
            }
        }

        // Load the requested scene
        SceneManager.LoadScene(sceneName);
        Debug.Log($"Loading scene: {sceneName}");
    }
}

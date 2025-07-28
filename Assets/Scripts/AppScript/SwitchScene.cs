using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SwitchScene : MonoBehaviour
{
    [Tooltip("Delay in seconds before switching scenes")]
    public float sceneSwitchDelay = 0.1f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneSwitchDelay);

        // Stop any active webcam textures
        foreach (var raw in Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None))
        {
            if (raw.texture is WebCamTexture cam && cam.isPlaying)
                cam.Stop();
        }

        SceneManager.LoadScene(sceneName);
        Debug.Log($"Loading scene: {sceneName}");
    }
}
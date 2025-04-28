using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private string loadingSceneName = "Loading";

    public void LoadScene(string targetSceneName)
    {
        SceneManager.LoadScene(loadingSceneName);
        StartCoroutine(LoadTargetSceneAsync(targetSceneName));
    }

    private IEnumerator LoadTargetSceneAsync(string targetSceneName)
    {
        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (progressBar is null)
                progressBar.value = progress;

            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
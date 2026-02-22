using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MySceneManager : MonoBehaviour
{
    public Image fadePanel;
    public float fadeTime = 2f;
    public Image loadingBar;
    public void LoadScene(string name)
    {
        StartCoroutine(SceneLoadRoutine(name));
    }

    IEnumerator SceneLoadRoutine(string scenename)
    {
        fadePanel.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = Color.black;
        c.a = 0;
        while (true)
        {
            yield return null;
            elapsed += Time.deltaTime;
            
            c.a = elapsed / fadeTime;
            fadePanel.color = c;

            if (elapsed > fadeTime) break;
        }

        loadingBar.gameObject.SetActive(true);
        var asyncload = SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Single);
        while (!asyncload.isDone)
        {
            yield return null;
            loadingBar.fillAmount = asyncload.progress;
        }
    }
}

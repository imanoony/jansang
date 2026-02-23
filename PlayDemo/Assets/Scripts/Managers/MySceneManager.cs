using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MySceneManager : MonoBehaviour
{
    public Image fadePanel;
    public float fadeTime = 2f;
    public Image loadingBar;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);

        Color c = Color.black;
        c.a = 1;

        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;

            c.a = 1 - elapsed / fadeTime;
            fadePanel.color = c;

            if (elapsed >= fadeTime)
            {
                break;
            }
        }
        
        fadePanel.gameObject.SetActive(false);
    }
    public void LoadScene(string name)
    {
        StartCoroutine(SceneLoadRoutine(name));
    }

    public void ReloadCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;
        LoadScene(scene.name);
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
        loadingBar.gameObject.SetActive(false);
        
        StartCoroutine(FadeIn());
    }
}

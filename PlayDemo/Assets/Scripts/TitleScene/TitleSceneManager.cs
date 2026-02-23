using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    public Image fadePanel;
    public Image[] backgrounds;

    public InputAction anykeyAction;
    public GameObject difficultyPanel;

    
    private bool changeStarted = false;
    public string targetSceneName = "Level0";

    public float changeTime = 2f;
    public float backgroundMoveTime = 5f;

    public Image loadingBar;
    
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
    }

    private void Start()
    {
        anykeyAction = InputSystem.actions.FindActionMap("Player").FindAction("AnyKey");
        anykeyAction.Enable();
        anykeyAction.started += GoToDifficulty;
    }

    private void GoToDifficulty(InputAction.CallbackContext context)
    {
        anykeyAction.started -= GoToDifficulty;
        difficultyPanel.SetActive(true);
    }

    public void SetDifficulty(int difficulty)
    {
        if (!changeStarted) DifficultyContainer.instance.difficulty = difficulty;
        GoToStage();
    }

    private void GoToStage()
    {
        if (!changeStarted)
        {
            changeStarted = true;
            StartCoroutine(ChangeScene());
        }
    }

    IEnumerator ChangeScene()
    {
        fadePanel.gameObject.SetActive(true);
        float elapsed = 0;
        Color c = Color.black;
        c.a = 0;
        while (true)
        {
            for (int i = 0; i < backgrounds.Length; i++)
            {
                var b = backgrounds[i];
                b.rectTransform.position += Vector3.left * (elapsed * Time.deltaTime * (backgroundMoveTime + i * 50));
            }
            c.a  = elapsed / changeTime;
            fadePanel.color = c;
            elapsed += Time.deltaTime;
            yield return null;

            if (elapsed >= changeTime)
            {
                break;
            }
        }
        
        var sceneLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        if (sceneLoad == null)
        {
            #if UNITY_EDITOR
            
            Debug.LogWarning("Scene not found");
            yield break;
            
            #endif
        }
        
        
        loadingBar.gameObject.SetActive(true);
        while (sceneLoad.progress < 0.9f)
        {
            loadingBar.fillAmount = sceneLoad.progress / 0.9f;
        }
    }
}

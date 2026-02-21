using System;
using UnityEngine;

public class NextStageTrigger : MonoBehaviour
{
    public string nextSceneName;
    public bool goingToNext;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goingToNext) return;
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.MySceneManager.LoadScene(nextSceneName);
            goingToNext = true;
        }
    }
}

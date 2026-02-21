using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DodgeBulletTutorialChecker : MonoBehaviour
{
    public TutorialComponent tc;
    public GameObject nextTutorialChecker;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TutorialManager.Instance.isTutorialActive) return;
        if (other.gameObject.CompareTag("EnemyProjectile"))
        {
            TutorialManager.Instance.tutorialTransforms[tc.highlightTransformId] = other.transform;
            TutorialManager.Instance.ShowTutorial(tc).Forget();
            nextTutorialChecker.gameObject.SetActive(true);
            Destroy(gameObject);
        }
    }
}

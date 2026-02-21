using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialBlockCheck : MonoBehaviour
{
    public TutorialComponent tc;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (TutorialManager.Instance == null) return;
        if (TutorialManager.Instance.isTutorialActive) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Lets go!!!!!" + gameObject.name);
            TutorialManager.Instance.ShowTutorial(tc).Forget();
            Destroy(gameObject);
        }
    }
}

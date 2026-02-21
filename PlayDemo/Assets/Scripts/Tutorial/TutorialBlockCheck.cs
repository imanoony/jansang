using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialBlockCheck : MonoBehaviour
{
    public TutorialComponent tc;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.ShowTutorial(tc).Forget();
            Destroy(this);
        }
    }
}

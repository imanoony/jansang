using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialBlockCheck : MonoBehaviour
{
    public TutorialComponent tc;
    public bool makePlayerDisalbed = false;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (TutorialManager.Instance == null) return;
        if (TutorialManager.Instance.isTutorialActive) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Lets go!!!!!" + gameObject.name);
            
            if (makePlayerDisalbed) other.GetComponent<PlayerMovement2D>().disable = true;
            TutorialManager.Instance.ShowTutorial(tc).Forget();
            
            Destroy(gameObject);
        }
    }
}

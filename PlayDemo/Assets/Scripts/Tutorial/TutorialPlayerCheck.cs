using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialPlayerCheck : MonoBehaviour
{
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement2D>().disable = true;
            Destroy(gameObject);
        }
    }
}

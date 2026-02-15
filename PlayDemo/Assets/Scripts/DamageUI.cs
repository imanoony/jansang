using System;
using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public int normalDamage;
    public float speed;
    public float disappearSpeed;
    
    public void Init(int damage)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;

        float sizeScale = Mathf.Clamp((float)damage / normalDamage, 0.7f, 1.3f);
        
        transform.localScale *= sizeScale;
        var text = GetComponentInChildren<TMP_Text>();
        text.text = damage.ToString();
        text.color = new Color(1 * sizeScale, 0, 0, 1);
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        canvasGroup.alpha -= Time.deltaTime * disappearSpeed;

        if (canvasGroup.alpha < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}

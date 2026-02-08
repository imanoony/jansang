using System;
using System.Collections;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class PlayerGFXController : MonoBehaviour
{
    private SpriteRenderer sr;


    public float squashTime;
    public float squashAmount;
    
    public float stretchTime;
    public float stretchAmount;

    private Coroutine sqStRoutine;
    private Vector3 originalLocalScale;
    
    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalLocalScale = sr.transform.localScale;
    }

    public void Squash()
    {
        if (sqStRoutine != null) StopCoroutine(sqStRoutine);
        
        BackToOriginalScale();
        sqStRoutine = StartCoroutine(SquashOrStretch(squashAmount, squashTime));
    }
    
    public void Stretch()
    {
        if (sqStRoutine != null) StopCoroutine(sqStRoutine);
        
        BackToOriginalScale();
        sqStRoutine = StartCoroutine(SquashOrStretch(-stretchAmount, stretchTime));
    }

    private void BackToOriginalScale()
    {
        sr.transform.localScale = originalLocalScale;
    }

    public void Flip(bool faceRight)
    {
        sr.flipX = !faceRight;
    }
    
    private IEnumerator SquashOrStretch(float amount, float time)
    {
        float elapsed = 0;
        Vector3 startScale = sr.transform.localScale;
        originalLocalScale = startScale;
        Vector3 scale = Vector3.one;
        
        scale.x = (1 + amount) * startScale.x;
        scale.y = (1 - amount) * startScale.y;
        sr.transform.localScale = scale;
        
        
        while (elapsed < time)
        {
            scale.x = 1 + amount * (1 - elapsed / time);
            scale.y = 1 - amount * (1 - elapsed / time);
            sr.transform.localScale = scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        BackToOriginalScale();
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapTarget : MonoBehaviour
{
    public GameObject highlightUI;
    private Action<float> onSwapTarget = null;
    private Action onNonTarget = null;
    private const float EPS = 0.01f;
    private float prevAmount = 0;
    private float amount = 0;
    private bool isHover = false;
    private Transform A = null;
    private Transform B = null;
    void Start()
    {
        SetHighlight(false);
    }

    public void SetHighlight(
        bool active,
        Transform A = null, 
        Transform B = null, 
        Action<float> onSwapTarget = null,
        Action onNonTarget = null
    )
    {
        if (active)
        {
            this.onSwapTarget = onSwapTarget;
            this.onNonTarget = onNonTarget;
        }
        else
        {
            isHover = false;
            this.onNonTarget?.Invoke();
            this.onSwapTarget = null;
            this.onNonTarget = null;
        }

        this.A = A;
        this.B = B;
        if (highlightUI != null)
            highlightUI.SetActive(active);
    }

    void Update()
    {
        if (!isHover || A == null || B == null)
            return;

        amount = Vector2.SqrMagnitude(A.position - B.position);

        if (Mathf.Abs(prevAmount - amount) > EPS)
        {
            prevAmount = amount;
            onSwapTarget?.Invoke(amount);
        }
    }
    public void OnMouseEnter()
    {
        Debug.Log("Pointer Enter Swap Target");
        isHover = true;
    }

    public void OnMouseExit()
    {
        Debug.Log("Pointer Exit Swap Target");
        isHover = false;
        prevAmount = 0;
        amount = 0;
        onNonTarget?.Invoke();
    }
}
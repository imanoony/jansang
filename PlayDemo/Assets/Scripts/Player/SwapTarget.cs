using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject highlightUI;
    private Action<float> onTarget;
    private float amount = 0;
    private bool isHover = false;
    private Transform A = null;
    private Transform B = null;
    void Start()
    {
        SetHighlight(false);
    }

    public void SetHighlight(bool active,Transform A = null, Transform B = null, Action<float> onSwapTarget = null)
    {
        if (active)
        {
            onTarget = onSwapTarget;
        }
        else
        {
            isHover = false;
            onTarget = null;
        }

        this.A = A;
        this.B = B;
        if (highlightUI != null)
            highlightUI.SetActive(active);
    }

    public void Update()
    {
        if (A != null && B != null)
        {
            amount = Vector2.SqrMagnitude(A.position - B.position);
        }
        else
        {
            amount = 0;
        }
        if (isHover)
        {
            onTarget?.Invoke(amount);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}
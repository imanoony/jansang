using UnityEngine;

public class SwapTarget : MonoBehaviour
{
    public GameObject highlightUI;

    void Start()
    {
        SetHighlight(false);
    }

    public void SetHighlight(bool active)
    {
        if (highlightUI != null)
            highlightUI.SetActive(active);
    }
}
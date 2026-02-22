using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Boss1_UI : MonoBehaviour
{
    [Header("Settings")]
    public float uiAppearDelay = 2.0f;

    [Header("Object References")]
    public GameObject nameObject;
    public GameObject healthUI;
    public GameObject healthBar;
    
    [Header("Rect Transforms")]
    public RectTransform nameRT;
    public RectTransform healthUI_RT;
    public RectTransform healthBarRT;

    [Header("Image")]
    public Image healthBarImage;

    [Header("TextMeshPro")]
    public TextMeshProUGUI nameTMP;

    void Awake()
    {
        nameObject = transform.Find("Name").gameObject;
        healthUI = transform.Find("Health UI").gameObject;
        healthBar = healthUI.transform.Find("Health Bar").gameObject;

        nameRT = nameObject.GetComponent<RectTransform>();
        healthUI_RT = healthUI.GetComponent<RectTransform>();
        healthBarRT = healthBar.GetComponent<RectTransform>();
        healthBarImage = healthBar.GetComponent<Image>();
        nameTMP = nameObject.GetComponent<TextMeshProUGUI>();

        nameObject.SetActive(false);
        healthUI.SetActive(false);
        healthBar.SetActive(false);
    }

    public void UpdateHealthBar(float healthPercent)
    {
        healthBarImage.fillAmount = healthPercent;
    }

    public IEnumerator UI_Appear()
    {
        nameTMP.color = new Color(nameTMP.color.r, nameTMP.color.g, nameTMP.color.b, 0f);
        healthUI_RT.localScale = new Vector3(0f, 1f, 1f);

        nameObject.SetActive(true);
        healthUI.SetActive(true);
        healthBar.SetActive(true);

        float timer = 0f;
        while(timer < uiAppearDelay)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / uiAppearDelay);
            nameTMP.color = new Color(nameTMP.color.r, nameTMP.color.g, nameTMP.color.b, alpha);
            healthUI_RT.localScale = new Vector3(alpha, 1f, 1f);
            yield return null;
        }

        yield return null;
    }

    public void UI_Disappear()
    {
        nameObject.SetActive(false);
        healthUI.SetActive(false);
        healthBar.SetActive(false);
    }

}

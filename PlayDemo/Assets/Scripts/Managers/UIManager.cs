using UnityEngine;
using UnityEngine.UI;

// Observer Pattern (currently)
public class UIManager : MonoBehaviour
{
    #region HP
    [Header("HP UI")]
    [SerializeField] private GameObject HPRoot;
    [SerializeField] private GameObject HPSlotPrefab;
    [SerializeField] private Sprite HP;
    [SerializeField] private Sprite HPNull;
    private int cachedHP;
    private bool isHPActive = false;
    public void SetActiveHP(bool enable = true)
    {
        GameObject slot;
        Image slotImage;

        if (HPRoot == null || HPSlotPrefab == null)
            return;
        if (isHPActive == enable)
            return;
        if (!enable)
        {
            HPRoot.SetActive(false);
            isHPActive = false;
            return;
        }
        for (int i = HPRoot.transform.childCount - 1; i >= 0; i--)
            Destroy(HPRoot.transform.GetChild(i).gameObject);
        for (int i = 0; i < GameManager.Instance.Char.MaxHP; i++)
        {
            slot = Instantiate(HPSlotPrefab, HPRoot.transform);
            slotImage = slot.GetComponent<Image>();
            slotImage.sprite = HP;
        }
        HPRoot.SetActive(true);
        cachedHP = GameManager.Instance.Char.HP;
        isHPActive = true;
    }
    public void UpdateHP(int amount)
    {
        int start, end;
        Image slotImage;
        Sprite slotSprite;

        if (HPRoot == null || HPSlotPrefab == null)
            return;
        if (cachedHP == amount)
            return;

        start = Mathf.Min(cachedHP, amount);
        end = Mathf.Max(cachedHP, amount);
        slotSprite = amount > cachedHP ? HP : HPNull;

        for (int i = start; i < end; i++)
        {
            slotImage = HPRoot.transform.GetChild(i).GetComponent<Image>();
            if (slotImage != null)
                slotImage.sprite = slotSprite;
        }

        cachedHP = amount;
    }
    #endregion

    #region Gauge
    [Header("Gauge UI")]
    [SerializeField] private GameObject GaugeRoot;
    [SerializeField] private RectTransform Background;
    [SerializeField] private RectTransform Gauge;
    [SerializeField] private RectTransform Tried;
    [SerializeField] private float gaugeRatio = 2f;
    private bool isGaugeActive = false;
    public void SetActiveGauge(bool enable = true)
    {
        Vector2 vec;

        if (isGaugeActive == enable)
            return;
        if (
            GaugeRoot == null ||
            Background == null ||
            Gauge == null ||
            Tried == null
        )
            return;
        if (!enable)
        {
            GaugeRoot.SetActive(false);
            isGaugeActive = false;
            return;
        }
        vec = Background.sizeDelta.SetX(
            GameManager.Instance.Char.MaxGauge * gaugeRatio
        );
        Background.sizeDelta = vec;
        vec = Gauge.sizeDelta.SetX(
            GameManager.Instance.Char.MaxGauge * gaugeRatio
        );
        Gauge.sizeDelta = vec;
        vec = Tried.sizeDelta.SetX(
            GameManager.Instance.Char.MaxGauge * gaugeRatio
        );
        Tried.sizeDelta = vec;
        GaugeRoot.SetActive(true);
        isGaugeActive = true;
    }
    public void UpdateGauge(float amount)
    {
        Vector2 vec;

        if (
            GaugeRoot == null ||
            Background == null ||
            Gauge == null ||
            Tried == null
        )
            return;
        if (!isGaugeActive)
            return;
        vec = Gauge.sizeDelta.SetX(amount * gaugeRatio);
        Gauge.sizeDelta = vec;
    }
    public void UpdateTried(float amount)
    {
        Vector2 vec;

        if (
            GaugeRoot == null ||
            Background == null ||
            Gauge == null ||
            Tried == null
        )
            return;
        if (!isGaugeActive)
            return;
        vec = Tried.sizeDelta.SetX(amount * gaugeRatio);
        Tried.sizeDelta = vec;
    }
    #endregion
    
    #region Damage
    public GameObject damageUI;
    #endregion
    
    #region Observers
    public void Init()
    {
        GameManager.Instance.Char.OnHPChanged += UpdateHP;
        GameManager.Instance.Char.OnGaugeChanged += UpdateGauge;
        GameManager.Instance.Char.OnTried += UpdateTried;
    }
    void OnDisable()
    {
        GameManager.Instance.Char.OnHPChanged -= UpdateHP;
        GameManager.Instance.Char.OnGaugeChanged -= UpdateGauge;
        GameManager.Instance.Char.OnTried -= UpdateTried;
    }
    #endregion
}
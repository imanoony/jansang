using UnityEngine;

// Observer Pattern (currently)
public class UIManager : MonoBehaviour
{
    #region HP
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

    #region Observers
    public void Init()
    {
        GameManager.Instance.Char.OnGaugeChanged += UpdateGauge;
        GameManager.Instance.Char.OnTried += UpdateTried;
    }
    void OnDisable()
    {
        GameManager.Instance.Char.OnGaugeChanged -= UpdateGauge;
        GameManager.Instance.Char.OnTried -= UpdateTried;
    }
    #endregion
}
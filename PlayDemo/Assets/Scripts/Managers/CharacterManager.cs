using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    #region HP
    [Header("HP Stats")]
    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    [SerializeField] private int initialMaxHP = 5;
    public event Action<int> OnHPChanged; 
    public void InitHP()
    {
        HP = MaxHP;
        OnHPChanged?.Invoke(HP);
    }
    public void AddHP(int amount)
    {
        HP = Math.Min(MaxHP, HP + amount);
        OnHPChanged?.Invoke(HP);
    }
    public void SubHP(int amount)
    {
        HP = Math.Max(0, HP - amount);
        OnHPChanged?.Invoke(HP);
    }
    #endregion

    #region Gauge
    [Header("Gauge Stats")]
    public float Gauge { get; private set; }
    public float Tried { get; private set; }
    public float MaxGauge { get; private set; }
    [SerializeField] private float initialMaxGauge = 100f;
    [SerializeField] private float recoveryRate = 10f;
    [SerializeField] private float recoveryDelay = 1f;
    [SerializeField] private float showDelay = 0.1f;
    private float lastSkillTime = 0f;
    private float lastShowTime = 0f;
    private bool canRecover = true;
    private bool canShow = true;
    public event Action<float> OnGaugeChanged;
    public event Action<float> OnTried;
    public void InitGauge()
    {
        Gauge = MaxGauge;
        Tried = 0f;
        OnGaugeChanged?.Invoke(Gauge);
        OnTried?.Invoke(Tried);
    }
    public bool UseGauge(float amount)
    {
        if (!CheckGauge(amount))
            return false;
        Gauge = Math.Max(0f, Gauge - amount);
        OnGaugeChanged?.Invoke(Gauge);
        lastSkillTime = Time.time;
        canRecover = false;
        return true;
    }
    public bool CheckGauge(float amount)
    {
        if (amount == 0f)
            return Gauge > 0f;
        return Gauge >= amount;
    }
    public void Try(float amount)
    {
        if (Gauge < amount)
            return;
        Tried = amount;
        OnTried?.Invoke(Tried);
    }
    public void CancelTry()
    {
        Tried = 0f;
        OnTried?.Invoke(Tried);
    }
    #endregion

    private bool initialized = false;
    public void Init()
    {
        // Max Gauge가 게임 진행에 따라 변할 수 있다는 점을 고려
        MaxGauge = initialMaxGauge;
        // Max HP가 게임 진행에 따라 변할 수 있다는 점을 고려
        MaxHP = initialMaxHP;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
            return;

        // Gauge recovery logic start
        if (!canRecover && Time.time - lastSkillTime > recoveryDelay)
            canRecover = true;
        if (canRecover && Gauge < MaxGauge)
        {
            Gauge = Math.Min(MaxGauge, Gauge + recoveryRate * Time.deltaTime);
            if (!canShow && Time.time - lastShowTime > showDelay)
                canShow = true;
            if (canShow)
            {
                OnGaugeChanged?.Invoke(Gauge);
                lastShowTime = Time.time;
                canShow = false;
            }
        }
        // Gauge recovery logic end
        // Add more stat update logic here if needed
    }
}

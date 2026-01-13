using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    #region Skill References
    public PlayerMovement2D movement;
    public SwapSkill mainSkill;
    public ProjectileCaster subSkill;
    public BulletTimeController timeSlow;
    public MeleeController2D attack;
    #endregion

    #region HP
    public int HP { get; private set; }
    [SerializeField] private int maxHP = 5;
    public event Action<int> OnHPChanged; 
    public void InitHP()
    {
        HP = maxHP;
        OnHPChanged?.Invoke(HP);
    }
    public void AddHP(int amount)
    {
        HP = Math.Min(maxHP, HP + amount);
        OnHPChanged?.Invoke(HP);
    }
    public void SubHP(int amount)
    {
        HP = Math.Max(0, HP - amount);
        OnHPChanged?.Invoke(HP);
    }
    #endregion

    #region Gauge
    public float Gauge { get; private set; }
    private float tried;
    [SerializeField] private float maxGauge = 100f;
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
        Gauge = maxGauge;
        tried = 0f;
        OnGaugeChanged?.Invoke(Gauge);
        OnTried?.Invoke(tried);
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
    public bool CheckGauge(float amount) => Gauge >= amount;
    public void Try(float amount)
    {
        if (Gauge < amount)
            return;
        tried = amount;
        OnTried?.Invoke(tried);
    }
    public void CancelTry()
    {
        tried = 0f;
        OnTried?.Invoke(tried);
    }
    #endregion

    void Awake()
    {
        mainSkill.Init(this);
        subSkill.Init(this);
        attack.Init(this);
        timeSlow.Init(this);
        movement.Init(this);

        InitHP();
        InitGauge();
    }

    void Update()
    {
        // Gauge recovery logic start
        if (!canRecover && Time.time - lastSkillTime > recoveryDelay)
            canRecover = true;
        if (canRecover && Gauge < maxGauge)
        {
            Gauge = Math.Min(maxGauge, Gauge + recoveryRate * Time.deltaTime);
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

    /*#region UI
    public RadialGauge radialGauge;
    public RadialGauge finalGauge;
    #endregion
    [SerializeField] private float maxGauge = 100;
    [SerializeField] private float recoverRate = 10f;
    private float gauge = 100;
    private bool canCast = true;
    [SerializeField] private float recoveryDelay = 1f;
    private bool useSkill = false;
    private float lastSkillTime = 0;
    private float lastShowTime = 0;
    private float showDelay = 0.1f;
    private void Awake()
    {
        mainSkill.Init(this);
        subSkill.Init(this);
        attack.Init(this);
        timeSlow.Init(this);
        movement.Init(this);
        gauge = maxGauge;
        lastSkillTime = Time.time;
    }

    public void Update()
    {
        if (!useSkill)
        {
            gauge = Math.Min(maxGauge, gauge + recoverRate * Time.deltaTime);
        }
        else if (Time.time - lastSkillTime > recoveryDelay)
        {
            useSkill = false;
        }
        if (gauge > 0 && !canCast)
        {
            canCast = true;
        }

        if (Time.time - lastShowTime > showDelay)
        {
            radialGauge.SetValue(gauge / maxGauge);
            finalGauge.SetValue(gauge / maxGauge);
        }
    }
    public bool CheckGauge(float needGauge)
    {
        if (canCast == false)
        {
            return false;
        }
        if (needGauge > gauge)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void ShowGauge(float needGauge)
    {
        if (canCast == false)
        {
            radialGauge.SetValue(gauge/maxGauge);
            finalGauge.SetValue(gauge/maxGauge);
            return;
        }
        if (needGauge > gauge)
        {
            radialGauge.SetValue(gauge/maxGauge);
            finalGauge.SetValue(gauge/maxGauge);
        }
        else
        {
            radialGauge.SetValue(gauge/maxGauge);
            finalGauge.SetValue((gauge-needGauge)/maxGauge);
            lastShowTime = Time.time;
        }
    }
    public bool UseGauge(float needGauge)
    {
        if (canCast == false)
        {
            return false;
        }
        gauge = Math.Max(0, gauge - needGauge);
        if (gauge <= 0)
        {
            canCast = false;
        }
        lastSkillTime = Time.time;
        useSkill = true;
        return true;
    }*/
}

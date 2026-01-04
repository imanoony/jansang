using System;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    #region Player_Skill_Reference
    public PlayerMovement2D movement;
    public SwapSkill mainSkill;
    public ProjectileCaster subSkill;
    public BulletTimeController timeSlow;
    public MeleeController2D attack;
    #endregion

    #region UI
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
    }
}

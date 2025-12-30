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
    #endregion
    [SerializeField] private float maxGauge = 100;
    [SerializeField] private float recoverRate = 10f;
    private float gauge = 100;
    private bool canCast = true;
    private void Awake()
    {
        mainSkill.Init(this);
        subSkill.Init(this);
        attack.Init(this);
        timeSlow.Init(this);
        movement.Init(this);
        gauge = maxGauge;
    }

    public void Update()
    {
        gauge = Math.Min(maxGauge, gauge + recoverRate * Time.deltaTime);
        if (gauge > maxGauge / 3)
        {
            canCast = true;
        }
        radialGauge.SetValue(gauge/maxGauge);
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
        return true;
    }
}

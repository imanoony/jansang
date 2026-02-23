using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    
    #region Resources

    public int LightMaterial = 3000;
    public int UsedMaterial = 0;
    public int HPLevel = 0;
    public int GaugeLevel = 0;
    public bool canUse(int Amount)
    {
        if (Amount <= LightMaterial)
        {
            return true;
        }

        return false;
    }

    public int HPUpMaterial()
    {
        return 100 + HPLevel * 50;
    }

    public void UpgradeHP()
    {
        LightMaterial -= HPUpMaterial();
        UsedMaterial += HPUpMaterial();
        HPLevel += 1;
        MaxHP += 1;
    }
    public int GaugeUpMaterial()
    {
        return 200 + GaugeLevel * 100;
    }
    public void UpgradeGauge()
    {
        LightMaterial -= GaugeUpMaterial();
        UsedMaterial += GaugeUpMaterial();
        GaugeLevel += 1;
        MaxGauge += 50;
    }

    public void ResetUpgrade()
    {
        LightMaterial += UsedMaterial;
        UsedMaterial = 0;
        MaxGauge -= GaugeLevel * 50;
        MaxHP -= HPLevel;
        HP = Mathf.Min(HP, MaxHP);
        Gauge = Mathf.Min(Gauge, MaxGauge);
        HPLevel = 0;
        GaugeLevel = 0;
    }
    #endregion
    private bool initialized = false;

    public void ApplyDifficultyStats(int maxHp, float maxGauge)
    {
        if (maxHp > 0) initialMaxHP = maxHp;
        if (maxGauge > 0f) initialMaxGauge = maxGauge;

        if (!initialized) return;

        MaxHP = initialMaxHP;
        MaxGauge = initialMaxGauge;
        HP = Mathf.Min(HP, MaxHP);
        Gauge = Mathf.Min(Gauge, MaxGauge);
        OnHPChanged?.Invoke(HP);
        OnGaugeChanged?.Invoke(Gauge);
    }
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

    public Image fadePanel;
    public TMP_Text deathText;
    public TMP_Text restartGuideText;
    public float fadeTime;
    public bool hitRestart;
    public IEnumerator Death()
    {
        InputSystem.actions.FindActionMap("Player").Disable();
        int bulletTimeid = GameManager.Instance.TimeManager.EnterBulletTime(0.2f);

        GameManager.Instance.Audio.StopBgm(1);
        
        fadePanel.gameObject.SetActive(true);
        Color c = Color.black;
        c.a = 0;
        float elapsed = 0;
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
            
            c.a = elapsed / fadeTime;
            fadePanel.color = c;

            if (elapsed >= fadeTime / 2)
            {
                if (!deathText.gameObject.activeSelf) deathText.gameObject.SetActive(true);
                deathText.alpha = (elapsed - fadeTime/2) / (fadeTime / 2);
            }
            if (elapsed >= fadeTime) break;
        }

        restartGuideText.gameObject.SetActive(true);
        var action = InputSystem.actions.FindActionMap("Player").FindAction("Restart");
        
        action.Enable();
        action.started += HitRestart;
        
        
        yield return new WaitUntil(() => hitRestart);
        restartGuideText.gameObject.SetActive(false);
        action.started -= HitRestart;

        elapsed = 0;
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
            
            deathText.alpha = 1 - elapsed / fadeTime;
            
            if (elapsed >= fadeTime) break;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InitHP();
        fadePanel.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);
        InputSystem.actions.FindActionMap("Player").Enable();
        
        GameManager.Instance.TimeManager.ExitBulletTime(bulletTimeid);
        
    }

    private void HitRestart(InputAction.CallbackContext ctx)
    {
        hitRestart = true;
    }
}

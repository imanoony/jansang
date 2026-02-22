using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1_Altar : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private List<Sprite> altarSprites;

    [Header("Health Settings")]
    [SerializeField] private int health;
    public bool active = true;
    private int currentHealth;

    [Header("Hit FX")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [SerializeField] private Boss1_Manage bossManage;
    private SpriteRenderer altarSR;
    private Color baseColor = Color.white;
    private Coroutine hitFlashRoutine;
    private bool isFlashing;
    private bool wasJudgementActive;

    private void Awake()
    {
        altarSR = GetComponent<SpriteRenderer>();
        health = altarSprites.Count - 1;
        if (altarSR != null) baseColor = altarSR.color;
    }

    private void Start()
    {
        active = true;
        currentHealth = health;
        altarSR.sprite = altarSprites[0];
        wasJudgementActive = IsJudgementPattern();
        RefreshJudgementState();
    }

    public void ResetAltar()
    {
        active = true;
        currentHealth = health;
        altarSR.sprite = altarSprites[0];
        RefreshJudgementState();
    }

    private void Update()
    {
        RefreshJudgementState();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HitBox>() != null) return;
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            TryHit();
        }
    }

    public bool TryHit()
    {
        if (bossManage == null) return false;
        if (bossManage.currentBodyPattern != Boss1_BodyPattern.Judgement) return false;
        if (currentHealth <= 0 || !active) return false;

        currentHealth--;
        altarSR.sprite = altarSprites[health - currentHealth];
        PlayHitFlash();
        bossManage.ApplyHitFx();

        if (currentHealth <= 0)
        {
            active = false;
        }

        return true;
    }

    private void RefreshJudgementState()
    {
        bool isJudgement = IsJudgementPattern();
        if (isJudgement != wasJudgementActive)
        {
            if (isJudgement)
            {
                currentHealth = health;
                if (altarSR != null && altarSprites.Count > 0) altarSR.sprite = altarSprites[0];
            }
            else
            {
                currentHealth = 0;
                if (altarSR != null && altarSprites.Count > 0) altarSR.sprite = altarSprites[0];
            }
            wasJudgementActive = isJudgement;
        }

        active = isJudgement && currentHealth > 0;

        if (isFlashing && active) return;
        ApplyStateColor();
    }

    private bool IsJudgementPattern()
    {
        if (bossManage == null) return false;
        return bossManage.currentBodyPattern == Boss1_BodyPattern.Judgement;
    }

    private void ApplyStateColor()
    {
        if (altarSR == null) return;
        if (isFlashing) StopHitFlash();

        altarSR.color = active ? baseColor : inactiveColor;
    }

    private void PlayHitFlash()
    {
        if (altarSR == null || hitFlashDuration <= 0f) return;
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        isFlashing = true;
        altarSR.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        isFlashing = false;
        ApplyStateColor();
        hitFlashRoutine = null;
    }

    private void StopHitFlash()
    {
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = null;
        isFlashing = false;
    }

}

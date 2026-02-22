using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using JetBrains.Annotations;


public class MeleeController2D : MonoBehaviour
{
    [Header("Attack")]
    public float attackCooldown = 0.5f;
    public float reloadTime = 1.2f; // ?�속 공격 ???�레??
    bool isReloading;
    bool isAttacking;
    public bool isRight = true;
    public Animator anim;
    public Transform shootPos;
    public HitBox hitBox;

    [Header("SFX")]
    [SerializeField] private AudioClip chargeSfx;
    [SerializeField, Range(0f, 1f)] private float chargeSfxVolume = 1f;
    [SerializeField] private AudioClip weakAttackSfx;
    [SerializeField] private AudioClip middleAttackSfx;
    [SerializeField] private AudioClip strongAttackSfx;
    [SerializeField, Range(0f, 1f)] private float attackSfxVolume = 1f;
    private AudioManager audioManager;

    public bool attackSilenced = false;
    public void AttackSilence(float time)
    {
        attackSilenced = true;
        StartCoroutine(AttackSilenceTimer(time));
    }
    IEnumerator AttackSilenceTimer(float time)
    {
        yield return new WaitForSeconds(time);
        attackSilenced = false;
    }



    void Start()
    {
        anim.gameObject.SetActive(false);
        playerMovement = GetComponent<PlayerMovement2D>();
        //hitBox.enabled = false;
        audioManager = GameManager.Instance != null ? GameManager.Instance.Audio : null;
    }

    private void Update()
    {
        if (isCharging) chargingTimeElapsed += Time.deltaTime;
        if (attackButtonPressed && CanAttack()) StartCharging();
    }

    [Header("Charging Attack!")]
    [SerializeField] private float[] chargingTime = new float[2];
    private float chargingTimeElapsed;
    private bool isCharging = false;
    private CancellationTokenSource chargingCTS;
    private bool attackButtonPressed = false;

    // TODO: Apply actual charging effect...
    [SerializeField] private SpriteRenderer chargingEffect;

    public enum AttackState
    {
        Weak,
        Middle,
        Strong
    }

    private AttackState attackState; 
    
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            attackButtonPressed = true;
            if (!CanAttack()) return;
            // 차징 ?�작
            
            StartCharging();
        }
        
        if (context.canceled)
        {
            attackButtonPressed = false;
            // 차징???�니?�면 리턴
            if (!isCharging) return; 
            // 차징 ??
            
            FinishCharging();
        }
    }
    
    private PlayerMovement2D playerMovement;
    private void StartCharging()
    {
        if (!playerMovement.CanMove()) return;
        isCharging = true;
        chargingTimeElapsed = 0f;
        if (audioManager != null) audioManager.PlaySfx(chargeSfx, chargeSfxVolume);
        
        ChargingLevel(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask ChargingLevel(CancellationToken token)
    {
        chargingEffect.gameObject.SetActive(true);
        chargingEffect.color = Color.green;
        attackState = AttackState.Weak;
        hitBox.attackState = attackState;
        
        chargingCTS = new CancellationTokenSource();
        var cts = chargingCTS.Token;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts, token);

        try
        {
            await UniTask.WaitUntil(() => chargingTimeElapsed > chargingTime[0], cancellationToken: linkedCts.Token);
            attackState = AttackState.Middle;
            hitBox.attackState = attackState;
            chargingEffect.color = Color.yellow;
            if (audioManager != null) audioManager.PlaySfx(chargeSfx, chargeSfxVolume);

            await UniTask.WaitUntil(() => chargingTimeElapsed > chargingTime[1], cancellationToken: linkedCts.Token);
            attackState = AttackState.Strong;
            hitBox.attackState = attackState;
            chargingEffect.color = Color.red;
            if (audioManager != null) audioManager.PlaySfx(chargeSfx, chargeSfxVolume);
        }
        finally
        {
            
        }
    }

    private void FinishCharging()
    {
        isCharging = false;
        chargingCTS.Cancel();
        chargingEffect.gameObject.SetActive(false);

        Attack();
    }
    
    private bool CanAttack()
    {
        return !isReloading && !isAttacking && !isCharging && !attackSilenced;
    }

    public float attackRadius;
    public LayerMask enemyLayerMask;

    private async UniTask AttackAsync(CancellationToken token, Vector3 target)
    {
        Vector2 dir = target - shootPos.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        shootPos.rotation = Quaternion.Euler(0f, 0f, angle);

        EnableHitBox();
        anim.SetTrigger("Attack");

        await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown), cancellationToken: token);

        isAttacking = false;
        DisableHitBox();
        StartCoroutine(Reload());
    }
    
    private void Attack()
    {
        isAttacking = true;
        PlayAttackSfx();

        // ?�단 주�??????�는지 체크
        var hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayerMask);

        // ?�이 ?�다�?가??가까운 ?�에 ?�겟팅
        // ?�이 ?�다�?마우??방향?�로
        if (hits.Length > 0)
        {
            var least = hits[0];
            var leastDist = Vector2.Distance(transform.position, least.transform.position);
            foreach (var hit in hits)
            {
                var dist = Vector3.Distance(transform.position, hit.transform.position);
                if (leastDist > dist)
                {
                    leastDist = dist;
                    least = hit;
                }
            }
            
            AttackAsync(this.GetCancellationTokenOnDestroy(), least.transform.position).Forget();
        }
        else
        {
            // === 마우??방향?�로 ?�전 ===
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttackAsync(this.GetCancellationTokenOnDestroy(), mouseWorld).Forget();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSecondsRealtime(reloadTime);
        isReloading = false;
    }

    // ?�니메이???�벤?�에???�출
    public void EnableHitBox()
    {
        hitBox.gameObject.SetActive(true);
    }

    public void DisableHitBox()
    {
        hitBox.gameObject.SetActive(false);
    }

    private void PlayAttackSfx()
    {
        if (audioManager == null) return;
        AudioClip clip = null;
        switch (attackState)
        {
            case AttackState.Weak:
                clip = weakAttackSfx;
                break;
            case AttackState.Middle:
                clip = middleAttackSfx;
                break;
            case AttackState.Strong:
                clip = strongAttackSfx;
                break;
        }
        if (clip != null) audioManager.PlaySfx(clip, attackSfxVolume);
    }
}


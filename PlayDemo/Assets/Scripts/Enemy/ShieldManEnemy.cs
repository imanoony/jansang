using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
public class ShieldManEnemy : EnemyBase
{
    #region  parameters
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask wallLayer;
    [Header("Movement!")] 
    [SerializeField] private float directionTimeChange = 2f;
    [SerializeField] private float flipTimeChange = 2f;
    [Header("Combat")] 
    [SerializeField] private float thrustRadius = 1.3f;
    [SerializeField] private LayerMask enemyHittableLayer;
    [Header("Hit FX")]
    [SerializeField] private float hitShakeDuration = 0.08f;
    [SerializeField] private float hitShakeAmplitude = 0.2f;
    [SerializeField] private float hitShakeFrequency = 25f;
    [Header("Hit Slow")]
    [SerializeField] private float hitSlowScale = 0.2f;
    [SerializeField] private float hitSlowDuration = 0.06f;
    #endregion
    #region components
    [SerializeField] private Collider2D attackArea;
    #endregion
    #region status
    private float directionTimeChangeElapsed;
    private float flipTimeChangeElapsed;
    private bool rushStart;
    private bool canThrust;
    private bool automaticFlip = false;
    private readonly HashSet<Collider2D> damageAreaHitTargets = new HashSet<Collider2D>();
    private readonly Collider2D[] damageAreaHitResults = new Collider2D[8];
    private ContactFilter2D enemyHittableFilter;
    private ContactFilter2D playerFilter;
    private CameraFollow2D camFollow;
    private CameraShake camShake;
    
    #endregion
    protected override void Start()
    {
        attackArea.enabled = false;
        base.Start();
        enemyHittableFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyHittableLayer,
            useTriggers = true
        };
        playerFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = playerMask,
            useTriggers = true
        };
        CacheCameraFx();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
        base.Update();
        if (!alerted)
        {
            if (directionTimeChangeElapsed > 0)
            {
                directionTimeChangeElapsed -= Time.deltaTime;
            }
            else
            {
                directionTimeChangeElapsed = directionTimeChange;
                int dir = Random.Range(-1, 2);
                ChangeDirection(dir);
                FlipByDirection(dir);
            }
            alerted = DetectPlayer(detectionRadius, sightMask);
        }
        else
        {
            if (automaticFlip && Player != null)
            {
                flipTimeChangeElapsed -= Time.deltaTime;
                if (flipTimeChangeElapsed <= 0)
                {
                    flipTimeChangeElapsed = flipTimeChange;
                    if (Player.position.x > transform.position.x) FlipByDirection(1);
                    else if (Player.position.x < transform.position.x) FlipByDirection(-1);
                }
            }
            if (rushStart)
            {
                if (DetectCliff(wallLayer) ||
                    Physics2D.OverlapBox(transform.position, col.bounds.size, 0, playerMask) ||
                    MoveDirection == 0)
                {
                    rushStart = false;
                    canThrust = true;
                }
            }

            UpdateFound(combatRadius, sightMask);
        }
        if (DetectCliff(wallLayer)) ChangeDirection(0);

        if (attackArea.enabled)
        {
            ApplyDamageAreaHits();
        }
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        ChangeDirection(0);
        SetBaseColor(new Color(0f, 1f, 0f, 1f));
        currentState = State.Alert;
        automaticFlip = true;
        await AlertedActionAsync(token);
    }
    private async UniTask AlertedActionAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        while (!token.IsCancellationRequested)
        {
            if (found == false || !TilemapPlatformIndex.Instance.AreOnSamePlatformByRay(Player, transform))
            {
                CurrentTarget = null;
                await WanderAsync(token);
            }
            else
            {
                CurrentTarget = Player;
                await AttackRoutineAsync(token);
            }
        }
    }
    private async UniTask WanderAsync(CancellationToken token)
    {
        SetBaseColor(new Color(0f, 1f, 0f, 1f));
        int dir = Random.Range(-1, 2);
        ChangeDirection(dir);
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
        ChangeDirection(0);
    }
    private async UniTask AttackRoutineAsync(CancellationToken token)
    {
        if (CurrentTarget == null) return;
        SetBaseColor(new Color(1f, 0f, 0f, 1f));
        int dir;
        if (CurrentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;
        automaticFlip = false;
        ChangeDirection(dir);
        ChangeMoveSpeed(2);
        FlipByDirection(dir);
        attackArea.enabled = true;
        damageAreaHitTargets.Clear();
        rushStart = true;
        canThrust = false;
        await UniTask.WaitUntil(() => canThrust, cancellationToken: token);
        attackArea.enabled = false;
        ChangeDirection(0);
        ChangeMoveSpeed(1f);
        GetComponent<Rigidbody2D>().linearVelocityY = 3; 
        SetBaseColor(new Color(0.4f, 0.4f, 0.4f, 1f));
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
        automaticFlip = true;
    }
    public override void Hit()
    {
        if (Player == null)
        {
            base.Hit();
            return;
        }
        if (Player.transform.position.x < transform.position.x && transform.localScale.x > 0) 
            ApplyDamageAsync(1f).Forget();
        else if (Player.transform.position.x > transform.position.x && transform.localScale.x < 0) 
            ApplyDamageAsync(1f).Forget();
        else 
            FlashColorAsync(Color.yellow, 0.5f, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void ApplyDamageAreaHits()
    {
        ApplyPlayerDamage();
        ApplyEnemyHittableDamage();
    }

    private void ApplyPlayerDamage()
    {
        if (playerMask.value == 0) return;

        int count = attackArea.Overlap(playerFilter, damageAreaHitResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = damageAreaHitResults[i];
            if (col == null) continue;
            if (col.transform.IsChildOf(transform)) continue;
            if (!damageAreaHitTargets.Add(col)) continue;

            PlayerHitCheck player = col.GetComponentInParent<PlayerHitCheck>();
            if (player != null)
            {
                rushStart = false;
                canThrust = true;
                player.TakeDamage(1);
            }
        }
    }

    private void ApplyEnemyHittableDamage()
    {
        if (enemyHittableLayer.value == 0) return;

        int count = attackArea.Overlap(enemyHittableFilter, damageAreaHitResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = damageAreaHitResults[i];
            if (col == null) continue;
            if (col.transform.IsChildOf(transform)) continue;
            if (!damageAreaHitTargets.Add(col)) continue;

            EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                rushStart = false;
                canThrust = true;
                enemy.Hit();
                ApplyHitFx();
            }
        }
    }

    private void ApplyHitFx()
    {
        ApplyHitSlow();
        ApplyHitFx(
            hitShakeDuration,
            hitShakeAmplitude,
            hitShakeFrequency
        );
    }

    private void ApplyHitFx(
        float shakeDuration,
        float shakeAmplitude,
        float shakeFrequency
    )
    {
        if (camFollow != null)
        {
            camFollow.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
            return;
        }

        if (camShake != null) camShake.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
    }

    private void ApplyHitSlow()
    {
        var timeManager = GameManager.Instance != null ? GameManager.Instance.TimeManager : null;
        if (timeManager == null) return;

        if (hitSlowDuration > 0f) timeManager.EnterBulletTime(hitSlowScale, hitSlowDuration);
        else timeManager.EnterBulletTime(hitSlowScale);
    }

    private void CacheCameraFx()
    {
        var cam = Camera.main;
        if (cam == null) return;
        camFollow = cam.GetComponent<CameraFollow2D>();
        camShake = cam.GetComponent<CameraShake>();
    }
}

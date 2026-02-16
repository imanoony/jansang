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
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask enemyHittableLayer;
    #endregion
    #region components
    [SerializeField] private Collider2D attackArea;
    private LineRenderer lineRenderer;
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
    #endregion
    protected override void Start()
    {
        attackArea.enabled = false;
        base.Start();
        lineRenderer = GetComponent<LineRenderer>();
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
                if (DetectWall(wallLayer) ||
                    MoveDirection == 0)
                {
                    rushStart = false;
                    canThrust = true;
                }
            }

            UpdateFound(combatRadius, sightMask);
        }
        
        if (!rushStart)
        {
            if (DetectWall(wallLayer) || DetectCliff()) ChangeDirection(0);
        }

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

    public float rushSpeedRate = 5;
    
    private async UniTask AttackRoutineAsync(CancellationToken token)
    {
        if (CurrentTarget == null) return;
        SetBaseColor(new Color(1f, 0f, 0f, 1f));
        int dir;
        if (CurrentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;
        automaticFlip = false;
        ChangeDirection(dir);
        // 1: 돌진 전에 알려주기
        if (lineRenderer != null)
        {
            ChangeMoveSpeed(0);

            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;

            
            lineRenderer.SetPosition(1, transform.position);

            lineRenderer.startWidth = 4f;
            lineRenderer.endWidth = 4f;

            Color c = Color.yellow;
            c.a = 0;
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = c;

            float elapsed = 0f;
            bool aiming = true;
            while (true)
            {
                elapsed += Time.deltaTime;
                lineRenderer.SetPosition(0, transform.position);
                if (elapsed < 0.7f) lineRenderer.SetPosition(1, transform.position + dir * elapsed * 5 * Vector3.right);
                else if (aiming)
                {
                    c = Color.red;
                    c.a = 0;
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = c;
                    aiming = false;
                }

                if (elapsed > 1) break;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
        }

        // 2: 돌진해버리기
        lineRenderer.enabled = false;
        ChangeMoveSpeed(rushSpeedRate);
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
    public override void Hit(int damage)
    {
        if (Player == null)
        {
            base.Hit(damage);
            return;
        }
        if (Player.transform.position.x < transform.position.x && transform.localScale.x > 0) 
            base.Hit(damage);
        else if (Player.transform.position.x > transform.position.x && transform.localScale.x < 0) 
            base.Hit(damage);
        else 
            FlashColorAsync(Color.yellow, 0.5f, this.GetCancellationTokenOnDestroy()).Forget();
    }
    
    public override void Hit(int damage, Vector3 pos)
    {
        if (Player == null)
        {
            base.Hit(damage);
            return;
        }
        if (pos.x < transform.position.x && transform.localScale.x > 0) 
            base.Hit(damage);
        else if (pos.x > transform.position.x && transform.localScale.x < 0) 
            base.Hit(damage);
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
                player.TakeDamage(8);
            }
        }
    }

    private void ApplyEnemyHittableDamage()
    {
        if (enemyHittableLayer.value == 0) return;

        LayerMask mask = enemyHittableLayer;
        if ((enemyHittableLayer.value & (1 << gameObject.layer)) != 0)
        {
            mask |= enemyLayer;
        }
        enemyHittableFilter.layerMask = mask;
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
                enemy.Hit(8, transform.position);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
public class SpearManEnemy : EnemyBase
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
    [SerializeField] private float rushSpeed = 2f;
    [Header("Combat")] 
    [SerializeField] private float thrustRadius = 1.3f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask enemyHittableLayer;
    public bool isWandering = true;
    #endregion
    #region components
    [SerializeField] private Collider2D damageArea;
    private LineRenderer lineRenderer;
    #endregion
    #region status
    private float directionTimeChangeElapsed;
    public bool rushStart;
    private bool canThrust;
    private bool automaticFlip = false;
    private float localSpeedRate = 1f;
    private float preRushSpeedRate = 1f;
    private readonly HashSet<Collider2D> damageAreaHitTargets = new HashSet<Collider2D>();
    private readonly Collider2D[] damageAreaHitResults = new Collider2D[8];
    private ContactFilter2D enemyHittableFilter;
    private ContactFilter2D playerFilter;
    
    #endregion
    protected override void Start()
    {
        damageArea.enabled = false;
        base.Start();

        lineRenderer = GetComponent<LineRenderer>();
        localSpeedRate = 1f;
        preRushSpeedRate = 1f;
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
                int dir = 0;
                if (isWandering) dir = Random.Range(-1, 2);
                ChangeDirection(dir);
                FlipByDirection(dir);
            }
            alerted = DetectPlayer(detectionRadius, sightMask);
        }
        else
        {
            if (automaticFlip && Player != null)
            {
                if (Player.position.x > transform.position.x) FlipByDirection(1);
                else if (Player.position.x < transform.position.x) FlipByDirection(-1);
            }
            if (rushStart)
            {
                if (DetectWall(wallLayer) ||
                    MoveDirection == 0)
                {
                    SetRushStart(false);
                }
            } 
            UpdateFound(combatRadius, sightMask);
        }

        if (!rushStart && localSpeedRate > 0.5f)
        {
            if (DetectWall(wallLayer) || DetectCliff()) ChangeDirection(0);
        }

        if (damageArea.enabled)
        {
            ApplyDamageAreaHits();
        }
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        ChangeDirection(0);
        currentState = State.Alert;
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
        int dir = Random.Range(-1, 2);
        ChangeDirection(dir);
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
        ChangeDirection(0);
    }

    public float rushDistance = 10f;
    private async UniTask AttackRoutineAsync(CancellationToken token)
    {
        if (CurrentTarget == null) return;
        int dir;
        if (CurrentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;
        
        automaticFlip = false;
        
        ChangeDirection(dir);
        FlipByDirection(dir);
        
        // 1: 돌진 전에 알려주기
        if (lineRenderer != null)
        {
            SetSpeedRate(0);

            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;

            
            lineRenderer.SetPosition(1, transform.position);

            lineRenderer.startWidth = 2f;
            lineRenderer.endWidth = 2f;

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
                lineRenderer.SetPosition(1, transform.position + dir * elapsed * rushDistance * Vector3.right);
                if (elapsed > 0.7f && aiming)
                {
                    c = Color.red;
                    c.a = 0.5f;
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = c;
                    aiming = false;
                }

                if (elapsed > 1) break;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
        }
        
        // 2: 돌진해버리기
        float currentX = transform.position.x; 
        lineRenderer.enabled = false;
        SetSpeedRate(1);
        SetRushStart(true);
        
        canThrust = false;
        damageArea.enabled = true;
        damageAreaHitTargets.Clear();
        await UniTask.WaitUntil(() => (canThrust || Mathf.Abs(transform.position.x - currentX) >= rushDistance), cancellationToken: token);
        
        
        // 3: 돌진 끝
        SetRushStart(false);
        automaticFlip = true;
        damageArea.enabled = false;
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);
        
        // 4: 돌진 끝 뒷걸음질 
        ChangeDirection(-dir);
        SetSpeedRate(0.2f);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        
        ChangeDirection(0);
        SetSpeedRate(1f);
    }

    private void SetSpeedRate(float rate)
    {
        localSpeedRate = rate;
        ChangeMoveSpeed(rate);
    }

    private void SetRushStart(bool value)
    {
        if (rushStart == value) return;
        if (!value) canThrust = true;
        rushStart = value;
        if (rushStart)
        {
            preRushSpeedRate = localSpeedRate;
            SetSpeedRate(rushSpeed);
        }
        else
        {
            SetSpeedRate(preRushSpeedRate);
        }
    }

    private void ApplyDamageAreaHits()
    {
        ApplyPlayerDamage();
        ApplyEnemyHittableDamage();
    } 

    private void ApplyPlayerDamage()
    {
        if (playerMask.value == 0) return;

        int count = damageArea.Overlap(playerFilter, damageAreaHitResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = damageAreaHitResults[i];
            if (col == null) continue;
            if (col.transform.IsChildOf(transform)) continue;
            if (!damageAreaHitTargets.Add(col)) continue;

            PlayerHitCheck player = col.GetComponentInParent<PlayerHitCheck>();
            if (player != null)
            {
                SetRushStart(false);
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
        int count = damageArea.Overlap(enemyHittableFilter, damageAreaHitResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = damageAreaHitResults[i];
            if (col == null) continue;
            if (col.transform.IsChildOf(transform)) continue;
            if (!damageAreaHitTargets.Add(col)) continue;

            EnemyBase enemy = col.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Hit(8, transform.position);
            }
        }
    }
}

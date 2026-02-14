using System;
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
    #endregion
    #region components
    [SerializeField] private Collider2D damageArea;
    #endregion
    #region status
    private bool found = false;
    private float directionTimeChangeElapsed;
    private bool rushStart;
    private bool canThrust;
    private bool automaticFlip = false;
    private float localSpeedRate = 1f;
    private float preRushSpeedRate = 1f;
    #endregion
    protected override void Start()
    {
        damageArea.enabled = false;
        base.Start();
        localSpeedRate = 1f;
        preRushSpeedRate = 1f;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
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
                if (Player.position.x > transform.position.x) FlipByDirection(1);
                else if (Player.position.x < transform.position.x) FlipByDirection(-1);
            }
            if (rushStart)
            {
                if (DetectCliff(wallLayer) ||
                    Physics2D.OverlapCircle(transform.position, thrustRadius, playerMask) != null ||
                    MoveDirection == 0)
                {
                    SetRushStart(false);
                    canThrust = true;
                }
            } 
            found = DetectPlayer(combatRadius, sightMask); 
        }
        if (DetectCliff(wallLayer)) ChangeDirection(0);
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        ChangeDirection(0);
        SetBaseColor(new Color(0f, 1f, 0f, 1f));
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
        FlipByDirection(dir);
        SetRushStart(true);
        canThrust = false;
        await UniTask.WaitUntil(() => canThrust, cancellationToken: token);
        SetBaseColor(new Color(1f, 1f, 0f, 1f));
        automaticFlip = true;
        damageArea.enabled = true;
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);
        SetBaseColor(new Color(0.5f, 0.5f, 1f, 1f));
        damageArea.enabled = false;
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
}

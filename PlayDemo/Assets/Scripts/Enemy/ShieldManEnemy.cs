using System;
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
    #endregion
    #region components
    [SerializeField] private Collider2D attackArea;
    #endregion
    #region status
    private bool found = false;
    private float directionTimeChangeElapsed;
    private float flipTimeChangeElapsed;
    private bool rushStart;
    private bool canThrust;
    private bool automaticFlip = false;
    #endregion
    protected override void Start()
    {
        attackArea.enabled = false;
        base.Start();
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
}


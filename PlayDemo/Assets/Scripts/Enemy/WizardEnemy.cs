using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class WizardEnemy : EnemyBase
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
    [SerializeField] private GameObject magicball;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float magicballSpeed = 2f;
    #endregion
    #region status
    private bool automaticFlip = false;
    private float attackCooldownElapsed;
    #endregion
    protected override void Start()
    {
        base.Start();
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
            alerted = DetectPlayer(detectionRadius, sightMask);
        }
        else
        {
            if (automaticFlip && Player != null)
            {
                if (Player.position.x > transform.position.x) FlipByDirection(1);
                else if (Player.position.x < transform.position.x) FlipByDirection(-1);
            }
            UpdateFound(combatRadius, sightMask);
        }
        if (attackCooldownElapsed > 0) attackCooldownElapsed -= Time.deltaTime;
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        ChangeDirection(0);
        currentState = State.Alert;
        automaticFlip = true;
        await AlertedActionAsync(token);
    }
    private async UniTask AlertedActionAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        while (!token.IsCancellationRequested)
        {
            if (found == false)
            {
                CurrentTarget = null;
                await AttackRoutineAsync(token);
            }
            else
            {
                CurrentTarget = Player;
                await AttackRoutineAsync(token);
            }
        }
    }
    private async UniTask AttackRoutineAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => attackCooldownElapsed <= 0, cancellationToken: token);
        if (CurrentTarget == null || Player == null) return;
        int dir;
        if (CurrentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;
        automaticFlip = false;
        FlipByDirection(dir);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        MagicBall ball = Instantiate(magicball, transform.position, Quaternion.identity).GetComponent<MagicBall>();
        ball.Init(Player.gameObject, magicballSpeed);
        automaticFlip = true;
        attackCooldownElapsed = attackCooldown;
    }
}

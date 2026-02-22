using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
public class ArcherEnemy : EnemyBase
{
    #region  parameters
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   
    [Header("Aiming!")]
    [SerializeField] private float aimingTime = 2f;
    [SerializeField] private Transform bowTransform;
    [Header("Attack!")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    [Header("Platform Finding")]
    [SerializeField] private LayerMask platformLayer;

    [Header("SERGEANT")]
    [SerializeField] private bool isSergeant = false;

    public List<ArcherEnemy> mySoldiers;

    public bool AreMySoldiersReady()
    {
        foreach (var ae in mySoldiers)
        {
            if (!ae.NotAttacking()) return false;
        }

        return true;
    }

    public bool NotAttacking()
    {
        return !isAttacking;
    }
    
    [SerializeField] private float sergeantSearchRadius = 8f;
    #endregion
    #region components
    private LineRenderer lineRenderer;
    #endregion
    #region status
    private Vector3? nextPlatform;
    private bool canJump = false;
    public bool canAttack = false;
    private bool isAttacking = false;

    public ArcherEnemy mySergeant;
    private UnityEvent onSergeantCommand;
    
    #endregion
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(nextPlatform ?? transform.position, 1);
    }
#endif
    protected override void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        base.Start();
        onSergeantCommand = new UnityEvent();
        onDeath.AddListener(PromoteOneSoldier);

        mySoldiers = new List<ArcherEnemy>();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
        base.Update();
        if (!alerted) alerted = DetectPlayer(detectionRadius, sightMask);
        else
        {
            UpdateFound(combatRadius, sightMask);
        }
    }

    public void PromoteOneSoldier()
    {
        if (!isSergeant) return;
        var archer = FindArcherByDistance(false);
        if (archer != null) archer.isSergeant = true;
    }

    public void CommandFromSergeant()
    {
        if (!isAttacking) canAttack = true;
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        currentState = State.Alert;
        await AlertedActionAsync(token);
    }

    [CanBeNull]
    private ArcherEnemy FindArcherByDistance(bool findingSergeant)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, sergeantSearchRadius, enemyLayer);
        float minDist = float.MaxValue;
        ArcherEnemy tmp, final = null;
        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (minDist > dist && (tmp = hit.GetComponent<ArcherEnemy>()) != null && tmp.isSergeant == findingSergeant)
            {
                minDist = dist;
                final = tmp;
            }
        }

        return final;
    }
    private void GetReadyForAttack() 
    {
        if (isSergeant)
        {
            canAttack = true;
            return;
        }
        
        mySergeant = FindArcherByDistance(true);

        if (mySergeant == null || !mySergeant.isSergeant) canAttack = true;
        else
        {
            mySergeant.onSergeantCommand.AddListener(CommandFromSergeant);
            mySergeant.mySoldiers.Add(this);
        }
    }

    [SerializeField] private LayerMask enemyLayer;
    private async UniTask AlertedActionAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        while (!token.IsCancellationRequested)
        {
            if (canJump && TilemapPlatformIndex.Instance.AreOnSamePlatformByRay(Player, transform))
            {
                CurrentTarget = null;
                await ChangePlatformAsync(token);
                canJump = false;
            }
            else if (found)
            {
                if (canAttack)
                {
                    canJump = true;
                    isAttacking = true;
                    canAttack = false;
                    CurrentTarget = Player;
                    if (isSergeant)
                    {
                        UniTask.WaitUntil(AreMySoldiersReady, cancellationToken: token);
                        onSergeantCommand?.Invoke();
                    }
                    if (mySergeant != null)
                    {
                        mySergeant.onSergeantCommand.RemoveListener(CommandFromSergeant);
                        mySergeant.mySoldiers.Remove(this);
                    }
                    await AttackRoutineAsync(token);
                    isAttacking = false;
                    mySergeant = null;
                    
                    if (isSergeant) await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
                }
                else if (mySergeant == null)
                {
                    GetReadyForAttack();
                }
                else {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
                }
            }
            else
            {
                canJump = true;
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }
        }
    }
    private async UniTask ChangePlatformAsync(CancellationToken token)
    {
        Vector3 nextPos = FindNextPlatform() ?? transform.position;
        if ((nextPos - transform.position).sqrMagnitude < 0.1f) return;

        float targetspeed = 0f;
        float jumpPower = 0f;
        float xposDiff = nextPos.x - transform.position.x;
        float calculatedTime = 0f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        
        if (nextPos.y > transform.position.y)
        {
            ChangeDirection(0);
            float yposDiff = nextPos.y - transform.position.y;
            
            jumpPower = CalculateJumpPower(1.5f, yposDiff);
            calculatedTime = jumpPower / gravity
                                   + Mathf.Sqrt(2 * 0.5f * yposDiff / gravity);
        }
        else
        {
            float yposDiff = Mathf.Abs(nextPos.y - transform.position.y);
            jumpPower  = 3f;
            float additionalS = Mathf.Pow(jumpPower, 2) / (2 * gravity);
            calculatedTime = jumpPower / gravity
                                   + Mathf.Sqrt(2 * (yposDiff + additionalS) * yposDiff / gravity);
        }
        targetspeed = xposDiff / calculatedTime;
        
        int jumpcnt = 0;
        while (!TryJump(jumpPower))
        {
            Debug.Log("HERE3");
            jumpcnt++;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            if (found || jumpcnt>10) return;
        }

        canJump = false;
        
        int tmp = (int)(nextPos.x - transform.position.x);
        if (tmp == 0) return;
        ChangeDirection(tmp / Math.Abs(tmp));
        ChangeMoveSpeed(Math.Abs(targetspeed / moveSpeed));
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token); 
        await UniTask.WaitUntil(() => isGrounded, cancellationToken: token);
        ChangeDirection(0);
        ChangeMoveSpeed(1);
    }
    float CalculateJumpPower(float rate, float s)
    {
        return Mathf.Sqrt(2 * rate * Mathf.Abs(Physics2D.gravity.y) * s);
    }
    [SerializeField] private float platformFindingHeight = 3f;
    [SerializeField] private int platformFindingSampleCount = 5;
    [CanBeNull]
    private Vector3? FindNextPlatform()
    {
        for (int i = 1; i <= 3; i++)
        {
            Vector3 sampleStart = transform.position + Vector3.up * (platformFindingHeight * i);
            for (int j = 0; j < platformFindingSampleCount; j++)
            {
                var tmp = sampleStart + Vector3.right * Random.Range(-5f, 5f);
                RaycastHit2D hit = Physics2D.Raycast(tmp, Vector3.down, platformFindingHeight * 2f, platformLayer);
                if (hit.collider != null && !hit.collider.OverlapPoint(tmp))
                {
                    if (Mathf.Abs(hit.point.y - transform.position.y + col.bounds.extents.y) < 0.1f) continue;
                    nextPlatform = hit.point;
                    return hit.point;
                }
            }
        }
        return null;
    }
    private async UniTask AttackRoutineAsync(CancellationToken token)
    {
        if (lineRenderer == null) return;
        float elapsed = 0f;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2; 
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        Vector3 finalTarget = Vector3.zero;
        bool goodtogo = false;
        while (elapsed < aimingTime && !token.IsCancellationRequested)
        {
            lineRenderer.startWidth = 0.1f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            lineRenderer.endWidth = 0.1f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            lineRenderer.SetPosition(0, transform.position);

            Vector3 aimTarget;
            if (elapsed < aimingTime / 2)
            {
                finalTarget = CurrentTarget?.position ?? transform.position;
                aimTarget = finalTarget;
                lineRenderer.SetPosition(1, aimTarget);
            }
            else 
            {
                lineRenderer.startColor = Color.red;
                Color c = Color.red;
                c.a = 0;
                lineRenderer.endColor = c;
                
                aimTarget = finalTarget;
                Vector3 dirgo = (aimTarget - transform.position).normalized;
                lineRenderer.SetPosition(1, transform.position + dirgo * 20);
            }

            if (bowTransform != null)
            {
                Vector3 bowDir = aimTarget - bowTransform.position;
                if (bowDir.sqrMagnitude > 0.0001f)
                {
                    float bowAngle = Mathf.Atan2(bowDir.y, bowDir.x) * Mathf.Rad2Deg;
                    bowTransform.rotation = Quaternion.Euler(0f, 0f, bowAngle);
                }
            }
            
            
            elapsed += Time.deltaTime;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        
        
        lineRenderer.enabled = false;
        Vector2 dir = finalTarget - transform.position;
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + (Vector3)dir ,
            Quaternion.Euler(0, 0, angle)
        );
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * bulletSpeed;
    }
}

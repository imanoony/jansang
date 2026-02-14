using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
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
    [Header("Attack!")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    [Header("Platform Finding")]
    [SerializeField] private LayerMask platformLayer;
    #endregion
    #region components
    private LineRenderer lineRenderer;
    #endregion
    #region status
    private bool found = false;
    private Vector3? nextPlatform;
    private bool canJump = false;
    private bool canAttack = false;
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
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
        if (!alerted) alerted = DetectPlayer(detectionRadius, sightMask);
        else found = DetectPlayer(combatRadius, sightMask);
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        SetBaseColor(new Color(0f, 1f, 0f, 1f));
        currentState = State.Alert;
        await AlertedActionAsync(token);
    }
    private async UniTask AlertedActionAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        while (!token.IsCancellationRequested)
        {
            if (canJump && TilemapPlatformIndex.Instance.AreOnSamePlatformByRay(Player, transform))
            {
                CurrentTarget = null;
                canAttack = true;
                SetBaseColor(new Color(0f, 1f, 0f, 1f));
                await ChangePlatformAsync(token);
                canJump = false;
                SetBaseColor(new Color(0f, 1f, 1f, 1f));
            }
            else if (found && canAttack)
            {
                canJump = true;
                CurrentTarget = Player;
                SetBaseColor(new Color(1f, 0f, 0f, 1f));
                await AttackRoutineAsync(token);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Where?");
#endif
                canJump = true;
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);
            }
        }
    }
    private async UniTask ChangePlatformAsync(CancellationToken token)
    {
        Debug.Log("HERE1");
        Vector3 nextPos = FindNextPlatform() ?? transform.position;
        SetBaseColor(new Color(0f, 0f, 1f, 1f));
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
        while (elapsed < aimingTime && !token.IsCancellationRequested)
        {
            lineRenderer.startWidth = 0.05f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            lineRenderer.endWidth = 0.05f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, CurrentTarget?.position ?? transform.position);
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        lineRenderer.enabled = false;
        Vector2 dir = (CurrentTarget?.position ?? transform.position) - transform.position;
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.Euler(0, 0, angle)
        );
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * bulletSpeed;
    }
}

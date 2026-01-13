using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArcherEnemy : EnemyBase
{
    #region  parameters
    
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   // Player + Wall

    [Header("Aiming!")]
    [SerializeField] private float aimingTime = 2f;

    [Header("Attack!")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    
    #endregion
    
    #region components

    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    
    #endregion
    
    #region status

    private bool alerted = false;
    private bool found = false;
    public GameObject player;
    private Transform currentTarget;

    private Vector3? nextPlatform;
    
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
        base.Start();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(WaitForAlert());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        if (!alerted) alerted = DetectPlayer(detectionRadius);
        else found = DetectPlayer(combatRadius);
    }

    private IEnumerator WaitForAlert()
    {
        yield return new WaitUntil(() => alerted);

        //TEST
        spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
            
        currentState = State.Alert;

        StartCoroutine(AlertedAction());
    }

    private IEnumerator AlertedAction()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (found == false)
            {
                currentTarget = null;
                //TEST
                spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
                
                yield return StartCoroutine(ChangePlatform());
                
                spriteRenderer.color = new Color(0f, 1f, 1f, 1f);
            }
            else
            {
                currentTarget = player.transform;
                //TEST
                spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
                
                yield return StartCoroutine(AttackRoutine());
            }
        }
    }
    
    private IEnumerator ChangePlatform()
    {
        Vector3 nextPos = FindNextPlatform() ?? transform.position;
        
        spriteRenderer.color = new Color(0f, 0f, 1f, 1f);

        if ((nextPos - transform.position).sqrMagnitude < 0.1f) yield break;
        
        //TODO: 반복 줄이기 흠;;
        if (nextPos.y > transform.position.y)
        {
            ChangeDirection(0);

            // 다음 위치 -> 점프력과 속도 계산
            float xposDiff = nextPos.x - transform.position.x;
            float yposDiff = nextPos.y - transform.position.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y);
            
            float calculatedJumpPower = CalculateJumpPower(1.5f, yposDiff);
            float calculatedTime = calculatedJumpPower / gravity
                                   + Mathf.Sqrt(2 * 0.5f * yposDiff / gravity);
            float targetspeed = xposDiff / calculatedTime;
            
            while (!TryJump(calculatedJumpPower))
            {
                // 최대 try 횟수 제한?
                Debug.Log("HELLO");
                yield return null;
                if (found) yield break;
            }
            
            int tmp = (int)(nextPos.x - transform.position.x);
            if (tmp == 0) yield break;
            
            ChangeDirection(tmp / Math.Abs(tmp));
            ChangeMoveSpeed(Math.Abs(targetspeed / moveSpeed));
            
            // 문제구간
            yield return new WaitForSeconds(0.2f); // 땜빵
            yield return new WaitUntil(() => isGrounded);
            
            ChangeDirection(0);
            ChangeMoveSpeed(1);
        }
        else
        {
            // 다음 위치 -> 점프력과 속도 계산
            float xposDiff = nextPos.x - transform.position.x;
            float yposDiff = Mathf.Abs(nextPos.y - transform.position.y);
            
            float jumpPower = 3f;
            float gravity = Mathf.Abs(Physics2D.gravity.y);
            
            float additionalS = Mathf.Pow(jumpPower, 2) / (2 * gravity);

            
            float calculatedTime = jumpPower / gravity
                                   + Mathf.Sqrt(2 * (yposDiff + additionalS) * yposDiff / gravity);
            float targetspeed = xposDiff / calculatedTime;
            
            while (!TryJump(jumpPower))
            {
                // 최대 try 횟수 제한?
                Debug.Log("HELLO");
                yield return null;
                if (found) yield break;
            }
            
            int tmp = (int)(nextPos.x - transform.position.x);
            if (tmp == 0) yield break;
            
            ChangeDirection(tmp / Math.Abs(tmp));
            ChangeMoveSpeed(Math.Abs(targetspeed / moveSpeed));
            
            // 문제구간
            yield return new WaitForSeconds(0.2f); // 땜빵
            yield return new WaitUntil(() => isGrounded);
            
            ChangeDirection(0);
            ChangeMoveSpeed(1);
        }
    }

    float CalculateJumpPower(float rate, float s)
    {
        return Mathf.Sqrt(2 * rate * Mathf.Abs(Physics2D.gravity.y) * s);
    }
    
    private void GetSignal()
    {
        alerted = true;
    }

    private bool DetectPlayer(float range)
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > range) return false;
        
        Vector2 dir = (player.transform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, sightMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        return false;
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
                RaycastHit2D hit = Physics2D.Raycast(tmp, Vector3.down, platformFindingHeight * 2f, groundLayer);
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

    private IEnumerator AttackRoutine()
    {
        // 조준; 레이저 형태 보이기
        float elapsed = 0f;
        
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2; 
        
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        
        while (elapsed < aimingTime)
        {
            lineRenderer.startWidth = 0.05f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            lineRenderer.endWidth = 0.05f * Mathf.Sin((elapsed / aimingTime) * (Mathf.PI / 2f));
            
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentTarget?.position ?? transform.position);
            
            elapsed += Time.deltaTime;
            yield return null;
            
        }

        lineRenderer.enabled = false;
        
        // Fire!
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            transform.rotation
        );
        
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(gameObject);
        }
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        Vector2 dir = (currentTarget?.position ?? transform.position) - transform.position;
        dir.Normalize();
        
        rb.linearVelocity = dir * bulletSpeed;
    }
}

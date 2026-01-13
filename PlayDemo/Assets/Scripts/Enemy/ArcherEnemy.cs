using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArcherEnemy : EnemyBase
{
    #region  parameters
    
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   // Player + Wall
    
    #endregion
    
    #region components

    private SpriteRenderer spriteRenderer;
    
    #endregion
    
    #region status

    private bool alerted = false;
    private bool found = false;
    private GameObject player;
    private Transform currentTarget;

    private Vector3? nextPlatform;
    
    #endregion
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(nextPlatform ?? transform.position, 1);
        
    }
#endif

    protected override void Start()
    {
        base.Start();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        else found = false;
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
        while (true)
        {
            if (found == false)
            {
                //TEST
                spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
                
                Debug.Log("what's up");
                
                yield return StartCoroutine(ChangePlatform());
                
                spriteRenderer.color = new Color(0f, 1f, 1f, 1f);
            }
            else
            {
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
            yield return new WaitForSeconds(calculatedTime); // 땜빵
            yield return new WaitUntil(() => isGrounded);
            
            spriteRenderer.color = new Color(0f, 1f, 1f, 1f);
            
            ChangeDirection(0);
            ChangeMoveSpeed(1);
            
            yield return new WaitForSeconds(2f); // 땜빵
            
            spriteRenderer.color = new Color(0f, 1f, 1f, 1f);
        }
        else
        {
            
        }
    }

    float CalculateJumpPower(float rate, float s)
    {
        return Mathf.Sqrt(2 * rate * Mathf.Abs(Physics2D.gravity.y) * s);
    }

    private IEnumerator AttackRoutine()
    {
        yield break;
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
                RaycastHit2D hit = Physics2D.Raycast(tmp, Vector3.down, platformFindingHeight, groundLayer);
                if (hit.collider != null && !hit.collider.OverlapPoint(tmp))
                {
                    if (Mathf.Abs(hit.point.y - transform.position.y) < float.Epsilon) continue;
                    nextPlatform = hit.point;
                    return hit.point;
                }
            }
        }
        
        return null;
    }

    private void Attack()
    {
        
    }
}

using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

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
                
                yield return StartCoroutine(ChangePlatform());
            }
            else
            {
                //TEST
                spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
                
                yield return StartCoroutine(AttackRoutine());
            }
        }
    }

    private int ia = 0;
    private IEnumerator ChangePlatform()
    {
        Vector3 nextPos = FindNextPlatform(ia) ?? transform.position;

        if ((nextPos - transform.position).sqrMagnitude < 0.0001f) yield break;

        if (nextPos.y > transform.position.y)
        {
            ChangeDirection(0);

            float xposDiff = nextPos.x - transform.position.x;
            float yposDiff = nextPos.y - transform.position.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y);
            
            float calculatedJumpPower = CalculateJumpPower(1.3f, yposDiff);
            float calculatedTime = calculatedJumpPower / gravity
                                   + Mathf.Sqrt(2 * 0.3f * yposDiff / gravity);
            float targetspeed = xposDiff / calculatedTime;
            
            while (!TryJump(calculatedJumpPower))
            {
                // 최대 try 횟수 제한?
                yield return null;
                if (found) yield break;
            }
            
            int tmp = (int)(nextPos.x - transform.position.x);
            if (tmp == 0) yield break;
            
            ChangeDirection(tmp / Math.Abs(tmp));
            ChangeMoveSpeed(Math.Abs(targetspeed / moveSpeed));
            
            // 문제구간
            yield return new WaitForSeconds(calculatedTime); // 땜빵
            yield return new WaitUntil(() => !isGrounded);
            
            ChangeDirection(0);
            ChangeMoveSpeed(1);
            
            ia++;

            if (ia >= fortest.Length) ia = fortest.Length - 1;
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

#if UNITY_EDITOR
    [SerializeField] private Transform[] fortest;
    private Vector3? nextPlatform;
#endif
    
    [CanBeNull]
    private Vector3? FindNextPlatform(int i)
    {
        var a = fortest[i].GetComponent<Collider2D>();
        Vector2 pos = a.bounds.center + new Vector3(0, a.bounds.extents.y, 0);
        nextPlatform = pos;
        return pos;
    }

    private void Attack()
    {
        
    }
}

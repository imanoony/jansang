using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpearManEnemy : EnemyBase
{
    #region  parameters
    
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   // Player + Wall
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask wallLayer;

    [Header("Movement!")] 
    [SerializeField] private float directionTimeChange = 2f;

    [Header("Combat")] 
    [SerializeField] private float thrustRadius = 1.3f;
    
    #endregion
    
    #region components

    private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D damageArea;
    #endregion
    
    #region status
    
    private bool found = false;
    
    private float directionTimeChangeElapsed;
    
    public GameObject player;
    private Transform currentTarget;
    
    #endregion
    protected override void Start()
    {
        base.Start();
        damageArea.enabled = false;
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
                Flip(dir);
            }
            
            alerted = DetectPlayer(detectionRadius);
        }
        else
        {
            if (automaticFlip)
            {
                //TODO: 하 이거 진짜;;;
                if (player.transform.position.x > transform.position.x) Flip(1);
                else if (player.transform.position.x < transform.position.x) Flip(-1);
            }
            if (rushStart)
            {
                if (DetectCliff() ||
                    Physics2D.OverlapCircle(transform.position, thrustRadius, playerMask) != null ||
                    MoveDirection == 0)
                {
                    rushStart = false;
                    canThrust = true;
                }
            } 
            found = DetectPlayer(combatRadius); // 같은 플랫폼에 있는거로 바꾸기
        }
        
        if (DetectCliff()) ChangeDirection(0);
    }

    private void Flip(int direction)
    {
        //TODO: 좀 이상한데 급하게 하느라 이렇게 됨... 이거 수정해야함

        var x = Mathf.Abs(transform.localScale.x);
        if (direction == -1) transform.localScale = new Vector3(-x, transform.localScale.y, transform.localScale.z);
        else if (direction == 1) transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }
    
    private bool DetectCliff()
    {
        if (MoveDirection == 0) return true;
        
        Vector3 start = transform.position + Vector3.right * (MoveDirection * col.bounds.extents.x * 1.2f);
        RaycastHit2D hit = Physics2D.Raycast(start, Vector3.down, 1, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, Vector3.right * MoveDirection, col.bounds.extents.x * 1.2f, wallLayer);
        
        if (hit.collider == null || hit2)
        {
            return true;
        }

        return false;
    }

    private IEnumerator WaitForAlert()
    {
        yield return new WaitUntil(() => alerted);
        
        ChangeDirection(0);
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
                
                yield return StartCoroutine(Wander());
            }
            else
            {
                currentTarget = player.transform;
                yield return StartCoroutine(AttackRoutine());
            }
        }
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

    private bool rushStart;
    private bool canThrust;

    private bool automaticFlip = false;

    private IEnumerator Wander()
    {
        //TEST
        spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
        int dir = Random.Range(-1, 2);

        ChangeDirection(dir);
        yield return new WaitForSeconds(2f);
        ChangeDirection(0);
    }
    
    private IEnumerator AttackRoutine()
    {
        if (currentTarget == null) yield break;
        
        //TEST
        spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
        
        // 돌진!
        int dir;
        if (currentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;

        automaticFlip = false;
        
        ChangeDirection(dir);
        ChangeMoveSpeed(2);
        Flip(dir);

        rushStart = true;
        canThrust = false;
        yield return new WaitUntil(() => canThrust);
        
        // 찌르기!
        spriteRenderer.color = new Color(1f, 1f, 0f, 1f);
        
        automaticFlip = true;
        damageArea.enabled = true;
        yield return new WaitForSeconds(0.3f);
        
        spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f);
        damageArea.enabled = false;
        ChangeDirection(-dir);
        ChangeMoveSpeed(0.2f);
        
        yield return new WaitForSeconds(1f);
        
        ChangeDirection(0);
        ChangeMoveSpeed(1f);
    }
}

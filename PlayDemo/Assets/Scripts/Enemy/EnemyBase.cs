using System;
using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    #region HP

    [Header("HP Stats")]
    public float HP { get; protected set; }

    public int MaxHP;

    #endregion
    #region  parameters

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;
    
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;

    [Header("Commander")] [SerializeField] private SummonerEnemy summoner;

    #endregion
    
    #region components

    protected Animator animator;
    protected Collider2D col;
    private Rigidbody2D rb;
    
    #endregion
    
    #region status

    private float currentSpeedRate;
    
    protected bool alerted = false;
    
    protected enum State
    {
        Idle,
        Alert,
        Combat
    }
    
    protected State currentState = State.Idle;
    
    protected bool isGrounded;
    private int moveDirection = 0; // -1 for left, 1 for right, and 0 for idle
    public int MoveDirection => moveDirection;
    
    #endregion
    
    
    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        if (summoner != null) summoner.RegisterSignal(GetSignal);
        HP = MaxHP;
        currentSpeedRate = 1;
    }

    protected virtual void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    protected virtual void Update()
    {
         //mola
    }
    
    private void GetSignal()
    {
        alerted = true;
    }

    private void CheckGround()
    {
        if (rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }
        
        Bounds bounds = col.bounds;

        var start = bounds.center + bounds.extents.y * Vector3.down;
        
        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (hit.collider != null && hit.collider.OverlapPoint(start + Vector3.up * float.Epsilon))
        {
            // 발이 플랫폼 안에 있다면 ㄴㄴ
            isGrounded = false;
            return;
        }
        
        // 노멀 체크 (옆면/벽 배제)
        isGrounded = hit.collider != null && hit.normal.y > 0.7f;
    }
    
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed * currentSpeedRate, rb.linearVelocity.y);
    }

    protected virtual void ChangeDirection(int direction)
    {
        moveDirection = direction;
        if (direction == 0)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    protected void ChangeMoveSpeed(float rate)
    {
        currentSpeedRate = rate;
    }
    
    protected bool TryJump(float jumpForce)
    {
        if (jumpForce < 0) return false;
        
        if (isGrounded)
        {
            Jump(jumpForce);
            return true;
        }

        return false;
    }

    private void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    protected virtual void OnDestroy()
    {
        if (summoner != null) summoner.RemoveSignal(GetSignal);
    }

    protected virtual void OnDisable()
    {
        if (summoner != null) summoner.RemoveSignal(GetSignal);
    }

    public virtual void Hit()
    {
        StartCoroutine(HitRoutine(1f));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            StartCoroutine(HitRoutine(0.5f));
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            StartCoroutine(HitRoutine(0.3f*Time.deltaTime));
        }
    }
    IEnumerator HitRoutine(float damage)
    {
        this.HP -= damage;
        if (HP <= 0)
        {
            Destroy(this.gameObject);
        }
        var sprite = GetComponent<SpriteRenderer>();
        var c = sprite.color;
        
        sprite.color = Color.red;
        
        yield return new WaitForSeconds(0.5f);

        sprite.color = c;
    }
}

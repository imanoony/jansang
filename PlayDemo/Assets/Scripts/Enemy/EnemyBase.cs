using System;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    #region  parameters

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;
    
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;

    #endregion
    
    
    #region components

    protected Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;
    
    #endregion
    
    #region status

    private float currentSpeedRate;
    
    protected enum State
    {
        Idle,
        Alert,
        Combat
    }
    
    protected State currentState = State.Idle;
    
    protected bool isGrounded;
    private int moveDirection = 0; // -1 for left, 1 for right, and 0 for idle
    
    #endregion
    
    
    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
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

    private void CheckGround()
    {
        Bounds bounds = col.bounds;

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // 노멀 체크 (옆면/벽 배제)
        isGrounded = hit.collider != null && hit.normal.y > 0.7f;
    }
    
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed * currentSpeedRate, rb.linearVelocity.y);
    }

    protected void ChangeDirection(int direction)
    {
        moveDirection = direction;
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
}

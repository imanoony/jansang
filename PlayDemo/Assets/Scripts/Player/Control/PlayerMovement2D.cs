using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;

    Rigidbody2D rb;
    Collider2D col;

    float moveInput;
    bool isGrounded;
    public bool IsGrouneded => isGrounded;
    
    bool airJumpUsed;

    private CharacterManager manager;

    public void Init(CharacterManager manager)
    {
        this.manager = manager;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) manager.attack.isRight = true;
        else if (moveInput < 0) manager.attack.isRight = false;
    }

    public void ResetJump()
    {
        airJumpUsed = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 4f);
    }
    void TryJump()
    {
        if (isGrounded)
        {
            Jump();
            airJumpUsed = false; // 바닥 점프는 소모 안 함
        }
        else if (!airJumpUsed)
        {
            Jump();
            airJumpUsed = true; // 공중 점프 1회 소모
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void CheckGround()
    {
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

        if (hit.collider != null && hit.collider.OverlapPoint(start))
        {
            // 발이 플랫폼 안에 있다면 ㄴㄴ
            return;
        }
        
        // 노멀 체크 (옆면/벽 배제)
        isGrounded = hit.collider != null && hit.normal.y > 0.7f;

        if (isGrounded)
        {
            airJumpUsed = false;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            col.bounds.center + Vector3.down * groundCheckDistance,
            col.bounds.size
        );
    }
#endif
}
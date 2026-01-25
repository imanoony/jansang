using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("1초당 이동하는 타일")]
    public float moveTile = 2f;
    [Tooltip("한 번의 전투에 이동하는 타일 수")]
    public float jumpTile  = 2f;
    [Header("Setting")] 
    [Tooltip("하나의 타일이 차지하는 월드 좌표상의 높이")]
    public float tileHeight=1f;
    [Tooltip("점프를 뛰었을 때 주는 여유 공간 비율(타일 기준)")]
    public float jumpOffset = 0.5f;
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;

    Rigidbody2D rb;
    Collider2D col;
    MeleeController2D attack;

    private float gravity = 1;
    float moveInput;
    bool isGrounded;
    public bool IsGrouneded => isGrounded;
    bool airJumpUsed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        attack = GetComponent<MeleeController2D>();
        gravity = -Physics.gravity.y * rb.gravityScale;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            moveInput = context.ReadValue<Vector2>().x;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
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
        rb.linearVelocity = new Vector2(moveInput * moveTile*tileHeight, rb.linearVelocity.y);

        if (moveInput > 0) attack.isRight = true;
        else if (moveInput < 0) attack.isRight = false;
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
        float currentTileY = Mathf.Round(transform.position.y / tileHeight);
        float targetY = (currentTileY+jumpOffset + jumpTile)* tileHeight;
        float jumpForce = Mathf.Sqrt(2*(targetY - transform.position.y/tileHeight)*gravity);
        Debug.Log(gravity);
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
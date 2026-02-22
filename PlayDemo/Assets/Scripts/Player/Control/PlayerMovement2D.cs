using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]

    public Vector2 rawInput;
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    


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

    [Header("Dash")]
    public float dashCooldown = 2f;
    public float dashCurrentCooldown = 0f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.3f;
    public bool dashing = false;

    [Header("Crowd Control")]
    public bool silenced = false;
    public void DashSilence(float time)
    {
        silenced = true;
        StartCoroutine(SilenceTimer(time));
    }
    private IEnumerator SilenceTimer(float time)
    {
        yield return new WaitForSeconds(time);
        silenced = false;
    }
    public bool stunned = false;
    public void Stun(float time)
    {
        stunned = true;
        StartCoroutine(StunTimer(time));
    }
    IEnumerator StunTimer(float time)
    {
        yield return new WaitForSeconds(time);
        stunned = false;
    }
    


    Rigidbody2D rb;
    Collider2D col;
    MeleeController2D attack;
    private PlayerGFXController playerGFX; 
    [Header("SFX")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField, Range(0f, 1f)] private float jumpSfxVolume = 1f;
    private AudioManager audioManager;

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
        playerGFX = GetComponent<PlayerGFXController>();
        gravity = -Physics.gravity.y * rb.gravityScale;
        audioManager = GameManager.Instance != null ? GameManager.Instance.Audio : null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();

        if (!CanMove()) { moveInput = 0f; return; }

        if (context.performed || context.canceled)
        {
            Vector2 raw = context.ReadValue<Vector2>();
            SetMoveInput(raw.x);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!CanMove()) return;

        if (context.performed)
            TriggerJump();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (silenced || stunned) return;
        if (context.performed)
            TriggerDash(Vector2.zero);
    }
    // 🔹 공통 로직 함수
    public void SetMoveInput(float value)
    {
        if (dashing) return;
        moveInput = value;
    }

    public void TriggerJump()
    {
        if (dashing) return;
        TryJump();
    }

    public void TriggerDash(Vector2 dir)
    {
        TryDash(dir);
    }

    void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    public float moveSpeedAccel = 5;
    public float moveSpeedDeaccel = 5;
    public float actualSpeed;
    
    private Vector2 movementVector;
    public bool disable = false;
    public bool CanMove()
    {
        return !dashing && !stunned && !disable;
    }
    
    void Move()
    {
        if (!CanMove()) moveInput = 0;
        if (moveInput != 0)
        {
            actualSpeed += moveInput * moveSpeedAccel * Time.deltaTime;
        }
        else
        {
            if (Mathf.Abs(actualSpeed) > 0.1f)
            {
                actualSpeed -= (Time.deltaTime * moveSpeedDeaccel) * (actualSpeed / Mathf.Abs(actualSpeed));
            }
            else
            {
                actualSpeed = 0;
            }
        }

        actualSpeed = Mathf.Clamp(actualSpeed, -moveTile, moveTile);

        movementVector.x = actualSpeed;
        movementVector.y = rb.linearVelocityY;
        
        rb.linearVelocity = movementVector;

        if (moveInput > 0) attack.isRight = true;
        else if (moveInput < 0) attack.isRight = false;
        
        playerGFX.Flip(attack.isRight);
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
        playerGFX.Stretch();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (audioManager != null) audioManager.PlaySfx(jumpSfx, jumpSfxVolume);
    }

    void TryDash(Vector2 dir)
    {
        if(dashCurrentCooldown <= 0f)
        {
            dashCurrentCooldown = dashCooldown;
            Dash(dir);
        }
    }

    void Dash(Vector2 dir)
    {
        Vector2 dashDir = dir.normalized;

        Vector2 dashV = new Vector2(dashDir.x * dashSpeed, dashDir.y * dashSpeed);
        rb.linearVelocity = dashV;
        StartCoroutine(DashCooldownRoutine(dashV));
    }

    IEnumerator DashCooldownRoutine(Vector2 dV)
    {
        float dashTime = 0.01f;
        dashing = true;
        float gS = rb.gravityScale;
        rb.gravityScale = 0f;

        while (dashTime <= dashDuration)
        {
            dashCurrentCooldown -= Time.fixedDeltaTime;
            dashTime += Time.fixedDeltaTime; 
            rb.linearVelocity = dV * (float)(1 - Math.Pow(dashTime / dashDuration, 2) * 0.5);

            if (isGrounded)
            {
                dashTime += Time.fixedDeltaTime * 4f; // 땅에 닿으면 대시 시간 빨리 소모
            }

            yield return new WaitForFixedUpdate();
        }


        dashing = false;
        rb.gravityScale = gS;

        while (dashCurrentCooldown > 0f)
        {
            dashCurrentCooldown -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        dashCurrentCooldown = 0f;
    }

    void CheckGround()
    {
        Bounds bounds = col.bounds;
        Vector2 start = bounds.center + bounds.extents.y * Vector3.down;

        // 발이 플랫폼 내부라면 땅으로 취급하지 않음 (PlatformerEffector 대응)
        if (Physics2D.OverlapPoint(start, groundLayer) != null)
        {
            isGrounded = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            start,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        
        if (!isGrounded)
        {
            // 노멀 체크 (옆면/벽 배제)
            if (rb.linearVelocityY <= 0.1f && hit.collider != null && hit.normal.y > 0.7f)
            {
                isGrounded = true;
                playerGFX.Squash();
                airJumpUsed = false;

                return;
            }
            else
            {
                return;
            }
        }
        
        isGrounded = hit.collider != null && hit.normal.y > 0.7f;
        
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (col == null) return;

        Gizmos.color = Color.red;
        Vector3 start = col.bounds.center + col.bounds.extents.y * Vector3.down;
        Gizmos.DrawLine(start, start + Vector3.down * groundCheckDistance);
    }
#endif
}

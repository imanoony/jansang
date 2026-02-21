using UnityEngine;

public class Seed : MonoBehaviour
{
    Rigidbody2D rb;

    bool passedApex;

    [Header("Drop Physics")]
    [SerializeField] float dropGravityScale = 5f;
    [SerializeField] bool stopXVelocityAtApex = true;

    [Header("Sway")]
    [SerializeField] float swayStrength = 0.3f;
    [SerializeField] float swaySpeed = 4f;
    [SerializeField] private SwapTarget swap;
    [Header("Ground Check")]
    [SerializeField] float groundCheckDistance = 0.15f;
    [SerializeField] LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        swap.enabled = false;
    }

    void Update()
    {
        CheckApex();
        CheckGround();
    }

    void FixedUpdate()
    {
        if (passedApex)
        {
            // 좌우 흔들림 (단풍나무 씨앗 느낌)
            float sway = Mathf.Sin(Time.time * swaySpeed) * swayStrength;
            rb.linearVelocity += Vector2.right * (sway * Time.fixedDeltaTime);
        }
    }

    void CheckApex()
    {
        if (passedApex) return;

        // 정점 통과
        if (rb.linearVelocity.y <= 0f)
        {
            passedApex = true;

            if (stopXVelocityAtApex)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.gravityScale = dropGravityScale;
            swap.enabled = true;
        }
    }

    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            OnGrounded(hit);
        }
    }

    void OnGrounded(RaycastHit2D hit)
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * groundCheckDistance
        );
    }
#endif
}
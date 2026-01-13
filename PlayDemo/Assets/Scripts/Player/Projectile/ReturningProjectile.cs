using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ReturningProjectile : MonoBehaviour
{
    public enum State
    {
        Going,
        Hovering,
        Returning
    }

    State m_State;

    Rigidbody2D rb;
    Transform player;

    float goingTimer;
    float hoveringTimer;

    [SerializeField] float goingTime = 0.4f;
    [SerializeField] float hoverTime = 1f;

    [SerializeField] float shootForce = 20f;
    [SerializeField] float returnSpeed = 30f;
    [SerializeField] float spinSpeed = 720f;
    [SerializeField] float autoCollectDistance = 0.5f;

    System.Action onReturn;

    public State state
    {
        get => m_State;
        set => m_State = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void Init(
        Vector3 startPos,
        Vector2 shootDirection,
        Transform playerTransform,
        System.Action onReturnCallback
    )
    {
        transform.position = startPos;
        player = playerTransform;
        onReturn = onReturnCallback;

        m_State = State.Going;
        goingTimer = goingTime;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(shootDirection.normalized * shootForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        if (player != null && state == State.Returning &&
            Vector3.Distance(transform.position, player.position) <= autoCollectDistance)
        {
            Collect();
            return;
        }

        switch (state)
        {
            case State.Going:
                UpdateGoing();
                break;

            case State.Hovering:
                UpdateHovering();
                break;

            case State.Returning:
                MoveToPlayer();
                break;
        }
    }

    void UpdateGoing()
    {
        goingTimer -= Time.deltaTime;
        if (goingTimer <= 0f)
        {
            m_State = State.Hovering;
            hoveringTimer = hoverTime;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void UpdateHovering()
    {
        hoveringTimer -= Time.deltaTime;
        if (hoveringTimer <= 0f)
        {
            m_State = State.Returning;
            rb.simulated = false; // 복귀 시 물리 OFF
        }
    }

    void MoveToPlayer()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            returnSpeed * Time.deltaTime
        );
    }

    public void Collect()
    {
        onReturn?.Invoke();
        Destroy(gameObject);
    }
}

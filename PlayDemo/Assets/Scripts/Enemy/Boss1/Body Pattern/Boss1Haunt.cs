using UnityEngine;

public class Boss1Haunt : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private GameObject playerObject;
    private Transform playerTransform;
    private PlayerHitCheck playerHithCheck;

    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float acceleration = 0.05f;

    private Transform spiritTransform;
    private Rigidbody2D spiritRigidbody;

    private void Awake()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        playerTransform = playerObject.transform;
        playerHithCheck = playerObject.GetComponent<PlayerHitCheck>();

        spiritTransform = GetComponent<Transform>();
        spiritRigidbody = GetComponent<Rigidbody2D>();
    }

    private float lifeTime = 5f;
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private Vector2 vDirection;
    private Vector2 playerDirection;
    private void Update()
    {
        vDirection = spiritRigidbody.linearVelocity;
        playerDirection = (playerTransform.position - spiritTransform.position).normalized;


        vDirection += playerDirection * acceleration;
        vDirection = Vector2.ClampMagnitude(vDirection, maxSpeed);
        spiritRigidbody.linearVelocity = vDirection;

        float angle = Mathf.Atan2(vDirection.y, vDirection.x) * Mathf.Rad2Deg;
        spiritTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHithCheck.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}

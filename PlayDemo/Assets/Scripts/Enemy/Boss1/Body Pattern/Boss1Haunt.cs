using UnityEngine;

public class Boss1Haunt : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float acceleration = 0.05f;


    private Transform spiritTransform;
    private Rigidbody2D spiritRigidbody;

    private Boss1Manage bossManage;

    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1Manage>();

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
        playerDirection = (bossManage.playerTransform.position - spiritTransform.position).normalized;


        vDirection += playerDirection * acceleration;
        vDirection = Vector2.ClampMagnitude(vDirection, maxSpeed);
        spiritRigidbody.linearVelocity = vDirection;

        float angle = Mathf.Atan2(vDirection.y, vDirection.x) * Mathf.Rad2Deg;
        spiritTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Spirit Collided with: " + other.tag);

        if (other.CompareTag("Player"))
        {
            bossManage.playerHitCheck.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}

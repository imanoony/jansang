using UnityEngine;

public class Boss1_Haunt : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 3f;
    public float acceleration = 0.05f;

    [Header("Health Settings")]
    [SerializeField] int health = 2;

    private Transform spiritTransform;
    private Rigidbody2D spiritRigidbody;

    private Boss1_Manage bossManage;

    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1_Manage>();

        spiritTransform = GetComponent<Transform>();
        spiritRigidbody = GetComponent<Rigidbody2D>();

        gameObject.transform.SetParent(null);
    }

    private float lifeTime = 5f;
    private void Start()
    {
        health = 2;
        Destroy(gameObject, lifeTime);
    }

    private Vector2 vDirection;
    private Vector2 playerDirection;
    private void FixedUpdate()
    {
        Vector2 dir = (bossManage.playerTransform.position - spiritTransform.position).normalized;
        spiritRigidbody.AddForce(dir * acceleration, ForceMode2D.Force);

        Vector2 v = spiritRigidbody.linearVelocity;
        if (v.sqrMagnitude > maxSpeed * maxSpeed)
        {
            spiritRigidbody.linearVelocity = v.normalized * maxSpeed;
        }

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        spiritTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossManage.playerHitCheck.TakeDamage(1);
            Destroy(gameObject);
        }

        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            health--;
            if(health <= 0){
                Destroy(gameObject);
            }
        }
    }
}

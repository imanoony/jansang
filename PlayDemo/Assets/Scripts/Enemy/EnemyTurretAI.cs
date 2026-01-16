using UnityEngine;

public class EnemyTurretAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 8f;
    public LayerMask sightMask;   // Player + Wall
    public Transform firePoint;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public float fireCooldown = 1.5f;
    public float bulletSpeed = 10f;
    float fireTimer;
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        fireTimer = fireCooldown;
    }

    void Update()
    {
        if (player == null) return;

        fireTimer -= Time.deltaTime;

        if (IsPlayerDetected())
        {
            AimAtPlayer();

            if (fireTimer <= 0f)
            {
                Fire();
                fireTimer = fireCooldown;
            }
        }
    }

    // =======================
    // 감지 로직 (벽 고려)
    // =======================
    bool IsPlayerDetected()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectRange) return false;

        Vector2 origin = firePoint.position;
        Vector2 dir = (player.position - firePoint.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, sightMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        return false;
    }

    // =======================
    // 조준
    // =======================
    void AimAtPlayer()
    {
        Vector2 dir = player.position - firePoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    // =======================
    // 발사
    // =======================
    void Fire()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
    /*
        // 🔥 발사자 자신과 잠깐 충돌 무시
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(gameObject);
        }
        */
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        Vector2 dir = player.position - firePoint.position;
        dir.Normalize();
        rb.linearVelocity = dir * bulletSpeed;
    }

    // =======================
    // 디버그 시야 표시
    // =======================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                firePoint.position,
                firePoint.position + firePoint.right * detectRange
            );
        }
    }
}

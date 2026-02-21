using UnityEngine;

public class Seed_Shooter : MonoBehaviour
{
    [Header("Apex Target")]
    [SerializeField] Transform target;   // 포물선 최고점
    [SerializeField] GameObject seedPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Vector2 start = transform.position;
        Vector2 apex = target.position;

        float g = Mathf.Abs(Physics2D.gravity.y);

        float deltaY = apex.y - start.y;
        if (deltaY <= 0f)
        {
            Debug.LogWarning("타겟은 시작 위치보다 위에 있어야 합니다");
            return;
        }

        // 1️⃣ 수직 초기 속도 (정점 도달용)
        float vy0 = Mathf.Sqrt(2f * g * deltaY);

        // 2️⃣ 정점까지 걸리는 시간
        float tApex = vy0 / g;

        // 3️⃣ 수평 초기 속도
        float vx0 = (apex.x - start.x) / tApex;

        Vector2 initialVelocity = new Vector2(vx0, vy0);

        GameObject seedObj = Instantiate(seedPrefab, start, Quaternion.identity);
        Rigidbody2D rb = seedObj.GetComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.linearVelocity = initialVelocity;
    }
}
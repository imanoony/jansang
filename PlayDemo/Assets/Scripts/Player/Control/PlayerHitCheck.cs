using UnityEngine;
using System.Collections;

public class PlayerHitCheck : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private CharacterManager manager;
    void Start()
    {
        manager = GameManager.Instance.Char;
    }

    [Header("Invincibility")]
    public float invincibleTime = 1.0f;
    private bool isInvincible = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            TakeDamage(1);
        }
        else if (other.CompareTag("EnemyProjectile"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            TakeDamage(1);
        }
        else if (other.CompareTag("EnemyAttack"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;
        manager.SubHP(damage);

        if (manager.HP <= 0)
        {
            Die();
            return;
        }
        StartCoroutine(NoDamage());
    }

    IEnumerator NoDamage()
    {
        isInvincible = true;
        spriteRenderer.color = Color.yellow;
        //애니메이션을 통해서 피격 이펙트 구현예상
        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;

        spriteRenderer.color = Color.black;
    }

    void Die()
    {
        Debug.Log("Player Dead");
    }
}
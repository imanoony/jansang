using System;
using System.Collections;
using UnityEngine;
public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask enemyHittableLayer;
    [SerializeField] private LayerMask playerLayer;
    Collider2D bulletCol;
    Collider2D ownerCol;
    [SerializeField] private int attackDamage = 3;

    public float lifeTime = 7f;

    private void Start()
    {
        StartCoroutine(LifeTimeRoutine());
    }

    IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        
        Destroy(this);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if ((enemyHittableLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Hit(attackDamage, transform.position);
            }
            Destroy(gameObject);
            return;
        }
        
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerHitCheck enemy = other.GetComponent<PlayerHitCheck>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
            Destroy(gameObject);
        }
    }
}

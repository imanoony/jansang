using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask enemyLayer;

    HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    void OnEnable()
    {
        hitTargets.Clear();

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position,
            ((BoxCollider2D)GetComponent<Collider2D>()).size,
            0f,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            Damage(hit);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Damage(other);
    }

    void Damage(Collider2D other)
    {
        if (!hitTargets.Add(other)) return;

        other.GetComponent<EnemyTurretAI>()?.gameObject.SetActive(false);
    }
}
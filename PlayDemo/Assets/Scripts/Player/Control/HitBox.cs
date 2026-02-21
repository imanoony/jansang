using System;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask enemyLayer;

    HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    public MeleeController2D.AttackState attackState;

    private Transform playerTransform;
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }


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
        if (playerTransform) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        bool hitEnemy = false;
        var turret = other.GetComponent<EnemyTurretAI>();
        if (turret != null)
        {
            turret.gameObject.SetActive(false);
            hitEnemy = true;
        }

        var enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            switch (attackState)
            {
                case MeleeController2D.AttackState.Weak:
                    enemy.Hit(1, playerTransform.position);
                    break;
                case MeleeController2D.AttackState.Middle:
                    enemy.Hit(2, playerTransform.position);
                    break;
                case MeleeController2D.AttackState.Strong:
                    enemy.Hit(5, playerTransform.position);
                    break;
                default:
                    break;
            }
            
            hitEnemy = true;
        }

        if (hitEnemy)
        {
        }
    }
}

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

        var boxCollider = GetComponent<BoxCollider2D>();
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position,
            boxCollider.size,
            0f,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            Damage(hit);
        }

        Collider2D[] bossHits = Physics2D.OverlapBoxAll(
            transform.position,
            boxCollider.size,
            0f
        );

        foreach (var hit in bossHits)
        {
            if (hit.GetComponent<Boss1_Altar>() != null ||
                hit.GetComponent<Boss1_Body>() != null ||
                hit.GetComponent<Boss1_LeftHand>() != null ||
                hit.GetComponent<Boss1_RightHand>() != null)
            {
                Damage(hit);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        
        Damage(other);
    }

    void Damage(Collider2D other)
    {
        if (!hitTargets.Add(other)) return;
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        bool hitEnemy = false;
        int attackDamage = GetAttackDamage();
        var turret = other.GetComponent<EnemyTurretAI>();
        if (turret != null)
        {
            turret.gameObject.SetActive(false);
            hitEnemy = true;
        }

        var altar = other.GetComponent<Boss1_Altar>();
        if (altar != null)
        {
            if (altar.TryHit())
            {
                hitEnemy = true;
            }
        }

        var bossManage = other.GetComponentInParent<Boss1_Manage>();
        if (bossManage != null && attackDamage > 0)
        {
            if (other.GetComponent<Boss1_Body>() != null)
            {
                bossManage.TakeDamage(Boss1_Part.Body, attackDamage);
                hitEnemy = true;
            }
            else if (other.GetComponent<Boss1_LeftHand>() != null)
            {
                bossManage.TakeDamage(Boss1_Part.LHand, attackDamage);
                hitEnemy = true;
            }
            else if (other.GetComponent<Boss1_RightHand>() != null)
            {
                bossManage.TakeDamage(Boss1_Part.RHand, attackDamage);
                hitEnemy = true;
            }
        }

        var enemy = other.GetComponent<EnemyBase>();
        if (enemy != null && attackDamage > 0)
        {
            enemy.Hit(attackDamage, playerTransform.position);
            hitEnemy = true;
        }

        if (hitEnemy)
        {
        }
    }

    int GetAttackDamage()
    {
        switch (attackState)
        {
            case MeleeController2D.AttackState.Weak:
                return 1;
            case MeleeController2D.AttackState.Middle:
                return 2;
            case MeleeController2D.AttackState.Strong:
                return 5;
            default:
                return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
public class MagicBall : MonoBehaviour
{
    private GameObject player;
    private float speed;
    private Vector2 velocity;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private bool useInitSpeed = true;
    [SerializeField] private Vector2 maxSpeed = new Vector2(2f, 2f);
    [SerializeField] private Vector2 acceleration = new Vector2(10f, 10f);
    [SerializeField] private float destroyTime;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float explosionRadius = 0.5f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackStunDuration = 0.2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyHittableLayer;
    [SerializeField] private Collider2D ball;
    [SerializeField] private Collider2D detectBoundary;
    [SerializeField] [CanBeNull] private BallCollisionDetection ballCollision;
    [SerializeField] [CanBeNull] private BallCollisionDetection boundaryCollision;
    [SerializeField] private bool detachParticlesOnDestroy = true;
    private ParticleSystem[] childParticles;
    private bool destroyed;

    private void Awake()
    {
        CacheChildParticles();
    }
    public void Init(GameObject player, float speed)
    {
        this.player = player;
        this.speed = speed;
        velocity = Vector2.zero;
        if (useInitSpeed) maxSpeed = new Vector2(speed, speed);
        float angle = Mathf.Atan2(
            player.transform.position.y - transform.position.y, 
            player.transform.position.x - transform.position.x);
        ballCollision?.onCollision.AddListener(Explosion);
        boundaryCollision?.onCollision.AddListener(Explosion);
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        DestroyTimerAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }
    private void Update()
    {
        if (player == null) return;

        Vector2 toPlayer = player.transform.position - transform.position;
        float maxX = Mathf.Max(0f, maxSpeed.x);
        float maxY = Mathf.Max(0f, maxSpeed.y);
        float accelX = Mathf.Max(0f, acceleration.x);
        float accelY = Mathf.Max(0f, acceleration.y);

        float targetVx = Mathf.Sign(toPlayer.x) * maxX;
        float targetVy = Mathf.Sign(toPlayer.y) * maxY;

        velocity.x = Mathf.MoveTowards(velocity.x, targetVx, accelX * Time.deltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, targetVy, accelY * Time.deltaTime);

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
    }
    private async UniTask DestroyTimerAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: token);
        DestroySelf();
    }
    private void Explosion(Collision2D collision)
    {
        TryApplyExplosionDamage(collision);
        DestroySelf();
    }

    private void TryApplyExplosionDamage(Collision2D collision)
    {
        if (collision == null) return;

        bool hitPlayer = collision.collider != null &&
            (collision.collider.CompareTag("Player") || IsInLayerMask(collision.gameObject.layer, playerLayer));

        if (!hitPlayer) return;

        var playerHit = collision.collider.GetComponentInParent<PlayerHitCheck>();
        if (playerHit != null) playerHit.TakeDamage(attackDamage);

        ApplyPlayerKnockback(collision.collider);
        ApplyEnemyHittableDamage();
    }

    private void ApplyPlayerKnockback(Collider2D playerCol)
    {
        if (playerCol == null) return;

        var playerRb = playerCol.attachedRigidbody != null
            ? playerCol.attachedRigidbody
            : playerCol.GetComponentInParent<Rigidbody2D>();
        if (playerRb == null) return;

        var playerMovement = playerRb.GetComponent<PlayerMovement2D>();
        if (playerMovement != null && knockbackStunDuration > 0f)
        {
            playerMovement.Stun(knockbackStunDuration);
        }

        Vector2 dir = (playerRb.transform.position - transform.position).normalized;
        playerRb.linearVelocity = dir * knockbackForce;
    }

    private void ApplyEnemyHittableDamage()
    {
        if (enemyHittableLayer.value == 0) return;

        float radius = GetExplosionRadius();
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyHittableLayer);
        if (hits == null || hits.Length == 0) return;

        HashSet<EnemyBase> damaged = new HashSet<EnemyBase>();
        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponentInParent<EnemyBase>();
            if (enemy == null || !damaged.Add(enemy)) continue;
            enemy.Hit(attackDamage, transform.position);
        }
    }

    private float GetExplosionRadius()
    {
        if (explosionRadius > 0f) return explosionRadius;
        if (ball != null) return Mathf.Max(ball.bounds.extents.x, ball.bounds.extents.y);
        return 0.5f;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void DestroySelf()
    {
        if (destroyed) return;
        destroyed = true;
        if (detachParticlesOnDestroy) DetachChildParticles();
        Destroy(gameObject);
    }

    private void CacheChildParticles()
    {
        childParticles = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void DetachChildParticles()
    {
        if (childParticles == null || childParticles.Length == 0) return;
        foreach (var ps in childParticles)
        {
            if (ps == null) continue;
            if (ps.transform == transform) continue;

            ps.transform.SetParent(null, true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            float delay = GetParticleTotalDuration(ps);
            Destroy(ps.gameObject, Mathf.Max(0.05f, delay));
        }
    }

    private static float GetParticleTotalDuration(ParticleSystem ps)
    {
        var main = ps.main;
        float lifetime = main.startLifetime.constantMax;
        return main.duration + lifetime;
    }
}


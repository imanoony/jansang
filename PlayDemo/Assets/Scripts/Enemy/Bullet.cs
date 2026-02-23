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
    [SerializeField] private bool detachParticlesOnDestroy = true;
    private ParticleSystem[] childParticles;
    private bool destroyed;

    public float lifeTime = 7f;

    private void Awake()
    {
        CacheChildParticles();
    }

    private void Start()
    {
        StartCoroutine(LifeTimeRoutine());
    }

    IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        
        DestroySelf();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            DestroySelf();
            return;
        }

        if ((enemyHittableLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Hit(attackDamage, transform.position);
            }
            DestroySelf();
            return;
        }
        
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerHitCheck enemy = other.GetComponent<PlayerHitCheck>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
            DestroySelf();
        }
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

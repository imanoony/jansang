using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask enemyLayer;
    [Header("Hit Slow")]
    [SerializeField] private float hitSlowScale = 0.2f;
    [SerializeField] private float hitSlowDuration = 0.06f;
    [Header("Hit FX")]
    [SerializeField] private float hitShakeDuration = 0.08f;
    [SerializeField] private float hitShakeAmplitude = 0.2f;
    [SerializeField] private float hitShakeFrequency = 25f;
    [SerializeField] private float hitZoomAmount = 0.5f;
    [SerializeField] private float hitZoomInDuration = 0.04f;
    [SerializeField] private float hitZoomHoldDuration = 0.05f;
    [SerializeField] private float hitZoomOutDuration = 0.08f;

    HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();
    private CameraFollow2D camFollow;
    private CameraShake camShake;
    private CameraZoom camZoom;

    void Start()
    {
        CacheCameraFx();
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
            enemy.Hit();
            hitEnemy = true;
        }

        if (hitEnemy)
        {
            var timeManager = GameManager.Instance != null ? GameManager.Instance.TimeManager : null;
            if (timeManager != null)
            {
                if (hitSlowDuration > 0f) timeManager.EnterBulletTime(hitSlowScale, hitSlowDuration);
                else timeManager.EnterBulletTime(hitSlowScale);
            }
            ApplyHitFx();
        }
    }

    private void ApplyHitFx()
    {
        if (camFollow != null)
        {
            camFollow.Shake(hitShakeDuration, hitShakeAmplitude, hitShakeFrequency);
            camFollow.ZoomInHoldOut(hitZoomAmount, hitZoomInDuration, hitZoomHoldDuration, hitZoomOutDuration);
            return;
        }

        if (camShake != null) camShake.Shake(hitShakeDuration, hitShakeAmplitude, hitShakeFrequency);
        if (camZoom != null) camZoom.ZoomInHoldOut(hitZoomAmount, hitZoomInDuration, hitZoomHoldDuration, hitZoomOutDuration);
    }

    private void CacheCameraFx()
    {
        var cam = Camera.main;
        if (cam == null) return;
        camFollow = cam.GetComponent<CameraFollow2D>();
        camShake = cam.GetComponent<CameraShake>();
        camZoom = cam.GetComponent<CameraZoom>();
    }
}

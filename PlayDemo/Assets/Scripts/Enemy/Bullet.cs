using UnityEngine;
public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask enemyHittableLayer;
    [Header("Hit FX")]
    [SerializeField] private float hitShakeDuration = 0.08f;
    [SerializeField] private float hitShakeAmplitude = 0.2f;
    [SerializeField] private float hitShakeFrequency = 25f;
    [Header("Hit Slow")]
    [SerializeField] private float hitSlowScale = 0.2f;
    [SerializeField] private float hitSlowDuration = 0.06f;
    Collider2D bulletCol;
    Collider2D ownerCol;
    private CameraFollow2D camFollow;
    private CameraShake camShake;
    

    private void Start()
    {
        CacheCameraFx();
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
                enemy.Hit();
                ApplyHitFx();
            }
            Destroy(gameObject);
        }
    }

    private void ApplyHitFx()
    {
        ApplyHitSlow();
        ApplyHitFx(
            hitShakeDuration,
            hitShakeAmplitude,
            hitShakeFrequency
        );
    }

    private void ApplyHitFx(
        float shakeDuration,
        float shakeAmplitude,
        float shakeFrequency
    )
    {
        if (camFollow != null)
        {
            camFollow.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
            return;
        }

        if (camShake != null) camShake.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
    }

    private void ApplyHitSlow()
    {
        var timeManager = GameManager.Instance != null ? GameManager.Instance.TimeManager : null;
        if (timeManager == null) return;

        if (hitSlowDuration > 0f) timeManager.EnterBulletTime(hitSlowScale, hitSlowDuration);
        else timeManager.EnterBulletTime(hitSlowScale);
    }

    private void CacheCameraFx()
    {
        var cam = Camera.main;
        if (cam == null) return;
        camFollow = cam.GetComponent<CameraFollow2D>();
        camShake = cam.GetComponent<CameraShake>();
    }
}

using UnityEngine;
using System.Collections;

public class PlayerHitCheck : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private CharacterManager manager;
    [Header("Hit FX")]
    [SerializeField] private float hitShakeDuration = 0.08f;
    [SerializeField] private float hitShakeAmplitude = 0.2f;
    [SerializeField] private float hitShakeFrequency = 25f;
    [SerializeField] private float hitZoomAmount = 0.5f;
    [SerializeField] private float hitZoomInDuration = 0.04f;
    [SerializeField] private float hitZoomHoldDuration = 0.05f;
    [SerializeField] private float hitZoomOutDuration = 0.08f;
    [Header("Hit Slow")]
    [SerializeField] private float hitSlowScale = 0.2f;
    [SerializeField] private float hitSlowDuration = 0.06f;
    private CameraFollow2D camFollow;
    private CameraShake camShake;
    private CameraZoom camZoom;
    [Header("SFX")]
    [SerializeField] private AudioClip hitSfx;
    [SerializeField, Range(0f, 1f)] private float hitSfxVolume = 1f;
    private AudioManager audioManager;
    void Start()
    {
        manager = GameManager.Instance.Char;
        CacheCameraFx();
        audioManager = GameManager.Instance != null ? GameManager.Instance.Audio : null;
    }

    [Header("Invincibility")]
    public float invincibleTime = 1.0f;
    private bool isInvincible = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            TakeDamage(5);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            TakeDamage(1);
        }
    }

    public float playerHeight;
    public bool death;
    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        damage = 1;
        manager.SubHP(damage);
        
        var go = Instantiate(GameManager.Instance.UI.damageUI, 
            transform.position + Vector3.up * playerHeight, 
            Quaternion.identity).GetComponent<DamageUI>();
        
        go.Init(damage);
        if (audioManager != null) audioManager.PlaySfx(hitSfx, hitSfxVolume);

        if (manager.HP <= 0 && !death)
        {
            Die();
            return;
        }
        ApplyHitSlow();
        ApplyHitFx();
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
        death = true;
        GetComponent<PlayerMovement2D>().disable = true;
        StartCoroutine(GameManager.Instance.Char.Death());
    }

    private void ApplyHitFx()
    {
        ApplyHitFx(
            hitShakeDuration,
            hitShakeAmplitude,
            hitShakeFrequency,
            hitZoomAmount,
            hitZoomInDuration,
            hitZoomHoldDuration,
            hitZoomOutDuration
        );
    }

    private void ApplyHitFx(
        float shakeDuration,
        float shakeAmplitude,
        float shakeFrequency,
        float zoomAmount,
        float zoomInDuration,
        float zoomHoldDuration,
        float zoomOutDuration
    )
    {
        if (camFollow != null)
        {
            camFollow.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
            camFollow.ZoomInHoldOut(zoomAmount, zoomInDuration, zoomHoldDuration, zoomOutDuration);
            return;
        }

        if (camShake != null) camShake.Shake(shakeDuration, shakeAmplitude, shakeFrequency);
        if (camZoom != null) camZoom.ZoomInHoldOut(zoomAmount, zoomInDuration, zoomHoldDuration, zoomOutDuration);
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
        camZoom = cam.GetComponent<CameraZoom>();
    }
}

using UnityEngine;
public class BulletTimeController : MonoBehaviour
{
    public float slowScale = 0.1f;
    [SerializeField] private float useRate = 12f;
    [SerializeField] private TimeManager timeManager;

    private CharacterManager manager;
    private int requestId;
    private AudioManager audioManager;

    void Start()
    {
        manager = GameManager.Instance.Char;
        if (timeManager == null && GameManager.Instance != null)
            timeManager = GameManager.Instance.TimeManager;
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();
        audioManager = GameManager.Instance != null ? GameManager.Instance.Audio : null;
    }

    public bool Use()
    {
        return manager.UseGauge(useRate * Time.deltaTime);
    }

    public void EnterBulletTime()
    {
        if (timeManager == null || requestId != 0) return;
        requestId = timeManager.EnterBulletTime(slowScale);
        ApplySwapLowPass(true);
    }

    public void ExitBulletTime()
    {
        if (timeManager == null || requestId == 0) return;
        timeManager.ExitBulletTime(requestId);
        requestId = 0;
        ApplySwapLowPass(false);
    }

    private void OnDisable()
    {
        ExitBulletTime();
    }

    private void ApplySwapLowPass(bool enabled)
    {
        if (audioManager == null) return;
        audioManager.SetLowPass(enabled);
    }
}

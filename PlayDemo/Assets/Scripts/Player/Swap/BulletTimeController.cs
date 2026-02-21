using UnityEngine;
public class BulletTimeController : MonoBehaviour
{
    public float slowScale = 0.1f;
    [SerializeField] private float useRate = 12f;
    [SerializeField] private TimeManager timeManager;

    private CharacterManager manager;
    private int requestId;

    void Start()
    {
        manager = GameManager.Instance.Char;
        if (timeManager == null && GameManager.Instance != null)
            timeManager = GameManager.Instance.TimeManager;
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();
    }

    public bool Use()
    {
        return manager.UseGauge(useRate * Time.deltaTime);
    }

    public void EnterBulletTime()
    {
        if (timeManager == null || requestId != 0) return;
        requestId = timeManager.EnterBulletTime(slowScale);
    }

    public void ExitBulletTime()
    {
        if (timeManager == null || requestId == 0) return;
        timeManager.ExitBulletTime(requestId);
        requestId = 0;
    }

    private void OnDisable()
    {
        ExitBulletTime();
    }
}

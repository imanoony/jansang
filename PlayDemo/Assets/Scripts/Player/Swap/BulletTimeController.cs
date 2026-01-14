using UnityEngine;
using UnityEngine.Rendering;

public class BulletTimeController : MonoBehaviour
{
    public float slowScale = 0.1f;
    [SerializeField] private float useRate = 12f;
    float defaultFixedDelta;
    
    private CharacterManager manager;
    void Start()
    {
        defaultFixedDelta = Time.fixedDeltaTime;
        manager = GameManager.Instance.Char;
    }

    public bool Use()
    {
        return manager.UseGauge(useRate * Time.deltaTime);
    }

    public void EnterBulletTime()
    {
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = defaultFixedDelta * slowScale;
    }

    public void ExitBulletTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDelta;
    }
}
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

    void Update()
    {
        if (Input.GetMouseButton(1)) // RMB
        {
            if (manager.UseGauge(useRate * Time.deltaTime))
            {
                EnterBulletTime();
            }
            else
            {
                ExitBulletTime();
            }
        }
        else
        {
            ExitBulletTime();
        }
    }

    void EnterBulletTime()
    {
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = defaultFixedDelta * slowScale;
    }

    void ExitBulletTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDelta;
    }
}
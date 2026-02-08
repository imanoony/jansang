using UnityEngine;

[RequireComponent(typeof(CameraFollow2D))]
public class CameraShake : MonoBehaviour
{
    [Header("Defaults")]
    public float defaultDuration = 0.08f;
    public float defaultAmplitude = 0.2f;
    public float defaultFrequency = 25f;
    public bool useUnscaledTime = true;

    private CameraFollow2D follow;

    void Awake()
    {
        follow = GetComponent<CameraFollow2D>();
        if (follow == null) return;

        follow.shakeDefaultDuration = defaultDuration;
        follow.shakeDefaultAmplitude = defaultAmplitude;
        follow.shakeDefaultFrequency = defaultFrequency;
        follow.shakeUseUnscaledTime = useUnscaledTime;
    }

    public void ShakeDefault()
    {
        if (follow == null) return;
        follow.ShakeDefault();
    }

    public void Shake(float duration, float amplitude)
    {
        if (follow == null) return;
        follow.Shake(duration, amplitude, defaultFrequency);
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        if (follow == null) return;
        follow.Shake(duration, amplitude, frequency);
    }

    public void StopAll()
    {
        if (follow == null) return;
        follow.StopAllShakes();
    }
}

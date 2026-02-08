using UnityEngine;

[RequireComponent(typeof(CameraFollow2D))]
public class CameraZoom : MonoBehaviour
{
    [Header("Defaults")]
    public float defaultDuration = 0.08f;
    public float defaultAmount = 0.5f;
    public bool useUnscaledTime = true;

    [Header("Clamp (Optional)")]
    public float minSize = 0f;
    public float maxSize = 0f;
    public bool stack = false;

    private CameraFollow2D follow;

    private void Awake()
    {
        follow = GetComponent<CameraFollow2D>();
        if (follow == null) return;

        follow.zoomFxDefaultDuration = defaultDuration;
        follow.zoomFxDefaultAmount = defaultAmount;
        follow.zoomFxUseUnscaledTime = useUnscaledTime;
        follow.zoomFxStack = stack;
        follow.zoomFxMinSize = minSize;
        follow.zoomFxMaxSize = maxSize;
    }

    public void ZoomDefaultIn()
    {
        if (follow == null) return;
        follow.ZoomDefaultIn();
    }

    public void ZoomDefaultOut()
    {
        if (follow == null) return;
        follow.ZoomDefaultOut();
    }

    public void ZoomIn(float amount, float duration)
    {
        if (follow == null) return;
        follow.ZoomIn(amount, duration);
    }

    public void ZoomInHoldOut(float amount, float inDuration, float holdDuration, float outDuration)
    {
        if (follow == null) return;
        follow.ZoomInHoldOut(amount, inDuration, holdDuration, outDuration);
    }

    public void ZoomOut(float amount, float duration)
    {
        if (follow == null) return;
        follow.ZoomOut(amount, duration);
    }

    public void ZoomOutHoldOut(float amount, float inDuration, float holdDuration, float outDuration)
    {
        if (follow == null) return;
        follow.ZoomOutHoldOut(amount, inDuration, holdDuration, outDuration);
    }

    public void Zoom(float duration, float amount)
    {
        if (follow == null) return;
        follow.Zoom(duration, amount);
    }

    public void StopAll()
    {
        if (follow == null) return;
        follow.StopAllZoomFx();
    }
}

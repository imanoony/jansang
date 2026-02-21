using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    Camera cam;

    [Header("Target")]
    public Transform targetRoot;
    private Transform target;

    [Header("Move")]
    public float followLerpSpeed = 10f;
    public float mouseInfluence = 3f;

    [Header("Zoom")]
    public float baseZoom = 6f;
    public float extraZoom = 2f;
    public float zoomLerpSpeed = 5f;

    [Header("Bounds")]
    public bool useBounds = false;
    public Collider2D boundsCollider;
    public Vector2 minBound;
    public Vector2 maxBound;

    bool isTransforming = true; // 변환 상태
    [Header("Interaction")] 
    public float desiredZoom = 3f;
    private bool isZooming = false;
    private Vector3 interactPos;
    [Header("Shake FX")]
    public float shakeDefaultDuration = 0.08f;
    public float shakeDefaultAmplitude = 0.2f;
    public float shakeDefaultFrequency = 25f;
    public bool shakeUseUnscaledTime = true;

    [Header("Zoom FX")]
    public float zoomFxDefaultDuration = 0.08f;
    public float zoomFxDefaultAmount = 0.5f;
    public bool zoomFxUseUnscaledTime = true;
    public bool zoomFxStack = false;
    public float zoomFxMinSize = 0f;
    public float zoomFxMaxSize = 0f;
    
    private Vector3 currentShakeOffset;
    private float currentZoomOffset;

    private struct ShakeRequest
    {
        public float amplitude;
        public float frequency;
        public float startTime;
        public float endTime;
        public float seedX;
        public float seedY;
    }

    private enum ZoomMode
    {
        Pulse,
        Hold
    }

    private struct ZoomRequest
    {
        public ZoomMode mode;
        public float amount;
        public float startTime;
        public float endTime;
        public float inDuration;
        public float holdDuration;
        public float outDuration;
    }

    private readonly List<ShakeRequest> shakeRequests = new List<ShakeRequest>();
    private readonly List<ZoomRequest> zoomRequests = new List<ZoomRequest>();

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        if (targetRoot != null && targetRoot.childCount > 0) target = targetRoot.GetChild(0);
        else target = targetRoot;
    }

    public void StartFocus(Vector3 position)
    {
        
    }
    
    public void StopFocus() {}

    void LateUpdate()
    {
        if (cam == null) return;

        if (currentShakeOffset != Vector3.zero) ApplyShakeOffset(Vector3.zero);
        if (!Mathf.Approximately(currentZoomOffset, 0f)) ApplyZoomOffset(0f);

        if (target)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offset = mouseWorld - target.position;
            offset.z = 0;
            offset = Vector3.ClampMagnitude(offset, mouseInfluence);

            Vector3 desiredPos = target.position + offset;
            desiredPos.z = transform.position.z;

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                Time.deltaTime * followLerpSpeed
            );
        }

        float targetZoom = baseZoom + (isTransforming ? extraZoom : 0f);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomLerpSpeed
        );

        if (useBounds)
        {
            transform.position = ClampToBounds(transform.position);
        }

        UpdateShake();
        UpdateZoomFx();
    }

    public void SetTransformMode(bool active)
    {
        isTransforming = active;
    }

    // ======================
    // Shake API
    // ======================
    public void ShakeDefault()
    {
        Shake(shakeDefaultDuration, shakeDefaultAmplitude, shakeDefaultFrequency);
    }

    public void Shake(float duration, float amplitude)
    {
        Shake(duration, amplitude, shakeDefaultFrequency);
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        if (duration <= 0f || amplitude <= 0f) return;
        if (frequency <= 0f) frequency = shakeDefaultFrequency;

        float now = GetTime(shakeUseUnscaledTime);
        shakeRequests.Add(new ShakeRequest
        {
            amplitude = amplitude,
            frequency = frequency,
            startTime = now,
            endTime = now + duration,
            seedX = Random.value * 100f,
            seedY = Random.value * 100f
        });
    }

    public void StopAllShakes()
    {
        shakeRequests.Clear();
        ApplyShakeOffset(Vector3.zero);
    }

    // ======================
    // Zoom FX API
    // ======================
    public void ZoomDefaultIn()
    {
        ZoomIn(zoomFxDefaultAmount, zoomFxDefaultDuration);
    }

    public void ZoomDefaultOut()
    {
        ZoomOut(zoomFxDefaultAmount, zoomFxDefaultDuration);
    }

    public void ZoomIn(float amount, float duration)
    {
        Zoom(duration, -Mathf.Abs(amount));
    }

    public void ZoomOut(float amount, float duration)
    {
        Zoom(duration, Mathf.Abs(amount));
    }

    public void Zoom(float duration, float amount)
    {
        if (duration <= 0f) return;
        if (Mathf.Approximately(amount, 0f)) return;

        float now = GetTime(zoomFxUseUnscaledTime);
        zoomRequests.Add(new ZoomRequest
        {
            mode = ZoomMode.Pulse,
            amount = amount,
            startTime = now,
            endTime = now + duration
        });
    }

    public void ZoomInHoldOut(float amount, float inDuration, float holdDuration, float outDuration)
    {
        ZoomHoldOut(-Mathf.Abs(amount), inDuration, holdDuration, outDuration);
    }

    public void ZoomOutHoldOut(float amount, float inDuration, float holdDuration, float outDuration)
    {
        ZoomHoldOut(Mathf.Abs(amount), inDuration, holdDuration, outDuration);
    }

    private void ZoomHoldOut(float amount, float inDuration, float holdDuration, float outDuration)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        if (inDuration < 0f) inDuration = 0f;
        if (holdDuration < 0f) holdDuration = 0f;
        if (outDuration < 0f) outDuration = 0f;

        float total = inDuration + holdDuration + outDuration;
        if (total <= 0f) return;

        float now = GetTime(zoomFxUseUnscaledTime);
        zoomRequests.Add(new ZoomRequest
        {
            mode = ZoomMode.Hold,
            amount = amount,
            startTime = now,
            endTime = now + total,
            inDuration = inDuration,
            holdDuration = holdDuration,
            outDuration = outDuration
        });
    }

    public void StopAllZoomFx()
    {
        zoomRequests.Clear();
        ApplyZoomOffset(0f);
    }

    public void StopAllEffects()
    {
        StopAllShakes();
        StopAllZoomFx();
    }

    private void UpdateShake()
    {
        if (shakeRequests.Count == 0)
        {
            if (currentShakeOffset != Vector3.zero) ApplyShakeOffset(Vector3.zero);
            return;
        }

        float now = GetTime(shakeUseUnscaledTime);
        for (int i = shakeRequests.Count - 1; i >= 0; i--)
        {
            if (now >= shakeRequests[i].endTime) shakeRequests.RemoveAt(i);
        }

        if (shakeRequests.Count == 0)
        {
            ApplyShakeOffset(Vector3.zero);
            return;
        }

        Vector2 combined = Vector2.zero;
        for (int i = 0; i < shakeRequests.Count; i++)
        {
            ShakeRequest req = shakeRequests[i];
            float duration = req.endTime - req.startTime;
            float t = (now - req.startTime) * req.frequency;
            float fade = duration > 0f ? 1f - Mathf.Clamp01((now - req.startTime) / duration) : 1f;
            float amp = req.amplitude * fade;

            float nx = Mathf.PerlinNoise(req.seedX, t) * 2f - 1f;
            float ny = Mathf.PerlinNoise(req.seedY, t) * 2f - 1f;
            combined += new Vector2(nx, ny) * amp;
        }

        ApplyShakeOffset(new Vector3(combined.x, combined.y, 0f));
    }

    private void UpdateZoomFx()
    {
        if (zoomRequests.Count == 0)
        {
            if (!Mathf.Approximately(currentZoomOffset, 0f)) ApplyZoomOffset(0f);
            return;
        }

        float now = GetTime(zoomFxUseUnscaledTime);
        for (int i = zoomRequests.Count - 1; i >= 0; i--)
        {
            if (now >= zoomRequests[i].endTime) zoomRequests.RemoveAt(i);
        }

        if (zoomRequests.Count == 0)
        {
            ApplyZoomOffset(0f);
            return;
        }

        float combined = 0f;
        for (int i = 0; i < zoomRequests.Count; i++)
        {
            ZoomRequest req = zoomRequests[i];
            float value;
            if (req.mode == ZoomMode.Pulse)
            {
                float duration = req.endTime - req.startTime;
                float progress = duration > 0f ? Mathf.Clamp01((now - req.startTime) / duration) : 1f;
                float weight = Mathf.Sin(progress * Mathf.PI);
                value = req.amount * weight;
            }
            else
            {
                float elapsed = now - req.startTime;
                if (elapsed < 0f) continue;

                float inD = req.inDuration;
                float holdD = req.holdDuration;
                float outD = req.outDuration;
                float total = inD + holdD + outD;
                if (total <= 0f) continue;

                float weight;
                if (elapsed < inD)
                {
                    weight = inD > 0f ? elapsed / inD : 1f;
                }
                else if (elapsed < inD + holdD)
                {
                    weight = 1f;
                }
                else if (elapsed < total)
                {
                    weight = outD > 0f ? 1f - ((elapsed - inD - holdD) / outD) : 0f;
                }
                else
                {
                    weight = 0f;
                }

                value = req.amount * weight;
            }

            if (zoomFxStack)
            {
                combined += value;
            }
            else
            {
                if (Mathf.Abs(value) > Mathf.Abs(combined)) combined = value;
            }
        }

        ApplyZoomOffset(combined);
    }

    private float GetTime(bool useUnscaled)
    {
        return useUnscaled ? Time.unscaledTime : Time.time;
    }

    private void ApplyShakeOffset(Vector3 offset)
    {
        if (currentShakeOffset != Vector3.zero) transform.position -= currentShakeOffset;
        currentShakeOffset = offset;
        transform.position += currentShakeOffset;
    }

    private void ApplyZoomOffset(float offset)
    {
        if (!Mathf.Approximately(currentZoomOffset, 0f)) cam.orthographicSize -= currentZoomOffset;

        float baseSize = cam.orthographicSize;
        float target = baseSize + offset;

        if (zoomFxMinSize > 0f) target = Mathf.Max(zoomFxMinSize, target);
        if (zoomFxMaxSize > 0f) target = Mathf.Min(zoomFxMaxSize, target);

        cam.orthographicSize = target;
        currentZoomOffset = target - baseSize;
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
        if (!TryGetBounds(out Vector2 min, out Vector2 max)) return pos;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        float minX = min.x + horzExtent;
        float maxX = max.x - horzExtent;
        float minY = min.y + vertExtent;
        float maxY = max.y - vertExtent;

        if (minX > maxX)
            pos.x = (min.x + max.x) * 0.5f;
        else
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (minY > maxY)
            pos.y = (min.y + max.y) * 0.5f;
        else
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

        pos.z = transform.position.z;
        return pos;
    }

    private bool TryGetBounds(out Vector2 min, out Vector2 max)
    {
        if (!useBounds)
        {
            min = Vector2.zero;
            max = Vector2.zero;
            return false;
        }

        if (boundsCollider != null)
        {
            Bounds b = boundsCollider.bounds;
            min = b.min;
            max = b.max;
            return true;
        }

        min = minBound;
        max = maxBound;
        return true;
    }

    private void OnDisable()
    {
        if (cam == null) return;
        if (currentShakeOffset != Vector3.zero) transform.position -= currentShakeOffset;
        if (!Mathf.Approximately(currentZoomOffset, 0f)) cam.orthographicSize -= currentZoomOffset;
        currentShakeOffset = Vector3.zero;
        currentZoomOffset = 0f;
        shakeRequests.Clear();
        zoomRequests.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        if (!TryGetBounds(out Vector2 min, out Vector2 max)) return;

        Gizmos.color = Color.yellow;
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
        Gizmos.DrawWireCube(center, size);
    }

    public void SetTargetRoot(Transform newRoot)
    {
        targetRoot = newRoot;
        target = targetRoot.GetChild(0);
    }
}

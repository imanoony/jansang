using System;
using UnityEngine;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    private float defaultFixedDelta;
    private float defaultTimeScale;
    private int nextRequestId;
    private readonly Dictionary<int, float> activeRequests = new Dictionary<int, float>();
    private readonly Dictionary<int, float> expiryTimes = new Dictionary<int, float>();

    private void Awake()
    {
        defaultFixedDelta = Time.fixedDeltaTime;
        defaultTimeScale = Time.timeScale;
    }

    private void Update()
    {
        if (expiryTimes.Count == 0) return;
        float now = Time.unscaledTime;
        bool removedAny = false;
        var keysToRemove = new List<int>();
        foreach (var pair in expiryTimes)
        {
            if (now >= pair.Value) keysToRemove.Add(pair.Key);
        }
        for (int i = 0; i < keysToRemove.Count; i++)
        {
            int id = keysToRemove[i];
            expiryTimes.Remove(id);
            activeRequests.Remove(id);
            removedAny = true;
        }
        if (removedAny) ApplySlowestScale();
    }

    private void OnDisable()
    {
        ExitAllBulletTime();
    }

    public int EnterBulletTime(float slowScale)
    {
        slowScale = Mathf.Clamp(slowScale, 0.00f, 1f);
        int id = ++nextRequestId;
        activeRequests[id] = slowScale;
        ApplySlowestScale();
        return id;
    }

    public int EnterBulletTime(float slowScale, float durationSeconds)
    {
        int id = EnterBulletTime(slowScale);
        if (durationSeconds > 0f)
        {
            expiryTimes[id] = Time.unscaledTime + durationSeconds;
        }
        return id;
    }

    public void ExitBulletTime(int requestId)
    {
        bool removed = activeRequests.Remove(requestId);
        expiryTimes.Remove(requestId);
        if (removed) ApplySlowestScale();
    }

    public void ExitBulletTime()
    {
        ExitAllBulletTime();
    }

    public void ExitAllBulletTime()
    {
        activeRequests.Clear();
        expiryTimes.Clear();
        ApplySlowestScale();
    }

    private void ApplySlowestScale()
    {
        if (activeRequests.Count == 0)
        {
            SetTimeScale(defaultTimeScale);
            return;
        }
        float slowest = 1f;
        foreach (var pair in activeRequests)
        {
            if (pair.Value < slowest) slowest = pair.Value;
        }
        SetTimeScale(slowest);
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = defaultFixedDelta * scale;
    }
}

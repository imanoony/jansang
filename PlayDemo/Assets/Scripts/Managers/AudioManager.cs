using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Low Pass")]
    [SerializeField] private float lowPassCutoff = 800f;
    [SerializeField] private float lowPassBypassCutoff = 22000f;
    [SerializeField] private float lowPassFadeDuration = 0.12f;
    private AudioLowPassFilter bgmLowPass;
    private AudioLowPassFilter sfxLowPass;
    private Coroutine lowPassBgmRoutine;
    private Coroutine lowPassSfxRoutine;

    private Coroutine bgmFadeRoutine;

    public AudioClip[] bgmClips;

    private void Awake()
    {
        EnsureSources();
        EnsureLowPassFilters();
    }

    public void Init()
    {
        EnsureSources();
        EnsureLowPassFilters();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSource == null) return;
        float prevPitch = sfxSource.pitch;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
        sfxSource.pitch = prevPitch;
    }

    public void PlayBgm(AudioClip clip, float volume = 1f, bool loop = true, float fadeIn = 0f)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float target = Mathf.Clamp01(volume) * bgmVolume;
        if (fadeIn > 0f)
        {
            StartBgmFade(target, fadeIn, false);
        }
        else
        {
            StopBgmFade();
            bgmSource.volume = target;
        }
    }

    public void StopBgm(float fadeOut = 0f)
    {
        if (bgmSource == null) return;
        if (fadeOut > 0f)
        {
            StartBgmFade(0f, fadeOut, true);
        }
        else
        {
            StopBgmFade();
            bgmSource.Stop();
        }
    }

    public void FadeBgm(float targetVolume, float duration)
    {
        if (bgmSource == null) return;
        StartBgmFade(Mathf.Clamp01(targetVolume) * bgmVolume, duration, false);
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(bgmSource.volume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetLowPass(bool enabled, float cutoff = -1f, bool includeSfx = false)
    {
        float target = enabled
            ? (cutoff > 0f ? cutoff : lowPassCutoff)
            : lowPassBypassCutoff;

        ApplyLowPass(bgmLowPass, ref lowPassBgmRoutine, enabled, target, lowPassFadeDuration, true);

        if (includeSfx)
        {
            ApplyLowPass(sfxLowPass, ref lowPassSfxRoutine, enabled, target, lowPassFadeDuration, false);
        }
    }

    private void EnsureSources()
    {
        if (bgmSource == null) bgmSource = FindOrCreateSource("BGM");
        if (sfxSource == null) sfxSource = FindOrCreateSource("SFX");

        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;
    }

    private AudioSource FindOrCreateSource(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            var go = new GameObject(childName);
            child = go.transform;
            child.SetParent(transform);
            child.localPosition = Vector3.zero;
        }

        var source = child.GetComponent<AudioSource>();
        if (source == null) source = child.gameObject.AddComponent<AudioSource>();
        return source;
    }

    private void EnsureLowPassFilters()
    {
        if (bgmSource != null)
        {
            bgmLowPass = bgmSource.GetComponent<AudioLowPassFilter>();
            if (bgmLowPass == null) bgmLowPass = bgmSource.gameObject.AddComponent<AudioLowPassFilter>();
            bgmLowPass.enabled = false;
            bgmLowPass.cutoffFrequency = lowPassBypassCutoff;
        }

        if (sfxSource != null)
        {
            sfxLowPass = sfxSource.GetComponent<AudioLowPassFilter>();
            if (sfxLowPass == null) sfxLowPass = sfxSource.gameObject.AddComponent<AudioLowPassFilter>();
            sfxLowPass.enabled = false;
            sfxLowPass.cutoffFrequency = lowPassBypassCutoff;
        }
    }

    private void ApplyLowPass(
        AudioLowPassFilter filter,
        ref Coroutine routine,
        bool enabled,
        float targetCutoff,
        float duration,
        bool isBgm
    )
    {
        if (filter == null) return;

        if (duration <= 0f)
        {
            if (routine != null) StopCoroutine(routine);
            filter.enabled = enabled;
            filter.cutoffFrequency = targetCutoff;
            routine = null;
            return;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LowPassRoutine(filter, enabled, targetCutoff, duration, isBgm));
    }

    private System.Collections.IEnumerator LowPassRoutine(
        AudioLowPassFilter filter,
        bool enabled,
        float targetCutoff,
        float duration,
        bool isBgm
    )
    {
        if (filter == null) yield break;
        filter.enabled = true;

        float start = filter.cutoffFrequency;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = duration > 0f ? t / duration : 1f;
            filter.cutoffFrequency = Mathf.Lerp(start, targetCutoff, lerp);
            yield return null;
        }

        filter.cutoffFrequency = targetCutoff;
        filter.enabled = enabled;

        if (isBgm) lowPassBgmRoutine = null;
        else lowPassSfxRoutine = null;
    }

    private void StartBgmFade(float target, float duration, bool stopAfter)
    {
        if (duration <= 0f)
        {
            StopBgmFade();
            if (bgmSource != null) bgmSource.volume = target;
            if (stopAfter && bgmSource != null) bgmSource.Stop();
            return;
        }

        StopBgmFade();
        bgmFadeRoutine = StartCoroutine(FadeBgmRoutine(target, duration, stopAfter));
    }

    private void StopBgmFade()
    {
        if (bgmFadeRoutine == null) return;
        StopCoroutine(bgmFadeRoutine);
        bgmFadeRoutine = null;
    }

    private System.Collections.IEnumerator FadeBgmRoutine(float target, float duration, bool stopAfter)
    {
        if (bgmSource == null) yield break;
        float start = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = duration > 0f ? t / duration : 1f;
            bgmSource.volume = Mathf.Lerp(start, target, lerp);
            yield return null;
        }
        bgmSource.volume = target;
        if (stopAfter) bgmSource.Stop();
        bgmFadeRoutine = null;
    }
}

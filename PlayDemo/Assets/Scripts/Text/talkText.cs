using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class talkText : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Vector3 offset = new Vector3(0, 1.8f, 0);

    [Header("Floating")]
    [SerializeField] float floatHeight = 0.6f;
    [SerializeField] AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Typing")]
    [SerializeField] float charsPerSecond = 20f;

    Transform target;
    [SerializeField]private Transform background;
    ObjectPool<talkText> pool;

    private float fixedUpTime = 0.5f;

    float duration;
    float timer;
    float elapsed;

    string fullMessage;
    int currentCharCount;

    Vector3 baseOffset;

    public void Bind(
        Transform target,
        string message,
        float duration,
        ObjectPool<talkText> pool
    )
    {
        this.target = target;
        this.pool = pool;
        this.duration = duration;

        timer = duration;
        elapsed = 0f;

        fullMessage = message;
        currentCharCount = 0;
        text.text = "";

        baseOffset = offset;

        gameObject.SetActive(true);
    }

    void Update()
    {
        // 타겟 사망 / 파괴 시 즉시 반환
        if (target == null)
        {
            Release();
            return;
        }

        elapsed += Time.deltaTime;
        timer -= Time.deltaTime;

        // === 위로 떠오르는 효과 ===
        float t = Mathf.Clamp01(elapsed / fixedUpTime);
        float floatY = floatCurve.Evaluate(t) * floatHeight;
        transform.position = target.position + baseOffset + Vector3.up * floatY;
        transform.localScale = new Vector2(t ,t);
        background.transform.localScale = new Vector2(text.rectTransform.rect.width+0.5f, 0.4f);
        // === 타이핑 효과 ===
        int targetCharCount = Mathf.FloorToInt(elapsed * charsPerSecond);
        if (targetCharCount != currentCharCount)
        {
            currentCharCount = Mathf.Clamp(targetCharCount, 0, fullMessage.Length);
            text.text = fullMessage.Substring(0, currentCharCount);
        }

        if (timer <= 0f)
            Release();
    }

    void Release()
    {
        target = null;
        pool.Release(this);
    }
}
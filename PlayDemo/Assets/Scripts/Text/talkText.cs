using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class talkText : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Vector3 offset = new Vector3(0, 1.8f, 0);

    Transform target;
    ObjectPool<talkText> pool;
    float timer;

    public void Bind(
        Transform target,
        string message,
        float duration,
        ObjectPool<talkText> pool
    )
    {
        this.target = target;
        this.pool = pool;
        text.text = message;
        timer = duration;

        gameObject.SetActive(true);
    }

    void Update()
    {
        // 타겟이 사라졌으면 즉시 반환
        if (target == null)
        {
            Release();
            return;
        }

        transform.position = target.position + offset;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Release();
    }

    void Release()
    {
        target = null;
        pool.Release(this);
    }
}
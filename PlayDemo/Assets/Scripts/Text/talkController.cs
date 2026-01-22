using UnityEngine;
using UnityEngine.Pool;

public class talkController : MonoBehaviour
{
    public static talkController Instance { get; private set; }
    [SerializeField] private talkChunk chunk;
    [SerializeField] talkText prefab;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxSize = 30;

    ObjectPool<talkText> pool;

    void Awake()
    {
        Instance = this;

        pool = new ObjectPool<talkText>(
            Create,
            OnGet,
            OnRelease,
            OnDestroyBubble,
            false,
            defaultCapacity,
            maxSize
        );
    }

    talkText Create()
    {
        return Instantiate(prefab, transform);
    }

    void OnGet(talkText bubble)
    {
        bubble.gameObject.SetActive(true);
    }

    void OnRelease(talkText bubble)
    {
        bubble.gameObject.SetActive(false);
    }

    void OnDestroyBubble(talkText bubble)
    {
        Destroy(bubble.gameObject);
    }

    public float Show(
        Transform target
    )
    {
        if (chunk.talkDatas.Count == 0)
        {
            return 0;
        }
        talkData textSmall = chunk.talkDatas[Random.Range(0, chunk.talkDatas.Count)];
        if (textSmall.randomLines.Count == 0)
        {
            return 0;
        }
        string text = textSmall.randomLines[Random.Range(0, textSmall.randomLines.Count)];
        var bubble = pool.Get();
        bubble.Bind(target, text, textSmall.remainingTime, pool);
        return textSmall.remainingTime;
    }
}
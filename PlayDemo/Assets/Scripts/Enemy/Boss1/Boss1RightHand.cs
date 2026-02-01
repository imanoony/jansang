using System.Collections;
using UnityEngine;

public class Boss1RightHand : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite smiteSprite;
    [SerializeField] private Sprite hauntSprite;

    [Header("Player References")]
    [SerializeField] private GameObject playerObject;
    private Transform playerTransform;

    [Header("Haunt")]
    [SerializeField] private GameObject spiritPrefab;

    private Boss1Manage bossManage;
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerObject == null)
        {
            playerObject = bossManage.playerObject;
        }
        playerTransform = playerObject.transform;
    }

    public void BackToOrigin()
    {
        StartCoroutine(Boss1Manage.ObjectMoveControl(
            gameObject,
            transform.position,
            bossManage.rHandOrigin,
            0.5f,
            0.5f
        ));
    }

    public void Boss1Haunt(int count)
    {
        StartCoroutine(HauntCoroutine(count));
    }

    private IEnumerator HauntCoroutine(int count)
    {
        spriteRenderer.sprite = hauntSprite;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = gameObject.transform.position + new Vector3(0f, 1f, 0f);
            GameObject spirit = Instantiate(spiritPrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D spiritRigidbody = spirit.GetComponent<Rigidbody2D>();
            spiritRigidbody.linearVelocity = Vector2.up * 2f;
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        spriteRenderer.sprite = idleSprite;
        bossManage.currentRightHandPattern = RHandPattern.Idle;
        bossManage.rHandPatternTimer = 3f;
    }
    




}

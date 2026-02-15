using System.Collections;
using UnityEngine;

public class Boss1_RightHand : MonoBehaviour
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
    [SerializeField] private float spiritMinSpeed = 1f;
    [SerializeField] private float spiritMaxSpeed = 3f;

    private Boss1_Manage bossManage;
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1_Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerObject == null)
        {
            playerObject = bossManage.playerObject;
        }
        playerTransform = playerObject.transform;
    }

    public void BackToOrigin()
    {
        StartCoroutine(Boss1_Manage.ObjectMoveControl(
            gameObject,
            transform.position,
            bossManage.rHandOrigin,
            0.5f,
            0.5f
        ));
    }

    public void Boss1_Haunt(int count)
    {
        StartCoroutine(HauntCoroutine(count));
    }

    private IEnumerator HauntCoroutine(int count)
    {
        spriteRenderer.sprite = hauntSprite;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = gameObject.transform.position + new Vector3(0f, 1f, 0f);
            GameObject spirit = Instantiate(spiritPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
            spirit.GetComponent<Boss1_Haunt>().maxSpeed = Mathf.Lerp(spiritMinSpeed, spiritMaxSpeed, i / (count - 1f));

            Rigidbody2D spiritRigidbody = spirit.GetComponent<Rigidbody2D>();
            spiritRigidbody.linearVelocity = Vector2.up * 2f;
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        spriteRenderer.sprite = idleSprite;
        bossManage.currentRightHandPattern = Boss1_RHandPattern.Idle;
        bossManage.SetPatternTimer("RHand");
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            bossManage.TakeDamage(Boss1_Part.RHand, 1);
        }
    }

    public IEnumerator Boss1_RHandHit()
    {
        // Right Hand Hit Animation
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    public IEnumerator Boss1_RHandDestroyed()
    {
        // Right Hand Destroyed Animation
        gameObject.SetActive(false);
        yield return null;
    }

}

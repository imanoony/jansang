using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BodyPattern
{
    Idle = 0,
    Shockwave = 1,
    Judgement = 2,
}

public enum LHandPattern
{
    Idle = 0,
    Grasp = 1,
    Bind = 2,
}

public enum RHandPattern
{
    Idle = 0,
    Smite = 1,
    Haunt = 2,
}

public class Boss1Manage : MonoBehaviour
{
    [Header("Part Objects")]
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private GameObject leftHandObject;
    [SerializeField] private GameObject rightHandObject;
    public Vector2 lHandOrigin = new Vector2(-2.4f, 0f);
    public Vector2 rHandOrigin = new Vector2(2.4f, 0f);
    private Boss1Body bossBody;
    private Boss1LeftHand bossLeftHand;
    private Boss1RightHand bossRightHand;


    [Header("Object References")]
    public GameObject playerObject;
    public Transform playerTransform;
    private PlayerMovement2D playerMovement;
    private Rigidbody2D playerRigidbody;
    [SerializeField] private List<GameObject> altarObjects;


    [Header("Patterns")]
    [SerializeField] private Vector2 destinationPoint;
    public BodyPattern currentBodyPattern = BodyPattern.Idle;
    public LHandPattern currentLeftHandPattern = LHandPattern.Idle;
    public RHandPattern currentRightHandPattern = RHandPattern.Idle;

    public float bodyPatternTimer = 1f;
    public float lHandPatternTimer = 3f;
    public float rHandPatternTimer = 2f;


    private void Awake()
    {
        bodyObject = transform.Find("Body").gameObject;
        leftHandObject = transform.Find("LeftHand").gameObject;
        rightHandObject = transform.Find("RightHand").gameObject;

        bossBody = bodyObject.GetComponent<Boss1Body>();
        bossLeftHand = leftHandObject.GetComponent<Boss1LeftHand>();
        bossRightHand = rightHandObject.GetComponent<Boss1RightHand>();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.GetComponent<Transform>();
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        altarObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None)
        .Where(t => t.gameObject.name == "Altar")
        .Select(t => t.gameObject)
        .ToList();
    }

    private void Update()
    {
        BodyPatternManage();
        LHandPatternManage();
        RHandPatternManage();
    }


    private void BodyPatternManage()
    {
        if (currentBodyPattern != BodyPattern.Idle) return;
        if (currentBodyPattern == BodyPattern.Idle && bodyPatternTimer > 0f)
        {
            bodyPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        patternChoice = 2; // Test Code
        currentBodyPattern = (BodyPattern)patternChoice;

        // Body Pattern 함수 호출
        // 그 함수에서 bodyPatternTimer 설정
        switch (currentBodyPattern)
        {
            case BodyPattern.Shockwave:
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                bossBody.Boss1Shockwave(direction);
                Debug.Log("Shockwave!! Direction: " + direction);
                break;
            case BodyPattern.Judgement:
                float fartestDistance = -1f;
                GameObject targetAltar = null;
                foreach (GameObject altar in altarObjects)
                {
                    float distance = Vector2.Distance(transform.position, altar.transform.position);
                    if (distance > fartestDistance)
                    {
                        fartestDistance = distance;
                        targetAltar = altar;
                    }
                }
                if (targetAltar != null) bossBody.Boss1Judgement(targetAltar);
                else currentBodyPattern = BodyPattern.Idle;
                break;
        }
    }

    private void LHandPatternManage()
    {
        if (currentLeftHandPattern != LHandPattern.Idle ||
           currentBodyPattern == BodyPattern.Judgement) return;
        if (currentLeftHandPattern == LHandPattern.Idle && lHandPatternTimer > 0f)
        {
            lHandPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        patternChoice = 1; // Test Code
        currentLeftHandPattern = (LHandPattern)patternChoice;
        // L Hand Pattern 함수 호출
        // 그 함수에서 lHandPatternTimer 설정
        switch (currentLeftHandPattern)
        {
            case LHandPattern.Grasp:
                bossLeftHand.Boss1Grasp();
                break;
            case LHandPattern.Bind:
                // bossLeftHand.Boss1LHandBind();
                break;

        }
    }

    private void RHandPatternManage()
    {
        if (currentRightHandPattern != RHandPattern.Idle ||
           currentBodyPattern == BodyPattern.Judgement) return;
        if (currentRightHandPattern == RHandPattern.Idle && rHandPatternTimer > 0f)
        {
            rHandPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        patternChoice = 2; // Test Code
        currentRightHandPattern = (RHandPattern)patternChoice;
        // R Hand Pattern 함수 호출
        // 그 함수에서 rHandPatternTimer 설정
        switch (currentRightHandPattern)
        {
            case RHandPattern.Smite:
                // bossRightHand.Boss1Smite();
                break;
            case RHandPattern.Haunt:
                bossRightHand.Boss1Haunt(3);
                break;

        }
    }



    public static IEnumerator ObjectMoveControl(GameObject obj, Vector2 startPos, Vector2 targetPos,
                                                float dur1, float dur2, System.Action onComplete = null)
    {
        float timer = 0f;
        float endPotion = dur1 / (dur1 * 2 + dur2 * 2);

        while (timer < dur1)
        {
            timer += Time.deltaTime;
            obj.transform.position = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, (float)Mathf.Pow(timer / dur1, 2) * endPotion),
                Mathf.Lerp(startPos.y, targetPos.y, (float)Mathf.Pow(timer / dur1, 2) * endPotion)
            );
            yield return null;
        }

        timer = 0f;
        while (timer < dur2)
        {
            timer += Time.deltaTime;
            obj.transform.position = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, endPotion + timer / (dur1 + dur2)),
                Mathf.Lerp(startPos.y, targetPos.y, endPotion + timer / (dur1 + dur2))
            );
            yield return null;
        }

        timer = 0f;
        while (timer < dur1)
        {
            timer += Time.deltaTime;
            obj.transform.position = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, 1f - (float)Mathf.Pow((dur1 - timer) / dur1, 2) * endPotion),
                Mathf.Lerp(startPos.y, targetPos.y, 1f - (float)Mathf.Pow((dur1 - timer) / dur1, 2) * endPotion)
            );
            yield return null;
        }


        onComplete?.Invoke();
    }

    public static IEnumerator ObjectMoveControlLocalPos(GameObject obj, Vector2 startPos, Vector2 targetPos,
                                                float dur1, float dur2, System.Action onComplete = null)
    {
        float timer = 0f;
        float endPotion = dur1 / (dur1 * 2 + dur2 * 2);
        
        while(timer < dur1)
        {
            timer += Time.deltaTime;
            obj.transform.localPosition = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, (float)Mathf.Pow(timer/dur1, 2) * endPotion),
                Mathf.Lerp(startPos.y, targetPos.y, (float)Mathf.Pow(timer/dur1, 2) * endPotion)
            );
            yield return null;
        }
        
        timer = 0f;
        while(timer < dur2)
        {
            timer += Time.deltaTime;
            obj.transform.localPosition = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, endPotion + timer/(dur1 + dur2)),
                Mathf.Lerp(startPos.y, targetPos.y, endPotion + timer/(dur1 + dur2))
            );
            yield return null;
        }
        
        timer = 0f;
        while(timer < dur1)
        {
            timer += Time.deltaTime;
            obj.transform.localPosition = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, 1f - (float)Mathf.Pow((dur1-timer)/dur1, 2) * endPotion),
                Mathf.Lerp(startPos.y, targetPos.y, 1f - (float)Mathf.Pow((dur1-timer)/dur1, 2) * endPotion)
            );
            yield return null;
        }

        
        onComplete?.Invoke();
    }



}

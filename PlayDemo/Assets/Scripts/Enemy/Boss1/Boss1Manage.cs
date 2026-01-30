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
    [SerializeField] private Vector2 lHandOrigin = new Vector2(-2.4f, 0f);
    [SerializeField] private Vector2 rHandOrigin = new Vector2(2.4f, 0f);
    private Boss1Body bossBody;
    private Boss1LeftHand bossLeftHand;
    private Boss1RightHand bossRightHand;
    

    [Header("Object References")]
    public GameObject playerObject;
    private Transform playerTransform;
    private PlayerMovement2D playerMovement;
    private Rigidbody2D playerRigidbody;
    [SerializeField] private List<GameObject> altarObjects;


    [Header("Patterns")]
    [SerializeField] private Vector2 destinationPoint;
    public BodyPattern currentBodyPattern = BodyPattern.Idle;
    public LHandPattern currentLeftHandPattern = LHandPattern.Idle;
    public RHandPattern currentRightHandPattern = RHandPattern.Idle;

    public float bodyPatternTimer = 0f;
    public float lHandPatternTimer = 0f;
    public float rHandPatternTimer = 0f;


    private void Awake()
    {
        bodyObject = transform.Find("Body").gameObject;
        leftHandObject = transform.Find("LeftHand").gameObject;
        rightHandObject = transform.Find("RightHand").gameObject;

        bossBody = bodyObject.GetComponent<Boss1Body>();
        bossLeftHand = bodyObject.GetComponent<Boss1LeftHand>();
        bossRightHand = bodyObject.GetComponent<Boss1RightHand>();

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
        if(currentBodyPattern != BodyPattern.Idle) return;
        if(currentBodyPattern == BodyPattern.Idle && bodyPatternTimer > 0f)
        {
            bodyPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
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
                foreach(GameObject altar in altarObjects)
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
        if(currentLeftHandPattern != LHandPattern.Idle ||
           currentBodyPattern == BodyPattern.Judgement) return;
        if(currentLeftHandPattern == LHandPattern.Idle && lHandPatternTimer > 0f)
        {
            lHandPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        currentLeftHandPattern = (LHandPattern)patternChoice;
        // L Hand Pattern 함수 호출
        // 그 함수에서 lHandPatternTimer 설정
    }

    private void RHandPatternManage()
    {
        if(currentRightHandPattern != RHandPattern.Idle ||
           currentBodyPattern == BodyPattern.Judgement) return;
        if(currentRightHandPattern == RHandPattern.Idle && rHandPatternTimer > 0f)
        {
            rHandPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        currentRightHandPattern = (RHandPattern)patternChoice;
        // R Hand Pattern 함수 호출
        // 그 함수에서 rHandPatternTimer 설정
    }
    
}

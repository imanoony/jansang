using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public enum Boss1_Part
{
    Body = 0,
    LHand = 1,
    RHand = 2,
}
public enum Boss1_BodyPattern
{
    Idle = 0,
    Shockwave = 1,
    Judgement = 2,
    Destroyed = 3,
}

public enum Boss1_LHandPattern
{
    Idle = 0,
    Grasp = 1,
    Bind = 2,
}

public enum Boss1_RHandPattern
{
    Idle = 0,
    Smite = 1,
    Haunt = 2,
}

public class Boss1_Manage : MonoBehaviour
{
    [Header("Cut Scene Settings")]
    public float appearTime = 3f;
    public float handAppearTime = 3f;
    public float destroyTime = 3f; 
    public float disappearTime = 1.5f;
    
    [Header("Cut Scene References")]
    public CameraFollow2D cameraFollow;
    public bool isInCutScene = false;
    public GameObject rewardSpirit;
    public ParticleSystem rewardParticle;
    public ParticleSystem destroyParticle;
    public SpriteMask bodyMask;

    [Header("Movement")]
    [SerializeField] private Rigidbody2D bossRB;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float breakSpeed = 1f;

    [Header("Part Objects")]
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private GameObject leftHandObject;
    [SerializeField] private GameObject rightHandObject;
    public Vector2 lHandOrigin = new Vector2(-2.4f, 0f);
    public Vector2 rHandOrigin = new Vector2(2.4f, 0f);
    private Boss1_Body bossBody;
    private Boss1_LeftHand bossLeftHand;
    private Boss1_RightHand bossRightHand;
    private SpriteRenderer leftHandSprite;
    private SpriteRenderer rightHandSprite;
    private SpriteRenderer bodySprite;


    [Header("Player References")]
    public GameObject playerObject;
    public Transform playerTransform;
    public PlayerHitCheck playerHitCheck;
    public PlayerMovement2D playerMovement;
    public MeleeController2D playerMeleeController;
    public Rigidbody2D playerRigidbody;
    public LayerMask attackLayer;

    [Header("Object References")]
    public List<GameObject> altarObjects;
    public List<GameObject> spawnPoints;

    [Header("Map Enemy Spawn")]
    public GameObject enemyRoot;
    public float spawnCooldown = 6f;
    public float spawnTimer = 0f;
    public int maxSpawnCount = 5;
    public int enemySpawnIndex = 0;
    public List<GameObject> enemyPrefabs;
    public List<GameObject> spawnedEnemies;


    [Header("Patterns")]
    [SerializeField] private Vector2 destinationPoint;
    public Boss1_BodyPattern currentBodyPattern = Boss1_BodyPattern.Idle;
    public Boss1_LHandPattern currentLeftHandPattern = Boss1_LHandPattern.Idle;
    public Boss1_RHandPattern currentRightHandPattern = Boss1_RHandPattern.Idle;

    [Header("Pattern Timers")]
    public float bodyPatternTimer = 1f;
    public float lHandPatternTimer = 3f;
    public float rHandPatternTimer = 2f;

    [Header("Pattern Cooldowns")]
    private float bodyPatternMinTime = 5f;
    private float bodyPatternMaxTime = 7f;
    private float lHandPatternMinTime = 4f;
    private float lHandPatternMaxTime = 6f;
    private float rHandPatternMinTime = 4f;
    private float rHandPatternMaxTime = 6f;

    [Header("Boss Health")]
    [SerializeField] private int lHandHealth = 10;
    [SerializeField] private int rHandHealth = 10;
    [SerializeField] private int bodyHealth = 20;

#region Get Components
    private void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow2D>();

        bossRB = GetComponent<Rigidbody2D>();

        bodyObject = transform.Find("Body").gameObject;
        leftHandObject = transform.Find("LeftHand").gameObject;
        rightHandObject = transform.Find("RightHand").gameObject;
        rewardSpirit = transform.Find("Reward").gameObject;
        rewardParticle = rewardSpirit.transform.Find("Reward Particle").GetComponent<ParticleSystem>();    
        destroyParticle = bodyObject.transform.Find("Destroy Particle").GetComponent<ParticleSystem>();
        bodyMask = bodyObject.transform.Find("Body Mask").GetComponent<SpriteMask>();

        bossBody = bodyObject.GetComponent<Boss1_Body>();
        bossLeftHand = leftHandObject.GetComponent<Boss1_LeftHand>();
        bossRightHand = rightHandObject.GetComponent<Boss1_RightHand>();

        leftHandSprite = leftHandObject.GetComponent<SpriteRenderer>();
        rightHandSprite = rightHandObject.GetComponent<SpriteRenderer>();
        bodySprite = bodyObject.GetComponent<SpriteRenderer>();
        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.GetComponent<Transform>();
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        playerHitCheck = playerObject.GetComponent<PlayerHitCheck>();
        playerMeleeController = playerObject.GetComponent<MeleeController2D>();
    }

    private void Start()
    {
        // For Cut Scene Test
        StartCoroutine(Boss1_AppearScene());

        altarObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None)
        .Where(t => t.gameObject.name == "Altar")
        .Select(t => t.gameObject)
        .ToList();

        spawnPoints = FindObjectsByType<Transform>(FindObjectsSortMode.None)
        .Where(t => t.gameObject.name == "Spawn Point")
        .Select(t => t.gameObject)
        .ToList();
    }
#endregion

    private void Update()
    {
        if(currentBodyPattern == Boss1_BodyPattern.Destroyed || isInCutScene) return;

        MapPatternManage();
        BossMoveManage();
        BodyPatternManage();
        LHandPatternManage();
        RHandPatternManage();
    }

#region Cut Scene

    public IEnumerator Boss1_AppearScene()
    {
        rewardSpirit.SetActive(false);

        isInCutScene = true;
        cameraFollow.SetTransformMode(false);
        cameraFollow.SetTargetRoot(transform);
        float originalMouseInfluence = cameraFollow.mouseInfluence;
        cameraFollow.mouseInfluence = 0f;

        playerMovement.Stun(appearTime + handAppearTime);
        playerMeleeController.AttackSilence(appearTime + handAppearTime);

        bodyObject.SetActive(true);
        leftHandObject.SetActive(false);
        rightHandObject.SetActive(false);

        bodyObject.transform.localPosition = Vector2.zero;
        leftHandObject.transform.localPosition = lHandOrigin;
        rightHandObject.transform.localPosition = rHandOrigin;


        yield return new WaitForSeconds(appearTime);

        leftHandObject.SetActive(true);
        leftHandObject.transform.localScale = Vector2.one * 0.5f;
        Color originLHandColor = leftHandSprite.color;
        leftHandSprite.color = new Vector4(originLHandColor.r, originLHandColor.g, originLHandColor.b, 0f);
        rightHandObject.SetActive(true);
        rightHandObject.transform.localScale = Vector2.one * 0.5f;
        Color originRHandColor = rightHandSprite.color;
        rightHandSprite.color = new Vector4(originRHandColor.r, originRHandColor.g, originRHandColor.b, 0f);
        
        float timer = 0f;
        while (timer < handAppearTime)
        {
            timer += Time.deltaTime;
            
            leftHandObject.transform.localScale = Vector2.Lerp(Vector2.one * 0.5f, Vector2.one, timer / handAppearTime);
            leftHandSprite.color = new Vector4(originLHandColor.r, originLHandColor.g, originLHandColor.b, Mathf.Lerp(0f, originLHandColor.a, timer / handAppearTime));
            rightHandObject.transform.localScale = Vector2.Lerp(Vector2.one * 0.5f, Vector2.one, timer / handAppearTime);
            rightHandSprite.color = new Vector4(originRHandColor.r, originRHandColor.g, originRHandColor.b, Mathf.Lerp(0f, originRHandColor.a, timer / handAppearTime));
            yield return null;
        }

        cameraFollow.SetTransformMode(true);
        cameraFollow.SetTargetRoot(playerTransform);
        cameraFollow.mouseInfluence = originalMouseInfluence;
        isInCutScene = false;
    }



    public IEnumerator Boss1_DestroyScene()
    {
        StopCoroutine("ObjectMoveControl");
        StopCoroutine("ObjectMoveControlLocalPos");

        gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        playerRigidbody.linearVelocity = Vector2.zero;

        isInCutScene = true;
        cameraFollow.SetTargetRoot(rewardSpirit.transform);
        float originalMouseInfluence = cameraFollow.mouseInfluence;
        cameraFollow.mouseInfluence = 0f;

        playerMovement.Stun(60f);
        playerMeleeController.AttackSilence(60f);

        float originalExtraZoom = cameraFollow.extraZoom;
        cameraFollow.extraZoom = -3f;

        destroyParticle.gameObject.SetActive(true);
        destroyParticle.Play();
        yield return new WaitForSeconds(destroyTime);

        rewardSpirit.SetActive(true);

        Color bodyOriginalColor = bodySprite.color;
        float timer = 0f;
        while(timer < disappearTime)
        {
            timer += Time.deltaTime;
            bodyObject.transform.localPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, 1f), timer / disappearTime);
            bodyMask.transform.localPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, 4.3f), timer / disappearTime);
            destroyParticle.transform.localPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, 3f), timer / disappearTime);
            destroyParticle.Play();
            bodySprite.color = new Vector4(bodyOriginalColor.r, bodyOriginalColor.g, bodyOriginalColor.b, Mathf.Lerp(1f, 0f, timer / disappearTime));
            yield return null;
        }
        destroyParticle.gameObject.SetActive(false);

        timer = 0f;
        bossRB.AddForce(new Vector2(0f, 1f), ForceMode2D.Impulse);
        bossRB.constraints = RigidbodyConstraints2D.None;
        while(Vector2.Distance(transform.position, playerTransform.position) > 0.3f)
        {
            timer += Time.deltaTime;
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            bossRB.AddForce(direction * (timer > 2f ? 5f : 1f));
            if(bossRB.linearVelocity.magnitude > 1f)
            {
                bossRB.linearVelocity = bossRB.linearVelocity.normalized * 1f;
            }

            Vector2 vDirection = bossRB.linearVelocity.normalized;
            rewardSpirit.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vDirection.y, vDirection.x) * Mathf.Rad2Deg - 90f);
            yield return null;
        }
        cameraFollow.extraZoom = originalExtraZoom;

        rewardParticle.gameObject.SetActive(true);
        rewardParticle.Play();
        SpriteRenderer rewardSR = rewardSpirit.GetComponent<SpriteRenderer>();
        rewardSR.color = new Vector4(rewardSR.color.r, rewardSR.color.g, rewardSR.color.b, 0f);
        bossRB.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        rewardSpirit.SetActive(false);

        playerMeleeController.attackSilenced = false;
        playerMovement.stunned = false;
        playerRigidbody.linearVelocity = Vector2.zero;

        gameObject.SetActive(false);
        isInCutScene = false;
        cameraFollow.SetTargetRoot(playerTransform);
        cameraFollow.mouseInfluence = originalMouseInfluence;
    }

#endregion

#region Damage and Death
    public void TakeDamage(Boss1_Part part, int damage)
    {
        switch(part){
            case Boss1_Part.Body:
                if(lHandHealth > 0 || rHandHealth > 0) return;
                bodyHealth -= damage;
                if(bodyHealth <= 0){
                    foreach(GameObject enemy in spawnedEnemies)
                    {
                        if(enemy != null) Destroy(enemy);
                    }
                    currentBodyPattern = Boss1_BodyPattern.Destroyed;

                    StartCoroutine(bossBody.Boss1_BodyDestroyed());
                }
                else
                {
                    StartCoroutine(bossBody.Boss1_BodyHit());
                }
                break;
            case Boss1_Part.LHand:
                lHandHealth -= damage;
                if(lHandHealth <= 0){
                    StartCoroutine(bossLeftHand.Boss1_LHandDestroyed());
                }
                else
                {
                    StartCoroutine(bossLeftHand.Boss1_LHandHit());
                }
                break;
            case Boss1_Part.RHand:
                rHandHealth -= damage;
                if(rHandHealth <= 0){
                    StartCoroutine(bossRightHand.Boss1_RHandDestroyed());
                }
                else
                {
                    StartCoroutine(bossRightHand.Boss1_RHandHit());
                }
                break;
        }
    }
#endregion


#region Pattern Manage

    private void MapPatternManage()
    {
        spawnedEnemies.RemoveAll(t => t == null);

        if(spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
        }
        else
        {
            spawnTimer = spawnCooldown;
            foreach(GameObject spawnPoint in spawnPoints)
            {
                if(spawnedEnemies.Count >= maxSpawnCount) break;

                GameObject enemy = Instantiate(
                    enemyPrefabs[enemySpawnIndex], 
                    spawnPoint.transform.position, 
                    Quaternion.identity,
                    enemyRoot != null ? enemyRoot.transform : null
                );
                spawnedEnemies.Add(enemy);

                enemySpawnIndex = (enemySpawnIndex + 1) % enemyPrefabs.Count;
            }
        }
    }

    private void BossMoveManage()
    {
        if(currentBodyPattern != Boss1_BodyPattern.Idle ||
           currentLeftHandPattern != Boss1_LHandPattern.Idle ||
           currentRightHandPattern != Boss1_RHandPattern.Idle){

            if (currentBodyPattern == Boss1_BodyPattern.Judgement) return;

            if(bossRB.linearVelocity.magnitude > 0.1f) bossRB.AddForce(bossRB.linearVelocity * -breakSpeed);
            else bossRB.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 playerPosNoise = playerTransform.position +
            new Vector3(
                Random.Range(-1f, 1f) * playerMovement.moveSpeed * 0.5f,
                Random.Range(-1f, 1f) * playerMovement.moveSpeed * 0.5f,
                0f
            );
        Vector2 direction = (playerPosNoise - (Vector2)transform.position).normalized;
        bossRB.AddForce(direction * moveSpeed);
        if(bossRB.linearVelocity.magnitude > moveSpeed)
        {
            bossRB.linearVelocity = bossRB.linearVelocity.normalized * moveSpeed;
        }
    }

    public float SetPatternTimer(string part)
    {
        switch(part){
            case "Body":
                bodyPatternTimer = Random.Range(bodyPatternMinTime, bodyPatternMaxTime);
                return bodyPatternTimer;
            case "LHand":
                lHandPatternTimer = Random.Range(lHandPatternMinTime, lHandPatternMaxTime);
                return lHandPatternTimer;
            case "RHand":
                rHandPatternTimer = Random.Range(rHandPatternMinTime, rHandPatternMaxTime);
                return rHandPatternTimer;
            default:
                return -1f;
        }
    }

    private void BodyPatternManage()
    {
        if (bodyHealth <= 0) return;
        if (currentBodyPattern != Boss1_BodyPattern.Idle) return;
        if (currentBodyPattern == Boss1_BodyPattern.Idle && bodyPatternTimer > 0f)
        {
            bodyPatternTimer -= Time.deltaTime;
            return;
        }

        int patternChoice = Random.Range(1, 3);
        currentBodyPattern = (Boss1_BodyPattern)patternChoice;

        // test
        currentBodyPattern = Boss1_BodyPattern.Judgement;

        // Body Pattern 함수 호출
        // 그 함수에서 bodyPatternTimer 설정
        switch (currentBodyPattern)
        {
            case Boss1_BodyPattern.Shockwave:
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                bossBody.Boss1_Shockwave(direction);
                Debug.Log("Shockwave!! Direction: " + direction);
                break;
            case Boss1_BodyPattern.Judgement:
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
                if (targetAltar != null) bossBody.Boss1_Judgement(targetAltar);
                else currentBodyPattern = Boss1_BodyPattern.Idle;
                break;
        }
    }

    private void LHandPatternManage()
    {
        if (lHandHealth <= 0) return;
        if (currentLeftHandPattern != Boss1_LHandPattern.Idle ||
           currentBodyPattern == Boss1_BodyPattern.Judgement) return;
        if (currentLeftHandPattern == Boss1_LHandPattern.Idle && lHandPatternTimer > 0f)
        {
            lHandPatternTimer -= Time.deltaTime;
            return;
        }

        Boss1_LHandPattern patternChoice = Boss1_LHandPattern.Grasp; // Test Code
        currentLeftHandPattern = patternChoice;
        // L Hand Pattern 함수 호출
        // 그 함수에서 lHandPatternTimer 설정
        switch (currentLeftHandPattern)
        {
            case Boss1_LHandPattern.Grasp:
                bossLeftHand.Boss1_Grasp();
                break;
            case Boss1_LHandPattern.Bind:
                // bossLeftHand.Boss1_LHandBind();
                break;

        }
    }

    private void RHandPatternManage()
    {
        if (rHandHealth <= 0) return;
        if (currentRightHandPattern != Boss1_RHandPattern.Idle ||
           currentBodyPattern == Boss1_BodyPattern.Judgement) return;
        if (currentRightHandPattern == Boss1_RHandPattern.Idle && rHandPatternTimer > 0f)
        {
            rHandPatternTimer -= Time.deltaTime;
            return;
        }

        Boss1_RHandPattern patternChoice = Boss1_RHandPattern.Haunt;
        currentRightHandPattern = patternChoice;
        // R Hand Pattern 함수 호출
        // 그 함수에서 rHandPatternTimer 설정
        switch (currentRightHandPattern)
        {
            case Boss1_RHandPattern.Smite:
                // bossRightHand.Boss1_Smite();
                break;
            case Boss1_RHandPattern.Haunt:
                bossRightHand.Boss1_Haunt(3);
                break;

        }
    }

#endregion


#region Smooth Move

    public IEnumerator ObjectMoveControl(GameObject obj, Vector2 startPos, Vector2 targetPos,
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

            if(isInCutScene) yield break;
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

            if(isInCutScene) yield break;
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

            if(isInCutScene) yield break;
        }


        onComplete?.Invoke();
    }

    public IEnumerator ObjectMoveControlLocalPos(GameObject obj, Vector2 startPos, Vector2 targetPos,
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

            if(isInCutScene) yield break;
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

            if(isInCutScene) yield break;
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

            if(isInCutScene) yield break;
        }

        
        onComplete?.Invoke();
    }

#endregion

}

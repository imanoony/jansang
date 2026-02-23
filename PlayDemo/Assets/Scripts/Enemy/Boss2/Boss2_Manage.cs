using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Boss2_Pattern
{
    Idle = 0,
    Moving = 1,
    Dash = 2,
    Slash = 3,
    Counter = 4,
    Laser = 5
}

public class Boss2_Manage : MonoBehaviour
{
    private bool difficultyApplied;
    [Header("Cut Scene Settings")]
    public float appearDashTime = 2f;
    public float appearDashDistance = 4f;
    public Vector3 bossStartPosition = new Vector3(6f, 2f, 0f);
    
    [Header("Cut Scene References")]
    public CameraFollow2D cameraFollow;
    public bool isInCutScene = false;
    public GameObject particleParent;
    public ParticleSystem destroyParticle;
    public ParticleSystem boomParticle;

    [Header("UI")]
    public Boss2_UI bossUI;

    [Header("Movement")]
    public Rigidbody2D bossRB;
    public bool isRight = true;
    public float moveSpeed = 2.5f;
    public float moveAccel = 0.5f;
    public float breakAccel = 4f;
    public GameObject bossObject;

    [Header("Jump")]
    public bool isJumped = false;
    public bool isDoubleJumped = false;
    public float jumpForce = 12f;
    public float doubleJumpWaitTime = 0.5f;
    public float waitForDoubleJump = 0.5f;
    public bool downJumping = false;


    [Header("Player References")]
    public GameObject playerObject;
    public Transform playerTransform;
    public PlayerHitCheck playerHitCheck;
    public PlayerMovement2D playerMovement;
    public MeleeController2D playerMeleeController;
    public Rigidbody2D playerRigidbody;
    public LayerMask attackLayer;
    
    [Header("Pattern")]
    public Boss2_Pattern currentPattern;
    [SerializeField] private Boss2_Pattern testingPattern = Boss2_Pattern.Dash;
    public float patternTimer = 3f;
    public float patternCooldown = 6f;
    public float moveTimer = 0f;
    public float moveCooldown = 1.5f;
    public bool fireOn = false;

    [Header("Pattern Settings")]
    public float laserDistance = 8f;
    public float closeDistance = 2f;

    [Header("Destination Check")]
    public float xDiff = 0f;
    public float yDiff = 0f;
    public Vector2 playerPlatform;
    public Vector2 destinationPos;
    public float groundCheckDistance = 3f;
    public LayerMask groundLayer;
    public float tileSize = 1f;

    private Collider2D bossCol;
    private Boss2_Action bossAction;

    [Header("Boss Health")]
    public int maxHealth = 10;
    public int health = 10;
    public float invincibleTime = 1f;
    public bool isInvincible = false;

    [Header("Sprites")]
    public GameObject spriteParent;
    public SpriteRenderer eyesSR;
    public SpriteRenderer headSR;
    public SpriteRenderer bodySR;
    public Vector2 originalEyePos;
    public Vector2 originalHeadPos;
    public Color dashEyeColor;
    public Color slashEyeColor;
    public Color counterEyeColor;
    public Color laserEyeColor;

#region Get References
    private void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow2D>();

        bossRB = GetComponent<Rigidbody2D>();
        bossObject = gameObject.transform.GetChild(0).gameObject;
        bossCol = bossObject.GetComponent<Collider2D>();
        bossAction = bossObject.GetComponentInChildren<Boss2_Action>();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.GetComponent<Transform>();
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        playerHitCheck = playerObject.GetComponent<PlayerHitCheck>();
        playerMeleeController = playerObject.GetComponent<MeleeController2D>();

        spriteParent = bossObject.transform.Find("Sprite").gameObject;
        particleParent = gameObject.transform.Find("Particle").gameObject;
        destroyParticle = particleParent.transform.Find("Destroy").GetComponent<ParticleSystem>();
        boomParticle = particleParent.transform.Find("Boom").GetComponent<ParticleSystem>();
        
        eyesSR = spriteParent.transform.Find("Eyes").GetComponent<SpriteRenderer>();
        headSR = spriteParent.transform.Find("Head").GetComponent<SpriteRenderer>();
        bodySR = spriteParent.transform.Find("Body").GetComponent<SpriteRenderer>();

        bossUI = GameObject.Find("Boss UI").GetComponent<Boss2_UI>();

        ApplyDifficulty();
    }
#endregion

    private void Start()
    {
        isInCutScene = true;
        StartBossFight();
    }
    
    public void StartBossFight()
    {
        health = maxHealth;
        StartCoroutine(Boss2_AppearScene());
    }

    private void ApplyDifficulty()
    {
        if (difficultyApplied) return;
        float multiplier = 1f;
        if (GameManager.Instance != null) multiplier = GameManager.Instance.EnemyHpMultiplier;
        if (multiplier <= 0f) multiplier = 1f;
        maxHealth = Mathf.Max(1, Mathf.CeilToInt(maxHealth * multiplier));
        difficultyApplied = true;
    }

    private void Update()
    {
        if(currentPattern == Boss2_Pattern.Moving)
        {
            eyesSR.gameObject.transform.localPosition = new Vector3(
                0.05f, 0.25f, 0f
            );
            headSR.gameObject.transform.localPosition = new Vector3(
                0.05f, 0.25f, 0f
            );
            bodySR.gameObject.transform.localRotation = Quaternion.Euler(
                0f, 0f, -15f
            );
        }
        else
        {
            eyesSR.gameObject.transform.localPosition = new Vector3(
                0.0f, 0.25f, 0f
            );
            headSR.gameObject.transform.localPosition = new Vector3(
                0.0f, 0.25f, 0f
            );
            bodySR.gameObject.transform.localRotation = Quaternion.Euler(
                0f, 0f, 0f
            );
        }

        if(isInCutScene || !bossObject.gameObject.activeSelf){ return; }

        bossUI.UpdateHealthBar((float)health / maxHealth);

        CheckPlayer();
        CheckGround();
        MoveManage();
        PatternManage();
        JumpManage();
    }

    public void SetBossColor(Color color, Color eyeColor)
    {
        eyesSR.color = eyeColor;
        headSR.color = color;
        bodySR.color = color;
    }

#region Damage
    public void TakeDamage(int damage)
    {
        if (isInvincible){ return; }

        health -= damage;
        SpawnHitEffect();

        if(health <= 0)
        {
            health = 0;
            StartCoroutine(Boss2_DestoryScene());
            return;
        }

        StartCoroutine(DamageEffect());
    }

    private void SpawnHitEffect()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        Vector3 pos = bossCol != null ? bossCol.bounds.center : transform.position;
        gm.SpawnHitEffect(pos);
    }

    IEnumerator DamageEffect()
    {
        isInvincible = true;
        Color eyeOriginalColor = bossAction.originalEyeColor;
        Color originalColor = bossAction.originalColor;
        SetBossColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f), 
                    new Color(eyeOriginalColor.r, eyeOriginalColor.g, eyeOriginalColor.b, 0.3f));
        yield return new WaitForSeconds(invincibleTime);
        SetBossColor(originalColor, eyeOriginalColor);
        isInvincible = false;
    }
#endregion

#region Cut Scene

    public IEnumerator Boss2_AppearScene()
    {
        bossObject.SetActive(false);
        currentPattern = Boss2_Pattern.Moving;

        float originalMouseInfluence = cameraFollow.mouseInfluence;
        float originalExtraZoom = cameraFollow.extraZoom;

        isInCutScene = true;
        playerRigidbody.linearVelocity = Vector2.zero;
        playerMovement.Stun(99f);
        playerMeleeController.AttackSilence(99f);
        cameraFollow.SetTargetRoot(playerTransform);
        cameraFollow.mouseInfluence = 0f;
        cameraFollow.extraZoom = -3f;

        yield return new WaitForSeconds(1f);

        bossObject.SetActive(true);

        Vector3 originalScale = transform.localScale;
        Color eyeOriginalColor = eyesSR.color;
        Color originalColor = bodySR.color;
        SetBossColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f), 
                    new Color(eyeOriginalColor.r, eyeOriginalColor.g, eyeOriginalColor.b, 0.3f));
        transform.position = playerTransform.position + new Vector3(7f, 0f, 0f);
        transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);

        float timer = 0f;
        while(timer < appearDashTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(
                playerTransform.position + new Vector3(appearDashDistance, 0f, 0f),
                playerTransform.position + new Vector3(-appearDashDistance, 0f, 0f),
                timer / appearDashTime
            );
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        timer = 0f;
        transform.localScale = originalScale;
        while(timer < appearDashTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(
                playerTransform.position + new Vector3(-appearDashDistance, 0f, 0f),
                playerTransform.position + new Vector3(appearDashDistance, 0f, 0f),
                timer / appearDashTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        SetBossColor(originalColor, eyeOriginalColor);
        bossRB.linearVelocity = Vector2.zero;
        transform.position = bossStartPosition;

        playerMovement.stunned = false;
        playerMeleeController.attackSilenced = false;

        bossUI.StartCoroutine(bossUI.UI_Appear());
        isInCutScene = false;
        fireOn = true;
        cameraFollow.extraZoom = originalExtraZoom;
        cameraFollow.mouseInfluence = originalMouseInfluence;
    }

    public IEnumerator Boss2_DestoryScene()
    {
        bossUI.UI_Disappear();
        fireOn = false;

        isInCutScene = true;
        bossRB.linearVelocity = Vector2.zero;
        bossRB.gravityScale = 0f;
        float originalMouseInfluence = cameraFollow.mouseInfluence;
        float originalExtraZoom = cameraFollow.extraZoom;

        playerRigidbody.linearVelocity = Vector2.zero;
        playerMovement.Stun(99f);
        playerMeleeController.AttackSilence(99f);

        cameraFollow.SetTargetRoot(transform);
        cameraFollow.mouseInfluence = 0f;
        cameraFollow.extraZoom = -3f;

        destroyParticle.gameObject.SetActive(true);
        destroyParticle.Play();
        yield return new WaitForSeconds(4f);
        destroyParticle.gameObject.SetActive(false);

        bossObject.SetActive(false);

        boomParticle.gameObject.SetActive(true);
        boomParticle.Play();
        yield return new WaitForSeconds(2f);
        boomParticle.gameObject.SetActive(false);

        isInCutScene = false;
        playerMovement.stunned = false;
        playerMeleeController.attackSilenced = false;
        cameraFollow.SetTargetRoot(playerTransform);
        cameraFollow.extraZoom = originalExtraZoom;
        cameraFollow.mouseInfluence = originalMouseInfluence;
    }

#endregion

#region Jump
    public void Jump()
    {
        if (!isJumped)
        {
            bossRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumped = true;
            waitForDoubleJump = doubleJumpWaitTime;
        }
        else if (!isDoubleJumped)
        {
            if(waitForDoubleJump > 0f) return;
            bossRB.linearVelocity = new Vector2(bossRB.linearVelocity.x, jumpForce);
            isDoubleJumped = true;
        }
    }
    void JumpManage()
    {
        if(waitForDoubleJump > 0f && isJumped && !isDoubleJumped)
        {
            waitForDoubleJump -= Time.deltaTime;
            return;
        }
    }

    private bool CheckGround()
    {
        if(downJumping){ return false; }


        Bounds bounds = bossCol.bounds;
        var start = bounds.center + bounds.extents.y * Vector3.down;

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            tileSize*0.2f,
            groundLayer
        );

        if (hit.collider != null && hit.collider.OverlapPoint(start)){ return false; }
        if (hit.collider != null && hit.normal.y > 0.7f && bossRB.linearVelocity.y <= 0f)
        {
            if(downJumping){ return true; }
            isJumped = false;
            isDoubleJumped = false;
            return true;
        }
        return false;
    }
#endregion

#region Move

    private void CheckPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            playerTransform.position, 
            Vector2.down, 
            groundCheckDistance, 
            groundLayer
        );
        
        if(hit)
        {
            Tilemap feetTile = hit.collider.GetComponent<Tilemap>();
            if(feetTile != null)
            {
                Debug.Log("Found Tilemap under Player");
                Vector3Int cell = feetTile.WorldToCell(hit.point);
                Vector3 cellCenter = feetTile.GetCellCenterWorld(cell);
                playerPlatform = cellCenter + new Vector3(0f, tileSize / 2f, 0f);
            }
            else{ playerPlatform = playerTransform.position; }
        }
        else{ playerPlatform = playerTransform.position; }
    }



    private void MoveManage()
    {
        if(currentPattern != Boss2_Pattern.Moving && currentPattern != Boss2_Pattern.Idle){ return; }
        
        if(moveTimer > 0f){ 
            moveTimer -= Time.deltaTime; 
            currentPattern = Boss2_Pattern.Idle;
            return; 
        }
        else if(currentPattern == Boss2_Pattern.Idle){ 
            currentPattern = Boss2_Pattern.Moving; 
            destinationPos = new Vector2(
                playerPlatform.x,
                playerPlatform.y
            );
            return; 
        }

        xDiff = destinationPos.x - gameObject.transform.position.x;
        yDiff = destinationPos.y - gameObject.transform.position.y;

        if(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2) < tileSize*0.5f)
        {
            bossRB.linearVelocity = new Vector2(0f, bossRB.linearVelocity.y);
            moveTimer = moveCooldown;
            currentPattern = Boss2_Pattern.Idle;
            return;
        }

        if(Math.Abs(xDiff) > tileSize*0.5f && !isJumped) {
            if(Math.Sign(xDiff) == Math.Sign(bossRB.linearVelocity.x)) bossRB.AddForce(new Vector2(xDiff > 0f ? moveAccel : -moveAccel, 0f), ForceMode2D.Impulse);
            else bossRB.AddForce(new Vector2(xDiff > 0f ? breakAccel : -breakAccel, 0f), ForceMode2D.Impulse);
        }
        if(Math.Abs(bossRB.linearVelocity.x) > moveSpeed && !isJumped)
        {
            bossRB.linearVelocity = new Vector2(
                xDiff > 0f ? moveSpeed : -moveSpeed,
                bossRB.linearVelocity.y
            );
        }
        if(Math.Abs(bossRB.linearVelocity.x) > moveSpeed*0.3f && isJumped)
        {
            bossRB.linearVelocity = new Vector2(
                xDiff > 0f ? moveSpeed*0.3f : -moveSpeed*0.3f,
                bossRB.linearVelocity.y
            );
        }
        

        if(xDiff > 0.1f) { 
            isRight = true;
            bossObject.transform.localScale = new Vector3(bossAction.originalScale.x, bossAction.originalScale.y, bossAction.originalScale.z);
        }
        else if(xDiff < -0.1f) { 
            isRight = false;
            bossObject.transform.localScale = new Vector3(-bossAction.originalScale.x, bossAction.originalScale.y, bossAction.originalScale.z);
        }

        // 앞에 밟을게 없으면 점프
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position + new Vector3(tileSize, -tileSize * 0.5f, 0f),
            new Vector2(tileSize * 0.5f, tileSize * 0.5f),
            0f,
            Vector2.down,
            tileSize,
            groundLayer
        );

        RaycastHit2D cellingHit = Physics2D.Raycast(
            transform.position,
            Vector2.up,
            tileSize * 2f,
            groundLayer            
        );

        // 그냥 점프로 갈 수 없는 상황
        if (yDiff > tileSize*0.5f)
        {
            if (hit.collider == null)
            {
                Jump();
            }
            else if(Math.Abs(xDiff) < tileSize * 2f)
            {
                if(cellingHit.collider != null){ Jump(); }
                else{
                    bossRB.linearVelocity = Vector2.zero;
                    moveTimer = moveCooldown;
                    currentPattern = Boss2_Pattern.Idle;
                }
            }
        }

        if (Math.Abs(xDiff) < tileSize*2f && yDiff < -0.5f && CheckGround())
        {
            StartCoroutine(DownJump());
        }
    }

    IEnumerator DownJump()
    {
        isJumped = true;
        isDoubleJumped = true;
        bossCol.isTrigger = true;
        downJumping = true;
        yield return new WaitForSeconds(0.1f);
        downJumping = false;
        yield return new WaitUntil(() => CheckGround());
        bossCol.isTrigger = false;
    }
#endregion

#region Pattern

    private void PatternManage()
    {
        if(patternTimer > 0f){ patternTimer -= Time.deltaTime; }
        if(currentPattern != Boss2_Pattern.Idle || patternTimer > 0f){ return; }

        // Pattern 조건 고려하여 패턴 변경
        currentPattern = testingPattern;
        float xD = playerTransform.position.x - gameObject.transform.position.x;
        float yD = playerTransform.position.y - gameObject.transform.position.y;
        float distance = Mathf.Sqrt(xD * xD + yD * yD);

        if(distance > laserDistance){ currentPattern = Boss2_Pattern.Laser; }
        else if(distance > closeDistance){ 
            currentPattern = (Boss2_Pattern)UnityEngine.Random.Range(
                (int)Boss2_Pattern.Slash, (int)Boss2_Pattern.Counter + 1
            );
        }
        else{ currentPattern = Boss2_Pattern.Dash; }

        // Test
        if(testingPattern != Boss2_Pattern.Idle) currentPattern = testingPattern;

        isRight = playerTransform.position.x > gameObject.transform.position.x ? true : false;

        switch (currentPattern)
        {
            case Boss2_Pattern.Dash:
                StartCoroutine(bossAction.Boss2_Charge<bool>(
                    1.0f,
                    isRight,
                    dashEyeColor,
                    bossAction.Boss2_DashAction,
                    isRight
                ));
                break;
            case Boss2_Pattern.Slash:
                StartCoroutine(bossAction.Boss2_Charge<bool>(
                    1.0f,
                    isRight,
                    slashEyeColor,
                    bossAction.Boss2_SlashAction,
                    isRight
                ));
                break;
            case Boss2_Pattern.Counter:
                StartCoroutine(bossAction.Boss2_Charge(
                    0.0f,
                    isRight,
                    counterEyeColor,
                    bossAction.Boss2_CounterAction
                ));
                break;
            case Boss2_Pattern.Laser:
                float angle = Mathf.Atan2(
                    playerTransform.position.y - 0.2f - transform.position.y,
                    playerTransform.position.x - transform.position.x
                ) * Mathf.Rad2Deg + 90f + bossAction.laserAngleOffset * (isRight ? 1f : -1f);
                StartCoroutine(bossAction.Boss2_Charge<float>(
                    1.0f,
                    isRight,
                    laserEyeColor,
                    bossAction.Boss2_LaserAction,
                    angle
                ));
                break;
        }

    }

#endregion

}

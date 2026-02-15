using System;
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
    [Header("Movement")]
    public Rigidbody2D bossRB;
    public bool isRight = true;
    public float moveSpeed = 2.5f;
    public float moveAccel = 0.5f;
    public float breakAccel = 4f;
    public GameObject bossObject;
    private SpriteRenderer bossSR;

    [Header("Jump")]
    public bool isJumped = false;
    public bool isDoubleJumped = false;
    public float jumpForce = 12f;
    public float doubleJumpWaitTime = 0.5f;
    public float waitForDoubleJump = 0.5f;


    [Header("Player References")]
    public GameObject playerObject;
    public Transform playerTransform;
    public PlayerHitCheck playerHitCheck;
    public PlayerMovement2D playerMovement;
    public Rigidbody2D playerRigidbody;
    public LayerMask attackLayer;
    
    [Header("Pattern")]
    public Boss2_Pattern currentPattern;
    [SerializeField] private Boss2_Pattern testingPattern = Boss2_Pattern.Dash;
    public float patternTimer = 3f;
    public float patternCooldown = 6f;
    public float moveTimer = 0f;
    public float moveCooldown = 1.5f;

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

    private void Awake()
    {
        bossRB = GetComponent<Rigidbody2D>();
        bossObject = gameObject.transform.GetChild(0).gameObject;
        bossSR = bossObject.GetComponent<SpriteRenderer>();
        bossCol = bossObject.GetComponent<Collider2D>();
        bossAction = bossObject.GetComponentInChildren<Boss2_Action>();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.GetComponent<Transform>();
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        playerHitCheck = playerObject.GetComponent<PlayerHitCheck>();
    }


    private void Update()
    {
        CheckPlayer();
        CheckGround();
        MoveManage();
        PatternManage();
        JumpManage();
    }

    public void TakeDamage(int damage)
    {
        
    }

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
        Bounds bounds = bossCol.bounds;
        var start = bounds.center + bounds.extents.y * Vector3.down;

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            0.2f,
            groundLayer
        );

        if (hit.collider != null && hit.collider.OverlapPoint(start)){ return false; }
        if (hit.collider != null && hit.normal.y > 0.7f && bossRB.linearVelocity.y <= 0f)
        {
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
        

        if(xDiff > 0.1f) { isRight = true; bossSR.flipX = false; }
        else if(xDiff < -0.1f) { isRight = false; bossSR.flipX = true; }

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
            transform.position = new Vector2(
                transform.position.x,
                transform.position.y - tileSize*0.1f
            );
        }
    }
#endregion

#region Pattern

    private void PatternManage()
    {
        if(patternTimer > 0f){ patternTimer -= Time.deltaTime; }
        if(currentPattern != Boss2_Pattern.Idle || patternTimer > 0f){ return; }

        // Pattern 조건 고려하여 패턴 변경
        currentPattern = testingPattern;
        isRight = playerTransform.position.x > gameObject.transform.position.x ? true : false;

        switch (currentPattern)
        {
            case Boss2_Pattern.Dash:
                StartCoroutine(bossAction.Boss2_Charge(
                    1.0f,
                    isRight,
                    bossAction.Boss2_DashAction,
                    isRight
                ));
                break;
            case Boss2_Pattern.Slash:
                StartCoroutine(bossAction.Boss2_Charge(
                    1.0f,
                    isRight,
                    bossAction.Boss2_SlashAction,
                    isRight
                ));
                break;
            case Boss2_Pattern.Counter:
                break;
            case Boss2_Pattern.Laser:
                break;
        }

    }

#endregion

}

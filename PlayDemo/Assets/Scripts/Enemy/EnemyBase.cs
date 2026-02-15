using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
public class EnemyBase : MonoBehaviour
{
    #region HP
    [Header("HP Stats")]
    public float HP { get; protected set; }
    public int MaxHP;
    #endregion
    #region  parameters
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;
    [Header("Commander")] [SerializeField] private EnemyAlertEmitter commander;
    [Header("Talk")] 
    [SerializeField] private float talkChance = 0.002f;
    #endregion
    #region components
    protected Animator animator;
    protected Collider2D col;
    private Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    public SpriteRenderer detectionStatusRenderer;
    public Sprite[] detectionStatusSprites; // 0 : miss, 1 : found
    #endregion
    #region status
    private float currentSpeedRate;
    protected bool alerted = false;
    protected bool found = false;
    protected enum State
    {
        Idle,
        Alert,
        Combat
    }
    protected State currentState = State.Idle;
    protected bool isGrounded;
    private int moveDirection = 0; 
    public int MoveDirection => moveDirection;
    public float canTalkTime = 0;
    #endregion
    protected Transform Player { get; private set; }
    protected Transform CurrentTarget { get; set; }
    private CancellationTokenSource hitFlashCts;
    private int flashVersion;
    private Color baseColor = Color.white;
    private bool isFlashing;
    public void TryTalk()
    {
        if (canTalkTime > Time.time) return;
        float talkchance = Random.Range(0f, 1f);
        if (talkchance > talkChance) return;
        float delta = GameManager.Instance.talk.Show(transform);
        canTalkTime = Time.time + delta;
    }
    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (commander != null) commander.Register(OnAlerted);
        HP = MaxHP;
        currentSpeedRate = 1;
        RunAIAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }
    protected virtual void FixedUpdate()
    {
        CheckGround();
        Move();
        TryTalk();
    }
    protected virtual void Update()
    {
        detectionStatusRenderer.flipX = transform.localScale.x < 0;
    }
    protected virtual UniTask RunAIAsync(CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
    private void OnAlerted()
    {
        alerted = true;
    }
    private void CheckGround()
    {
        if (rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }
        Bounds bounds = col.bounds;
        Vector2 start = bounds.center + bounds.extents.y * Vector3.down;

        if (Physics2D.OverlapPoint(start, groundLayer) != null)
        {
            isGrounded = false;
            return;
        }
        RaycastHit2D hit = Physics2D.Raycast(
            start,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        isGrounded = hit.collider != null && hit.normal.y > 0.7f;
    }
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed * currentSpeedRate, rb.linearVelocity.y);
    }
    protected virtual void ChangeDirection(int direction)
    {
        moveDirection = direction;
        if (direction == 0)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
    protected void FlipByDirection(int direction)
    {
        var x = Mathf.Abs(transform.localScale.x);
        if (direction == -1) transform.localScale = new Vector3(-x, transform.localScale.y, transform.localScale.z);
        else if (direction == 1) transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }
    protected void ChangeMoveSpeed(float rate)
    {
        currentSpeedRate = rate;
    }
    protected bool TryJump(float jumpForce)
    {
        if (jumpForce < 0) return false;
        if (isGrounded)
        {
            Jump(jumpForce);
            return true;
        }
        return false;
    }
    private void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    protected virtual void OnDestroy()
    {
        if (commander != null) commander.Unregister(OnAlerted);
    }
    protected virtual void OnDisable()
    {
        if (commander != null) commander.Unregister(OnAlerted);
    }
    public void Hit()
    {
        Hit(1);
    }
    
    public virtual void Hit(int damage)
    {
        ApplyDamageAsync(damage).Forget();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            ApplyDamageAsync(0.5f).Forget();
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Pitfall"))
        {
            ApplyDamageAsync(0.3f * Time.deltaTime).Forget();
        }
    }
    protected bool DetectPlayer(float range, LayerMask sightMask)
    {
        if (Player == null) return false;
        float dist = Vector2.Distance(transform.position, Player.position);
        if (dist > range) return false;
        Vector2 dir = (Player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, sightMask);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;
        return false;
    }

    private int _detectionSeq = 0;
    private async UniTask UpdateDetectionStatusRenderer(CancellationToken token, Color color, Sprite status)
    {
        int myid = _detectionSeq++;
        
        detectionStatusRenderer.gameObject.SetActive(true);
        detectionStatusRenderer.color = color;

        detectionStatusRenderer.sprite = status;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        
            float a = 1f;
            Color c = detectionStatusRenderer.color;
            while (!token.IsCancellationRequested && a > 0)
            {
                a -= Time.deltaTime;
                c.a = a;
                detectionStatusRenderer.color = c;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
        
        }
        finally
        {
            if (myid == _detectionSeq) detectionStatusRenderer.gameObject.SetActive(false);
        }
    }

    private CancellationTokenSource _detectionStatusCTS;
    
    private async UniTaskVoid RunDetectionStatusAsync(Color color, Sprite status, CancellationTokenSource localCts)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            localCts.Token,
            this.GetCancellationTokenOnDestroy());

        try
        {
            await UpdateDetectionStatusRenderer(linked.Token, color, status);
        }
        catch (OperationCanceledException)
        {
            // 정상 취소
        }
    }

    protected void UpdateFound(float radius, LayerMask sightMask)
    {
        var tmp = found;
        found = DetectPlayer(radius, sightMask);

        if (detectionStatusRenderer == null) return;
        if (tmp && !found)
        {
            MissTarget();
        }

        if (!tmp && found)
        {
            FoundTarget();
        }
    }

    protected void MissTarget()
    {
        _detectionStatusCTS?.Cancel();
        _detectionStatusCTS?.Dispose();
        
        _detectionStatusCTS = new CancellationTokenSource();
        RunDetectionStatusAsync(Color.yellow, detectionStatusSprites[0], _detectionStatusCTS).Forget();
    }
    
    protected void FoundTarget()
    {
        _detectionStatusCTS?.Cancel();
        _detectionStatusCTS?.Dispose();
        
        _detectionStatusCTS = new CancellationTokenSource();
        RunDetectionStatusAsync(Color.red, detectionStatusSprites[1], _detectionStatusCTS).Forget();
    }
    protected bool DetectCliff(LayerMask wallLayer)
    {
        if (MoveDirection == 0) return true;
        Vector3 start = transform.position + Vector3.right * (MoveDirection * col.bounds.extents.x * 1.2f);
        RaycastHit2D hit = Physics2D.Raycast(start, Vector3.down, 1, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, Vector3.right * MoveDirection, col.bounds.extents.x * 1.2f, wallLayer);
        if (hit.collider == null || hit2)
        {
            return true;
        }
        return false;
    }
    protected void SetBaseColor(Color color)
    {
        baseColor = color;
        if (!isFlashing && spriteRenderer != null) spriteRenderer.color = baseColor;
    }
    protected async UniTask ApplyDamageAsync(float damage)
    {
        this.HP -= damage;
        if (HP <= 0)
        {
            Destroy(this.gameObject);
            return;
        }
        await FlashColorAsync(Color.red, 0.5f, this.GetCancellationTokenOnDestroy());
    }
    protected async UniTask FlashColorAsync(Color flashColor, float durationSeconds, CancellationToken token)
    {
        if (spriteRenderer == null) return;
        hitFlashCts?.Cancel();
        hitFlashCts?.Dispose();
        hitFlashCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var linkedToken = hitFlashCts.Token;
        int version = ++flashVersion;
        isFlashing = true;
        spriteRenderer.color = flashColor;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(durationSeconds), cancellationToken: linkedToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (flashVersion == version)
            {
                isFlashing = false;
                if (spriteRenderer != null) spriteRenderer.color = baseColor;
            }
        }
    }
}

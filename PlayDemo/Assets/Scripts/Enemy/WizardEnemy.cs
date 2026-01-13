using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class WizardEnemy : EnemyBase
{
    #region  parameters
    
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   // Player + Wall
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask wallLayer;

    [Header("Movement!")] 
    [SerializeField] private float directionTimeChange = 2f;
    [SerializeField] private float flipTimeChange = 2f;

    [Header("Combat")] 
    [SerializeField] private GameObject magicball;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float magicballSpeed = 2f;
    
    #endregion
    
    #region components

    private SpriteRenderer spriteRenderer;
    
    #endregion
    
    #region status
    
    private bool found = false;
    private bool automaticFlip = false;
    
    private float directionTimeChangeElapsed;
    private float flipTimeChangeElapsed;
    private float attackCooldownElapsed;
    
    public GameObject player;
    private Transform currentTarget;
    
    #endregion
    protected override void Start()
    {
        base.Start();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(WaitForAlert());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        if (!alerted)
        {
            alerted = DetectPlayer(detectionRadius);
        }
        else
        {
            if (automaticFlip)
            {
                if (player.transform.position.x > transform.position.x) Flip(1);
                else if (player.transform.position.x < transform.position.x) Flip(-1);
            }
            
            found = DetectPlayer(combatRadius); // 같은 플랫폼에 있는거로 바꾸기
        }
        
        if (attackCooldownElapsed > 0) attackCooldownElapsed -= Time.deltaTime;
    }

    private void Flip(int direction)
    {
        //TODO: 좀 이상한데 급하게 하느라 이렇게 됨... 이거 수정해야함

        var x = Mathf.Abs(transform.localScale.x);
        if (direction == -1) transform.localScale = new Vector3(-x, transform.localScale.y, transform.localScale.z);
        else if (direction == 1) transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }
    
    private IEnumerator WaitForAlert()
    {
        yield return new WaitUntil(() => alerted);
        
        ChangeDirection(0);
        //TEST
        spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
            
        currentState = State.Alert;
        automaticFlip = true;

        StartCoroutine(AlertedAction());
    }

    private IEnumerator AlertedAction()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (found == false)
            {
                currentTarget = null;
                
                yield return StartCoroutine(AttackRoutine());
            }
            else
            {
                currentTarget = player.transform;
                yield return StartCoroutine(AttackRoutine());
            }
        }
    }
    
    private bool DetectPlayer(float range)
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > range) return false;
        
        Vector2 dir = (player.transform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, sightMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        return false;
    }
    
    private IEnumerator AttackRoutine()
    {
        yield return new WaitUntil(() => attackCooldownElapsed <= 0);
        if (currentTarget == null) yield break;
        
        //TEST
        spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
        
        // flip!
        int dir;
        if (currentTarget.transform.position.x < transform.position.x) dir = -1;
        else dir = 1;
        automaticFlip = false;
        Flip(dir);
        
        // 대기
        yield return new WaitForSeconds(0.5f);
        
        MagicBall ball = Instantiate(magicball, transform.position, Quaternion.identity).GetComponent<MagicBall>();
        ball.Init(player, magicballSpeed);
        
        automaticFlip = true;

        attackCooldownElapsed = attackCooldown;
    }
}

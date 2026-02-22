using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Assertions.Must;

public class Boss1_LeftHand : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite graspSprite;
    [SerializeField] private Sprite bindSprite;

    [Header("Player References")]
    [SerializeField] private GameObject playerObject;
    private PlayerMovement2D playerMovement;
    private SwapSkill playerSwapSkill;
    private MeleeController2D playerMeleeController;


    [Header("Movement")]
    [SerializeField] private float graspMoveDuration = 0.7f;
    [SerializeField] private float graspHoldDuration = 0.7f;
    [SerializeField] private float returnMoveDuration = 0.7f;
    [SerializeField] private float returnHoldDuration = 0.7f;
    [Header("Grasp Reach")]
    [SerializeField] private float minReach = 3f;
    [SerializeField] private float maxReach = 7f;
    [SerializeField] private float reachPadding = 0.5f;

    private Boss1_Manage bossManage;
    private SpriteRenderer spriteRenderer;
    private Collider2D handCollider;

    private bool grasping = false;
    private Coroutine graspCoroutine;


    private void Start()
    {   
        bossManage = GetComponentInParent<Boss1_Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        handCollider = GetComponent<PolygonCollider2D>();

        if(playerObject == null)
        {
            playerObject = bossManage.playerObject;
        }
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerSwapSkill = playerObject.GetComponent<SwapSkill>();
        playerMeleeController = playerObject.GetComponent<MeleeController2D>();
    }


    public void BackToOrigin()
    {
        StartCoroutine(bossManage.ObjectMoveControlLocalPos(
            gameObject,
            transform.localPosition,
            bossManage.lHandOrigin,
            returnMoveDuration,
            returnHoldDuration
        ));  
    }


    private void Update()
    {
        if(bossManage.currentLeftHandPattern == Boss1_LHandPattern.Grasp)
        {
            if(Vector2.Distance(gameObject.transform.position, playerObject.transform.position) < 1f && !grasping)
            {

                grasping = true;
                StopCoroutine(graspCoroutine);
                spriteRenderer.sprite = graspSprite;
                spriteRenderer.sortingOrder = 4;
                StunPlayer(2.0f);
                StartCoroutine(StunningPlayerCoroutine(2.0f));
            }
        }
    }
    private IEnumerator StunningPlayerCoroutine(float time)
    {
        Vector2 startPosition = gameObject.transform.position;
        float timer = 0f;
        while(timer < time)
        {
            timer += Time.deltaTime;
            gameObject.transform.position = startPosition;
            playerObject.transform.position = startPosition;
            yield return null;
        }
        
        grasping = false;
        bossManage.lHandPatternTimer = 5f;
        bossManage.currentLeftHandPattern = Boss1_LHandPattern.Idle;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.sortingOrder = 2;
        BackToOrigin();
    }

    
    public void Boss1_Grasp()
    {
        float playerDistance = Vector2.Distance(gameObject.transform.position, playerObject.transform.position);
        Vector2 playerDirection = (playerObject.transform.position - transform.position).normalized;
        float reach = Mathf.Clamp(playerDistance + reachPadding, minReach, maxReach);
        Vector2 targetPos = (Vector2)transform.position + playerDirection * reach;
        

        graspCoroutine = StartCoroutine(bossManage.ObjectMoveControl(
            gameObject,
            transform.position,
            targetPos,
            graspMoveDuration,
            graspHoldDuration,
            GraspDone
        ));

        
    }

    private void StunPlayer(float time)
    {
        playerMovement.Stun(time);
        playerMovement.DashSilence(time);
        playerSwapSkill.SwapSilence(time);
        playerMeleeController.AttackSilence(time);
    }

    private void GraspDone(){
        if(!grasping){
            BackToOrigin();
            bossManage.SetPatternTimer("LHand");
            bossManage.currentLeftHandPattern = Boss1_LHandPattern.Idle;
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.sortingOrder = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HitBox>() != null) return;
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            bossManage.TakeDamage(Boss1_Part.LHand, 1);
        }
    }

    public IEnumerator Boss1_LHandHit()
    {
        // Left Hand Hit Animation
        Vector2 originalPos = transform.localPosition;
        float timer = 0f;

        spriteRenderer.color = Color.Lerp(Color.red, Color.white, 0.8f);
        while(timer < 0.3f)
        {
            timer += Time.deltaTime;
            transform.localPosition = originalPos + new Vector2(
                Mathf.Sin(timer * 20f) * 0.1f,
                Mathf.Cos(timer * 20f) * 0.1f
            );
            yield return null;
        }
        spriteRenderer.color = Color.white;
    }
    public IEnumerator Boss1_LHandDestroyed()
    {
        // Left Hand Destroyed Animation
        handCollider.enabled = false;
        Vector2 originalSize = transform.localScale;
        Vector2 originalPos = transform.localPosition;
        Color originalColor = spriteRenderer.color;
        float timer = 0f;

        while(timer < 1f)
        {
            if(timer < 0.3f)
            {
                transform.localPosition = originalPos + new Vector2(
                    Mathf.Sin(timer * 20f) * 0.1f,
                    Mathf.Cos(timer * 20f) * 0.1f
                );
            }
            timer += Time.deltaTime;
            transform.localScale = Vector2.Lerp(originalSize, originalSize*0.8f, timer/1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.clear, timer/1f);
            yield return null;
        }

        handCollider.enabled = true;
        gameObject.SetActive(false);
        yield return null;
    }

}

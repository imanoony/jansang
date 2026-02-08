using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Assertions.Must;

public class Boss1LeftHand : MonoBehaviour
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


    private Boss1Manage bossManage;
    private SpriteRenderer spriteRenderer;

    private bool grasping = false;
    private Coroutine graspCoroutine;


    private void Awake()
    {   
        bossManage = GetComponentInParent<Boss1Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        StartCoroutine(Boss1Manage.ObjectMoveControlLocalPos(
            gameObject,
            transform.localPosition,
            bossManage.lHandOrigin,
            0.5f,
            0.5f
        ));  
    }


    private void Update()
    {
        if(bossManage.currentLeftHandPattern == LHandPattern.Grasp)
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
        bossManage.currentLeftHandPattern = LHandPattern.Idle;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.sortingOrder = 2;
        BackToOrigin();
    }

    
    public void Boss1Grasp()
    {
        float playerDistance = Vector2.Distance(gameObject.transform.position, playerObject.transform.position);
        Vector2 playerDirection = (playerObject.transform.position - transform.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + playerDirection * Math.Max(Math.Min(playerDistance, 5f), 3f);
        

        graspCoroutine = StartCoroutine(Boss1Manage.ObjectMoveControl(
            gameObject,
            transform.position,
            targetPos,
            0.5f,
            0.5f,
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
            bossManage.currentLeftHandPattern = LHandPattern.Idle;
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.sortingOrder = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            bossManage.TakeDamage(BossPart.LHand, 1);
        }
    }

    public IEnumerator Boss1LHandHit()
    {
        // Left Hand Hit Animation
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    public IEnumerator Boss1LHandDestroyed()
    {
        // Left Hand Destroyed Animation
        gameObject.SetActive(false);
        yield return null;
    }

}

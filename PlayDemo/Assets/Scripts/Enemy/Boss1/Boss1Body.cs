using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Body : MonoBehaviour
{
    
    [Header("Shockwave")]
    [SerializeField] private GameObject shockwave;
    [SerializeField] private List<Sprite> shockwaveSprites;
    private Boss1Shockwave boss1Shockwave;
    private SpriteRenderer shockwaveRenderer;
    private Transform shockwaveTransform;

    [Header("Judgement")]
    [SerializeField] private float judgementWait = 0.75f;
    [SerializeField] private GameObject judgementLazer;
    private Transform judgementTransform;

    private Boss1Manage bossManage;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (shockwave == null) {
            shockwave = transform.Find("Shockwave").gameObject;
        }
        shockwaveRenderer = shockwave.GetComponent<SpriteRenderer>();
        shockwaveTransform = shockwave.GetComponent<Transform>();
        boss1Shockwave = shockwave.GetComponent<Boss1Shockwave>();
        shockwave.SetActive(false);

        if (judgementLazer == null) {
            judgementLazer = transform.Find("Judgement").gameObject;
        }
        judgementTransform = judgementLazer.GetComponent<Transform>();
        judgementLazer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Boss1Shockwave(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shockwaveTransform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        StartCoroutine(ShockwaveCoroutine());
    }

    private IEnumerator ShockwaveCoroutine()
    {
        shockwave.SetActive(true);
        
        shockwaveRenderer.sprite = shockwaveSprites[0];
        yield return new WaitForSeconds(0.5f);
        shockwaveRenderer.sprite = shockwaveSprites[1];
        boss1Shockwave.CheckHit();
        yield return new WaitForSeconds(0.5f);
        shockwave.SetActive(false);

        bossManage.currentBodyPattern = BodyPattern.Idle;
        bossManage.SetPatternTimer("Body");
    }

    public void Boss1Judgement(GameObject targetAltar)
    {
        StartCoroutine(JudgementCoroutine(targetAltar));
        
    }

    private IEnumerator JudgementCoroutine(GameObject targetAltar)
    {
        Vector2 startPosition = bossManage.gameObject.transform.position;
        yield return StartCoroutine(Boss1Manage.ObjectMoveControl(
            bossManage.gameObject,
            startPosition,
            targetAltar.transform.position,
            1f,
            6f
        ));
        
        float timer = 0f;

        yield return new WaitForSeconds(judgementWait);

        if(targetAltar.GetComponent<Boss1Altar>().active == false)
        {
            bossManage.currentBodyPattern = BodyPattern.Idle;
            bossManage.SetPatternTimer("Body");
            foreach(GameObject altar in bossManage.altarObjects)
            {
                altar.GetComponent<Boss1Altar>().ResetAltar();
            }
            yield break;
        }

        Vector2 direction = (bossManage.playerTransform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        judgementTransform.rotation = Quaternion.Euler(0f, 0f, angle + 45f);
        judgementLazer.SetActive(true);
        timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            judgementTransform.rotation = Quaternion.Euler(0f, 0f, angle + 45f + 90f * ((float)Math.Pow(timer/0.5f, 2) / 4f));
            yield return null;
        }
        timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            judgementTransform.rotation = Quaternion.Euler(0f, 0f, angle + 45f + 90f * (1f/4f + timer/1f));
            yield return null;
        }
        timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            judgementTransform.rotation = Quaternion.Euler(0f, 0f, angle + 45f + 90f * (1f - (float)Math.Pow((0.5f - timer)/0.5f, 2) / 4f));
            yield return null;
        }
        judgementLazer.SetActive(false);

        bossManage.currentBodyPattern = BodyPattern.Idle;
        bossManage.SetPatternTimer("Body");
        foreach(GameObject altar in bossManage.altarObjects)
        {
            altar.GetComponent<Boss1Altar>().ResetAltar();
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            bossManage.TakeDamage(BossPart.Body, 1);
        }
    }

    public IEnumerator Boss1BodyHit()
    {
        // Body Hit Animation
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    public IEnumerator Boss1BodyDestroyed()
    {
        // Body Destroyed Animation
        gameObject.SetActive(false);
        yield return null;
    }    

}

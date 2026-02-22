using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1_Body : MonoBehaviour
{
    
    [Header("Shockwave")]
    [SerializeField] private GameObject shockwave;
    [SerializeField] private List<Sprite> shockwaveSprites;
    private Boss1_Shockwave boss1_Shockwave;
    private SpriteRenderer shockwaveRenderer;
    private Transform shockwaveTransform;

    [Header("Judgement")]
    [SerializeField] private float judgementWait = 0.75f;
    [SerializeField] private GameObject judgementLazer;
    private Transform judgementTransform;

    private Boss1_Manage bossManage;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1_Manage>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (shockwave == null) {
            shockwave = transform.Find("Shockwave").gameObject;
        }
        shockwaveRenderer = shockwave.GetComponent<SpriteRenderer>();
        shockwaveTransform = shockwave.GetComponent<Transform>();
        boss1_Shockwave = shockwave.GetComponent<Boss1_Shockwave>();
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

    public void Boss1_Shockwave(Vector2 direction)
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
        boss1_Shockwave.CheckHit();
        yield return new WaitForSeconds(0.5f);
        shockwave.SetActive(false);

        bossManage.currentBodyPattern = Boss1_BodyPattern.Idle;
        bossManage.SetPatternTimer("Body");
    }

    public void Boss1_Judgement(GameObject targetAltar)
    {
        StartCoroutine(JudgementCoroutine(targetAltar));
        
    }

    private IEnumerator JudgementCoroutine(GameObject targetAltar)
    {
        Vector2 startPosition = bossManage.gameObject.transform.position;
        yield return StartCoroutine(bossManage.ObjectMoveControl(
            bossManage.gameObject,
            startPosition,
            targetAltar.transform.position,
            1f,
            6f
        ));
        
        if(bossManage.isInCutScene) yield break;

        float timer = 0f;

        yield return new WaitForSeconds(judgementWait);

        if(targetAltar.GetComponent<Boss1_Altar>().active == false)
        {
            bossManage.currentBodyPattern = Boss1_BodyPattern.Idle;
            bossManage.SetPatternTimer("Body");
            foreach(GameObject altar in bossManage.altarObjects)
            {
                altar.GetComponent<Boss1_Altar>().ResetAltar();
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

        bossManage.currentBodyPattern = Boss1_BodyPattern.Idle;
        bossManage.SetPatternTimer("Body");
        foreach(GameObject altar in bossManage.altarObjects)
        {
            altar.GetComponent<Boss1_Altar>().ResetAltar();
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HitBox>() != null) return;
        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            bossManage.TakeDamage(Boss1_Part.Body, 1);
        }
    }

    public IEnumerator Boss1_BodyHit()
    {
        // Body Hit Animation
        spriteRenderer.color = Color.Lerp(Color.red, Color.white, 0.8f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    public IEnumerator Boss1_BodyDestroyed()
    {
        yield return StartCoroutine(bossManage.Boss1_DestroyScene());
    }    

}

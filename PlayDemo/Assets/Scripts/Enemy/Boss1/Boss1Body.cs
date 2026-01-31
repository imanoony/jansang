using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Body : MonoBehaviour
{
    [SerializeField] private Boss1Manage bossManage;

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

    private void Awake()
    {
        if (bossManage == null) bossManage = GetComponentInParent<Boss1Manage>();
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
        bossManage.bodyPatternTimer = 4.0f;
    }

    public void Boss1Judgement(GameObject targetAltar)
    {
        StartCoroutine(JudgementCoroutine(targetAltar));
        
    }

    private IEnumerator JudgementCoroutine(GameObject targetAltar)
    {
        Vector2 startPosition = bossManage.gameObject.transform.position;
        float timer = 0f;
        while(timer < 1f)
        {
            timer += Time.deltaTime;
            bossManage.gameObject.transform.position = new Vector2(
                Mathf.Lerp(startPosition.x, targetAltar.transform.position.x, (float)Math.Pow(timer/1f, 2) / 10f),
                Mathf.Lerp(startPosition.y, targetAltar.transform.position.y, (float)Math.Pow(timer/1f, 2) / 10f)
            );
            yield return null;
        }
        
        timer = 0f;
        while(timer < 4f)
        {
            timer += Time.deltaTime;
            bossManage.gameObject.transform.position = new Vector2(
                Mathf.Lerp(startPosition.x, targetAltar.transform.position.x, 0.1f + (timer/5f)),
                Mathf.Lerp(startPosition.y, targetAltar.transform.position.y, 0.1f + (timer/5f))
            );
            yield return null;
        }
        
        timer = 0f;
        while(timer < 1f)
        {
            timer += Time.deltaTime;
            bossManage.gameObject.transform.position = new Vector2(
                Mathf.Lerp(startPosition.x, targetAltar.transform.position.x, 1f - (float)Math.Pow((1f-timer)/1f, 2) / 10f),
                Mathf.Lerp(startPosition.y, targetAltar.transform.position.y, 1f - (float)Math.Pow((1f-timer)/1f, 2) / 10f)
            );
            yield return null;
        }

        yield return new WaitForSeconds(judgementWait);

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
        bossManage.bodyPatternTimer = 4.0f;
        yield return null;
    }
}

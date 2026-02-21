using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Boss2_Counter : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Action bossAction;
    public Collider2D counterCol;

    [Header("Effects")]
    public GameObject parryEffect;
    public GameObject counterEffect;
    public GameObject explodeEffect;

    [Header("Parry Effect")]
    public float counterRadius = 0.45f;
    public float parryEffectDuration = 0.3f;

    private void Awake()
    {
        bossAction = GetComponentInParent<Boss2_Action>();
        bossManage = bossAction.GetComponentInParent<Boss2_Manage>();
        counterCol = GetComponent<PolygonCollider2D>();

        parryEffect = gameObject.transform.Find("Parry").gameObject;
        counterEffect = gameObject.transform.Find("Counter").gameObject;
        explodeEffect = gameObject.transform.Find("Explode").gameObject;

        parryEffect.SetActive(false);
    }

    private void Update()
    {
        if (bossAction.exploded)
        {
            parryEffect.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (!bossAction.exploded && ((1 << col.gameObject.layer) & bossManage.attackLayer.value) != 0)
        {
            Vector2 playerDir = (bossManage.playerTransform.position - transform.position).normalized;
            StartCoroutine(bossAction.KnockbackCoroutine(
                playerDir,
                bossAction.knockbackForce,
                bossAction.knockbackDuration / 5f
            ));  

            playerDir.x = bossAction.gameObject.transform.localScale.x < 0f ? -playerDir.x : playerDir.x;
            parryEffect.transform.localPosition = playerDir * counterRadius;
            StartCoroutine(ParryEffect());
        }

        if (bossAction.exploded && col.gameObject.CompareTag("Player"))
        {
            bossManage.playerHitCheck.TakeDamage(1);

            Vector2 playerDir = (bossManage.playerTransform.position - transform.position).normalized;
            StartCoroutine(bossAction.KnockbackCoroutine(
                playerDir,
                bossAction.knockbackForce * 2f,
                bossAction.knockbackDuration / 5f
            ));
        }
    }

    IEnumerator ParryEffect()
    {
        parryEffect.SetActive(true);
        yield return new WaitForSeconds(parryEffectDuration);
        parryEffect.SetActive(false);
    }
}

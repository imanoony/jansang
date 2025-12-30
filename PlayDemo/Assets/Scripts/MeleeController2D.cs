using System.Collections;
using UnityEngine;

public class MeleeController2D : MonoBehaviour
{
    [Header("Attack")]
    public float attackCooldown = 0.5f;
    public float reloadTime = 1.2f; // 연속 공격 후 딜레이
    bool isReloading;
    bool isAttacking;
    public bool isRight = true;
    public Animator anim;
    public HitBox hitBox;
    
    private CharacterManager manager;
    public void Init(CharacterManager manager)
    {
        this.manager = manager;
    }
    void Start()
    {
        anim.gameObject.SetActive(false);
        //hitBox.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (isReloading || isAttacking) return;
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        EnableHitBox();
        if (isRight)
        {
            anim.gameObject.transform.localPosition = new Vector2(1, 0);
            anim.gameObject.transform.localScale = new Vector3(2, 2, 1);
        }
        else
        {
            anim.gameObject.transform.localPosition = new Vector2(-1, 0);
            anim.gameObject.transform.localScale = new Vector3(-2, 2, 1);
        }
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        DisableHitBox();
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSecondsRealtime(reloadTime);
        isReloading = false;
    }

    // 애니메이션 이벤트에서 호출
    public void EnableHitBox()
    {
        hitBox.gameObject.SetActive(true);
    }

    public void DisableHitBox()
    {
        hitBox.gameObject.SetActive(false);
    }
}
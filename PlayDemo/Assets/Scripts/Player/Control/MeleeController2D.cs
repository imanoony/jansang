using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeController2D : MonoBehaviour
{
    [Header("Attack")]
    public float attackCooldown = 0.5f;
    public float reloadTime = 1.2f; // 연속 공격 후 딜레이
    bool isReloading;
    bool isAttacking;
    public bool isRight = true;
    public Animator anim;
    public Transform shootPos;
    public HitBox hitBox;

    void Start()
    {
        anim.gameObject.SetActive(false);
        //hitBox.enabled = false;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
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

        // === 마우스 방향으로 회전 ===
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mouseWorld - shootPos.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        shootPos.rotation = Quaternion.Euler(0f, 0f, angle);

        EnableHitBox();
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
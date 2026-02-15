using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Boss2_Action : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Collision bossCollision;
    public Boss2_Slash bossSlash;
    public Boss2_Counter bossCounter;
    public Boss2_Laser bossLaser;
    public SpriteRenderer bossSR;

    [Header("Dash")]
    public bool dashing = false;
    public float dashSpeed = 20f;
    public float dashDistance = 5f;
    public float knockbackForce = 10f;
    public float knockbackDuration = 1f;

    [Header("Counter")]
    public float counterDuration = 3f;
    public bool exploded = false;

    [Header("Laser")]
    public float laserDuration = 0.4f;

    void Awake()
    {
        bossManage = GetComponentInParent<Boss2_Manage>();
        bossCollision = GetComponentInChildren<Boss2_Collision>(includeInactive: true);
        bossSlash = GetComponentInChildren<Boss2_Slash>(includeInactive: true);
        bossCounter = GetComponentInChildren<Boss2_Counter>(includeInactive: true);
        bossLaser = GetComponentInChildren<Boss2_Laser>(includeInactive: true);
        
        bossSR = GetComponent<SpriteRenderer>();

        bossSlash.gameObject.SetActive(false);
        bossCounter.gameObject.SetActive(false);
        bossLaser.gameObject.SetActive(false);
    }

    public IEnumerator Boss2_Charge<T>(float chargeTime, bool isRight, System.Func<T, IEnumerator> skill, T parameter)
    {
        bossSR.flipX = !isRight;

        // Charge Animation
        Color color = Color.blue;
        bossSR.color = color * 0.5f;
        yield return new WaitForSeconds(chargeTime);
        bossSR.color = color;

        StartCoroutine(skill(parameter));
    }

    public IEnumerator Boss2_Charge(float chargeTime, bool isRight, System.Func<IEnumerator> skill)
    {
        bossSR.flipX = !isRight;

        // Charge Animation
        Color color = Color.blue;
        bossSR.color = color * 0.5f;
        yield return new WaitForSeconds(chargeTime);
        bossSR.color = color;

        StartCoroutine(skill());
    }

#region Dash
    public IEnumerator Boss2_DashAction(bool isRight)
    {
        Vector2 dashDirection = isRight ? Vector2.right : Vector2.left;
        Vector2 startPos = gameObject.transform.position;

        dashing = true;
        while(Vector2.Distance(startPos, gameObject.transform.position) < dashDistance)
        {
            bossManage.bossRB.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }
        bossManage.bossRB.linearVelocity = Vector2.zero;
        dashing = false;

        bossManage.patternTimer = bossManage.patternCooldown;
        bossManage.currentPattern = Boss2_Pattern.Idle;
    }

    public void DashKnockBack(Collider2D col)
    {
        bossManage.playerHitCheck.TakeDamage(1);

        Vector2 knockbackDirection = (col.transform.position - transform.position).normalized;
        StartCoroutine(KnockbackCoroutine(knockbackDirection, knockbackForce, knockbackDuration));
    }

    public IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration)
    {
        bossManage.playerMovement.Stun(duration);

        float timer = 0f;
        while (timer < duration)
        {
            bossManage.playerRigidbody.linearVelocity = direction * force;
            timer += Time.deltaTime;
            yield return null;
        }
    }
#endregion

#region Slash
    public IEnumerator Boss2_SlashAction(bool isRight)
    {
        bossSlash.FlipSlash(isRight);
        bossSlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        bossSlash.gameObject.SetActive(false);

        bossManage.patternTimer = bossManage.patternCooldown;
        bossManage.currentPattern = Boss2_Pattern.Idle;
    }
#endregion

#region Counter
    public IEnumerator Boss2_CounterAction()
    {
        bossCounter.gameObject.SetActive(true);
        bossCounter.counterEffect.SetActive(true);

        exploded = false;
        bossCounter.gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(counterDuration);

        bossCounter.counterEffect.SetActive(false);
        bossCounter.explodeEffect.SetActive(true);

        exploded = true;
        bossCounter.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        yield return new WaitForSeconds(0.2f);

        bossCounter.explodeEffect.SetActive(false);
        bossCounter.gameObject.SetActive(false);


        bossManage.patternTimer = bossManage.patternCooldown;
        bossManage.currentPattern = Boss2_Pattern.Idle;

        yield return null;
    }
#endregion

#region Laser
    public IEnumerator Boss2_LaserAction(float angle)
    {
        bossLaser.gameObject.SetActive(true);
        bossLaser.SetRotation(angle);
        yield return new WaitForSeconds(laserDuration);
        bossLaser.gameObject.SetActive(false);


        bossManage.patternTimer = bossManage.patternCooldown;
        bossManage.currentPattern = Boss2_Pattern.Idle;
    }
#endregion


}

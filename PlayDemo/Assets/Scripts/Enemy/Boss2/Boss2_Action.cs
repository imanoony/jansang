using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Boss2_Action : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Boss2_Manage bossManage;
    private SpriteRenderer bossSR;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDistance = 5f;

    void Awake()
    {
        bossManage = GetComponentInParent<Boss2_Manage>();
        bossSR = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Boss2_Charge<T>(float chargeTime, System.Func<T, IEnumerator> skill, T parameter)
    {
        // Charge Animation
        Color color = Color.blue;
        bossSR.color = color * 0.5f;
        yield return new WaitForSeconds(chargeTime);
        bossSR.color = color;

        StartCoroutine(skill(parameter));
    }

    public IEnumerator Boss2_DashAction(bool isRight)
    {
        Vector2 dashDirection = isRight ? Vector2.right : Vector2.left;
        Vector2 startPos = gameObject.transform.position;

        while(Vector2.Distance(startPos, gameObject.transform.position) < dashDistance)
        {
            bossManage.bossRB.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }
        bossManage.bossRB.linearVelocity = Vector2.zero;

        bossManage.patternTimer = bossManage.patternCooldown;
        bossManage.currentPattern = Boss2_Pattern.Idle;
    }

}

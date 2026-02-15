using UnityEngine;

public class Boss2_Slash : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Action bossAction;
    public SpriteRenderer slashSR;

    public bool hitPlayer = false;

    private void OnEnable()
    {
        bossAction = GetComponentInParent<Boss2_Action>();
        bossManage = bossAction.bossManage;
        slashSR = GetComponent<SpriteRenderer>();

        hitPlayer = false;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Player") && !hitPlayer)
        {
            bossManage.playerHitCheck.TakeDamage(1);
            hitPlayer = true;
        }
    }

    public void FlipSlash(bool isRight)
    {
        if(bossAction == null || bossManage == null || slashSR == null) { OnEnable(); }

        transform.localPosition = new Vector2(isRight ? 0.1f : -0.1f, 0.1f);
        slashSR.flipX = !isRight;
    }
}

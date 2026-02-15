using UnityEngine;

public class Boss2_Collision : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Action bossAction;

    private void OnEnable()
    {
        bossAction = GetComponentInParent<Boss2_Action>();
        bossManage = bossAction.bossManage;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(bossAction.dashing && col.gameObject.CompareTag("Player"))
        {
            bossAction.DashKnockBack(col);
        }
    }
}

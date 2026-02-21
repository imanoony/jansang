using UnityEngine;

public class Boss2_Collision : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Action bossAction;

    private void Awake()
    {
        bossAction = GetComponentInParent<Boss2_Action>();
        bossManage = bossAction.GetComponentInParent<Boss2_Manage>();
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(bossAction.dashing && col.gameObject.CompareTag("Player"))
        {
            bossAction.DashKnockBack(col);
            return;
        }

        if(bossManage.currentPattern != Boss2_Pattern.Counter && 
        ((1 << col.gameObject.layer) & bossManage.attackLayer.value) != 0)
        {
            bossManage.TakeDamage(1);
        }
    }
}

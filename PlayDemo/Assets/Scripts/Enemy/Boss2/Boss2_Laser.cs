using UnityEngine;

public class Boss2_Laser : MonoBehaviour
{
    [Header("References")]
    public Boss2_Manage bossManage;
    public Boss2_Action bossAction;

    private void Awake()
    {
        bossAction = GetComponentInParent<Boss2_Action>();
        bossManage = bossAction.GetComponentInParent<Boss2_Manage>();
    }

    public void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            bossManage.playerHitCheck.TakeDamage(1);
        }
    }
}

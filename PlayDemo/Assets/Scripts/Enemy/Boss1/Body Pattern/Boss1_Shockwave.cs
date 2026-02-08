using UnityEngine;


public class Boss1_Shockwave : MonoBehaviour
{
    [SerializeField] private Collider2D myCollider;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Boss1_Manage bossManage;
    private PlayerMovement2D playerMovement;
    private SwapSkill playerSwapSkill;

    private void Awake()
    {
        bossManage = GetComponentInParent<Boss1_Manage>();
        playerMovement = bossManage.playerObject.GetComponent<PlayerMovement2D>();
        playerSwapSkill = bossManage.playerObject.GetComponent<SwapSkill>();
        myCollider = GetComponent<PolygonCollider2D>();
    }

    public void CheckHit()
    {
        Collider2D[] results = new Collider2D[1];
        int hit = Physics2D.OverlapCollider(
            myCollider,
            new ContactFilter2D { layerMask = targetLayer, useLayerMask = true },
            results
        );

        if (hit > 0)
        {
            playerMovement.DashSilence(4.0f);
            playerSwapSkill.SwapSilence(4.0f);
        }
    }
}

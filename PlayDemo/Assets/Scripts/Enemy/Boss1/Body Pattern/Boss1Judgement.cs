using UnityEngine;

public class Boss1Judgement : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool hitPlayer = false;
    
    
    private Boss1Manage bossManage;
    private Collider2D myCollider;
    
    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        bossManage = GetComponentInParent<Boss1Manage>();
    }

    private void OnEnable()
    {
        hitPlayer = false;

        Collider2D[] results = new Collider2D[1];
        int hit = Physics2D.OverlapCollider(
            myCollider,
            new ContactFilter2D { layerMask = playerLayer, useLayerMask = true },
            results
        );

        if(hit > 0) { hitPlayer = true; bossManage.playerHitCheck.TakeDamage(1); }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && !hitPlayer)
        {
            hitPlayer = true;
            bossManage.playerHitCheck.TakeDamage(1);
        }
    }
}

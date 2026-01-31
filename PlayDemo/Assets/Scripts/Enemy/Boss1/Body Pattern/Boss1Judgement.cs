using UnityEngine;

public class Boss1Judgement : MonoBehaviour
{
    [SerializeField] private Collider2D myCollider;
    [SerializeField] private LayerMask targetLayer;

    private void Awake()
    {
        myCollider = GetComponent<PolygonCollider2D>();
    }

    private void OnEnable()
    {
        Collider2D[] results = new Collider2D[1];
        int hit = Physics2D.OverlapCollider(
            myCollider,
            new ContactFilter2D { layerMask = targetLayer, useLayerMask = true },
            results
        );
    }
}

using UnityEngine;

public class Bullet : MonoBehaviour
{
    Collider2D bulletCol;
    Collider2D ownerCol;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
    /*
    public void Init(GameObject owner)
    {
        bulletCol = GetComponent<Collider2D>();
        ownerCol = owner.GetComponent<Collider2D>();

        if (bulletCol && ownerCol)
            Physics2D.IgnoreCollision(bulletCol, ownerCol, true);

        Invoke(nameof(EnableCollision), 0.1f);
    }
    void EnableCollision()
    {
        if (bulletCol && ownerCol)
            Physics2D.IgnoreCollision(bulletCol, ownerCol, false);
    }
    */
}

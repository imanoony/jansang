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
}


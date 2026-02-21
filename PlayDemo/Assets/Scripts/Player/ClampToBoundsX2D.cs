using UnityEngine;

public class ClampToBoundsX2D : MonoBehaviour
{
    [Header("Bounds")]
    public Collider2D boundsCollider;
    public Vector2 minBound;
    public Vector2 maxBound;

    [Header("Target (Optional)")]
    public Rigidbody2D targetRigidbody;
    public Collider2D targetCollider;
    public bool useColliderExtents = true;

    private void Awake()
    {
        if (targetRigidbody == null) targetRigidbody = GetComponent<Rigidbody2D>();
        if (targetCollider == null) targetCollider = GetComponentInChildren<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (targetRigidbody == null) return;
        Vector2 pos = targetRigidbody.position;
        float clampedX = ClampX(pos.x);
        if (!Mathf.Approximately(pos.x, clampedX))
        {
            targetRigidbody.MovePosition(new Vector2(clampedX, pos.y));
        }
    }


    private float ClampX(float x)
    {
        if (!TryGetBounds(out float min, out float max)) return x;

        float extent = GetExtentX();
        float minX = min + extent;
        float maxX = max - extent;

        if (minX > maxX) return (min + max) * 0.5f;
        return Mathf.Clamp(x, minX, maxX);
    }

    private float GetExtentX()
    {
        if (!useColliderExtents || targetCollider == null) return 0f;
        return targetCollider.bounds.extents.x;
    }

    private bool TryGetBounds(out float min, out float max)
    {
        if (boundsCollider != null)
        {
            Bounds b = boundsCollider.bounds;
            min = b.min.x;
            max = b.max.x;
            return true;
        }

        min = minBound.x;
        max = maxBound.x;
        return true;
    }
}

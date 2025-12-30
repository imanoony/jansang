using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    Camera cam;

    [Header("Move")]
    public float moveLerpSpeed = 10f;
    public Transform target;
    [Header("Zoom")]
    public float zoomLerpSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 5f;
    [Header("Limit")]
    public Vector2 minBound;
    public Vector2 maxBound;
    Vector3 targetPosition;
    float targetZoom;
    private bool isFollowing;
    Vector3 lastMouseWorld;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;

        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
        isFollowing = false;
    }

    void Update()
    {
        if (isFollowing)
        {
            targetPosition = target.position;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isFollowing)
            {
                isFollowing = false;
                targetPosition = Vector3.zero;
                targetZoom = 5;
            }
            else
            {
                isFollowing = true;
                targetZoom = 2;
            }
        }

        ClampTargetPosition(); // ⭐ 범위 제한

        // 실제 카메라 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * moveLerpSpeed
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomLerpSpeed
        );
    }


    // ======================
    // 📦 카메라 범위 제한
    // ======================
    void ClampTargetPosition()
    {
        float vertExtent = targetZoom;
        float horzExtent = targetZoom * cam.aspect;

        float minX = minBound.x + horzExtent;
        float maxX = maxBound.x - horzExtent;
        float minY = minBound.y + vertExtent;
        float maxY = maxBound.y - vertExtent;

        // 맵보다 카메라가 큰 경우 → 중앙 고정
        if (minX > maxX)
            targetPosition.x = (minBound.x + maxBound.x) * 0.5f;
        else
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

        if (minY > maxY)
            targetPosition.y = (minBound.y + maxBound.y) * 0.5f;
        else
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        targetPosition.z = transform.position.z;
    }
    // ======================
    // 🟨 Bound 시각화 (선택)
    // ======================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            (minBound + maxBound) * 0.5f,
            maxBound - minBound
        );
    }
}

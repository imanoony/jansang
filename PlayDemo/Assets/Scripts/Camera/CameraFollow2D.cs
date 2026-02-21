using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    Camera cam;

    [Header("Target")]
    public Transform targetRoot;
    private Transform target;

    [Header("Move")]
    public float followLerpSpeed = 10f;
    public float mouseInfluence = 3f;

    [Header("Zoom")]
    public float baseZoom = 6f;
    public float extraZoom = 2f;
    public float zoomLerpSpeed = 5f;

    bool isTransforming = true; // 변환 상태

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        target = targetRoot.GetChild(0);
    }

    void LateUpdate()
    {
        if (!target) return;

        // === 마우스 오프셋 계산 ===
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 offset = mouseWorld - target.position;
        offset.z = 0;
        offset = Vector3.ClampMagnitude(offset, mouseInfluence);

        // === 카메라 위치 ===
        Vector3 desiredPos = target.position + offset;
        desiredPos.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * followLerpSpeed
        );

        // === 줌 ===
        float targetZoom = baseZoom + (isTransforming ? extraZoom : 0f);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomLerpSpeed
        );
    }

    // 외부에서 호출
    public void SetTransformMode(bool active)
    {
        isTransforming = active;
    }

    public void SetTargetRoot(Transform newRoot)
    {
        targetRoot = newRoot;
        target = targetRoot.GetChild(0);
    }
}
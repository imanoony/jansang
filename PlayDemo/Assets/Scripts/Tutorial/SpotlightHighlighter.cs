using UnityEngine;
using UnityEngine.UI;

public class SpotlightHighlighter : MonoBehaviour
{
    [Header("Overlay root (setActive on/off)")]
    [SerializeField] private GameObject overlayRoot;   // 전체 오버레이 (Image 포함)
    [SerializeField] private Image overlayImage;        // 머티리얼 붙은 Image
    [SerializeField] private RectTransform overlayRect; // overlayImage.rectTransform

    [Header("Cameras")]
    [Tooltip("Canvas가 Screen Space - Overlay면 null 가능. Screen Space - Camera/World면 해당 UI 카메라.")]
    [SerializeField] private Camera uiCamera;

    [Tooltip("월드 오브젝트를 ScreenPoint로 바꾸는 카메라(보통 MainCamera).")]
    [SerializeField] private Camera worldCamera;

    // 머티리얼(인스턴스)
    private Material _mat;

    // follow 용 타겟
    private Transform _worldTarget;
    private RectTransform _uiTarget;
    private bool _follow;

    // 현재 파라미터(픽셀 단위 입력)
    private float _radiusPx;
    private float _softnessPx;

    // shader property ids
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SoftId   = Shader.PropertyToID("_Softness");

    private void Awake()
    {
        if (overlayImage == null) overlayImage = GetComponentInChildren<Image>(true);
        if (overlayRect == null && overlayImage != null) overlayRect = overlayImage.rectTransform;
        if (overlayRoot == null && overlayImage != null) overlayRoot = overlayImage.gameObject;

        if (worldCamera == null) worldCamera = Camera.main;

        // 공유 머티리얼 오염 방지용 인스턴스
        if (overlayImage != null && overlayImage.material != null)
        {
            _mat = Instantiate(overlayImage.material);
            overlayImage.material = _mat;
        }

        // 시작은 꺼둔다고 가정
        if (overlayRoot != null) overlayRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!_follow) return;

        if (_worldTarget != null)
        {
            UpdateByWorldTarget(_worldTarget, _radiusPx, _softnessPx);
        }
        else if (_uiTarget != null)
        {
            UpdateByUiTarget(_uiTarget, _radiusPx, _softnessPx);
        }
    }

    // =========================
    // Public API
    // =========================

    /// <summary>월드 오브젝트 기준 하이라이트. follow=true면 계속 따라감.</summary>
    public void HighlightWorld(Transform target, float radiusPixels, float softnessPixels = 20f, bool follow = true)
    {
        _worldTarget = target;
        _uiTarget = null;
        _follow = follow;

        _radiusPx = radiusPixels;
        _softnessPx = softnessPixels;

        if (overlayRoot != null) overlayRoot.SetActive(true);
        UpdateByWorldTarget(target, radiusPixels, softnessPixels);
    }

    /// <summary>UI(RectTransform) 기준 하이라이트. follow=true면 계속 따라감.</summary>
    public void HighlightUI(RectTransform target, float radiusPixels, float softnessPixels = 20f, bool follow = true)
    {
        _uiTarget = target;
        _worldTarget = null;
        _follow = follow;

        _radiusPx = radiusPixels;
        _softnessPx = softnessPixels;

        if (overlayRoot != null) overlayRoot.SetActive(true);
        UpdateByUiTarget(target, radiusPixels, softnessPixels);
    }

    /// <summary>이미 화면 좌표를 알고 있으면 그 점 기준으로 하이라이트. follow는 의미 없음(정적 1회 세팅).</summary>
    public void HighlightScreenPoint(Vector2 screenPoint, float radiusPixels, float softnessPixels = 20f)
    {
        _worldTarget = null;
        _uiTarget = null;
        _follow = false;

        _radiusPx = radiusPixels;
        _softnessPx = softnessPixels;

        if (overlayRoot != null) overlayRoot.SetActive(true);
        UpdateByScreenPoint(screenPoint, radiusPixels, softnessPixels);
    }

    public void Hide()
    {
        _follow = false;
        _worldTarget = null;
        _uiTarget = null;

        if (overlayRoot != null) overlayRoot.SetActive(false);
    }

    // =========================
    // Internal updates
    // =========================

    private void UpdateByWorldTarget(Transform target, float radiusPixels, float softnessPixels)
    {
        if (target == null || worldCamera == null) return;

        Vector3 sp3 = worldCamera.WorldToScreenPoint(target.position);
        if (sp3.z <= 0f) return; // 카메라 뒤

        UpdateByScreenPoint((Vector2)sp3, radiusPixels, softnessPixels);
    }

    private void UpdateByUiTarget(RectTransform target, float radiusPixels, float softnessPixels)
    {
        if (target == null) return;

        Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
        UpdateByScreenPoint(sp, radiusPixels, softnessPixels);
    }

    private void UpdateByScreenPoint(Vector2 screenPoint, float radiusPixels, float softnessPixels)
    {
        if (_mat == null || overlayRect == null) return;

        
        if (!TryScreenPointToOverlayUV(screenPoint, out Vector2 uv))
            return;
        
        // 픽셀 -> UV 변환 (원형 유지 위해 min 사용)
        float minSize = Mathf.Min(overlayRect.rect.width, overlayRect.rect.height);
        if (minSize <= 1e-3f) return;

        float radiusUV = radiusPixels / minSize;
        float softUV   = softnessPixels / minSize;

        _mat.SetVector(CenterId, new Vector4(uv.x, uv.y, 0, 0));
        _mat.SetFloat(RadiusId, radiusUV);
        _mat.SetFloat(SoftId, softUV);
    }

    private bool TryScreenPointToOverlayUV(Vector2 screenPt, out Vector2 uv)
    {
        // overlayRect 로컬 좌표로 변환
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect, screenPt, uiCamera, out Vector2 local))
        {
            uv = default;
            return false;
        }

        // local -> UV(0~1)
        Rect r = overlayRect.rect;
        float u = (local.x - r.xMin) / r.width;
        float v = (local.y - r.yMin) / r.height;

        uv = new Vector2(u, v);
        return true;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwapSkill : MonoBehaviour
{
    public LayerMask targetLayer;
    public LayerMask wallLayer;
    [SerializeField] private BulletTimeController bulletTimeController;
    [Header("Swap Hittable")]
    [SerializeField] private string enemyHittableLayerName = "enemyHittable";
    [SerializeField] private float enemyHittableDuration = 1f;
    private List<SwapTarget> visibleTargets = new List<SwapTarget>();
    private Camera cam;
    private bool skillActive = false;
    private CharacterManager manager;
    private Rigidbody2D rb;
    private readonly Dictionary<GameObject, int> layerSwapVersion = new Dictionary<GameObject, int>();

    [Header("Silenced")]
    [SerializeField] private bool silenced = false;
    public void SwapSilence(float time)
    {
        silenced = true;
        if (skillActive) DeactivateSkill();

        StartCoroutine(SilenceTimer(time));
    }

    private IEnumerator SilenceTimer(float time)
    {
        yield return new WaitForSeconds(time);
        silenced = false;
    }
    


    void Start()
    {
        cam = Camera.main;
        manager = GameManager.Instance.Char;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (silenced) return;

        if (skillActive)
        {
            UpdateTargets();
            if (!bulletTimeController.Use())
            {
                skillActive = false;
                DeactivateSkill();
            }
        }
    }

    public void TrySkillByKey(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TryUseSkill();
        }
    }
    public void TryUseSkill()
    {
        if (skillActive)
        {
            DeactivateSkill();
        }
        else if (manager.CheckGauge(0))
        {
            ActivateSkill();
        }
    }
    public void OnTimeSlow(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (manager.CheckGauge(0))
            {
                if(silenced) return;
                ActivateSkill();
            }
        }
        

        if (ctx.canceled)
        {
            CheckClick();
            DeactivateSkill();
        }
    }


    // =============================
    void ActivateSkill()
    {
        skillActive = true;
        bulletTimeController.EnterBulletTime();
    }

    void DeactivateSkill()
    {
        skillActive = false;
        bulletTimeController.ExitBulletTime();
        foreach (var t in visibleTargets)
            t.SetHighlight(false);
        visibleTargets.Clear();
    }

    // =============================
    void UpdateTargets()
    {
        SwapTarget[] allTargets = FindObjectsByType<SwapTarget>(FindObjectsSortMode.None);
        foreach (var t in allTargets)
        {
            if (t.gameObject == gameObject) continue;
            if (!t.enabled) continue;
            bool visible = HasLineOfSight(transform.position, t.transform.position);
            // [comment]
            // 아래 HasLineOfSight() 에서 적은대로 땅에 의한 막힘을 생각해서
            // 일단 벽에 막혀 있어도 스왑 가능하도록 한 것.
            visible = true; // for check
            if (!manager.CheckGauge(Vector2.SqrMagnitude(transform.position - t.transform.position)))
            {
                visible = false;
            }
            if (visible && !visibleTargets.Contains(t))
            {
                visibleTargets.Add(t);
                t.SetHighlight(true, transform, t.transform, manager.Try, manager.CancelTry);
            }
            else if (!visible && visibleTargets.Contains(t))
            {
                visibleTargets.Remove(t);
                t.SetHighlight(false);
            }
        }
    }

    // =============================
    void CheckClick()
    {
        if (skillActive == false) return;
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, targetLayer);
        
        if (hit == null) return;

        SwapTarget target = hit.GetComponent<SwapTarget>();
        if (target == null) return;
        if(!target.enabled) return;
        if (!visibleTargets.Contains(target)) return;

        manager.UseGauge(Vector2.SqrMagnitude(transform.position - target.transform.position));
        SwapPosition(target.transform);
    }

    // =============================
    void SwapPosition(Transform target)
    {
        (transform.position, target.position) = (target.position, transform.position);
        rb.linearVelocityY = 0;
        GameManager.Instance.Char.CancelTry();
        TryApplyEnemyHittable(target);
    }

    // =============================
    // [comment]
    // 벽을 넘는 swap도 가능해야 하지 않나?
    // 그리고 '땅'도 벽으로 처리되어서, 플레이어가 보기에는 뚫려 있는데 (큰 문제 없이 도달 가능한데)
    // '땅'에 의해 일직선 거리는 막혀 있어 swap 대상으로 선택되지 못하는 경우가 있는 듯 하다.
    bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            from,
            (to - from).normalized,
            Vector2.Distance(from, to),
            wallLayer
        );

        return hit.collider == null;
    }

    private void TryApplyEnemyHittable(Transform target)
    {
        if (target == null) return;
        if (target.GetComponent<EnemyBase>() == null) return;

        int hittableLayer = LayerMask.NameToLayer(enemyHittableLayerName);
        if (hittableLayer < 0)
        {
            Debug.LogWarning($"SwapSkill: layer '{enemyHittableLayerName}' not found.");
            return;
        }

        ApplyTemporaryLayer(target.gameObject, hittableLayer, enemyHittableDuration);
    }

    private void ApplyTemporaryLayer(GameObject root, int layer, float duration)
    {
        if (root == null) return;
        if (duration <= 0f)
        {
            return;
        }

        int version = 0;
        if (layerSwapVersion.TryGetValue(root, out int current))
        {
            version = current + 1;
            layerSwapVersion[root] = version;
        }
        else
        {
            version = 1;
            layerSwapVersion.Add(root, version);
        }

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        int[] originalLayers = new int[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            originalLayers[i] = transforms[i].gameObject.layer;
            transforms[i].gameObject.layer = layer;
        }

        StartCoroutine(RestoreLayerAfter(root, transforms, originalLayers, version, duration));
    }

    private IEnumerator RestoreLayerAfter(
        GameObject root,
        Transform[] transforms,
        int[] originalLayers,
        int version,
        float duration
    )
    {
        yield return new WaitForSeconds(duration);

        if (root == null) yield break;
        if (!layerSwapVersion.TryGetValue(root, out int current) || current != version) yield break;

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] == null) continue;
            transforms[i].gameObject.layer = originalLayers[i];
        }
    }

    private void SetLayerRecursive(GameObject root, int layer)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layer;
        }
    }
}

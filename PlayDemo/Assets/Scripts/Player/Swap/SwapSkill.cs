using System.Collections.Generic;
using UnityEngine;

public class SwapSkill : MonoBehaviour
{
    
    public KeyCode skillKey = KeyCode.E;
    public LayerMask targetLayer;
    public LayerMask wallLayer;

    private List<SwapTarget> visibleTargets = new List<SwapTarget>();
    private Camera cam;
    private bool skillActive = false;
    private CharacterManager manager;
    public void Init(CharacterManager manager)
    {
        this.manager = manager;
    }
    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && manager.CheckGauge(0))
        {
            ActivateSkill();
        }

        if (Input.GetMouseButton(1))
        {
            UpdateTargets();
        }

        if (Input.GetMouseButtonUp(1))
        {
            CheckClick();
            DeactivateSkill();
        }
    }

    // =============================
    void ActivateSkill()
    {
        skillActive = true;
    }

    void DeactivateSkill()
    {
        skillActive = false;

        foreach (var t in visibleTargets)
            t.SetHighlight(false);

        visibleTargets.Clear();
    }

    // =============================
    void UpdateTargets()
    {
        SwapTarget[] allTargets = FindObjectsOfType<SwapTarget>();
        foreach (var t in allTargets)
        {
            if (t.gameObject == gameObject) continue;
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
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, targetLayer);
        
        if (hit == null) return;

        SwapTarget target = hit.GetComponent<SwapTarget>();
        if (target == null) return;

        if (!visibleTargets.Contains(target)) return;

        manager.UseGauge(Vector2.SqrMagnitude(transform.position - target.transform.position));
        SwapPosition(target.transform);
        DeactivateSkill();
    }

    // =============================
    void SwapPosition(Transform target)
    {
        Vector3 temp = transform.position;
        transform.position = target.position;
        target.position = temp;
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
}

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
            if (!manager.CheckGauge(Vector2.SqrMagnitude(transform.position - t.transform.position)))
            {
                visible = false;
            }
            if (visible && !visibleTargets.Contains(t))
            {
                visibleTargets.Add(t);
                t.SetHighlight(true);
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
        
        print(hit);
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

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapPlatformIndex : MonoBehaviour
{
    public static TilemapPlatformIndex Instance;
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("Raycast")]
    public LayerMask groundMask;          // TilemapCollider2D가 있는 레이어
    public float rayDistance = 1.5f;      // 발 아래로 쏘는 길이(캐릭터 크기에 맞게)
    public Vector2 rayOffset = Vector2.zero; // 발 위치 보정(필요시)

    // cell -> platformId
    private readonly Dictionary<Vector3Int, int> cellToPlatform = new();

    static readonly Vector3Int[] Dirs =
    {
        new(1,0,0), new(-1,0,0), new(0,1,0), new(0,-1,0)
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        BuildIndex();
    }

    [ContextMenu("Rebuild Platform Index")]
    public void BuildIndex()
    {
        cellToPlatform.Clear();
        int platformId = 0;

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;
            if (cellToPlatform.ContainsKey(pos)) continue;

            // BFS 라벨링
            var q = new Queue<Vector3Int>();
            q.Enqueue(pos);
            cellToPlatform[pos] = platformId;

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                foreach (var d in Dirs)
                {
                    var next = cur + d;
                    if (!tilemap.HasTile(next)) continue;
                    if (cellToPlatform.ContainsKey(next)) continue;

                    cellToPlatform[next] = platformId;
                    q.Enqueue(next);
                }
            }

            platformId++;
        }

        Debug.Log($"[TilemapPlatformIndex] Platform count = {platformId}");
    }

    /// <summary>
    /// 발 위치에서 아래로 Raycast 해서 밟고 있는 플랫폼 ID를 얻는다.
    /// </summary>
    public bool TryGetStandingPlatformIdByRay(Transform foot, out int platformId, out RaycastHit2D hit)
    {
        Vector2 origin = (Vector2)foot.position + rayOffset;

        hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundMask);

        if (!hit.collider)
        {
            platformId = -1;
            return false;
        }

        // hit 지점을 타일 셀로 변환
        Vector3Int cell = tilemap.WorldToCell(hit.point);

        // 가끔 hit.point가 타일 경계/콜라이더 경계라 한 칸 옆으로 찍히는 경우가 있어서,
        // 아래로 아주 살짝 더 내린 점으로 보정하면 안정적임
        Vector3Int cell2 = tilemap.WorldToCell(hit.point + Vector2.down * 0.01f);

        if (cellToPlatform.TryGetValue(cell2, out platformId)) return true;
        if (cellToPlatform.TryGetValue(cell, out platformId)) return true;

        platformId = -1;
        return false;
    }

    public bool AreOnSamePlatformByRay(Transform footA, Transform footB)
    {
        if (!TryGetStandingPlatformIdByRay(footA, out int aId, out _)) return false;
        if (!TryGetStandingPlatformIdByRay(footB, out int bId, out _)) return false;
        return aId == bId;
    }
}

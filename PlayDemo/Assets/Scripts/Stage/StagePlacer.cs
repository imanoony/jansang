// StageData 객체 바탕으로 씬에 Stage를 배치한다.
// (0,0) 타일맵을 좌측 하단 경계로 생각하고 배치해서
// 카메라의 적절한 조정이 필요하다.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StagePlacer : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    private List<GameObject> entities = new();

#region Terrain
    public Tilemap groundTM;    // ground tilemap
    public Tilemap wallTM;      // wall tilemap
    public TileBase[] tiles;    // tiles, 지금은 테스트용으로 2개만 (0: ground, 1: wall)
#endregion

#region Test
    public StageData testData;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            PlaceStage(testData);
    }
#endregion

    public void PlaceStage(StageData _stage)
    {
        StageData stage = Instantiate(_stage);
        Vector2 entityPos;
        GameObject entity;

        // 땅 타일 배치하기
        foreach (Int2 pos in stage.grounds)
            groundTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[0]);

        // 벽 타일 배치하기
        foreach (Int2 pos in stage.walls)
            wallTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[1]);

        // 적들 스폰하기
        foreach (Int2 pos in stage.enemies)
        {
            entityPos = GetEnemyPos(pos);
            entity = Instantiate(enemyPrefab, entityPos, Quaternion.identity);
            entities.Add(entity);
        }

        // 플레이어 스폰하기
        entityPos = GetPlayerPos(stage.player);
        entity = Instantiate(playerPrefab, entityPos, Quaternion.identity);
        entities.Add(entity);

        Destroy(stage);
    }

    public void RemoveStage()
    {
        groundTM.ClearAllTiles();
        wallTM.ClearAllTiles();
        foreach (GameObject entity in entities)
            Destroy(entity);
        entities.Clear();
    }

    // tile 좌표를 받아 플레이어가 위치할 로컬 좌표를 반환한다
    // enemy도 동일한 로직을 수행하지만 일단은 함수를 분리해뒀다
    // 일단 조정 없이 타일 좌하단 지점에 스폰되도록 함 (이후 offset 추가할 예정)
    private Vector2 GetPlayerPos(Int2 pos)
    {
        return groundTM.CellToLocal(new Vector3Int(pos. x, pos.y));
    }

    private Vector2 GetEnemyPos(Int2 pos)
    {
        return GetPlayerPos(pos);
    }
}

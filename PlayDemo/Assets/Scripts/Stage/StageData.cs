// scriptable object로 각 스테이지 데이터 저장
// 저장되는 정보: 플레이어의 시작 스폰 위치,
// 적들의 스폰 위치, 벽과 바닥 타일 위치
// 스테이지 요소가 많아짐에 따라 확장 가능함

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class StageData : ScriptableObject
{
    public string id;               // 구분용 이름 (integer 대신)
    public Int2 player;             // 플레이어의 시작 좌표
    public List<Int2> enemies = new();      // 적들의 시작 좌표
    
#region Terrain
    public List<Int2> grounds = new();      // 땅 타일 좌표
    public List<Int2> walls = new();        // 벽 타일 좌표
    public List<Int2> pitfalls = new();      // 함정 타일 좌표
    public List<Int2> decos = new();         // 장식 타일 좌표
#endregion

}

// 오브젝트의 2차원 좌표 표시를 위한 클래스
// 오브젝트: 발판 타일, 벽 타일, 플레이어, 적 등
// scriptable object의 필드이므로 직렬화
[Serializable]
public class Int2
{
    public int x;
    public int y;
}

// 스테이지 에디터를 위한 정적 함수 모음
// 현재 에디터 상황을 저장해 스크립터블 오브젝트로 변환 등
// save: 에디터 상황 -> 스크립터블 오브젝트 (직렬화)
// load: 스크립터블 오브젝트 -> 에디터 상황 (역직렬화)
// TODO: 하드한 prefab, tilebase 소프트하게 바꾸기.
// TODO: 비 tilemap 오브젝트의 위치 저장을 조금 더 정밀하게.
public static class StageSerializer
{
#if UNITY_EDITOR
    // grid (terrian), player 위치 정보, enemy 위치 정보를
    // 받아 ScriptableObject 형태 (stage) 로 저장한다.
    public static void Save(
        Grid grid,
        Transform player,
        Transform enemy, 
        StageData stage
    )
    {
        int i;
        Vector3Int entityPos;
        Tilemap tilemap;
        TileBase tile;

        if (player.childCount != 1) 
        {
            Debug.LogError("StageSerializer.Save(): invalid player count");
            return;
        }

        stage.grounds.Clear();
        stage.walls.Clear();
        stage.pitfalls.Clear();
        stage.decos.Clear();
        for (i = 0; i < grid.transform.childCount; i++)
        {
            tilemap = grid.transform.GetChild(i).GetComponent<Tilemap>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                tile = tilemap.GetTile(pos);
                if (tile == null) continue;
                if (tilemap.name.ToLower().Contains("ground"))
                    stage.grounds.Add(new Int2 { x = pos.x, y = pos.y });
                else if (tilemap.name.ToLower().Contains("wall"))
                    stage.walls.Add(new Int2 { x = pos.x, y = pos.y });
                else if (tilemap.name.ToLower().Contains("pitfall"))
                    stage.pitfalls.Add(new Int2 { x = pos.x, y = pos.y });
                else if (tilemap.name.ToLower().Contains("deco"))
                    stage.decos.Add(new Int2 { x = pos.x, y = pos.y });
            }
        }

        entityPos = grid.LocalToCell(player.GetChild(0).position);
        stage.player = new Int2 { x = entityPos.x, y = entityPos.y };

        stage.enemies.Clear();
        for (i = 0; i < enemy.childCount; i++)
        {
            entityPos = grid.LocalToCell(enemy.GetChild(i).position);
            stage.enemies.Add(new Int2 { x = entityPos.x, y = entityPos.y });
        }

        EditorUtility.SetDirty(stage);
        AssetDatabase.SaveAssets();
    }
#endif

    // ScriptableObject를 받아 현재 에디터에
    // grid (terrian), player, enemy 를 배치한다.
    // 인자가 더럽지만 일단 테스트용으로... 이후에 고치겠지 뭐~
    public static void Load(
        StageData stage,
        Grid grid,
        Transform player,
        Transform enemy,
        TileBase[] tiles,
        GameObject playerPrefab,
        GameObject enemyPrefab
    )
    {
        int i;
        GameObject entity;
        Tilemap groundTM = grid.transform.Find("Ground").GetComponent<Tilemap>();
        Tilemap wallTM = grid.transform.Find("Wall").GetComponent<Tilemap>();
        Tilemap pitfallTM = grid.transform.Find("Pitfall").GetComponent<Tilemap>();
        Tilemap decoTM = grid.transform.Find("Deco").GetComponent<Tilemap>();

        groundTM.ClearAllTiles();
        wallTM.ClearAllTiles();
        pitfallTM.ClearAllTiles();
        decoTM.ClearAllTiles();

        if (player.childCount > 0)
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(player.GetChild(0).gameObject);
#else
            Destroy(player.GetChild(0).gameObject);
#endif
        for (i = enemy.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(enemy.GetChild(i).gameObject);
#else
            Destroy(enemy.GetChild(i).gameObject);
#endif

        // 땅 타일 배치하기
        foreach (Int2 pos in stage.grounds)
            groundTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[0]);

        // 벽 타일 배치하기
        foreach (Int2 pos in stage.walls)
            wallTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[1]);

        // 함정 타일 배치하기
        foreach (Int2 pos in stage.pitfalls)
            pitfallTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[2]);

        // 장식 타일 배치하기
        foreach (Int2 pos in stage.decos)
            decoTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[3]);

        // 적들 스폰하기
        foreach (Int2 pos in stage.enemies)
        {
#if UNITY_EDITOR
            entity = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
#else
            entity = Instantiate(enemyPrefab);
#endif
            entity.transform.position = GetEnemyPos(grid, pos);
            entity.transform.SetParent(enemy);
        }

        // 플레이어 스폰하기
#if UNITY_EDITOR
        entity = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
#else   
        entity = Instantiate(playerPrefab);
#endif
        entity.transform.position = GetPlayerPos(grid, stage.player);
        entity.transform.SetParent(player);
    }

    // TODO: 화면 clear 함수 만들기

    // tile 좌표를 받아 플레이어가 위치할 로컬 좌표를 반환한다
    // enemy도 동일한 로직을 수행하지만 일단은 함수를 분리해뒀다
    // 일단 조정 없이 타일 좌하단 지점에 스폰되도록 함 (이후 offset 추가할 예정)
    private static Vector2 GetPlayerPos(Grid grid, Int2 pos)
    {
        Vector2 playerPos;
        playerPos = grid.CellToLocal(new Vector3Int(pos. x, pos.y));
        playerPos += new Vector2(grid.cellSize.x, grid.cellSize.y) * 0.5f;
        return playerPos;
    }

    private static Vector2 GetEnemyPos(Grid grid, Int2 pos)
    {
        return GetPlayerPos(grid, pos);
    }
}
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
        for (i = 0; i < grid.transform.childCount; i++)
        {
            tilemap = grid.transform.GetChild(i).GetComponent<Tilemap>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                tile = tilemap.GetTile(pos);
                if (tile == null) continue;
                if (tilemap.name.ToLower().Contains("ground"))
                    stage.grounds.Add(new Int2 { x = pos.x, y = pos.y });
                else 
                    stage.walls.Add(new Int2 { x = pos.x, y = pos.y });
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

        groundTM.ClearAllTiles();
        wallTM.ClearAllTiles();
        if (player.childCount > 0)
            Undo.DestroyObjectImmediate(player.GetChild(0).gameObject);
        for (i = enemy.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(enemy.GetChild(i).gameObject);

        // 땅 타일 배치하기
        foreach (Int2 pos in stage.grounds)
            groundTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[0]);

        // 벽 타일 배치하기
        foreach (Int2 pos in stage.walls)
            wallTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[1]);

        // 적들 스폰하기
        foreach (Int2 pos in stage.enemies)
        {
            entity = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            entity.transform.position = grid.CellToLocal(new Vector3Int(pos.x, pos.y));
            entity.transform.SetParent(enemy);
        }

        // 플레이어 스폰하기
        entity = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        entity.transform.position = grid.CellToLocal(new Vector3Int(stage.player.x, stage.player.y));
        entity.transform.SetParent(player);
    }

    // TODO: 화면 clear 함수 만들기
#endif
}
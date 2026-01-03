// StageData 객체 바탕으로 씬에 Stage를 배치한다.
// (0,0) 타일맵을 좌측 하단 경계로 생각하고 배치해서
// 카메라의 적절한 조정이 필요하다.
// TODO: 스테이지 배치에 맞게 카메라 조정하기.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StagePlacer : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerRoot;
    public Transform enemyRoot;

#region Terrain
    public Grid grid;
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
        StageSerializer.Load(stage, grid, playerRoot, enemyRoot, tiles, playerPrefab, enemyPrefab);
        Destroy(stage);
    }

    public void RemoveStage()
    {
        
    }
}

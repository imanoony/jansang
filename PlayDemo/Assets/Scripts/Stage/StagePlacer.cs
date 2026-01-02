// StageData 객체 바탕으로 씬에 Stage를 배치한다.
// (0,0) 타일맵을 좌측 하단 경계로 생각하고 배치해서
// 카메라의 적절한 조정이 필요하다.

using UnityEngine;
using UnityEngine.Tilemaps;

public class StagePlacer : MonoBehaviour
{
    public Tilemap groundTM;    // ground tilemap
    public Tilemap wallTM;      // wall tilemap
    public TileBase[] tiles;    // tiles, 지금은 테스트용으로 2개만 (0: ground, 1: wall)

#region Test
    public StageData testData;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            PlaceStage(testData);
    }
#endregion

    public void PlaceStage(StageData _stage)
    {
        StageData stage = Instantiate(_stage);

        // 땅 타일 배치하기
        foreach (Int2 pos in stage.grounds)
            groundTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[0]);

        // 벽 타일 배치하기
        foreach (Int2 pos in stage.walls)
            wallTM.SetTile(new Vector3Int(pos.x, pos.y), tiles[1]);

        // 적들 스폰하기
        // TODO

        // 플레이어 스폰하기
        // TODO

        Destroy(stage);
    }
}

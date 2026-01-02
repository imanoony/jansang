// scriptable object로 각 스테이지 데이터 저장
// 저장되는 정보: 플레이어의 시작 스폰 위치,
// 적들의 스폰 위치, 벽과 바닥 타일 위치
// 스테이지 요소가 많아짐에 따라 확장 가능함

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class StageData : ScriptableObject
{
    public string id;               // 구분용 이름 (integer 대신)
    public Int2 player;             // 플레이어의 시작 좌표
    public List<Int2> enemies;      // 적들의 시작 좌표
    
#region Terrain
    public List<Int2> grounds;      // 땅 타일 좌표
    public List<Int2> walls;        // 벽 타일 좌표
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
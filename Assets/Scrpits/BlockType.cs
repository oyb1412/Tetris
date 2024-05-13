using UnityEngine;
using UnityEngine.Tilemaps;

//각 블록의 타입
public enum BlockType { I, J, L, O, S, T, Z }

/// <summary>
/// 블록 타입, 블록 타일, 각 셀의 기본 위치 좌표, 각 블록의 벽 회전 좌표값
/// </summary>
[System.Serializable]
public struct BlockData
{
    //블록 타입
    public BlockType blockType;
    //블록 타일
    public Tile tile;
    //벽 회전시 좌표값
    public Vector2Int[,] wallKick;
    //각 셀의 기본 좌표값
    public Vector2Int[] cells { get; private set; }

    public void Init()
    {
        cells = new Vector2Int[4];
        cells = Data.Cells[blockType];
        wallKick = Data.WallKicks[blockType];
    }
}



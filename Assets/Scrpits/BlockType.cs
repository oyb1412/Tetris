using UnityEngine;
using UnityEngine.Tilemaps;

//�� ����� Ÿ��
public enum BlockType { I, J, L, O, S, T, Z }

/// <summary>
/// ��� Ÿ��, ��� Ÿ��, �� ���� �⺻ ��ġ ��ǥ, �� ����� �� ȸ�� ��ǥ��
/// </summary>
[System.Serializable]
public struct BlockData
{
    //��� Ÿ��
    public BlockType blockType;
    //��� Ÿ��
    public Tile tile;
    //�� ȸ���� ��ǥ��
    public Vector2Int[,] wallKick;
    //�� ���� �⺻ ��ǥ��
    public Vector2Int[] cells { get; private set; }

    public void Init()
    {
        cells = new Vector2Int[4];
        cells = Data.Cells[blockType];
        wallKick = Data.WallKicks[blockType];
    }
}



using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BlockType { I, J, L, O, S, T, Z }

[System.Serializable]
public struct BlockData
{
    public BlockType blockType;
    public Tile tile;
    public Vector2Int[,] wallKick;
    public Vector2Int[] cells { get; private set; }

    public void Init()
    {
        cells = new Vector2Int[4];
        cells = Data.Cells[blockType];
        wallKick = Data.WallKicks[blockType];
    }
}



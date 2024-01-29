using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class NextBlock : MonoBehaviour
{
    public Board board;
    public BlockData currentBlockData;
    public Tile tile;
    public Tilemap tilemap;
    public Vector2Int startPosition;
    public Vector2Int[] cells;
    public Vector2Int[] currentPosition;


    // Start is called before the first frame update
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        cells = new Vector2Int[4];
        currentPosition = new Vector2Int[4];

    }

    public void SpawnBlock()
    {
        currentBlockData = board.nextBlockData;
        tile = board.nextBlockData.tile;

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = currentBlockData.cells[i];
            currentPosition[i] = cells[i] + startPosition;
        }
        tilemap.ClearAllTiles();

        UseRotate(1);

    }

    private void UseRotate(int rotationIndex)
    {
        float cos = Mathf.Cos(Mathf.PI / 2f);
        float sin = Mathf.Sin(Mathf.PI / 2f);
        Vector2Int save;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2 pos = cells[i];
            int x, y;
            switch (currentBlockData.blockType)
            {
                case BlockType.I:
                case BlockType.O:
                    pos.x -= 0.5f;
                    pos.y -= 0.5f;
                    x = Mathf.CeilToInt((pos.x * cos * rotationIndex) + (pos.y * sin * rotationIndex));
                    y = Mathf.CeilToInt((pos.x * -sin * rotationIndex) + (pos.y * cos * rotationIndex));
                    break;

                default:
                    x = Mathf.RoundToInt((pos.x * cos * rotationIndex) + (pos.y * sin * rotationIndex));
                    y = Mathf.RoundToInt((pos.x * -sin * rotationIndex) + (pos.y * cos * rotationIndex));
                    break;
            }
            save = new Vector2Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y));
            cells[i] = new Vector2Int(x, y);
            currentPosition[i] += (cells[i] - save);
            tilemap.SetTile((Vector3Int)currentPosition[i], tile);

        }
    }
}

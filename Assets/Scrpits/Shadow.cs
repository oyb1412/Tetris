using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Shadow : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Tilemap tilemap { get; private set; }
    public Vector2Int[] currentPosition;
    public Vector2Int[] cells;
    public Vector2Int startPosition;
    public Vector2Int Position;
    public float count;
    // Start is called before the first frame update
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        currentPosition = new Vector2Int[4];
    }
    private void Start()
    {
    }
    private void LateUpdate()
    {
        if (!UIManager.Instance.isLive)
            return;

        count += Time.deltaTime;
        DeleteBlock();
        Copy();

        Drop();
        SetBlock();

    }

    void SetBlock()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, tile);
        }
    }
    void DeleteBlock()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, null);
        }
    }
    public void Copy()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            currentPosition[i] = board.currentPosition[i];
        }
    }


    void Drop()
    {
        Vector2Int position = board.Position;
        board.DeleteBlock();

        int current = position.y;
        int bottom = -board.boardSize.y / 2 - 1;
        for (int row = current; row >= bottom; row--)
        {
            if (IsValidMove(Vector2Int.down))
            {
                for(int i = 0; i < currentPosition.Length; i++)
                {
                    currentPosition[i] += Vector2Int.down;
                }
            }
            else
            {
                break;
            }
        }
        board.SetBlock();




    }

    public bool IsValidMove(Vector2Int trans)
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            if (!board.boardRect.Contains((currentPosition[i] + trans)))
            {
                return false;
            }
            if (board.tilemap.HasTile((Vector3Int)(currentPosition[i] + trans)))
            {
                return false;
            }
        }

        return true;

    }
}

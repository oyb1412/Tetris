using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 섀도우 블록
/// </summary>
public class Shadow : MonoBehaviour
{
    //섀도우 블록 타일
    [SerializeField]private Tile tile;
    //보드 클래스
    [SerializeField] private Board board;
    //보드 타일맵
    private Tilemap tilemap;
    //각 셀의 현재 포지션
    private Vector2Int[] currentPosition;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        currentPosition = new Vector2Int[4];
    }

    private void LateUpdate()
    {
        if (!UIManager.Instance.IsLive)
            return;

        DeleteBlock();
        Copy();

        Drop();
        SetBlock();

    }

    /// <summary>
    /// 현재 셀 정보로 섀도우 블록 드로우
    /// </summary>
    void SetBlock()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, tile);
        }
    }

    /// <summary>
    /// 섀도우 블록 드로우 지우기
    /// </summary>
    void DeleteBlock()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, null);
        }
    }

    /// <summary>
    /// 현재 블록 위치를 복사
    /// </summary>
    public void Copy()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            currentPosition[i] = board.CurrentPosition[i];
        }
    }

    /// <summary>
    /// 섀도우 블록을 가장 밑 열로 드랍
    /// </summary>
    void Drop()
    {
        Vector2Int position = board.Position;
        board.DeleteBlock();

        int current = position.y;
        int bottom = -board.BoardSize.y / 2 - 1;
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

    /// <summary>
    /// 이동이 가능한가?
    /// </summary>
    /// <param name="trans">이동 목표 좌표</param>
    /// <returns></returns>
    public bool IsValidMove(Vector2Int trans)
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            if (!board.BoardRect.Contains((currentPosition[i] + trans)))
            {
                return false;
            }
            if (board.Tilemap.HasTile((Vector3Int)(currentPosition[i] + trans)))
            {
                return false;
            }
        }
        return true;
    }
}

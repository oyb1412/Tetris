using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ������ ���
/// </summary>
public class Shadow : MonoBehaviour
{
    //������ ��� Ÿ��
    [SerializeField]private Tile tile;
    //���� Ŭ����
    [SerializeField] private Board board;
    //���� Ÿ�ϸ�
    private Tilemap tilemap;
    //�� ���� ���� ������
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
    /// ���� �� ������ ������ ��� ��ο�
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
    /// ������ ��� ��ο� �����
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
    /// ���� ��� ��ġ�� ����
    /// </summary>
    public void Copy()
    {
        for (int i = 0; i < currentPosition.Length; i++)
        {
            currentPosition[i] = board.CurrentPosition[i];
        }
    }

    /// <summary>
    /// ������ ����� ���� �� ���� ���
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
    /// �̵��� �����Ѱ�?
    /// </summary>
    /// <param name="trans">�̵� ��ǥ ��ǥ</param>
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

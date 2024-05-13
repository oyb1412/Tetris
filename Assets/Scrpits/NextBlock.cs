using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ���� ��� ����
/// </summary>
public class NextBlock : MonoBehaviour
{
    //���� Ŭ����
    [SerializeField]private Board board;
    //���� ��� ������
    private BlockData currentBlockData;
    //���� ��� Ÿ��
    private Tile tile;
    //���� Ÿ�ϸ�
    private Tilemap tilemap;
    //���� ���� ���� ��ǥ
    [SerializeField]private Vector2Int startPosition;
    //����� �� �� 
    private Vector2Int[] cells;
    //����� �� ���� ��ǥ
    private Vector2Int[] currentPosition;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector2Int[4];
        currentPosition = new Vector2Int[4];
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    public void SpawnBlock()
    {
        currentBlockData = board.NextBlockData;
        tile = board.NextBlockData.tile;

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = currentBlockData.cells[i];
            currentPosition[i] = cells[i] + startPosition;
        }
        tilemap.ClearAllTiles();
        UseRotate(1);
    }

    /// <summary>
    /// ��� ȸ��
    /// </summary>
    /// <param name="rotationIndex">ȸ���� �ε���</param>
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

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public BlockData[] blockData { get; private set; }
    public BlockData currentBlockData;
    Tilemap tilemap;
    public Vector2Int startPosition;
    public Vector2Int[] currentPosition;
    public Vector2Int[] cells;
    public Vector2Int boardSize = new Vector2Int(10,20);
    public float stepDelay = 1f;
    float stepTime;
    int currentIndex;
    public RectInt boardRect
    {
        get
        {
            Vector2Int pos = new Vector2Int(-boardSize.x / 2, -boardSize.y /2);
            return new RectInt(pos, boardSize);
        }
    }
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        for(int i = 0; i< blockData.Length; i++)
        {
            blockData[i].Init();
        }
        currentPosition = new Vector2Int[blockData[0].cells.Length];
    }

    private void Start()
    {
        SpawnBlock();
    }

    private void Update()
    {
        DeleteBlock();

        stepTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.A))
            MoveBlock(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D))
            MoveBlock(Vector2Int.right);
        else if (Input.GetKeyDown(KeyCode.S))
            MoveBlock(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.Q))
            Rotate(-1);        
        else if (Input.GetKeyDown(KeyCode.E))
            Rotate(1);
        else if (Input.GetKeyDown(KeyCode.Space))
            HardDrop();

        
        if (stepTime > stepDelay)
            Step();

        SetBlock();
    }

    private void Rotate(int rotationIndex)
    {
        var originalIndex = currentIndex;
        currentIndex = Wrap(currentIndex + rotationIndex, 0, 4);
        UseRotate(rotationIndex);
        if(!CheckWallKick(currentIndex, rotationIndex))
        {
            currentIndex = originalIndex;
            UseRotate(-rotationIndex);
        }
    }

    bool CheckWallKick(int currentIndex, int rotationIndex)
    {
        int index = GetWallKickIndex(currentIndex, rotationIndex);

        for(int i = 0; i <currentBlockData.wallKick.GetLength(1); i++)
        {
            Vector2Int save = currentBlockData.wallKick[index,i];

            if (MoveBlock(save))
                return true;
        }
        return false;
    }
    int GetWallKickIndex(int currentIndex, int rotationIndex)
    {
        int index = currentIndex * 2;
        if (rotationIndex < 0)
            index--;

        return Wrap(index, 0, currentBlockData.wallKick.GetLength(0));
    }
    private void UseRotate(int rotationIndex)
    {
        float cos = Mathf.Cos(Mathf.PI / 2f);
        float sin = Mathf.Sin(Mathf.PI / 2f);
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
            Vector2Int save = new Vector2Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y));
            cells[i] = new Vector2Int(x, y);
            currentPosition[i] += (cells[i] - save);
        }
    }
    private void HardDrop()
    {
        while(MoveBlock(Vector2Int.down))
        {
            DeleteBlock();
            continue;
        }
    }

    void Lock()
    {
        SetBlock();
        SpawnBlock();
        stepTime = 0;
    }
    void Step()
    {
        stepTime = 0;
        MoveBlock(Vector2Int.down);
    }

    void SpawnBlock()
    {
        int ran = Random.Range(0, blockData.Length);
        currentBlockData = blockData[ran];
        cells = currentBlockData.cells;
        stepTime = 0;

        for (int i = 0; i < cells.Length; i++)
        {
            currentPosition[i] = cells[i] + startPosition;
            tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile);
        }
    }
    void SetBlock()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, currentBlockData.tile);
        }
    }

    void DeleteBlock()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, null);
        }
    }

    private bool MoveBlock(Vector2Int translate)
    {
        DeleteBlock();

        var move = IsValidMove(translate);
        if (move)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                currentPosition[i] = currentPosition[i] + translate;
                tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile);
            }
        }
        SetBlock();
        return move;
    }

    bool IsValidMove(Vector2Int translate)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (!boardRect.Contains((currentPosition[i] + translate)))
            {
                if (currentPosition[i].y + translate.y <= -boardSize.y / 2)
                    Lock();

                return false;
            }
            if (tilemap.HasTile((Vector3Int)(currentPosition[i] + translate)))
            {
                if (currentPosition[i].y > currentPosition[i].y + translate.y)
                    Lock();

                return false;
            }
        }
        return true;
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}

using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Text levelText;
    public Text scoreText;
    public Text linesText;
    public NextBlock nextBlock;
    public BlockData[] blockData;
    public BlockData currentBlockData;
    public BlockData nextBlockData;
    public Tilemap tilemap;
    public Vector2Int startPosition;
    public Vector2Int Position;
    public Vector2Int[] currentPosition;
    public Vector2Int[] cells;
    public Vector2Int[] rotateCells;
    public Vector2Int boardSize = new Vector2Int(10,20);
    public float stepDelay = 1f;
    float stepTime;
    public int currentIndex;
    int currentLevel;
    int currentScore;
    int currentLines;
    float levelUpDelay = 20f;
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
        cells = new Vector2Int[blockData[0].cells.Length];
        rotateCells = new Vector2Int[blockData[0].cells.Length];
        Position = startPosition;
        currentLevel = 1;
        levelText.text = currentLevel.ToString();
        scoreText.text = currentScore.ToString("D8");
        linesText.text = currentLines.ToString();
        int ran = Random.Range(0, blockData.Length);
        nextBlockData = blockData[ran];
    }


    void ChangeLevel()
    {
        if(Time.time > levelUpDelay)
        {
            currentLevel++;
            levelUpDelay += Time.time;
            levelText.text = currentLevel.ToString();
            if (stepDelay > 0.1f)
                stepDelay -= 0.1f;
        }
    }
    private void Start()
    {
        SpawnBlock();
    }

    private void Update()
    {
        if (!UIManager.Instance.isLive)
            return;

        DeleteBlock();
        ChangeLevel();
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
        if (!CheckWallKick(currentIndex, rotationIndex))
        {
            currentIndex = originalIndex;
            UseRotate(-rotationIndex);
        }
    }
  
    bool CheckWallKick(int currentIndex, int rotationIndex)
    {
        int index = GetWallKickIndex(currentIndex, rotationIndex);

        for (int i = 0; i <currentBlockData.wallKick.GetLength(1); i++)
        {
            Vector2Int save = currentBlockData.wallKick[index,i];
            if (MoveBlock(save))
            {
                return true;
            }
        }
        for(int j = 0; j <cells.Length; j++)
        {
            if (RotateBlock(rotateCells[j]))
                return true;
            else
                return false;
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


    public void CrearLines()
    {
        RectInt bounds = boardRect;
        int row = bounds.yMin;
        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                currentScore += 100;
                currentLines++;
                linesText.text = currentLines.ToString();
                scoreText.text = currentScore.ToString("D8");
            }
            else
                row++;
        }
    }
    void LineClear(int row)
    {
        RectInt bounds = boardRect;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.yMin; col < bounds.yMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);
                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }
            row++;
        }
    }
    private bool IsLineFull(int row)
    {
        RectInt bounds = boardRect;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
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
            rotateCells[i] = (cells[i] - save);
        }
    }
    public void HardDrop()
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
        CrearLines();
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
        currentBlockData = nextBlockData;
        stepTime = 0;
        Position = startPosition;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = currentBlockData.cells[i];
            currentPosition[i] = cells[i] + startPosition;
            if (!boardRect.Contains((currentPosition[i])))
            GameOver(); 
            if (tilemap.HasTile((Vector3Int)(currentPosition[i])))
            GameOver();
            else
            tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile);
        }
        int ran = Random.Range(0, blockData.Length);
        nextBlockData = blockData[ran];
        nextBlock.SpawnBlock();

    }

    private void GameOver()
    {
        tilemap.ClearAllTiles();
    }

    public void SetBlock()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var pos = currentPosition[i];
            tilemap.SetTile((Vector3Int)pos, currentBlockData.tile);
        }
    }

    public void DeleteBlock()
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
        Vector2Int newPosition = Position;
        newPosition.x += translate.x;
        newPosition.y += translate.y;
        var move = IsValidMove(translate);
        if (move)
        {
            Position = newPosition;
            for (int i = 0; i < cells.Length; i++)
            {
                currentPosition[i] = currentPosition[i] + translate;
                tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile);
            }
        }
        SetBlock();
        return move;
    }

    private bool RotateBlock(Vector2Int translate)
    {
        DeleteBlock();
        Vector2Int newPosition = Position;
        newPosition.x += translate.x;
        newPosition.y += translate.y;
        var move = IsValidRotate(translate);
        if (move)
        {
            Position = newPosition;
            for (int i = 0; i < cells.Length; i++)
            {
                currentPosition[i] = currentPosition[i] + translate;
                tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile);
            }
        }
        SetBlock();
        return move;
    }
    public bool IsValidSpawn()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (!boardRect.Contains((currentPosition[i])))
                return false;
            if (tilemap.HasTile((Vector3Int)(currentPosition[i])))
                return false;
        }
        return true;
    }
    public bool IsValidMove(Vector2Int translate)
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

    public bool IsValidRotate(Vector2Int translate)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (tilemap.HasTile((Vector3Int)(currentPosition[i] + translate)))
            {
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

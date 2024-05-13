using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// ���� Ÿ�ϸ� ����
/// </summary>
public class Board : MonoBehaviour
{
    #region Variable
    //���� ǥ�� �ؽ�Ʈ
    [SerializeField]private Text levelText;
    //���ھ� ǥ�� �ؽ�Ʈ
    [SerializeField] private Text scoreText;
    //�ı� ���� ǥ�� �ؽ�Ʈ
    [SerializeField] private Text linesText;
    //���� ��� Ŭ����
    [SerializeField] private NextBlock nextBlock;
    //�� ������ ��� ������
    [SerializeField] private BlockData[] blockData;
    //���� ��� ������
    private BlockData currentBlockData;
    //���� ��� ������
    public BlockData NextBlockData { get; private set; }
    //���� Ÿ�ϸ�
    public Tilemap Tilemap { get; private set; }
    //���� ���� ������
    [SerializeField]private Vector2Int startPosition;
    //�̵� ���� ������
    public Vector2Int Position {  get; private set; }   
    //���� ������
    public Vector2Int[] CurrentPosition { get; private set; }
    //����� �� ��
    private Vector2Int[] cells;
    //ȸ�� ���� ����
    private Vector2Int[] rotateCells;
    //���� ������
    public Vector2Int BoardSize { get; private set; } = new Vector2Int(10, 20);
    //����� �ڵ� �̵� ������
    private float stepDelay = 1f;
    //����� �ڵ� �̵� Ÿ�̸�
    private float stepTime;
    //ȸ�� ����(����)
    private int currentIndex;
    //���� ����
    private int currentLevel;
    //���� ����
    private int currentScore;
    //���� �ı��� ���� ��
    private int currentLines;
    //���̵� ���� Ÿ�̸�
    private float levelUpDelay = 20f;

    //������ min, max, width, height����
    public RectInt BoardRect {
        get {
            Vector2Int pos = new Vector2Int(-BoardSize.x / 2, -BoardSize.y / 2);
            return new RectInt(pos, BoardSize);
        }
    }
    #endregion

    #region InitMethod
    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        for(int i = 0; i< blockData.Length; i++)
        {
            blockData[i].Init();
        }
        CurrentPosition = new Vector2Int[blockData[0].cells.Length];
        cells = new Vector2Int[blockData[0].cells.Length];
        rotateCells = new Vector2Int[blockData[0].cells.Length];
        Position = startPosition;
        currentLevel = 1;
        levelText.text = currentLevel.ToString();
        scoreText.text = currentScore.ToString("D8");
        linesText.text = currentLines.ToString();
        int ran = Random.Range(0, blockData.Length);
        NextBlockData = blockData[ran];
    }

    private void Start() {
        SpawnBlock();
    }

    #endregion

    private void Update()
    {
        if (!UIManager.Instance.IsLive)
            return;

        //�� �����Ӹ��� ����� ����
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

        //�� �����Ӹ��� ����� ���� ��ο�
        SetBlock();
    }

    /// <summary>
    /// Ÿ�̸� ������ ���� ���� ���̵� ����
    /// </summary>
    private void ChangeLevel() {
        if (Time.time > levelUpDelay) {
            currentLevel++;
            levelUpDelay += Time.time;
            levelText.text = currentLevel.ToString();
            if (stepDelay > 0.1f)
                stepDelay -= 0.1f;
        }
    }

    /// <summary>
    /// clampȿ��
    /// </summary>
    /// <param name="input">ȿ���� ������ ��</param>
    /// <param name="min">�ּ� ����</param>
    /// <param name="max">�ִ� ����</param>
    private int Wrap(int input, int min, int max) {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

    #region BlockBehaivour

    /// <summary>
    /// ��� ȸ��
    /// </summary>
    /// <param name="rotationIndex">ȸ���� �ε���</param>
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

    /// <summary>
    /// ȸ�� ��, ������ �浹 üũ
    /// </summary>
    /// <param name="currentIndex">���� ȸ�� �ε���</param>
    /// <param name="rotationIndex">���� ȸ�� �ε���</param>
    /// <returns></returns>
    private bool CheckWallKick(int currentIndex, int rotationIndex)
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

    /// <summary>
    /// ������ �浹 üũ�� ���� ȸ������ �ε��� ��ȯ
    /// </summary>
    /// <param name="currentIndex">���� ȸ�� �ε���</param>
    /// <param name="rotationIndex">���� ȸ�� �ε���</param>
    /// <returns></returns>
    private int GetWallKickIndex(int currentIndex, int rotationIndex)
    {
        int index = currentIndex * 2;
        if (rotationIndex < 0)
            index--;

        return Wrap(index, 0, currentBlockData.wallKick.GetLength(0));
    }

    /// <summary>
    /// �ϼ��� ������ üũ
    /// </summary>
    public void CrearLines()
    {
        RectInt bounds = BoardRect;
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

    /// <summary>
    /// �ϼ��� ������ Ŭ����
    /// </summary>
    /// <param name="row">�ϼ��� ��</param>
    void LineClear(int row)
    {
        RectInt bounds = BoardRect;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            Tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.yMin; col < bounds.yMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = Tilemap.GetTile(position);
                position = new Vector3Int(col, row, 0);
                Tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    /// <summary>
    /// ������ �� á�� üũ
    /// </summary>
    /// <param name="row">üũ�� ��</param>
    /// <returns></returns>
    private bool IsLineFull(int row)
    {
        RectInt bounds = BoardRect;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!Tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// ȸ���� ������ ��� ȸ��
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
            CurrentPosition[i] += (cells[i] - save);
            rotateCells[i] = (cells[i] - save);
        }
    }

    /// <summary>
    /// Ư�� Ű �Է� �� ��� ��� ���
    /// </summary>
    public void HardDrop()
    {
        while(MoveBlock(Vector2Int.down))
        {
            DeleteBlock();
            continue;
        }
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    void Lock()
    {
        SetBlock();
        CrearLines();
        SpawnBlock();
        stepTime = 0;
    }

    /// <summary>
    /// ����� �ڵ� �̵�
    /// </summary>
    void Step()
    {
        stepTime = 0;
        MoveBlock(Vector2Int.down);
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    void SpawnBlock()
    {
        currentBlockData = NextBlockData;
        stepTime = 0;
        Position = startPosition;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = currentBlockData.cells[i];
            CurrentPosition[i] = cells[i] + startPosition;
            if (!BoardRect.Contains((CurrentPosition[i])))
            GameOver(); 
            if (Tilemap.HasTile((Vector3Int)(CurrentPosition[i])))
            GameOver();
            else
            Tilemap.SetTile((Vector3Int)CurrentPosition[i], currentBlockData.tile);
        }
        int ran = Random.Range(0, blockData.Length);
        NextBlockData = blockData[ran];
        nextBlock.SpawnBlock();

    }

    /// <summary>
    /// ���� ����(��� Ÿ�� Ŭ����)
    /// </summary>
    private void GameOver()
    {
        Tilemap.ClearAllTiles();
    }

    /// <summary>
    /// ��� ���Ӱ� ��ο�
    /// </summary>
    public void SetBlock()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var pos = CurrentPosition[i];
            Tilemap.SetTile((Vector3Int)pos, currentBlockData.tile);
        }
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    public void DeleteBlock()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var pos = CurrentPosition[i];
            Tilemap.SetTile((Vector3Int)pos, null);
        }
    }

    /// <summary>
    /// ��� �̵�(���� ����)
    /// </summary>
    /// <param name="translate">�̵��� ��ġ</param>
    /// <returns></returns>
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
                CurrentPosition[i] = CurrentPosition[i] + translate;
                Tilemap.SetTile((Vector3Int)CurrentPosition[i], currentBlockData.tile);
            }
        }
        SetBlock();
        return move;
    }

    /// <summary>
    /// ��� ȸ��
    /// </summary>
    /// <param name="translate">ȸ���� ��ǥ</param>
    /// <returns></returns>
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
                CurrentPosition[i] = CurrentPosition[i] + translate;
                Tilemap.SetTile((Vector3Int)CurrentPosition[i], currentBlockData.tile);
            }
        }
        SetBlock();
        return move;
    }

    /// <summary>
    /// ������ �����Ѱ�?
    /// </summary>
    /// <returns></returns>
    public bool IsValidSpawn()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (!BoardRect.Contains((CurrentPosition[i])))
                return false;
            if (Tilemap.HasTile((Vector3Int)(CurrentPosition[i])))
                return false;
        }
        return true;
    }

    /// <summary>
    /// �̵��� �����Ѱ�?
    /// </summary>
    /// <param name="translate">�̵��� ��ǥ</param>
    /// <returns></returns>
    public bool IsValidMove(Vector2Int translate)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (!BoardRect.Contains((CurrentPosition[i] + translate)))
            {
                if (CurrentPosition[i].y + translate.y <= -BoardSize.y / 2)
                    Lock();

                return false;
            }
            if (Tilemap.HasTile((Vector3Int)(CurrentPosition[i] + translate)))
            {
                if (CurrentPosition[i].y > CurrentPosition[i].y + translate.y)
                    Lock();

                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// ȸ���� �����Ѱ�?
    /// </summary>
    /// <param name="translate">ȸ���� ��ǥ</param>
    /// <returns></returns>
    public bool IsValidRotate(Vector2Int translate)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (Tilemap.HasTile((Vector3Int)(CurrentPosition[i] + translate)))
            {
                 return false;
            }
        }
        return true;
    }

    #endregion
    
}

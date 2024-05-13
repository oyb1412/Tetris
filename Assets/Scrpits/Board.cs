using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// 보드 타일맵 관리
/// </summary>
public class Board : MonoBehaviour
{
    #region Variable
    //레벨 표기 텍스트
    [SerializeField]private Text levelText;
    //스코어 표기 텍스트
    [SerializeField] private Text scoreText;
    //파괴 라인 표기 텍스트
    [SerializeField] private Text linesText;
    //다음 블록 클래스
    [SerializeField] private NextBlock nextBlock;
    //각 종류별 블록 데이터
    [SerializeField] private BlockData[] blockData;
    //현재 블록 데이터
    private BlockData currentBlockData;
    //다음 블록 데이터
    public BlockData NextBlockData { get; private set; }
    //보드 타일맵
    public Tilemap Tilemap { get; private set; }
    //보드 시작 포지션
    [SerializeField]private Vector2Int startPosition;
    //이동 예정 포지션
    public Vector2Int Position {  get; private set; }   
    //현재 포지션
    public Vector2Int[] CurrentPosition { get; private set; }
    //블록의 각 셀
    private Vector2Int[] cells;
    //회전 정보 저장
    private Vector2Int[] rotateCells;
    //보드 사이즈
    public Vector2Int BoardSize { get; private set; } = new Vector2Int(10, 20);
    //블록의 자동 이동 딜레이
    private float stepDelay = 1f;
    //블록의 자동 이동 타이머
    private float stepTime;
    //회전 상태(정수)
    private int currentIndex;
    //현재 레벨
    private int currentLevel;
    //현재 점수
    private int currentScore;
    //현재 파괴한 라인 수
    private int currentLines;
    //난이도 증가 타이머
    private float levelUpDelay = 20f;

    //보드의 min, max, width, height정보
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

        //매 프레임마다 블록을 삭제
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

        //매 프레임마다 블록을 새로 드로우
        SetBlock();
    }

    /// <summary>
    /// 타이머 증가에 따른 게임 난이도 증가
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
    /// clamp효과
    /// </summary>
    /// <param name="input">효과를 적용할 값</param>
    /// <param name="min">최소 정수</param>
    /// <param name="max">최대 정수</param>
    private int Wrap(int input, int min, int max) {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

    #region BlockBehaivour

    /// <summary>
    /// 블록 회전
    /// </summary>
    /// <param name="rotationIndex">회전할 인덱스</param>
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
    /// 회전 시, 벽과의 충돌 체크
    /// </summary>
    /// <param name="currentIndex">현재 회전 인덱스</param>
    /// <param name="rotationIndex">다음 회전 인덱스</param>
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
    /// 벽과의 충돌 체크를 위한 회전정보 인덱스 반환
    /// </summary>
    /// <param name="currentIndex">현재 회전 인덱스</param>
    /// <param name="rotationIndex">다음 회전 인덱스</param>
    /// <returns></returns>
    private int GetWallKickIndex(int currentIndex, int rotationIndex)
    {
        int index = currentIndex * 2;
        if (rotationIndex < 0)
            index--;

        return Wrap(index, 0, currentBlockData.wallKick.GetLength(0));
    }

    /// <summary>
    /// 완성된 라인을 체크
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
    /// 완성된 라인을 클리어
    /// </summary>
    /// <param name="row">완성된 행</param>
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
    /// 라인이 꽉 찼나 체크
    /// </summary>
    /// <param name="row">체크할 행</param>
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
    /// 회전이 가능한 경우 회전
    /// </summary>
    /// <param name="rotationIndex">회전할 인덱스</param>
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
    /// 특정 키 입력 시 블록 즉시 드랍
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
    /// 블록 고정
    /// </summary>
    void Lock()
    {
        SetBlock();
        CrearLines();
        SpawnBlock();
        stepTime = 0;
    }

    /// <summary>
    /// 블록의 자동 이동
    /// </summary>
    void Step()
    {
        stepTime = 0;
        MoveBlock(Vector2Int.down);
    }

    /// <summary>
    /// 블록 스폰
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
    /// 게임 오버(모든 타일 클리어)
    /// </summary>
    private void GameOver()
    {
        Tilemap.ClearAllTiles();
    }

    /// <summary>
    /// 블록 새롭게 드로우
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
    /// 블록 제거
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
    /// 블록 이동(직접 조작)
    /// </summary>
    /// <param name="translate">이동할 위치</param>
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
    /// 블록 회전
    /// </summary>
    /// <param name="translate">회전할 좌표</param>
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
    /// 스폰이 가능한가?
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
    /// 이동이 가능한가?
    /// </summary>
    /// <param name="translate">이동할 좌표</param>
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
    /// 회전이 가능한가?
    /// </summary>
    /// <param name="translate">회전할 좌표</param>
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

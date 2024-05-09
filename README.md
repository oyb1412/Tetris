## **📃핵심 기술**

### ・충돌감지, 객체생성방식이 아닌, 타일맵을 덧그리는 방식으로 보드를 구현

🤔**WHY?**

테트리스라는 게임에서 보드는 지속적으로 생성되고, 반복적인 회전 및 이동이 발생하고, 특정 조건이 발생하면 객체의 일부만을 제거해야하는 상황이 발생한다.
이를 하나하나 생성을 하고 제거를 하고, 유니티의 충돌감지 시스템을 이용하여 보드끼리의 충돌을 감지하면 연산량이 급격하게 증가하기 때문에,
보드의 변화가 발생한 타일맵의 셀을 새로 그리는 방식으로 보드의 움직임을 구현

🤔**HOW?**

 관련 코드

- Board
    
```csharp
public class Board : MonoBehaviour
{
    private void Update()
    {
     DeleteBlock(); // 매 프레임마다 현재 블록을 지우고
     ChangeLevel();
     stepTime += Time.deltaTime;
     if (Input.GetKeyDown(KeyCode.A)) // 각 인풋에 따른 현재 블록의 데이터를 변경
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

     SetBlock();  //변경된 현재 블록의 데이터대로 타일맵을 새롭게 드로우
    }
}
```
    

🤓**Result!**

사용자가 이질감을 느끼지 않으면서도, 타일맵을 새롭게 드로우하는 방식으로 모든 로직을 구현할 수 있게 되어 최적화에 필요한 비용 대폭 감소

### ・블록이 도착할 위치를 표시해주는 섀도우 블록

🤔**WHY?**

사용자가 블록이 도착할 지점을 어림짐작하며 플레이하는 것이 아닌, 판단할 수 있는 확실한 무엇인가를 표현해주는 로직을 구현하기 위해 제작

🤔**HOW?**

 관련 코드
- Shadow
    
    ```csharp
    using UnityEngine;
    using UnityEngine.Tilemaps;
    
    public class Shadow : MonoBehaviour
    {
      private void LateUpdate() // 일반 블록을 따라하는 그림자기 때문에 LateUpdate에서 호출
      {
        DeleteBlock(); // 일반 블록과 동일하게, 타일맵을 지우고 새로 드로우하는 방식으로 작동
        Copy(); // 현재 조작중인 일반 블록의 데이터를 복제해 섀도우 블록에 적용
        Drop(); // LateUpdate와 Update간의 동기화를 위해 수동으로 섀도우 블록을 한 칸 내려준다.
        SetBlock(); // 일반 블록과 동일하게, 타일맵을 지우고 새로 드로우하는 방식으로 작동
      }
    }
    ```
    

🤓**Result!**

블록이 도착할 지점에 현재 블록과 동일한 섀도우 블록을 생성해, 사용자가 어떤 위치에서던 블록의 도착 위치, 형태등을 파악해 조금 더 전략적으로 게임을
진행 할 수 있도록 변화

### ・하드드롭, 회전, 이동 등 블록의 핵심 로직

🤔**WHY?**

테트리스의 핵심이 되는 이동, 회전 및 블록을 즉시 떨어뜨리는 하드드롭 기능을 구현

🤔**HOW?**

 관련 코드

- Board(이동)
    
    ```csharp
    public class Board : MonoBehaviour
    {
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
                tilemap.SetTile((Vector3Int)currentPosition[i], currentBlockData.tile); // 블록 이동 시, 데이터만을 변경
            }
        }
        SetBlock(); // 변경된 데이터를 바탕으로 타일맵을 덧그림
        return move;
    	 }
    }
    ```
    
- Board(회전)
    
    ```csharp
    public class Board : MonoBehaviour
    {
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
    }
    ```
    
- Board(하드드랍)
    
    ```csharp
    public class Board : MonoBehaviour
    {
       public void HardDrop()
    	{
        while(MoveBlock(Vector2Int.down))
        {
            DeleteBlock();
            continue;
        }
    	}
    }
    ```
    

🤓**Result!**

테트리스의 근간이 되는 로직들을 구현


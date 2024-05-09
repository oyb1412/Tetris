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

하나의 애너미 프리펩만을 사용하며, 애너미 생성 시 레벨에 맞는 데이터 및 애니메이션을 적용시켜 하나의 프리펩으로 여러 종류의 애너미를 관리할 수 있게 되어 관리 효율성 증가

### ・각종 무기 및 패시브형 스킬 구현

🤔**WHY?**

직접적으로 적을 공격하는 무기 및 능력치를 상승시켜주는 패시브형 스킬 구현

🤔**HOW?**

 관련 코드

- Weapon
    
    ```csharp
    using UnityEngine;
    
    public class Weapon : MonoBehaviour
    {
        public int id;
        public int prefabId;
        public float damage;
        public float coolTime;
        public int count =1;
        public int weaponType;
        float timer;
        float chargeTimer;
        bool chargeTrigger;
        public float range;
        public float baseRange;
        public float baseCoolTime;
        public float baseDamage;
        public ItemData data;
        
        private void Update()
        {
            if (!GameManager.instance.isLive)
                return;
    
            switch (id)
            {
                case 0:
                    if (Input.GetMouseButton(0) && timer > coolTime)
                    {
                        if (chargeTimer < 10)
                            chargeTimer += Time.deltaTime * 5;
    
                        if(chargeTimer > 0.5f)
                            GameManager.instance.player.ChargeaAnimation(true);
    
                        chargeTrigger = true;
                    }
                    if (Input.GetMouseButtonUp(0) && timer > coolTime)
                    {
                        GameManager.instance.player.ChargeaAnimation(false);
                        chargeTrigger = false;
    
                        timer = 0;
                        FireMoonSlash();
                        AudioManager.instance.PlayerSfx(AudioManager.Sfx.Sword);
    
                    }
                    if (!chargeTrigger)
                        timer += Time.deltaTime;
    
                    break;
                case 1:
                    timer += Time.deltaTime;
                    if(timer > coolTime)
                    {
                        timer = 0;
                        FireDagger();
                    }
                    break;
                case 7:
                    AutoRotate();
                    break;
                case 8:
                    timer += Time.deltaTime;
                    if(timer > coolTime)
                    {
                        timer = 0;
                        FireCross();
                    }
                    break;
            }
        }
    
        void FireMoonSlash()
        {
            Vector2 playerPos = GameManager.instance.player.transform.position;
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = mouse - playerPos;
            dir = dir.normalized;
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.parent = GameObject.Find("Weapon0").transform;
            bullet.transform.position = GameManager.instance.player.transform.position;
            bullet.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            bullet.transform.localScale = Vector3.one * (range + chargeTimer);
            bullet.GetComponent<Bullet>().Init(damage + chargeTimer * 15, id, dir, count);
            chargeTimer = 1;
    
        }
    
        void FireDagger()
        {
            //스캐너에 걸린 애너미가 없으면 함수 종료
            if (!GameManager.instance.player.scanner.target)
                return;
            //플레이어 포지션
            Vector3 playerPos = GameManager.instance.player.transform.position;
            //스캐너에 걸린 애너미 위치 
            Vector3 targetPos = GameManager.instance.player.scanner.target.position;
            //플레이어->애너미 벡터 저장후 정규화
            Vector3 vecDir = targetPos - playerPos;
            vecDir = vecDir.normalized;
            //풀 매니저에 새롭게 자식오브젝트로 생성
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            //불렛 오브젝트를 웨폰1 오브젝트의 하위로 이동
            bullet.parent = GameObject.Find("Weapon1").transform;
            //불렛 위치 초기화
            bullet.position = playerPos;
            //불렛의 쿼터니온 회전
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, vecDir);
    
            bullet.localScale = Vector3.one * range;
            //데미지와 관통력, 발사 방향 지정
            bullet.GetComponent<Bullet>().Init(damage, weaponType, vecDir, count);
        }
    
        void FireCross()
        {
            //스캐너에 걸린 애너미가 없으면 함수 종료
            if (!GameManager.instance.player.scanner.target)
                return;
            //플레이어 포지션
            Vector3 playerPos = GameManager.instance.player.transform.position;
            //스캐너에 걸린 애너미 위치 
            Vector3 targetPos = GameManager.instance.player.scanner.target.position;
            //플레이어->애너미 벡터 저장후 정규화
            Vector3 vecDir = targetPos - playerPos;
            vecDir = vecDir.normalized;
            //풀 매니저에 새롭게 자식오브젝트로 생성
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            //불렛 오브젝트를 웨폰1 오브젝트의 하위로 이동
            bullet.parent = GameObject.Find("Weapon8").transform;
            //불렛 위치 초기화
            bullet.position = playerPos;
            bullet.localScale = Vector3.one * range;
            bullet.GetComponent<Bullet>().Init(damage, id, vecDir, count);
    
        }
        public void LevelUp(float damage, float range)
        {
            this.damage = damage;
            this.range = range;
            count++;
    
            if (id == 7)
                Assign();
    
            GameManager.instance.player.BroadcastMessage("ApplyPassive", SendMessageOptions.DontRequireReceiver);
        }
    
        public void Init(ItemData data)
        {
            name = "Weapon" + data.itemId;
            transform.parent = GameManager.instance.player.transform;
            transform.localPosition = Vector3.zero;
    
            id = data.itemId;
            damage = data.damage / 10;
            count = data.count;
            range = data.range / 100;
            coolTime = data.CT / 100;
    
            baseRange = range;
            baseCoolTime = coolTime;
            baseDamage = damage;
            for (int i = 0; i<GameManager.instance.pool.prefabs.Length; i++)
            {
                if(data.weaponObject == GameManager.instance.pool.prefabs[i])
                {
                    prefabId = i;
                    break;
                }
            }
    
            weaponType = (int)data.itemType;
            switch (id)
            {
                case 7:
                    Assign();
                    break;
            }
    
            GameManager.instance.player.BroadcastMessage("ApplyPassive", SendMessageOptions.DontRequireReceiver);
        }
    
        //무기 생성, 배치
        void Assign()
        {
            //무기의 갯수만큼 반복 실행
            for(int i = 0; i< count; i++)
            {
                Transform bullet;
                if (i < transform.childCount)
                {
                    //그 자식 오브젝트를 그대로 사용
                    bullet = transform.GetChild(i);
                }
                //레벨업으로 인해 새롭게 배치를 진행할 경우
                else
                {
                    bullet = GameManager.instance.pool.Get(prefabId).transform;
                    bullet.parent = transform;
                }
                //생성직후 위치 0,0,0으로 초기화
                bullet.transform.position = GameManager.instance.player.transform.position;
    
                //생성직후 회전량 초기화
                bullet.transform.rotation = Quaternion.identity;
    
                //z축을 기준으로 360 * index * count로 각도 조정
                bullet.transform.Rotate(Vector3.forward * 360 * i / count);
    
                //각도 조정 후 y축으로 위치이동
                bullet.transform.Translate(0, 1.5f, 0f);
    
                bullet.localScale = Vector3.one * range;
    
                //불렛의 초기화 진행
                bullet.GetComponent<Bullet>().Init(damage, id, Vector3.zero, count);
            }
    
        }
    
        //무기 자동회전 함수
        void AutoRotate()
        {
               //z축을 기준으로 속도만큼 자동회전
               transform.Rotate(Vector3.forward , (300f - (100 - (coolTime * 10))) * Time.deltaTime);
        }
    
    }
    ```
    
- PassiveItem
    
    ```csharp
    using UnityEngine;
    
    public class PassiveItem : MonoBehaviour
    {
        public ItemData.ItemType itemDate;
        public float value;
    
        public void Init(ItemData data)
        {
            name = "Passive" + data.itemId;
            transform.parent = GameManager.instance.player.transform;
            transform.localPosition = Vector3.zero;
    
            itemDate = data.itemType;
            switch (itemDate)
            {
                case ItemData.ItemType.Damage:
                    value = data.upgradeDamages[0];
                    break;
                case ItemData.ItemType.Range:
                    value = data.upgradeRange[0];
                    break;
                case ItemData.ItemType.CoolTime:
                    value = data.upgradeCT[0];
                    break;
                case ItemData.ItemType.MoveSpeed:
                    value = data.upgradeMoveSpeed[0];
                    break;
            }
            ApplyPassive();
        }
    
        public void LevelUp(float value)
        {
            this.value = value;
            ApplyPassive();
        }
    
        void CoolDown()
        {
            Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
    
            foreach (Weapon weapon in weapons)
            {
                weapon.coolTime = weapon.baseCoolTime * (1 - value);
            }
        }
    
        void MoveUp()
        {
            GameManager.instance.player.speed = GameManager.instance.player.baseSpeed * (1 + value);
        }
    
        void DamageUp()
        {
            Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
    
            foreach (Weapon weapon in weapons)
            {
               weapon.damage = weapon.baseDamage * (1 + value);
            }
        }
    
        void RangeUp()
        {
            Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
    
            foreach (Weapon weapon in weapons)
            {
                float basevalue = 1 + value;
                weapon.range = weapon.baseRange * (1 + value);
            }
        }
    
        void ApplyPassive()
        {
            switch (itemDate)
            {
                case ItemData.ItemType.Damage:
                    DamageUp();
                    break;
                case ItemData.ItemType.Range:
                    RangeUp();
                    break;
                case ItemData.ItemType.CoolTime:
                    CoolDown();
                    break;
                case ItemData.ItemType.MoveSpeed:
                    MoveUp();
                    break;
            }
        }
    }
    
    ```
    

🤓**Result!**

무기 및 스킬 습득 및 레벨업, 레벨업 시 무기의 공격력 같은 단순한 데이터 뿐만이 아닌 무기의 크기, 숫자 등 외형적인 변화도 추가해 플레이어가 직접적으로 변화를 감지할 수 있도록 함

### ・풀링 오브젝트 시스템

🤔**WHY?**

각종 오브젝트를 필요할 때 마다 생성, 필요가 없어지면 제거해 짧은 시간 내에 다량의 객체를 생성하는 상황이 발생 시 심각한 퍼포먼스 하락 발생

🤔**HOW?**

 관련 코드

- PoolManager
    
    ```csharp
    using System.Collections.Generic;
    using UnityEngine;
    
    public class PoolManager : MonoBehaviour
    {
        public GameObject[] prefabs;
        public List<GameObject>[] poolList;
    
        void Start()
        {
            //리스트 배열 크기, 내용 초기화
            poolList = new List<GameObject>[prefabs.Length];
            
            for(int i = 0;i < poolList.Length; i++) 
            {
                poolList[i] = new List<GameObject>();
            }
        }
    
        //직접적인 풀링을 담당할 함수
        public GameObject Get(int index)
        {
            GameObject obj = null;
                //오브젝트 풀링으로 생성된 모든 오브젝트를 검사
                foreach (GameObject item in poolList[index])
                {
                    // 비활성화 된(재활용 가능한)오브젝트가 있다면
                    if (!item.activeSelf)
                    {
                        obj = item;// 재활용
                        obj.SetActive(true); // 활성화
                        break;// 함수 종료
                    }
                }
            
            //재활용 가능한 오브젝트가 없다면
            if (!obj)
            {
                //오브젝트를 새롭게 생성 후
                obj = Instantiate(prefabs[index],transform);
    
                // 풀 리스트에 추가
                poolList[index].Add(obj);
            }
    
            return obj;
        }
    }
    ```
    

🤓**Result!**

  객체의 직접적인 생성 / 파괴를 최대한 피하고 풀링 시스템을 이용, 이미 생성된 객체를 재사용하는 과정을 통해 객체의 생성에 들어가는 비용을 줄여 퍼포먼스 상승

## 📈보완점

🤔**문제점**

에디터 상에선 문제가 없었던 로직이, 빌드상에선 계속해서 문제가 발생

🤔**문제의 원인**

물리 효과를 FixedUpdate가 아닌 Update상에서 구현해, 컴퓨터 성능 및 빌드 / 에디터 상의 차이로 인한 문제 발생이 원인

🤓**해결방안**

게임 내 모든 물리로직들을 Update가 아닌 FixedUpdate에서 구현해, 어떤 상황에서도 동일하게 동작하도록 변경

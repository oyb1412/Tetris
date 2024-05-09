## **📃핵심 기술**

### ・플레이어 이동에 따른 맵 위치 동기화

🤔**WHY?**

맵의 위치가 고정되어있어, 플레이어가 이동시 맵을 벗어나버리는 문제 발생

🤔**HOW?**

 관련 코드

- Reposition
    
    ```csharp
    using UnityEngine;
    
    public class Reposition : MonoBehaviour
    {
        //오브젝트간 충돌에서 벗어날때 호출되는 함수
        private void OnTriggerExit2D(Collider2D collision)
        {
            //타일맵과 에리어(플레이어 주변 영역)간의 충돌이 아니면 무시
            if (!collision.CompareTag("Area"))
                return;
    
            //타일맵의 이동 위치를 정하기 위해 각종 정보를 취득한다.
            //플레이어 위치 저장
            Vector2 playerPos = GameManager.instance.player.transform.position;
    
            //타일맵 위치 저장
            Vector2 tilePos = transform.position;
    
            //플레이어와 타일간의 거리를 각각 x,y로 절대값으로 저장
            Vector2 curDiff;
            curDiff.x = Mathf.Abs(playerPos.x - tilePos.x);
            curDiff.y = Mathf.Abs(playerPos.y - tilePos.y);
    
            //플레이어 방향 저장
            Vector2 playerVec = GameManager.instance.player.inputVec;
    
            //플레이어 방향을 근거로 타일맵의 이동경로 상하좌우를 1,-1로 저장
            float dirX = playerVec.x > 0 ? 1 : -1;
            float dirY = playerVec.y > 0 ? 1 : -1;
    
            //취득한 정보를 토대로 타일맵의 위치를 변환한다.
            switch(transform.tag)
            {
                //어떤 태그의 오브젝트를 변경할지 결정
                //태그가 그라운드(타일맵)일때
                case "Ground":
                    //플레이어가 타일맵의 x축 방향으로 탈출하려 할 때
                    if(curDiff.x > curDiff.y)
                    {
                        //타일을 x축 방향으로 타일맵크기*2만큼 이동
                        transform.Translate(new Vector3(dirX * 40f, 0f, 0f));
                    }
                    //플레이어가 타일맵의 y축 방향으로 탈출하려 할 때
                    if (curDiff.y > curDiff.x)
                    {
                        //타일을 y축 방향으로 타일맵크기*2만큼 이동
                        transform.Translate(new Vector3(0f, dirY * 40f, 0f));
                    }
                    break;
                case "Enemy":
                    if (curDiff.x > curDiff.y)
                    {
                        transform.Translate(new Vector3(dirX * 25f + Random.Range(-3f,3f), Random.Range(-3f, 3f), 0f));
                    }
                    if (curDiff.y > curDiff.x)
                    {
                        transform.Translate(new Vector3(Random.Range(-3f, 3f), dirY * 25f + Random.Range(-3f, 3f), 0f));
                    }
                    break;
            }
        }
    }
    ```
    

🤓**Result!**

플레이어가 4등분된 맵의 한 부분을 벗어날 시, 남겨진 맵을 반대 방향으로 이동시켜 어느 방향으로 이동해도 맵이 무한히 이동되는듯한 효과를 연출

### ・하나의 프리펩으로 여러 애너미 관리

🤔**WHY?**

애너미의 종류가 증가할 수록 애너미 프리펩의 수도 증가해, 점점 프리펩 관리가 힘들어지는 문제 발생

🤔**HOW?**

 관련 코드

- Spawner
    
    ```csharp
    using UnityEngine;
    
    public class Spawner : MonoBehaviour
    {
        //스폰 포인트를 랜덤으로 하기 위해 자식 오브젝트로 여러개 설정. 저장하기 위한 배열 변수
        public Transform[] spawnerPoint;
        public SpawnDate[] spawnDate;
        public float eleteSpawn = 5;
        //애너미의 스폰율을 조정하기 위한 타이머
        float timer;
        float eleteTimer;
        public int gameLevel = 0;
        // Start is called before the first frame update
        void Start()
        {
            //자식 오브젝트로 초기화
            spawnerPoint = GetComponentsInChildren<Transform>();
        }
    
        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.isLive)
                return;
    
            gameLevel = Mathf.Min(GameManager.instance.minTime , spawnDate.Length - 2);
            Spawn();
        }
        //게임레벨로 몹 소환
        //
        void Spawn()
        {
            timer += Time.deltaTime;
            eleteTimer += Time.deltaTime;
            if(eleteTimer > eleteSpawn)
            {
                //사용할 프리펩을 파라매터로 입력
                GameObject enemy = GameManager.instance.pool.Get(0);
    
                //스폰된 애너미의 위치는 여러개의 스폰 포인트중 랜덤하게 지정
                enemy.transform.position = spawnerPoint[UnityEngine.Random.Range(1, spawnerPoint.Length)].position;
                enemy.GetComponent<Enemy>().Init(spawnDate[5]);
                eleteTimer = 0;
            }
            if (timer > spawnDate[gameLevel].spawnTime)
            {
                //사용할 프리펩을 파라매터로 입력
                GameObject enemy = GameManager.instance.pool.Get(0);
                //스폰된 애너미의 위치는 여러개의 스폰 포인트중 랜덤하게 지정
                enemy.transform.position = spawnerPoint[UnityEngine.Random.Range(1, spawnerPoint.Length)].position;
                enemy.GetComponent<Enemy>().Init(spawnDate[gameLevel]);
                timer = 0;
            }
        }
    }
    
    [System.Serializable]
    public class SpawnDate
    {
        public float spawnTime;
        public int SpriteType;
        public int health;
        public float speed;
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

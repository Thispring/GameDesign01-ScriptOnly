using System.Collections;
using System;
using UnityEngine;

// Enemy의 생성 관리를 위한 메모리 풀 스크립트 입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyMemoryPool : MonoBehaviour
{
    public static Action onSpawnTileAction; // 스폰 타일 생성 시 호출되는 액션
    public static EnemyMemoryPool Instance { get; private set; }    // EnemyMemoryPool의 인스턴스를 저장하는 정적 프로퍼티(싱글톤)

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    private MemoryPool spawnPointMemoryPool;    // 적 등장 위치를 알려주는 오브젝트 생성, 활성/비활성 관리
    [SerializeField]    // Inspector에서 끌어서 사용
    private StageManager stageManager; // 스테이지 매니저 스크립트의 인스턴스

    [Header("Enemy Prefabs")]
    [SerializeField]
    private GameObject enemySpawnPointPrefab; // 적이 등장하기 전 적의 등장 위치를 알려주는 프리팹
    [SerializeField]
    private GameObject smallEnemyPrefab; // 생성되는 적 프리팹
    [SerializeField]
    private GameObject mediumEnemyPrefab; // 생성되는 중형 적 프리팹
    [SerializeField]
    private GameObject bigEnemyPrefab; // 생성되는 대형 적 프리팹

    [Header("Elite Enemy Prefabs")]
    [SerializeField]
    private GameObject eliteSmallEnemyPrefab;
    [SerializeField]
    private GameObject eliteMediumEnemyPrefab;
    [SerializeField]
    private GameObject eliteBigEnemyPrefab;

    [Header("Enemy Array")]
    [SerializeField]
    private GameObject[] enemyPrefabArray; // 소환할 프리팹을 저장하는 배열

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public float enemySpawnTime = 2;   // 적이 생성되는 주기
    public float enemySpawnLatency = 1; // 적이 재생성되기 까지의 대기시간
    public float enemyTileLatency = 3; // 적의 타일이 재생성되기 까지의 대기시간
    private int numberOfEnemySpawnedAtOnce = 1; // 동시에 생성되는 적의 수
    private int currentNumber = 0;  // 현재 생성된 적의 수, 루프마다 초기화
    public int killScroe = 0;   // 처치한 적의 수
    public int maximumNumber;  // 최대 나오는 enemy 개수, smokeSpawn을 위해 public으로 설정
    private int eliteEnemyCount = 0; // elite 소환 개수

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isClearStage = false; // 스테이지 클리어 여부
    public bool stageCheck = false; // EnemyMemoryPool에서 스테이지를 체크하는 변수

    [Header("Target")]
    [SerializeField]
    private Transform target; // 적이 공격할 타겟

    [Header("Position")]
    private Vector3[] spawnPositions; // 스폰된 위치를 저장하는 배열

    [Header("Spawn Area")]
    public GameObject rightHighSpawnArea;  // 오른쪽 상단 스폰구역
    public GameObject leftHighSpawnArea;  // 왼쪽 상단 스폰구역
    public GameObject mediumLowSpawnArea;   // Medium 타입 스폰구역
    public GameObject bigLowSpawnArea;   // Big 타입 스폰구역
    public GameObject rightStartArea;   // Small 타입 오른쪽 스폰 시작 구역
    public GameObject leftStartArea;    // Small 타입 왼쪽 스폰 시작 구역

    // 스폰 구역의 Bounds 값 저장 변수들
    private Bounds rightHighSpawnBounds;
    private Bounds leftHighSpawnBounds;
    private Bounds mediumLowSpawnBounds;
    private Bounds bigLowSpawnBounds;
    private Bounds rightStartBounds;
    private Bounds leftStartBounds;

    [Header("Start Text Effect")]
    [SerializeField]
    private GameObject textNum1;
    [SerializeField]
    private GameObject textNum2;
    [SerializeField]
    private GameObject textNum3;
    [SerializeField]
    private GameObject textStart;

    void Awake()
    {
        maximumNumber = 5;

        // 설정된 SpawnArea의 Bounds값 저장
        rightHighSpawnBounds = rightHighSpawnArea.GetComponent<Collider>().bounds;
        leftHighSpawnBounds = leftHighSpawnArea.GetComponent<Collider>().bounds;

        mediumLowSpawnBounds = mediumLowSpawnArea.GetComponent<Collider>().bounds;
        bigLowSpawnBounds = bigLowSpawnArea.GetComponent<Collider>().bounds;

        rightStartBounds = rightStartArea.GetComponent<Collider>().bounds;
        leftStartBounds = leftStartArea.GetComponent<Collider>().bounds;

        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 초기 배열 생성
        enemyPrefabArray = new GameObject[maximumNumber];
        spawnPositions = new Vector3[maximumNumber]; // 스폰 위치 배열 초기화

        // 액션에 코루틴 등록
        onSpawnTileAction += () => StartCoroutine(SpawnTile());

        textNum1.SetActive(false);
        textNum2.SetActive(false);
        textNum3.SetActive(false);
        textStart.SetActive(false);
    }

    void OnDestroy()
    {
        onSpawnTileAction = null;
    }

    void Start()
    {
        StartCoroutine(StartTextEffect());
    }

    void Update()
    {
        // 만약 gameScore(적 처지 수)가 maximumNumber(생성되는 최대 적 수)와 같다면, 일시정지 이후 '상점' 열기
        // stageCheck 변수로 여러번 연산 방지
        if (killScroe == maximumNumber && stageCheck == false)
        {
            StartCoroutine(PasueAfterDelay());
        }
    }

    // 시작 시, 화면에 보여줄 이펙트, for문으로 반복 변경 
    private IEnumerator StartTextEffect()
    {
        textNum3.SetActive(true);
        yield return new WaitForSeconds(1f);
        textNum2.SetActive(true);
        yield return new WaitForSeconds(1f);
        textNum1.SetActive(true);
        yield return new WaitForSeconds(1f);
        textStart.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        textNum1.SetActive(false);
        textNum2.SetActive(false);
        textNum3.SetActive(false);
        textStart.SetActive(false);

        // 게임 시작, 액션 타일 호출
        onSpawnTileAction?.Invoke();
    }

    // stage끝난 이후 다음 스테이지에 대한 설정을 진행
    private IEnumerator PasueAfterDelay()
    {
        stageCheck = true;
        stageManager.gameScore += killScroe; // 스테이지 매니저의 gameScore에 처치한 수를 더함
        killScroe = 0; // 적 처치 수 초기화 하여, 스테이지 진행
        eliteEnemyCount = 0;
        stageManager.PauseGame(); // 로그라이트 함수 실행

        // 스테이지 번호 별, 생성되는 수, 시간을 조절하여 난이도 설정
        switch (stageManager.stageNumber)
        {
            case <= 3: // 스테이지 1~3
                maximumNumber += stageManager.stageNumber;
                enemySpawnTime -= 0.1f;
                enemyTileLatency -= 0.1f;
                break;

            case >= 4 and <= 6: // 스테이지 4~6
                maximumNumber += stageManager.stageNumber;
                enemySpawnTime -= 0.05f;
                enemyTileLatency -= 0.05f;
                break;

            case >= 7 and <= 10: // 스테이지 8~10
                maximumNumber += 1;
                break;

            default:
                break;
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator SpawnTile()
    {
        stageManager.firstStage = true;
        stageCheck = false;
        yield return new WaitForSeconds(enemyTileLatency); // 스테이지 시작 2초 대기

        // 배열 초기화 및 랜덤 프리팹 선택
        enemyPrefabArray = new GameObject[maximumNumber];
        spawnPositions = new Vector3[maximumNumber]; // 스폰 위치 배열 초기화

        for (int i = 0; i < maximumNumber; i++)
        {
            // Normal 타입 enemy 랜덤 소환 
            int randomValue = UnityEngine.Random.Range(0, 101); // 0~100
            // Elite 타입 enemy 랜덤 소환
            int eliteRandomValue = UnityEngine.Random.Range(0, 3); // 0: Small, 1: Medium, 2: Big

            // 3스테이지 이하에서는 엘리트 소환 X
            if (stageManager.stageNumber <= 2 || randomValue < 70)
            {
                // 일반 enemy 소환
                switch (stageManager.stageNumber)
                {
                    case <= 2: // 1~2스테이지
                        enemyPrefabArray[i] = (randomValue <= 71) ? smallEnemyPrefab : mediumEnemyPrefab;
                        break;
                    case >= 3 and <= 7:
                        if (randomValue <= 61)
                            enemyPrefabArray[i] = smallEnemyPrefab;
                        else if (randomValue <= 90)
                            enemyPrefabArray[i] = mediumEnemyPrefab;
                        else
                            enemyPrefabArray[i] = bigEnemyPrefab;
                        break;
                    case >= 8 and <= 10:
                        if (randomValue <= 51)
                            enemyPrefabArray[i] = smallEnemyPrefab;
                        else if (randomValue <= 80)
                            enemyPrefabArray[i] = mediumEnemyPrefab;
                        else
                            enemyPrefabArray[i] = bigEnemyPrefab;
                        break;
                    default:
                        enemyPrefabArray[i] = smallEnemyPrefab;
                        break;
                }
            }
            else
            {
                // 엘리트 enemy 소환 (3스테이지 이상만)
                int maxElite = (stageManager.stageNumber >= 8) ? 3 : 2;
                if (eliteEnemyCount < maxElite)
                {
                    switch (eliteRandomValue)
                    {
                        case 0:
                            enemyPrefabArray[i] = eliteSmallEnemyPrefab;
                            break;
                        case 1:
                            enemyPrefabArray[i] = eliteMediumEnemyPrefab;
                            break;
                        case 2:
                            enemyPrefabArray[i] = eliteBigEnemyPrefab;
                            break;
                    }
                    eliteEnemyCount++;
                }
                else
                {
                    // 엘리트 최대치 도달 시 일반 enemy로 대체
                    switch (stageManager.stageNumber)
                    {
                        case >= 3 and <= 7:
                            if (randomValue <= 61)
                                enemyPrefabArray[i] = smallEnemyPrefab;
                            else if (randomValue <= 90)
                                enemyPrefabArray[i] = mediumEnemyPrefab;
                            else
                                enemyPrefabArray[i] = bigEnemyPrefab;
                            break;
                        case >= 8 and <= 10:
                            if (randomValue <= 51)
                                enemyPrefabArray[i] = smallEnemyPrefab;
                            else if (randomValue <= 80)
                                enemyPrefabArray[i] = mediumEnemyPrefab;
                            else
                                enemyPrefabArray[i] = bigEnemyPrefab;
                            break;
                        default:
                            enemyPrefabArray[i] = smallEnemyPrefab;
                            break;
                    }
                }
            }
        }

        currentNumber = 0;

        while (stageManager.stageStart)
        {
            // ★★★ 핵심 수정: 남은 적 수만큼만 반복 ★★★ //
            int spawnCount = Mathf.Min(numberOfEnemySpawnedAtOnce, maximumNumber - currentNumber);
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem();

                Vector3 newPosition;
                bool validPosition;
                Bounds spawnBounds;
                // Small Enemy 소환 위치, 랜덤으로 결정 (1: 오른쪽, 0: 왼쪽)
                int randomSide = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;

                // 스폰 바운드 결정
                if (enemyPrefabArray[currentNumber] == smallEnemyPrefab ||
                    enemyPrefabArray[currentNumber] == eliteSmallEnemyPrefab)
                {
                    spawnBounds = randomSide == 1 ? rightHighSpawnBounds : leftHighSpawnBounds;
                }
                else if (
                    enemyPrefabArray[currentNumber] == mediumEnemyPrefab ||
                    enemyPrefabArray[currentNumber] == eliteMediumEnemyPrefab
                )
                {
                    spawnBounds = mediumLowSpawnBounds;
                }
                else
                {
                    spawnBounds = bigLowSpawnBounds;
                }

                int attemptCount = 0;
                int maxAttempts = 50;
                float minDistance = 3.0f;

                do
                {
                    // 엘리트 enemy라면 z축을 min.z로 고정하여 맨 앞에 배치
                    bool isElite =
                        enemyPrefabArray[currentNumber] == eliteSmallEnemyPrefab ||
                        enemyPrefabArray[currentNumber] == eliteMediumEnemyPrefab ||
                        enemyPrefabArray[currentNumber] == eliteBigEnemyPrefab;

                    if (spawnBounds == mediumLowSpawnBounds || spawnBounds == bigLowSpawnBounds)
                    {
                        if (isElite)
                        {
                            newPosition = new Vector3(
                                UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                                spawnBounds.center.y,
                                spawnBounds.min.z // z축을 최소값으로 고정
                            );
                        }
                        else
                        {
                            newPosition = new Vector3(
                                UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                                spawnBounds.center.y,
                                UnityEngine.Random.Range(spawnBounds.min.z, spawnBounds.max.z)
                            );
                        }
                    }
                    else
                    {
                        if (isElite)
                        {
                            newPosition = new Vector3(
                                UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                                UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                                spawnBounds.min.z // z축을 최소값으로 고정
                            );
                        }
                        else
                        {
                            newPosition = new Vector3(
                                UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                                UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                                UnityEngine.Random.Range(spawnBounds.min.z, spawnBounds.max.z)
                            );
                        }
                    }

                    validPosition = true;
                    for (int j = 0; j < currentNumber; j++)
                    {
                        if (Vector3.Distance(spawnPositions[j], newPosition) < minDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    attemptCount++;

                    if (attemptCount >= maxAttempts)
                    {
                        Debug.LogWarning("적 스폰 위치가 반복 충돌로 강제로 배정됨.");
                        break;
                    }

                } while (!validPosition);

                item.transform.position = newPosition;
                spawnPositions[currentNumber] = newPosition;

                StartCoroutine(SpawnEnemy(item, currentNumber, newPosition, randomSide));
                currentNumber++;
            }

            if (currentNumber >= maximumNumber)
            {
                break;
            }

            yield return new WaitForSeconds(enemySpawnTime);
        }
    }

    private IEnumerator SpawnEnemy(GameObject point, int index, Vector3 spawnPosition, int randomSide)
    {
        yield return new WaitForSeconds(enemySpawnLatency); // 적이 재생성되기 까지의 대기시간

        // 배열에서 프리팹 가져오기
        GameObject prefabToSpawn = enemyPrefabArray[index];
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"배열의 인덱스 {index}에 프리팹이 없습니다.");
            yield break;
        }

        // 적 오브젝트를 생성하고, 적의 위치를 point의 위치로 설정
        GameObject item = Instantiate(prefabToSpawn, point.transform.position, Quaternion.identity);

        // EnemyFSM에 EnemyMemoryPool 참조 전달
        EnemyFSM enemyFSM = item.GetComponentInChildren<EnemyFSM>();
        EnemyStatusManager enemyStatusManager = item.GetComponentInChildren<EnemyStatusManager>();

        if (enemyStatusManager != null && enemyStatusManager.enemySetting.enemyName == EnemyName.Small) // enemyType이 Small인지 확인
        {
            if (enemyStatusManager.enemySetting.isEliteEnemy == false)
            {
                enemyStatusManager.sliderOffset = new Vector3(0, 6, 0);
            }
            else
            {
                enemyStatusManager.sliderOffset = new Vector3(0, 8, 0);
            }
        }

        // EnemySmallSpawn 컴포넌트에 스폰된 위치 전달
        EnemySmallSpawn smallSpawn = item.GetComponentInChildren<EnemySmallSpawn>();
        if (smallSpawn != null)
        {
            // 랜덤 방향에 따라 적절한 Bounds 전달 (1: 오른쪽, 0: 왼쪽)
            Bounds spawnBounds = randomSide == 1 ? rightStartBounds : leftStartBounds;
            smallSpawn.StartMovement(spawnPosition, randomSide, spawnBounds); // Bounds 전달
        }
        else
        {
            Debug.Log("EnemySmallSpawn 컴포넌트를 찾을 수 없습니다.");
            // EnemySmallSpawn이 없는 경우에도 기본 스폰 처리
        }

        if (enemyStatusManager != null)
        {
            // 스테이지 번호 별, enemy의 체력과 공격력을 조절하여 난이도 설정
            switch (stageManager.stageNumber)
            {
                case <= 3: // 스테이지 1~3
                    enemyStatusManager.IncreaseEnemyHP(stageManager.stageNumber);
                    break;

                case >= 4 and <= 7: // 스테이지 4~7
                    numberOfEnemySpawnedAtOnce = 2;
                    enemyStatusManager.IncreaseEnemyHP(stageManager.stageNumber);
                    enemyStatusManager.IncreaseEnemyDamage(stageManager.stageNumber);
                    break;

                case >= 8 and <= 10: // 스테이지 8~10
                    numberOfEnemySpawnedAtOnce = 3;
                    enemyStatusManager.IncreaseEnemyHP(stageManager.stageNumber * 4);
                    enemyStatusManager.IncreaseEnemyDamage(stageManager.stageNumber);
                    break;

                default:
                    break;
            }
            // Setup 메서드 호출하여 Enemy행동 실행
            enemyFSM.Setup(target);
        }
        else
        {
            Debug.LogError("EnemyFSM 컴포넌트를 찾을 수 없습니다.");
        }

        // 배열의 해당 인덱스를 Null로 설정
        enemyPrefabArray[index] = null;

        spawnPointMemoryPool.DeactivatePoolItem(point); // 스폰 기둥 비활성화
    }
}

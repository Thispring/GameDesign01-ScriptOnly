using UnityEngine;
using System.Collections;

// 스테이지마다 Enemy의 숫자를 계산하여, 필드에 스모크를 소환하여 시야방해 목적의 스크립트 입니다.
// 특정 스테이지 이후, 주어진 시간동안 특정 수의 Enemy를 처치할 때까지 스모크를 소환합니다.
// 스크립트 목적 상, 플레이어를 방해하므로 Enemy 폴더에 분류
public class SmokeSpawnManager : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private EnemyMemoryPool enemyMemoryPool;    // 각 스테이지 별, 총 소환되는 enemy 숫자와, 처지된 숫자를 계산하기 위한 참조
    [SerializeField]
    private StageManager stageManager;  // stageNumber를 가져오기 위한 참조

    // 해당 스크립트에 필요한 프리팹, 게임오브젝트 
    [Header("Prefabs & GameObjects")]
    [SerializeField]
    private GameObject smokePrefab;
    [SerializeField]
    private GameObject smokeSpawnPoint; // 스모크가 소환될 위치를 지정하는 게임 오브젝트

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    [SerializeField]
    private float previousSpawnTime = 10f;  // 소환 전 대기 시간

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    [SerializeField]
    private bool isSmokeSpawned = false; // 스모크가 소환되었는지 여부

    // 기타 필요한 변수들
    [Header("Other Variables")]
    private Bounds smokeSpawnBounds; // 스모크 소환 범위

    // 오디오 관련 변수들
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip smokeSpawnAudioClip;

    void Awake()
    {
        // 초기화
        previousSpawnTime = 10f;
        isSmokeSpawned = false;

        if (smokeSpawnPoint != null)
        {
            // smokePrefab이 소환될 공간을 smokeSpawnPoint의 Collider를 가져와 smokeSpawnBounds로 설정
            smokeSpawnBounds = smokeSpawnPoint.GetComponent<Collider>().bounds;
        }
    }

    void Start()
    {
        if (stageManager != null)
            stageManager.onPauseGameEvent.AddListener(OnPauseHandler);
        // stageManager의 Pause 이벤트에 OnPauseHandler함수 등록
    }

    void Update()
    {
        // 4스테이지 부터, 10초가 지나면 시야방해 스모크 소환
        if (stageManager.stageNumber > 3 && stageManager.stageStart && !isSmokeSpawned && previousSpawnTime >= 0f)
        {
            previousSpawnTime -= Time.deltaTime;
            // 만약 스모크가 소환되지 않고, 이전 소환 시간이 0 이하라면
            if (!isSmokeSpawned && previousSpawnTime <= 0f)
            {
                StartCoroutine(spawnSmokeCoroutine());
            }

            // 만약 스테이지에 소환된 maximumNumber의 절반이상을 처치했다면
            if (enemyMemoryPool.maximumNumber / 2 < enemyMemoryPool.killScroe)
            {
                // isSmokeSpawned를 true로 설정하여 스모크 소환을 멈춤
                isSmokeSpawned = true;
            }
        }
    }

    // 스모크를 소환하는 Coroutine
    private IEnumerator spawnSmokeCoroutine()
    {
        isSmokeSpawned = true; // 스모크가 소환되었음

        // smokeSpawnBounds 내에서 랜덤 위치 선정, y축과 z축은 중앙값으로 고정
        Vector3 randomPos = new Vector3(
            Random.Range(smokeSpawnBounds.min.x, smokeSpawnBounds.max.x),
            smokeSpawnBounds.center.y,
            smokeSpawnBounds.center.z
        );

        smokePrefab.transform.position = randomPos; // 선정된 randomPos로 smokePrefab 위치 이동
        audioSource.PlayOneShot(smokeSpawnAudioClip); // 스모크 소환 사운드 재생
        smokePrefab.SetActive(true);    // 스모크 프리팹 활성화

        yield return new WaitForSeconds(3f); // 3초 후에 스모크 비활성화
        smokePrefab.SetActive(false);   // 스모크 프리팹 비활성화
        isSmokeSpawned = false; // 스모크가 소환되지 않았음으로 설정함으로, 스모크 소환 반복
        previousSpawnTime = 10f; // 시간 초기화
    }

    // 스테이지가 끝난 후, 다음 스모크 소환을 위해 필요한 변수들을 초기화하는 함수
    private void OnPauseHandler()
    {
        isSmokeSpawned = false; // 스모크 여부 소환 초기화
        previousSpawnTime = 10f;   // 소환 시간 초기화
        smokePrefab.SetActive(false);   // 스모크 프리팹 비활성화
    }
}

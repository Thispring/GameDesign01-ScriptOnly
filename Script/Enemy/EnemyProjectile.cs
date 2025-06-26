using System.Collections;
using UnityEngine;

// Enemy 발사체에 대한 스크립트 입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyProjectile : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private PlayerStatusManager playerStatusManager;
    [SerializeField]
    private EnemyFSM enemyFSM;

    [Header("Vector3")]
    private Vector3 originPos; // 발사체의 원래 위치
    private Vector3 targetPos; // 발사체가 날아갈 목표 위치

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private float travelTime = 1f; // 타겟까지 도달하는 데 걸리는 시간
    private float acceleration; // 가속도 (스칼라 값)
    private float currentSpeed; // 현재 속도

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    [SerializeField]
    private bool isMoving = false; // 발사체 이동 상태 플래그

    void Awake()
    {
        // Rigidbody 추가
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; // 물리 계산 비활성화

        originPos = transform.position; // 발사체의 원래 위치 저장

        playerStatusManager = PlayerStatusManager.Instance; // player 엄폐 여부를 위한 Instance
        enemyFSM = GetComponentInParent<EnemyFSM>();    // 부모 EnemyFSM 스크립트 참조
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        // 부모 오브젝트가 비활성화되었는지 확인
        if (enemyFSM == null || !enemyFSM.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("부모 EnemyFSM 오브젝트가 비활성화되었습니다. 발사체를 반환합니다.");
            StartCoroutine(WaitAttack());
            return; // 더 이상 Update 로직을 실행하지 않음
        }

        if (isMoving && !gameObject.activeInHierarchy) return; // 이동 중이라면 로직 실행 안 함

        // 타겟 방향으로 회전
        transform.LookAt(targetPos);

        // 회전 보정: X축을 목표 방향으로 정렬 (90도 회전)
        transform.rotation *= Quaternion.Euler(90, 0, 0);

        // 타겟까지의 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        // 가속도 계산: a = 2 * d / t^2
        acceleration = 2 * distanceToTarget / (travelTime * travelTime);

        // 속도 계산: v = v0 + at
        currentSpeed += acceleration * Time.fixedDeltaTime;

        // 이동 방향 계산 (targetPos - 현재 위치)
        Vector3 direction = (targetPos - transform.position).normalized;

        // 이동
        transform.position += direction * currentSpeed * Time.fixedDeltaTime;

        // 목표 위치에 도달했는지 확인
        if (distanceToTarget <= 0.1f)
        {
            StartCoroutine(WaitAttack()); // 공격 종료 처리
        }
    }

    public void Setup(Vector3 position, EnemyFSM fsm)
    {
        targetPos = position;
        enemyFSM = fsm; // EnemyFSM 참조 설정
        currentSpeed = 0f; // 초기 속도 초기화
        isMoving = true; // 이동 상태 활성화

        // 이벤트 구독
        if (StageManager.Instance != null)
        {
            StageManager.Instance.onPauseGameEvent.RemoveListener(OnPauseGame);
            StageManager.Instance.onPauseGameEvent.AddListener(OnPauseGame);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Turret"))
        {
            // EnemySetting의 Enemy Damage 값을 사용
            other.GetComponent<Turret>().TakeDamage(enemyFSM.status.enemySetting.enemyDamage);
            StartCoroutine(WaitAttack());
        }
        else if (playerStatusManager.isHiding == false && other.CompareTag("Player"))
        {
            // EnemySetting의 Enemy Damage 값을 사용
            other.GetComponent<PlayerStatusManager>().TakeDamage(enemyFSM.status.enemySetting.enemyDamage);
            StartCoroutine(WaitAttack());
        }
        else if (playerStatusManager.isHiding && other.CompareTag("Base"))
        {
            // EnemySetting의 Enemy Damage 값을 사용
            other.GetComponent<BaseStatus>().TakeDamage(enemyFSM.status.enemySetting.enemyDamage);
            StartCoroutine(WaitAttack());
        }
    }

    public void HitEMP()
    {
        StartCoroutine(WaitAttack());
    }

    private IEnumerator WaitAttack()
    {
        isMoving = false; // 이동 상태 비활성화

        if (!gameObject.activeInHierarchy)
        {
            yield break; // 이미 비활성화 상태라면 중단
        }

        gameObject.SetActive(false);
        transform.position = originPos;

        // FSM의 공격 신호를 초기화
        if (enemyFSM != null)
        {
            enemyFSM.attackSign = false; // 공격 신호 초기화
        }
        else
        {
            Debug.LogError("EnemyFSM 참조가 유효하지 않습니다.");
        }

        yield return null; // 다음 프레임까지 대기
    }

    private void OnPauseGame()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("EnemyProjectile이 비활성화된 상태에서 OnPauseGame 호출됨");
            return;
        }

        StartCoroutine(WaitAttack());
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (StageManager.Instance != null)
        {
            StageManager.Instance.onPauseGameEvent.RemoveListener(OnPauseGame);
        }
    }
}

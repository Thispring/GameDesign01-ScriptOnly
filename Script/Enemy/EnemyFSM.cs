using System.Collections;
using UnityEngine;

// Enemy의 행동을 관리하는 스크립트입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyFSM : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private EnemyMemoryPool enemyMemoryPool; // EnemyMemoryPool 참조
    public EnemyStatusManager status; // EnemyStatusManager 참조
    [SerializeField]
    private EnemyAnimatorController animator;   // 애니메이션 관리
    [SerializeField]
    private EnemyEffectController effectController; // 이펙트 관리
    [SerializeField]    // Inspector에서 끌어서 사용
    private EnemyAudioController enemyAudioController;  // 중형, 대형 움직임 사운드 관리

    // 공격 관련 변수
    [Header("Attack")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private Transform projectileSpawnPoint; // 발사체 발사 위치
    [SerializeField]    // Inspector에서 끌어서 사용
    private GameObject projectile; // 발사체 프리팹
    [SerializeField]
    private Transform target;   // Enemy가 공격할 타겟
    [SerializeField]
    private Transform originalTarget; // 기존 타겟 (플레이어)
    public bool attackSign = false; // 공격 신호를 위한 변수
    public Coroutine attackCoroutine; // 공격 코루틴 참조
    public int attackCount = 0; // 공격 횟수

    // 이동 관련 변수
    [Header("Move")]
    private int moveDirection; // 이동 방향 (-1: 왼쪽, 1: 오른쪽)
    public bool isMoving = true; // 움직임 상태 플래그
    public Coroutine moveCoroutine; // Move 코루틴 참조
    [SerializeField]    // Inspector에서 끌어서 사용
    private AudioClip enemyAttackAudio;   // enemy 공격 사운드
    [SerializeField]    // Inspector에서 끌어서 사용
    private AudioSource audioSource;  // AudioSource 컴포넌트

    [Header("Enemy Effect")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private GameObject stunEffect;  // EMP 이펙트
    [SerializeField]
    private GameObject stunTextEffect;

    void Awake()
    {
        effectController = GetComponent<EnemyEffectController>();
        animator = GetComponent<EnemyAnimatorController>();
        status = GetComponent<EnemyStatusManager>();

        stunEffect.SetActive(false);
        stunTextEffect.SetActive(false);
    }

    void Start()
    {
        // SoundEffect 조절을 위한 audioSource 등록
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && audioSource != null)
            soundManager.RegisterEnemyAudio(audioSource);

        // 랜덤 방향 설정 (-1: 왼쪽, 1: 오른쪽)
        moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        // TurretController에서 활성화된 터렛 목록 확인
        TurretController turretController = FindFirstObjectByType<TurretController>();
        if (turretController != null)
        {
            Transform closestTurret = GetClosestTurret(turretController.GetActiveTurrets());
            if (closestTurret != null)
            {
                target = closestTurret;
            }
        }

        attackSign = false;

        enemyMemoryPool = EnemyMemoryPool.Instance;

        if (enemyMemoryPool == null)
        {
            Debug.LogError("EnemyMemoryPool이 설정되지 않았습니다.");
        }
    }

    public void Setup(Transform target)
    {
        this.target = target; // 타겟 설정
        originalTarget = target; // 원래 타겟 저장
        StartCoroutine(WaitIdleTime()); // Idle 애니메이션을 위한 상태 대기
        StartCoroutine(WaitFirstAttackTime());
    }

    public void SetMemoryPool(EnemyMemoryPool memoryPool)
    {
        enemyMemoryPool = memoryPool;   // enemy의 메모리풀 설정
    }

    private IEnumerator WaitFirstAttackTime()
    {
        // Big 타입은 소환 애니메이션이 길기 때문에, 1.5초 동안 BoxCollider 비활성화 필요
        if (status.enemySetting.enemyName == EnemyName.Big)
        {
            // 모든 BoxCollider를 비활성화
            BoxCollider[] boxColliders = GetComponents<BoxCollider>();
            foreach (BoxCollider boxCollider in boxColliders)
            {
                boxCollider.enabled = false; // 활성화
            }
            yield return new WaitForSeconds(1.5f);
            foreach (BoxCollider boxCollider in boxColliders)
            {
                boxCollider.enabled = true; // 활성화
            }
        }

        yield return new WaitForSeconds(1f);
        attackCoroutine = StartCoroutine(Attack()); // 공격 코루틴 시작 
    }

    // 첫 소환 시, Idle 애니메이션 유지를 위한 대기 코루틴
    private IEnumerator WaitIdleTime()
    {
        // Small 타입의 경우, 비행을 하며 소환되는 컨셉으로, 별개로 관리
        EnemySmallSpawn enemySmallSpawn = GetComponent<EnemySmallSpawn>();
        if (enemySmallSpawn != null)
        {
            // enemySmallSpawn.isMoving이 true인 동안 대기
            while (enemySmallSpawn.isMoving)
            {
                attackSign = true;
                yield return null; // 다음 프레임까지 대기
                attackSign = false;
            }
        }
        else
        {
            if (status.enemySetting.enemyName == EnemyName.Big)
            {
                // 모든 BoxCollider를 비활성화
                BoxCollider[] boxColliders = GetComponents<BoxCollider>();
                foreach (BoxCollider boxCollider in boxColliders)
                {
                    boxCollider.enabled = false; // 비활성화
                }
            }
        }
    }

    // 시각적으로 Enemy가 공격대상을 바라보게 하는 함수    
    private void LookRotationToTarget()
    {
        // 목표 위치
        Vector3 to = new Vector3(target.position.x, transform.position.y, target.position.z);
        // 내 위치
        Vector3 from = transform.position;

        // 목표를 바라보도록 회전 (Z 축 기준)
        Vector3 direction = to - from;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        if (status.enemySetting.enemyName == EnemyName.Big)
        {
            transform.rotation = lookRotation * Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // 로컬 축 보정 (예: 180도 회전)
            transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
        }
    }

    //** <터렛 관련 메서드> **//
    // 터렛 소환 시 호출되는 메서드
    public void OnTurretSpawned(Transform turretTransform)
    {
        // 터렛이 소환되면 타겟을 터렛으로 변경
        target = turretTransform;
    }

    public void OnTurretDestroyed(Transform turretTransform)
    {
        // 터렛이 파괴되면 타겟을 기존 타겟(플레이어)으로 복원
        if (target == turretTransform)
        {
            target = originalTarget;
        }
    }

    private Transform GetClosestTurret(System.Collections.Generic.List<Transform> turrets)
    {
        // 터렛 리스트에서 활성화된 첫 번째 터렛 반환
        foreach (Transform turret in turrets)
        {
            if (turret.gameObject.activeSelf) // 터렛이 활성화된 경우
            {
                return turret; // 활성화된 터렛 반환
            }
        }

        return null; // 활성화된 터렛이 없으면 null 반환
    }

    //** <공격 관련 메서드> **//
    public void StartAttack()
    {
        StopMove(); // Move 코루틴 중지
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine); // 기존 Attack 코루틴 중지
        }
        attackCoroutine = StartCoroutine(Attack()); // 새로운 Attack 코루틴 시작
    }

    public void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine); // Attack 코루틴 중지
            attackCoroutine = null; // 참조 초기화
        }
        effectController.OffAttackEffect();
        attackSign = false; // 공격 신호 초기화
        animator.SetBool("isShot", false); // 공격 애니메이션 종료
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.5f); // 첫 스폰 후 1.5초간 대기

        while (status.isDead == false) // 적이 죽지 않은 동안만 실행
        {
            if (!attackSign)
            {
                animator.SetBool("isShot", true);
                effectController.OnAttackEffect();
                if (isMoving)
                    isMoving = false; // 이동 중지

                attackSign = true;
                LookRotationToTarget(); // 타겟을 바라보도록 회전

                // 미리 지정된 projectile 사용
                if (projectile != null)
                {
                    audioSource.PlayOneShot(enemyAttackAudio);
                    // 발사체의 위치와 회전 설정
                    projectile.transform.position = projectileSpawnPoint.position; // 발사 위치 설정
                    projectile.transform.rotation = projectileSpawnPoint.rotation; // 발사 방향 설정

                    // EnemyProjectile의 Setup 호출
                    EnemyProjectile projectileComponent = projectile.GetComponent<EnemyProjectile>();
                    if (projectileComponent != null)
                    {
                        projectileComponent.Setup(target.position, this); // 타겟과 EnemyFSM 전달
                    }
                    else
                    {
                        Debug.LogError("EnemyProjectile 컴포넌트를 찾을 수 없습니다.");
                    }

                    projectile.SetActive(true); // 발사체 활성화
                }
                else
                {
                    Debug.LogError("Projectile이 설정되지 않았습니다.");
                }
                yield return new WaitForSeconds(0.5f);
                attackCount++;
                effectController.OffAttackEffect();
                animator.SetBool("isShot", false);
                // 재장전 대기
                yield return Reload();
            }

            if (attackCount >= 2)
            {
                attackCount = 0; // 공격 횟수 초기화
                attackSign = true; // 공격 신호 초기화
                StartMove();
            }

            yield return null; // 다음 프레임까지 대기
        }
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(status.enemySetting.enemyAttackRate); // EnemySetting의 공격 속도 사용
    }

    //** <이동 관련 메서드> **//
    public void StartMove()
    {
        StopAttack(); // Attack 코루틴 중지
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine); // 기존 Move 코루틴 중지
        }
        isMoving = true;

        // 랜덤 방향 설정 (-1: 왼쪽, 1: 오른쪽)
        moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        animator.SetMoveDirection(moveDirection);
        moveCoroutine = StartCoroutine(Move()); // 새로운 Move 코루틴 시작
    }

    public void StopMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine); // Move 코루틴 중지
            moveCoroutine = null; // 참조 초기화
        }
        isMoving = false; // 이동 상태 초기화
        animator.SetMoveDirection(0); // 이동 애니메이션 종료
        attackSign = false;
    }

    private IEnumerator Move()
    {
        float moveTime = 0f; // 이동 누적 시간
        if (enemyAudioController != null) enemyAudioController.PlayMoveSound();
        while (isMoving)
        {
            if (!attackSign)
                attackSign = true;

            // 이동
            transform.position += Vector3.right * moveDirection * status.enemySetting.enemyMoveSpeed * Time.deltaTime;

            // 이동 시간 누적
            moveTime += Time.deltaTime;
            // 2초가 지나면 이동 멈춤
            if (moveTime >= 2f)
            {
                animator.SetMoveDirection(0);
                StartAttack();
                yield break;
            }

            // EndPoint와의 충돌(콜라이더 접촉) 체크, 충돌 시 이동 멈춤
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("EndPoint"))
                {
                    // EndPoint에 닿았을 때
                    animator.SetMoveDirection(0);
                    StartAttack();
                    yield break;
                }
            }

            yield return null;
        }
        yield return null;
    }

    //** <상태 이상 관련 메서드> **//
    public void HitEMP(int stunTime)
    {
        StartCoroutine(Stun(stunTime));
    }

    // EMP를 맞으면 5초간 소환된 Enemy 정지
    private IEnumerator Stun(int stunTime)
    {
        stunEffect.SetActive(true);
        stunTextEffect.SetActive(true);

        if (isMoving)
        {
            StopMove();
            yield return new WaitForSeconds(stunTime);
            StartMove();
            stunEffect.SetActive(false);
            stunTextEffect.SetActive(false);
        }

        if (!attackSign || attackSign)
        {
            StopAttack();
            yield return new WaitForSeconds(stunTime);
            StartAttack();
            stunEffect.SetActive(false);
            stunTextEffect.SetActive(false);
        }
    }
}

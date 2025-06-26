using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 튜토리얼에서 등장하는 Enemy를 관리하는 스크립트 입니다.
// 모든 튜토리얼에 관련한 스크립트는 Tutorial을 이름 앞에 붙여줍니다.
public class TutorialEnemyManager : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    public EnemySetting enemySetting; // 에너미 설정 (이미 존재)
    [SerializeField]
    private EnemyEffectController effectController; // 이펙트 관리
    [SerializeField]
    private EnemyAudioController enemyAudioController;
    [SerializeField]
    private EnemyAnimatorController animator;

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private float currentHP; // 현재 체력
    private float originalHP;
    public int criticalHit = 0;    // enemy 치명타 변수

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isDead = false; // 적이 죽었는지 여부를 확인하는 플래그

    [Header("UI")]
    [SerializeField]
    private Slider healthSlider; // 체력 슬라이더
    [SerializeField]
    public Vector3 sliderOffset = new Vector3(0, 2, 0); // 슬라이더 위치 오프셋

    [Header("Effect")]
    [SerializeField]
    private GameObject destoryEffect; // 파괴 이펙트
    [SerializeField]    // Inspector에서 끌어서 사용
    private GameObject stunEffect;  // EMP 이펙트
    [SerializeField]
    private GameObject stunTextEffect;
    [SerializeField]
    private GameObject bonusCoinTextEffect;

    [Header("Audio")]
    [SerializeField]
    private AudioSource statusAudioSource;
    [SerializeField]
    private AudioClip idleAudioClip;
    [SerializeField]
    private AudioClip deathAudioClip;
    [SerializeField]
    private AudioClip enemyAttackAudio;
    [SerializeField]    // Inspector에서 끌어서 사용
    private AudioSource attackAudioSource;  // AudioSource 컴포넌트

    [Header("Renderer")]
    // 튜토리얼용 무한 부활 시 비활성화 할 Renderer
    [SerializeField]
    private GameObject enemyRenderer;
    [SerializeField]
    private GameObject[] enemyRendererArry;

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
    public bool isMoving = false; // 움직임 상태 플래그
    public Coroutine moveCoroutine; // Move 코루틴 참조

    void Awake()
    {
        // 이펙트 비활성화
        destoryEffect.SetActive(false);
        stunEffect.SetActive(false);
        stunTextEffect.SetActive(false);
        bonusCoinTextEffect.SetActive(false);

        // 변수 초기화 및 타겟 설정
        isMoving = false;
        Setup(target);
    }

    void Start()
    {
        // SoundEffect 조절을 위한 audioSource 등록
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && statusAudioSource != null)
            soundManager.RegisterEnemyAudio(statusAudioSource);
        if (soundManager != null && attackAudioSource != null)
            soundManager.RegisterEnemyAudio(attackAudioSource);

        // TurretController에서 활성화된 터렛 목록 확인
        TutorialTurretController turretController = FindFirstObjectByType<TutorialTurretController>();
        if (turretController != null)
        {
            Transform closestTurret = GetClosestTutorialTurret(turretController.GetActiveTurrets());
            if (closestTurret != null)
            {
                target = closestTurret;
            }
        }

        statusAudioSource.PlayOneShot(idleAudioClip);
        currentHP = enemySetting.enemyHP; // 초기 체력 설정
        originalHP = enemySetting.enemyHP;  // 초기 체력 저장

        // 슬라이더 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = enemySetting.enemyHP;
            healthSlider.value = currentHP;
        }

        animator = GetComponent<EnemyAnimatorController>();

        StartCoroutine(WaitIdleTime());
    }

    void Update()
    {
        // 슬라이더를 에너미 상단에 고정
        if (healthSlider != null)
        {
            healthSlider.transform.position = Camera.main.WorldToScreenPoint(transform.position + sliderOffset);
            healthSlider.value = currentHP;
        }
    }

    public void Setup(Transform target)
    {
        this.target = target; // 타겟 설정
        originalTarget = target; // 원래 타겟 저장
    }

    private Transform GetClosestTutorialTurret(System.Collections.Generic.List<Transform> turrets)
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

    public void TakeDamage(float damage)
    {
        if (isDead) return; // 이미 죽은 상태라면 데미지 처리하지 않음

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, enemySetting.enemyHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return; // 이미 죽은 상태라면 Die()를 실행하지 않음

        isDead = true; // 적이 죽었음을 표시

        StartCoroutine(DeathAni());
    }

    private IEnumerator DeathAni()
    {
        statusAudioSource.PlayOneShot(deathAudioClip);
        animator.isDeath();
        if (destoryEffect != null)
            destoryEffect.SetActive(true); // 파괴 이펙트 활성화

        // 상성 데미지를 더 받게 되는 경우 코인을 추가로 지급
        // 튜토리얼에서는 이펙트만 출력
        if (enemySetting.enemyName == EnemyName.Small && criticalHit > 3)
        {
            bonusCoinTextEffect.SetActive(true);
        }
        else if (enemySetting.enemyName == EnemyName.Medium && criticalHit > 3)
        {
            bonusCoinTextEffect.SetActive(true);
        }
        else if (enemySetting.enemyName == EnemyName.Big && criticalHit >= 1)
        {
            bonusCoinTextEffect.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
        // 자연스러운 부활을 위한 Renderer 비활성화
        if (enemyRenderer != null)
            enemyRenderer.SetActive(false);

        destoryEffect.SetActive(false);
        bonusCoinTextEffect.SetActive(false);
        yield return new WaitForSeconds(2f);
        // 부활을 위한 변수 초기화
        isDead = false;
        currentHP = originalHP;
        animator.isBack();
        criticalHit = 0; // 치명타 횟수 초기화

        if (enemyRenderer != null)
            enemyRenderer.SetActive(true);
    }

    private IEnumerator WaitIdleTime()
    {
        yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(enemySetting.enemyAttackRate);
        StartCoroutine(Attack());
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

        if (enemySetting.enemyName == EnemyName.Big)
        {
            transform.rotation = lookRotation * Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // 로컬 축 보정 (예: 180도 회전)
            transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
        }
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
        attackSign = false; // 공격 신호 초기화
        animator.SetBool("isShot", false); // 공격 애니메이션 종료
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.5f); // 첫 스폰 후 1.5초간 대기

        while (isDead == false) // 적이 죽지 않은 동안만 실행
        {
            if (!attackSign)
            {
                animator.SetBool("isShot", true);
                effectController.OnAttackEffectTutorial();
                if (isMoving)
                    isMoving = false; // 이동 중지

                attackSign = true;
                LookRotationToTarget(); // 타겟을 바라보도록 회전

                // 미리 지정된 projectile 사용
                if (projectile != null)
                {
                    attackAudioSource.PlayOneShot(enemyAttackAudio);
                    // 발사체의 위치와 회전 설정
                    projectile.transform.position = projectileSpawnPoint.position; // 발사 위치 설정
                    projectile.transform.rotation = projectileSpawnPoint.rotation; // 발사 방향 설정

                    // EnemyProjectile의 Setup 호출
                    TutorialProjectile projectileComponent = projectile.GetComponent<TutorialProjectile>();
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
                effectController.OffAttackEffectTutorial();
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
        yield return new WaitForSeconds(enemySetting.enemyAttackRate); // EnemySetting의 공격 속도 사용
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
        if (enemyAudioController != null) enemyAudioController.PlayMoveSound();
        float moveTime = 0f; // 이동 누적 시간

        while (isMoving)
        {
            if (!attackSign)
                attackSign = true;

            // 이동
            transform.position += Vector3.right * moveDirection * enemySetting.enemyMoveSpeed * Time.deltaTime;

            // 이동 시간 누적
            moveTime += Time.deltaTime;
            if (moveTime >= 2f)
            {
                animator.SetMoveDirection(0);
                StartAttack();
                yield break;
            }

            // EndPoint와의 충돌(콜라이더 접촉) 체크
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("EndPoint"))
                {
                    moveDirection *= -1;
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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Turret에 관한 스크립트 입니다.
// Turret은 소환 시 Enemy의 첫번째 공격 우선순위가 되며, 가까운 1개의 Enemy를 공격합니다.
// Elite Enemy 소환 시, Elite를 우선 공격 합니다.
public class Turret : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private TurretController turretController; // 터렛 컨트롤러 참조
    [SerializeField]
    private EnemyStatusManager currentTarget; // 현재 공격 중인 적

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public int turretHP; // 터렛의 체력
    public int maxTurretHP = 50; // 터렛의 최대 체력
    public int turretDamage = 10; // 터렛의 공격력
    private float attackCooldown = 1f; // 공격 쿨타임
    private int targetEliteCount = 0;   // Elite 소환 여부 카운트, 0이면 Elite 소환 X

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isAttack = false; // 공격 중인지 여부

    [Header("UI")]
    [SerializeField]
    private Slider turretHPBar; // 체력바 UI
    [SerializeField]
    public Vector3 sliderOffset = new Vector3(0, 4, 0); // 슬라이더 위치 오프셋

    [Header("Effects")]
    [SerializeField]
    private GameObject attackEffectPrefab; // 공격 이펙트 프리팹
    [SerializeField]
    private GameObject attackTextEffect;
    [SerializeField]
    private GameObject destoryEffect;

    [Header("Audio")]
    [SerializeField]
    private AudioClip turretAttackSound; // 공격 사운드
    [SerializeField]
    private AudioSource audioSource; // 오디오 소스

    void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // AudioSource 가져오기
        attackEffectPrefab.SetActive(false);
        attackTextEffect.SetActive(false);
        destoryEffect.SetActive(false);
        // TurretController에게 파괴 신호를 주기위한 Instance
        turretController = TurretController.Instance;
        // TurretController의 값을 적용
        maxTurretHP = turretController.turretMaxHP;
        turretDamage = turretController.turretDamage;
        turretHP = maxTurretHP;

        targetEliteCount = 0;
    }

    void Start()
    {
        // SoundEffect 조절을 위한 audioSource 등록
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && audioSource != null)
            soundManager.RegisterEnemyAudio(audioSource);
    }

    void OnEnable()
    {
        // TurretController의 값을 항상 반영
        maxTurretHP = turretController.turretMaxHP;
        turretDamage = turretController.turretDamage;
        turretHP = maxTurretHP;
        isAttack = false;

        // Elite 활성화 시, 타겟을 Elite 적으로 즉시 변경
        if (EnemyEliteManager.IsEliteActive)
        {
            currentTarget = FindNearestEliteEnemy();
        }
        else
        {
            currentTarget = FindNearestEnemy();
        }
        StartCoroutine(AttackNearestEnemy());
    }

    void Update()
    {
        UpdateTurretHPHUD(); // 체력바 업데이트

        EnemyEliteManager enemyEliteManager = FindObjectOfType<EnemyEliteManager>();
        foreach (EnemyStatusManager enemy in FindObjectsOfType<EnemyStatusManager>())
        {
            if (enemyEliteManager != null && enemy != null)
            {
                OnEliteShieldToggleHandler(1);
            }
            else
            {
                OnEliteShieldToggleHandler(0);
            }
        }
    }

    // 이벤트 핸들러: 엘리트가 소환되면 즉시 타겟 변경
    public void OnEliteShieldToggleHandler(int count)
    {
        targetEliteCount = count;
    }

    private void UpdateTurretHPHUD()
    {
        if (turretHPBar != null)
        {
            // 체력바 위치 업데이트
            turretHPBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + sliderOffset);

            // 체력바 값 업데이트
            turretHPBar.value = (float)turretHP / maxTurretHP;
        }
    }

    // Elite Enemy + EnemyStatusManager를 가진 가장 가까운 적 찾기
    private EnemyStatusManager FindNearestEliteEnemy()
    {
        float detectionRadius = 50f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        EnemyStatusManager nearestElite = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            // EnemyStatusManager와 EnemyEliteManager 둘 다 가진 오브젝트만
            EnemyStatusManager enemyStatus = hitCollider.GetComponent<EnemyStatusManager>();
            EnemyEliteManager eliteManager = hitCollider.GetComponent<EnemyEliteManager>();
            if (enemyStatus != null && eliteManager != null && enemyStatus.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, enemyStatus.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestElite = enemyStatus;
                }
            }
        }

        return nearestElite;
    }

    private IEnumerator AttackNearestEnemy()
    {
        while (true)
        {
            if (!isAttack && targetEliteCount == 0)
            {
                isAttack = true;

                // 타겟 탐색 및 공격 로직 (기존과 동일)
                if (currentTarget == null || !currentTarget.gameObject.activeSelf)
                {
                    currentTarget = FindNearestEnemy();
                }

                if (currentTarget != null)
                {
                    // 터렛이 타겟 방향으로 회전
                    Vector3 directionToTarget = currentTarget.transform.position - transform.position;
                    directionToTarget.y = 0; // Y축 회전을 제한하여 수평으로만 회전
                    if (directionToTarget != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(directionToTarget);
                    }

                    audioSource.PlayOneShot(turretAttackSound); // 공격 사운드 재생
                    StartCoroutine(OnflashEffect()); // 공격 이펙트 실행
                    currentTarget.TakeDamage(turretDamage); // 적에게 데미지 전달
                }
                else
                {
                    Debug.LogWarning("공격할 타겟이 없습니다.");
                }

                yield return new WaitForSeconds(attackCooldown);
                isAttack = false;
            }
            else if (!isAttack && targetEliteCount == 1)
            {
                isAttack = true;
                // 타겟이 엘리트 적인 경우
                currentTarget = FindNearestEliteEnemy();
                if (currentTarget != null)
                {
                    // 터렛이 타겟 방향으로 회전
                    Vector3 directionToTarget = currentTarget.transform.position - transform.position;
                    directionToTarget.y = 0; // Y축 회전을 제한하여 수평으로만 회전
                    if (directionToTarget != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(directionToTarget);
                    }

                    audioSource.PlayOneShot(turretAttackSound); // 공격 사운드 재생
                    StartCoroutine(OnflashEffect()); // 공격 이펙트 실행
                    currentTarget.TakeDamage(turretDamage); // 적에게 데미지 전달
                }
                else
                {
                    Debug.LogWarning("공격할 타겟이 없습니다.");
                }

                yield return new WaitForSeconds(attackCooldown);
                isAttack = false;
            }
            else
            {
                yield return null;
            }
        }
    }

    private EnemyStatusManager FindNearestEnemy()
    {
        float detectionRadius = 50f; // 탐지 반경 (기존보다 크게 설정)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        EnemyStatusManager nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            // "Enemy" 태그를 가진 오브젝트만 처리
            if (hitCollider.CompareTag("EnemySmall") && hitCollider.CompareTag("EnemyMedium") && hitCollider.CompareTag("EnemyBig"))
            {
                continue;
            }

            EnemyStatusManager enemy = hitCollider.GetComponent<EnemyStatusManager>();
            if (enemy == null)
            {
                continue;
            }

            if (enemy != null && enemy.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }

    // 체력 감소 메서드
    public void TakeDamage(int damage)
    {
        turretHP -= damage; // 체력 감소
        turretHP = Mathf.Clamp(turretHP, 0, maxTurretHP); // 체력이 0 미만으로 내려가지 않도록 제한

        // 체력이 0이 되면 터렛 비활성화
        if (turretHP <= 0)
        {
            OnTurretDestroyed();
        }
    }

    // 터렛 파괴 처리
    private void OnTurretDestroyed()
    {
        StartCoroutine(TurretDestoryEffect());
    }

    private IEnumerator TurretDestoryEffect()
    {
        destoryEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        turretController.DestorySign();
        destoryEffect.SetActive(false);
        gameObject.SetActive(false); // 터렛 비활성화
    }

    // Gizmos를 사용하여 공격 범위와 방향을 시각적으로 표시
    private void OnDrawGizmos()
    {
        // 탐지 반경 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 50f); // 탐지 반경(50f)

        // 현재 타겟이 있다면 타겟 방향 표시
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position); // 터렛에서 타겟까지 선 그리기
        }
    }

    private IEnumerator OnflashEffect()
    {
        attackTextEffect.SetActive(true);   // 공격 텍스트 이펙트 활성화
        attackEffectPrefab.SetActive(true);    // 총구 이펙트 오브젝트 활성화

        yield return new WaitForSeconds(attackCooldown / 2);   // 무기의 공격속도보다 빠르게 설정

        attackTextEffect.SetActive(false);  // 공격 텍스트 이펙트 활성화
        attackEffectPrefab.SetActive(false);   // 총구 이펙트 오브젝트 비활성화
    }
}

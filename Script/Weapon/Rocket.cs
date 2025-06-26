using System.Collections;
using UnityEngine;

// 로켓 발사체 스크립트입니다.
// WeaponRocket에서 공격이 이루어졌다면, 로켓프리팹을 생성해, 이동과 폭발을 담당합니다.
public class Rocket : MonoBehaviour
{
    // 해당 스크립트에 필요한 Vector3 변수들
    [Header("Vector3")]
    public Vector3 originPos = new Vector3(0, 0, 0); // 로켓의 원래 위치
    private Vector3 targetDirection; // 로켓이 날아갈 방향

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private float rocketSpeed = 20f; // 로켓 속도
    public float rocketTravelTime = 2f; // 로켓이 날아가는 시간
    public float explosionDamage = 100f;    // 로겟 데미지
    public float explosionRadius = 5f;  // 폭발 범위

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    private bool isFireRocket = false; // 로켓 이동 상태

    [Header("Effects")]
    [SerializeField]
    private GameObject explosionEffect; // 폭발 효과 프리팹
    [SerializeField]
    private GameObject explosionTextEffect;

    [Header("Audio")]
    [SerializeField]
    private AudioClip audioExplosion; // 로켓 폭발 사운드
    [SerializeField]
    private AudioSource audioSource; // 오디오 소스

    // 기타 필요한 변수들
    [Header("Other Variables")]
    private MeshRenderer meshRenderer; // 프리팹 재사용을 위한, 메쉬 렌더러 변수

    void Awake()
    {
        // SoundEffect 조절을 위한 audioSource 등록
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && audioSource != null)
            soundManager.RegisterEnemyAudio(audioSource);

        meshRenderer = GetComponent<MeshRenderer>(); // MeshRenderer 가져오기
        audioSource = GetComponent<AudioSource>(); // AudioSource 가져오기
        explosionEffect.SetActive(false); // 폭발 효과 비활성화
        explosionTextEffect.SetActive(false); // 폭발 텍스트 비활성화
    }

    void Update()
    {
        if (!isFireRocket) return; // 로켓이 이동 중이 아닐 때는 Update 로직 실행 안 함

        rocketTravelTime -= Time.deltaTime; // 로켓의 비행 시간 감소
        if (rocketTravelTime <= 0f)
        {
            // 로켓이 비행 시간을 초과하면 폭발 처리
            Explosion();
            isFireRocket = false; // 로켓 이동 상태 비활성화
            return;
        }

        // 로켓을 targetDirection 방향으로 이동
        transform.position += targetDirection * rocketSpeed * Time.deltaTime;

        // 로켓의 회전을 이동 방향에 맞게 설정하고, 정면으로 날아가기 위해 X축 회전을 90도로 고정
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = targetRotation * Quaternion.Euler(0f, 0f, 0f); // X축 회전을 90도로 고정
        }
    }

    public void Setup(Vector3 direction)
    {
        rocketTravelTime = 2f;
        isFireRocket = true;
        // 로켓의 이동 방향 설정
        targetDirection = direction.normalized;

        // Rigidbody가 있다면 물리 효과를 비활성화
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 계산 비활성화
        }
    }

    private void Explosion()
    {
        // 폭발 범위 내의 모든 Collider 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            // 탐지된 Collider가 어떤 Enemy인지 확인
            EnemyStatusManager enemy = hitCollider.GetComponent<EnemyStatusManager>();
            TutorialEnemyManager tutoralEnemy = hitCollider.GetComponent<TutorialEnemyManager>();
            EnemyShield enemyShield = hitCollider.GetComponent<EnemyShield>();

            // EnemyShield가 있을 때
            if (enemyShield != null)
            {
                // shieldActiveCount가 0보다 크면 elite enemy가 있는 상태이므로 데미지 무시하기
                if (enemyShield.shieldActiveCount <= 0)
                {
                    if (enemy != null) enemy.TakeDamage(explosionDamage);
                    if (tutoralEnemy != null) tutoralEnemy.TakeDamage(explosionDamage);
                }
                else
                {

                }
            }
            // EnemyShield가 없을 때(즉, 일반 enemy)
            else
            {
                if (enemy != null) enemy.TakeDamage(explosionDamage);
                if (tutoralEnemy != null) tutoralEnemy.TakeDamage(explosionDamage);
            }
        }

        StartCoroutine(WaitEffect()); // 폭발 효과 대기
    }

    // Collider 충돌을 통해 폭발 공격 처리
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemySmall") || other.CompareTag("EnemyMedium") || other.CompareTag("EnemyBig"))
        {
            // EnemyStatusManager 스크립트에서 TakeDamage 메서드 호출
            Explosion();
        }
    }

    // Gizmo로 폭발 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Gizmo 색상 설정
        float explosionRadius = 5f; // 폭발 범위 반경
        Gizmos.DrawWireSphere(transform.position, explosionRadius); // 폭발 범위 그리기
    }

    // 폭발 효과를 위한 대기시간 코루틴
    private IEnumerator WaitEffect()
    {
        meshRenderer.enabled = false; // MeshRenderer 비활성화
        explosionEffect.SetActive(true); // 폭발 효과 활성화
        explosionTextEffect.SetActive(true); // 폭발 텍스트 비활성화
        audioSource.PlayOneShot(audioExplosion); // 폭발 사운드 재생
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false); // 로켓 비활성화
        explosionEffect.SetActive(false); // 폭발 효과 비활성화
        explosionTextEffect.SetActive(false); // 폭발 텍스트 비활성화
        meshRenderer.enabled = true; // MeshRenderer 비활성화
        isFireRocket = false; // 로켓 이동 상태 비활성화
        transform.position = originPos; // 원래 위치로 초기화
    }
}

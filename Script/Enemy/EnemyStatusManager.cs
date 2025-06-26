using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Enemy의 상태를 관리하는 스크립트입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyStatusManager : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private EnemyMemoryPool enemyMemoryPool; // EnemyMemoryPool 참조
    public EnemySetting enemySetting; // 에너미 설정

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private float currentHP; // 현재 체력
    public int criticalHit = 0;    // enemy 치명타 변수
    public int hitCount = 0;    // Enemy 회피 이동을 위한 변수

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isDead = false; // 적이 죽었는지 여부를 확인하는 플래그

    [Header("UI")]
    [SerializeField]
    private Slider healthSlider; // 체력 슬라이더
    public Vector3 sliderOffset = new Vector3(0, 2, 0); // 슬라이더 위치 오프셋

    [Header("Animator")]
    [SerializeField]
    private EnemyAnimatorController animator;

    [Header("Effect")]
    [SerializeField]
    private GameObject destoryEffect; // 파괴 이펙트
    [SerializeField]
    private GameObject bonusCoinTextEffect; // 추가 코인 텍스트 이펙트

    [Header("Audio")]
    [SerializeField]
    private AudioSource statusAudioSource;
    [SerializeField]
    private AudioClip idleAudioClip;
    [SerializeField]
    private AudioClip deathAudioClip;

    void Awake()
    {
        destoryEffect.SetActive(false); // 파괴 이펙트 비활성화
        bonusCoinTextEffect.SetActive(false);   // 추가 코인 이펙트 비활성화
    }

    void Start()
    {
        // SoundEffect 등록을 위한 코드
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && statusAudioSource != null)
            soundManager.RegisterEnemyAudio(statusAudioSource);

        statusAudioSource.PlayOneShot(idleAudioClip);   // Idle 사운드 재생
        currentHP = enemySetting.enemyHP; // 초기 체력 설정

        // 슬라이더 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = enemySetting.enemyHP;
            healthSlider.value = currentHP;
        }

        animator = GetComponent<EnemyAnimatorController>();

        enemyMemoryPool = EnemyMemoryPool.Instance;
        if (enemyMemoryPool == null)
        {
            Debug.LogError("EnemyMemoryPool이 설정되지 않았습니다.");
        }
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

    private IEnumerator DeathAni()
    {
        statusAudioSource.PlayOneShot(deathAudioClip);

        if (animator != null)
        {
            animator.isDeath();
        }

        destoryEffect.SetActive(true); // 파괴 이펙트 활성화

        // PlayerStatusManager의 coin 증가
        if (PlayerStatusManager.Instance != null)
        {
            PlayerStatusManager.Instance.coin += enemySetting.enemyCoin; // 코인 증가
        }
        else
        {
            Debug.LogError("PlayerStatusManager 싱글톤 인스턴스를 찾을 수 없습니다.");
        }

        // 상성 데미지를 더 받게 되는 경우 코인을 추가로 지급
        if (enemySetting.enemyName == EnemyName.Small && criticalHit > 0 && criticalHit < 8)
        {
            PlayerStatusManager.Instance.coin += 10;
            bonusCoinTextEffect.SetActive(true);
        }
        else if (enemySetting.enemyName == EnemyName.Medium && criticalHit > 0 && criticalHit < 5)
        {
            PlayerStatusManager.Instance.coin += 20;
            bonusCoinTextEffect.SetActive(true);
        }
        else if (enemySetting.enemyName == EnemyName.Big && criticalHit > 0 && criticalHit < 2)
        {
            PlayerStatusManager.Instance.coin += 30;
            bonusCoinTextEffect.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false); // 적 비활성화
        enemyMemoryPool.killScroe++; // 스코어 증가
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // 이미 죽은 상태라면 데미지 처리하지 않음

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, enemySetting.enemyHP);

        hitCount++; // 적에게 맞은 횟수 증가

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

    // 매개변수로 스테이지 번호를 받아 enemy의 체력을 증가시키는 함수
    public void IncreaseEnemyHP(int stageNum)
    {
        switch (enemySetting.enemyName)
        {
            case EnemyName.Small:
                enemySetting.enemyHP += 5f * stageNum; // Small 유형의 HP 증가
                break;
            case EnemyName.Medium:
                enemySetting.enemyHP += 10f * stageNum; // Medium 유형의 HP 증가
                break;
            case EnemyName.Big:
                enemySetting.enemyHP += 20f * stageNum; // Big 유형의 HP 증가
                break;
        }
    }

    // 매개변수로 스테이지 번호를 받아 enemy의 데미지를 증가시키는 함수
    public void IncreaseEnemyDamage(int stageNum)
    {
        switch (enemySetting.enemyName)
        {
            case EnemyName.Small:
                enemySetting.enemyDamage += 1; // Small 유형의 데미지 증가
                break;
            case EnemyName.Medium:
                enemySetting.enemyDamage += 1 * stageNum; // Medium 유형의 데미지 증가
                break;
            case EnemyName.Big:
                enemySetting.enemyDamage += 1 * stageNum; // Big 유형의 데미지 증가
                break;
        }
    }
}

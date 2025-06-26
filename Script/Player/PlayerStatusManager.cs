using UnityEngine;
using System.Collections;

// Player의 상태를 관리하는 스크립트 입니다.
// 모든 Player에 관련한 스크립트는 Player을 앞에 붙여줍니다.
[System.Serializable]
public class HPEvent : UnityEngine.Events.UnityEvent<int, int> { } // 체력 이벤트를 위한 클래스
public class PlayerStatusManager : MonoBehaviour
{
    public static PlayerStatusManager Instance { get; private set; } // 싱글톤 인스턴스

    [HideInInspector]
    public HPEvent onHPEvent = new HPEvent(); // 체력 이벤트, 체력이 바뀔 때 마다 외부에 있는 메소드 자동 호출 할 이벤트

    [Header("Player HP")]
    [SerializeField]
    private int maxHP = 100;
    private int currentHP; // 현재 체력
    public int increaseCount;   // 최대 체력 증가를 위한 변수

    public int MaxHp => maxHP;  // 최대 체력 Getter 추가
    public int CurrentHP => currentHP; // 현재 체력 Getter 추가

    public int coin; // 코인 수

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isHiding = false; // 플레이어의 엄폐 여부, false = 비엄폐, true = 엄폐
    public bool isShot = false; // 플레이어가 총을 쏘는지 여부

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private SceneManager sceneManager; // 씬 매니저

    [Header("HitEffect")]
    [SerializeField]
    private SpriteRenderer playerHitEffectRenderer; // 플레이어가 맞았을 때 이펙트
    public float effectRenderTime = 2f;
    private Coroutine hitEffectCoroutine; // 코루틴 핸들 추가
    private float hitEffectEndTime = 0f; // 가장 마지막 호출된 hitEffect 종료 시간

    void Awake()
    {
        currentHP = maxHP; // 현재 체력을 최대 체력으로 초기화
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Sprite의 투명도 초기화
        if (playerHitEffectRenderer != null)
        {
            Color color = playerHitEffectRenderer.color;
            color.a = 0; // 투명도 0으로 설정
            playerHitEffectRenderer.color = color;
        }
    }

    void Update()
    {
        if (Time.timeScale != 0 && !isShot)    // 게임이 정지 상태가 아닐 때만 실행
        {
            isHiding = true;
        }
        else if (Time.timeScale != 0 && isShot)
        {
            isHiding = false;
        }

        // HitEffect실행 후 2초가 지나면 자동으로 투명도 0으로 설정
        if (playerHitEffectRenderer.color.a != 0)
        {
            effectRenderTime -= Time.deltaTime;
            if (effectRenderTime <= 0)
            {
                Color color = playerHitEffectRenderer.color;
                color.a = 0; // 투명도 0으로 설정
                playerHitEffectRenderer.color = color;
            }
        }
        else
        {
            effectRenderTime = 2f;
        }
    }

    // 체력 감소 함수
    public bool DecreaseHP(int damage)
    {
        if (!isHiding)
        {
            int previousHP = currentHP;

            currentHP = currentHP - damage > 0 ? currentHP - damage : 0;

            onHPEvent.Invoke(previousHP, currentHP);

            if (currentHP == 0)
            {
                return true;
            }
        }

        return false;
    }

    // 데미지 호출 및, 타격 이펙트 호출 함수
    public void TakeDamage(int damage)
    {
        bool isDie = DecreaseHP(damage);
        if (isDie == true)
        {
            sceneManager.DefeatScene();
        }
        else
        {
            if (hitEffectCoroutine != null)
            {
                StopCoroutine(hitEffectCoroutine);
            }
            hitEffectCoroutine = StartCoroutine(ShowHitEffect());
        }
    }

    // 타격 이펙트
    private IEnumerator ShowHitEffect()
    {
        if (playerHitEffectRenderer != null)
        {
            // 투명도 0.5로 설정
            Color color = playerHitEffectRenderer.color;
            color.a = 0.5f;
            playerHitEffectRenderer.color = color;

            yield return new WaitForSeconds(0.2f); // 0.2초 유지

            // 페이드 아웃
            float fadeDuration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0.5f, 0f, elapsedTime / fadeDuration);
                playerHitEffectRenderer.color = color;
                yield return null;
            }

            if (Time.time >= hitEffectEndTime)
            {
                color.a = 0;
                playerHitEffectRenderer.color = color;
            }
        }
        else
        {
            Debug.LogError("playerHitEffectRenderer가 null입니다.");
        }
    }

    public void HealToMax()
    {
        int previousHP = currentHP;
        currentHP = maxHP; // 현재 체력을 최대 체력으로 설정
        onHPEvent.Invoke(previousHP, currentHP); // 체력 이벤트 호출
    }

    public void IncreaseMaxHP(int amount)
    {
        maxHP += amount; // 최대 체력 증가
        increaseCount++;
    }
}

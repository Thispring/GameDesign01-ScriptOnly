using UnityEngine;
using System.Collections;

// 게임에 존재하는 기지의 상태에 관한 스크립트 입니다.
[System.Serializable]
public class BaseHPEvent : UnityEngine.Events.UnityEvent<int, int> { } // 체력 이벤트를 위한 클래스
public class BaseStatus : MonoBehaviour
{
    [HideInInspector]
    public BaseHPEvent onBaseHPEvent = new BaseHPEvent(); // 체력 이벤트, 체력이 바뀔 때 마다 외부에 있는 메소드 자동 호출 할 이벤트

    [Header("Base HP")]
    [SerializeField]
    private int baseMaxHP = 300;
    private int baseCurrentHP; // 현재 체력

    public int BaseMaxHP => baseMaxHP;  // 최대 체력 Getter 추가
    public int BaseCurrentHP => baseCurrentHP; // 현재 체력 Getter 추가

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerStatusManager playerStatusManager; // 플레이어 상태 관리 스크립트
    [SerializeField]
    private SceneManager sceneManager; // 씬 매니저

    [Header("Base Effect")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private GameObject hitEffectPrefab; // 기지 맞았을 때 이펙트 프리팹
    [SerializeField]
    private GameObject[] fireEffectPrefab = new GameObject[2]; // fireEffectPrefab 배열 초기화

    void Awake()
    {
        hitEffectPrefab.SetActive(false); // 기지 이펙트 비활성화
        // fireEffectPrefab 배열의 모든 오브젝트 비활성화
        for (int i = 0; i < fireEffectPrefab.Length; i++)
        {
            if (fireEffectPrefab[i] != null)
            {
                fireEffectPrefab[i].SetActive(false);
            }
        }
        baseCurrentHP = baseMaxHP; // 현재 체력을 최대 체력으로 초기화
    }

    void Update()
    {
        BaseFireEffect();
    }

    public bool DecreaseHP(int damage)
    {
        if (playerStatusManager.isHiding)   // Player가 엄폐 상태
        {
            int previousHP = baseCurrentHP;

            baseCurrentHP = baseCurrentHP - damage > 0 ? baseCurrentHP - damage : 0;

            onBaseHPEvent.Invoke(previousHP, baseCurrentHP);

            if (baseCurrentHP == 0)
            {
                return true;
            }
        }

        return false;
    }

    public void TakeDamage(int damage)
    {
        bool isDie = DecreaseHP(damage);
        StartCoroutine(BaseHitEffect()); 
        if (isDie == true)
        {
            // 기지 체력 0일 때 처리
            sceneManager.DefeatScene();
        }
    }

    public void HealToMax()
    {
        int previousHP = baseCurrentHP;
        baseCurrentHP = baseMaxHP;

        // 체력 변경 이벤트 호출
        onBaseHPEvent.Invoke(previousHP, baseCurrentHP);
    }

    private IEnumerator BaseHitEffect()
    {
        hitEffectPrefab.SetActive(true); // 기지 이펙트 활성화
        yield return new WaitForSeconds(0.5f);
        hitEffectPrefab.SetActive(false); // 기지 이펙트 비활성화
    }

    private void BaseFireEffect()
    {
        // fireEffectPrefab 배열의 오브젝트 활성화/비활성화
        // 기지 체력이 150 미만이면 fireEffect 활성화
        if (baseCurrentHP < 150)
        {
            for (int i = 0; i < fireEffectPrefab.Length; i++)
            {
                if (fireEffectPrefab[i] != null)
                {
                    fireEffectPrefab[i].SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < fireEffectPrefab.Length; i++)
            {
                if (fireEffectPrefab[i] != null)
                {
                    fireEffectPrefab[i].SetActive(false);
                }
            }
        }
    }
}

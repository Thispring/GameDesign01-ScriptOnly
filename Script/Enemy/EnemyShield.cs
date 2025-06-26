using UnityEngine;

// Enemy의 무적 상태를 표시를 담당하는 스크립트 입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyShield : MonoBehaviour
{
    [SerializeField]
    private GameObject shieldPrefab;

    public int shieldActiveCount = 0; // 쉴드 활성화 카운트

    void Awake()
    {
        if (shieldPrefab != null) shieldPrefab.SetActive(false);

        shieldActiveCount = 0; // 초기화
    }

    void Update()
    {
        if (shieldActiveCount > 0)
        {
            if (shieldPrefab != null)
                shieldPrefab.SetActive(true);
        }
        else
        {
            if (shieldPrefab != null)
                shieldPrefab.SetActive(false);
        }
    }

    void OnEnable()
    {
        EnemyEliteManager.OnEliteShieldToggle += HandleEliteShieldToggle;

        // 현재 Elite 상태를 직접 체크해서 쉴드 상태 맞추기
        HandleEliteShieldToggle(EnemyEliteManager.IsEliteActive);
    }

    void OnDisable()
    {
        EnemyEliteManager.OnEliteShieldToggle -= HandleEliteShieldToggle;
    }

    private void HandleEliteShieldToggle(bool isActive)
    {
        Debug.Log($"HandleEliteShieldToggle 호출됨: {isActive}");
    }

    public void HandleShieldActiveCount(int count)
    {
        shieldActiveCount = count;
        Debug.Log($"HandleShieldActiveCount 호출됨: {shieldActiveCount}");
    }
}

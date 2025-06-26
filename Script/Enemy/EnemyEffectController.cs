using UnityEngine;

// Enemy의 Effect관리를 위한 스크립트입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyEffectController : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용 
    private EnemyStatusManager enemyStatusManager;
    [SerializeField]    // Inspector에서 끌어서 사용 
    private TutorialEnemyManager tutorialEnemyManager;

    [Header("Effect")]
    [SerializeField]    // Inspector에서 끌어서 사용 
    private GameObject attackEffectPrefab; // 공격 이펙트 프리팹
    [SerializeField]    // Inspector에서 끌어서 사용 
    private GameObject attackEffectPrefab2; // 공격 이펙트 프리팹

    void Awake()
    {
        // Enemy 타입별 시각적으로 보여줄 attackEffectPrefab 필요 개수가 달라 else 문에 디버그 추가
        if (attackEffectPrefab == null)
        {
            Debug.Log("attackEffectPrefab is not assigned in the inspector.");
        }
        else
        {
            attackEffectPrefab.SetActive(false);
        }

        if (attackEffectPrefab2 == null)
        {
            Debug.Log("attackEffectPrefab2 is not assigned in the inspector.");
        }
        else
        {
            attackEffectPrefab2.SetActive(false);
        }
    }

    // 공격 시 이펙트 활성 함수
    public void OnAttackEffect()
    {
        switch (enemyStatusManager.enemySetting.enemyName)
        {
            case EnemyName.Small:
                attackEffectPrefab.SetActive(true);
                break;
            case EnemyName.Medium:
                attackEffectPrefab.SetActive(true);
                attackEffectPrefab2.SetActive(true);
                break;
            case EnemyName.Big:
                attackEffectPrefab.SetActive(true);
                attackEffectPrefab2.SetActive(true);
                break;
            default:
                break;
        }
    }

    // 공격 끝난 후 이펙트 비활성 함수
    public void OffAttackEffect()
    {
        switch (enemyStatusManager.enemySetting.enemyName)
        {
            case EnemyName.Small:
                attackEffectPrefab.SetActive(false);
                break;
            case EnemyName.Medium:
                attackEffectPrefab.SetActive(false);
                attackEffectPrefab2.SetActive(false);
                break;
            case EnemyName.Big:
                attackEffectPrefab.SetActive(false);
                attackEffectPrefab2.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void OnAttackEffectTutorial()
    {
        switch (tutorialEnemyManager.enemySetting.enemyName)
        {
            case EnemyName.Small:
                attackEffectPrefab.SetActive(true);
                break;
            case EnemyName.Medium:
                attackEffectPrefab.SetActive(true);
                attackEffectPrefab2.SetActive(true);
                break;
            case EnemyName.Big:
                attackEffectPrefab.SetActive(true);
                attackEffectPrefab2.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OffAttackEffectTutorial()
    {
        switch (tutorialEnemyManager.enemySetting.enemyName)
        {
            case EnemyName.Small:
                attackEffectPrefab.SetActive(false);
                break;
            case EnemyName.Medium:
                attackEffectPrefab.SetActive(false);
                attackEffectPrefab2.SetActive(false);
                break;
            case EnemyName.Big:
                attackEffectPrefab.SetActive(false);
                attackEffectPrefab2.SetActive(false);
                break;
            default:
                break;
        }
    }
}

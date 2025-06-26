using UnityEngine;

// 아이템으로 등장하는 EMP에 관한 스크립트 입니다.
// EMP는 사용 시 Enemy의 행동을 중지시키고, 일정시간 동안 기절 시킵니다.
public class EMP : MonoBehaviour
{
    [Header("EMP Settings")]
    public float empCoolTime = 15f; // EMP 쿨타임
    public bool isCoolTime = false; // EMP 쿨타임 체크용
    public int empStunTime = 5;

    [Header("Audio")]
    [SerializeField]
    private AudioClip audioEMP; // EMP 사운드
    [SerializeField]
    private AudioSource audioSource; // 오디오 소스

    void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // AudioSource 가져오기
        empCoolTime = 15f; // 초기 쿨타임 설정
        isCoolTime = false; // 초기 쿨타임 상태 설정
    }

    void Update()
    {
        if (!isCoolTime)
        {
            empCoolTime -= Time.deltaTime; // EMP 쿨타임 감소
            if (empCoolTime < 0f)
                empCoolTime = 0f; // 0 미만으로 내려가지 않도록 예외 처리   
        }

        if (empCoolTime <= 0f)
        {
            isCoolTime = true;
        }

        // 특정 키를 눌렀을 때 실행
        if (isCoolTime && Input.GetKeyDown(KeyCode.E))
        {
            ControlEnemyFSMVariables();
        }
    }

    private void ControlEnemyFSMVariables()
    {
        audioSource.PlayOneShot(audioEMP); // EMP 사운드 재생
        empCoolTime = 15f; // EMP 쿨타임 초기화
        isCoolTime = false; // EMP 쿨타임 상태 초기화

        // 모든 EnemyFSM 컴포넌트를 가진 오브젝트 찾기
        EnemyFSM[] enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
        EnemyProjectile[] enemyProjectiles = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
        TutorialEnemyManager[] tutorialEnemyes = FindObjectsByType<TutorialEnemyManager>(FindObjectsSortMode.None);
        TutorialProjectile[] tutorialProjectiles = FindObjectsByType<TutorialProjectile>(FindObjectsSortMode.None);

        // 각 EnemyFSM의 변수를 컨트롤
        foreach (EnemyFSM enemy in enemies)
        {
            enemy.HitEMP(empStunTime);
        }

        foreach (EnemyProjectile enemyProjectile in enemyProjectiles)
        {
            enemyProjectile.HitEMP();
        }

        foreach (TutorialEnemyManager tutorialEnemy in tutorialEnemyes)
        {
            tutorialEnemy.HitEMP(empStunTime);
        }

        foreach (TutorialProjectile tutorialProjectile in tutorialProjectiles)
        {
            tutorialProjectile.HitEMP();
        }
    }
}

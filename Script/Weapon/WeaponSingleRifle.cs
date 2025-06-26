using System.Collections;   // 코루틴 사용
using UnityEngine;

// 게임에 존재하는 특수 돌격 소총의 '단발'(Single)사격에 대한 스크립트 입니다.
// 모든 무기에 관련한 스크립트는 Weapon을 앞에 붙여줍니다.
// WeaponBase를 상속받아 기본적인 무기 기능을 구현합니다.
public class WeaponSingleRifle : WeaponBase
{
    [Header("Effects")]
    [SerializeField]
    private GameObject flashEffect;  // 총구 이펙트

    [Header("Audio")]
    [SerializeField]
    private AudioClip audioRapid;   // 단발 공격 사운드
    [SerializeField]
    private AudioClip audioClipReload;  // 재장전 사운드

    void Awake()
    {
        base.Setup();   // WeaponBase의 Setup() 메소드 호출

        mainCamera = Camera.main; // 메인 카메라 캐싱
        audioSource = GetComponent<AudioSource>();  // AudioSource 가져오기

        weaponSetting.currentAmmo = weaponSetting.maxAmmo;  // 처음 탄 수는 최대로 설정
        SetCurrentDamage(15);  // WeaponBase의 SetCurrentDamage 함수를 통해 Single 타입 초기 데미지 설정, 괄호 안에 숫자가 초기 값
    }

    void OnEnable()
    {
        shotTextEffect.SetActive(false);    // 사격 텍스트 이펙트 비활성화
        flashEffect.SetActive(false);   // 총구 이펙트 비활성화
        onAmmoEvnet.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);   // 무기가 활성화될 때 해당 무기의 탄 수 정보를 갱신한다
    }

    void Update()
    {
        if (weaponSetting.currentAmmo == 0) // 탄 수가 0이 되면 자동재장전
        {
            StartReload();
        }
    }

    public override void StartWeaponAction(int type = 0)
    {
        // 재장전 중일 때는 무기 액션(=사격 등)을 할 수 없다
        if (isReload == true) return;
        // 마우스 왼쪽 클릭 (공격 시작)
        if (type == 0)
        {
            // 연속 공격이 설정되어 있으면, OnAttackLoop 코루틴을 시작
            if (weaponSetting.isAutomaticAttack == true)
            {
                StartCoroutine("OnAttackLoop");
            }
        }
        else
        {
            OnAttack();
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
        // 마우스 왼쪽 클릭 (공격 종료)
        if (type == 0)
        {
            if (animator != null)
            {
                animator.SetAnimationBool("isShot", false);
            }
            else
            {
                Debug.LogWarning("Animator가 null 상태입니다.");
            }

            StopCoroutine("OnAttackLoop");
        }
    }

    public override void StartReload()
    {
        // 현재 재장전 중이면 재장전 불가능
        if (isReload == true) return;
        // 탄약이 다 차있으면 재장전 불가능
        if (weaponSetting.currentAmmo == weaponSetting.maxAmmo) return;
        // 무기 액션 도중에 'R'키를 눌러 재장전을 시도하면 무기 액션 종료 후 재장전
        StopWeaponAction();

        StartCoroutine("OnReload");
    }

    private IEnumerator OnAttackLoop()  // 연발 설정 시 공격 코루틴
    {
        while (true)
        {
            OnAttack();

            yield return new WaitForSeconds(weaponSetting.attackRate); // 공격 속도에 맞춰 실행
        }
    }

    public void OnAttack()
    {
        if (Time.time - lastAttackTime > weaponSetting.attackRate)
        {
            // 공격주기가 되어야 공격할 수 있도록 하기 위해 현재 시간 저장
            lastAttackTime = Time.deltaTime;

            // 탄 수가 없으면 공격 불가능
            if (weaponSetting.currentAmmo <= 0)
            {
                return;
            }
            // 공격 시 currentAmmo 1 감소
            weaponSetting.currentAmmo--;
            // 현재 탄수 정보를 갱신
            onAmmoEvnet.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);
            // 무기 애니메이션 재생
            animator.SetAnimationBool("isShot", true);
            // 총구 이펙트 재생
            StartCoroutine("OnflashEffect");
            StartCoroutine("OnTextEffect");
            PlaySound(audioRapid);

            // 광선을 발사해 원하는 위치 공격
            Vector3 mouseScreenPos = Input.mousePosition;
            if (Shoot(mouseScreenPos, out RaycastHit hit)) // Shoot 호출
            {
                //Debug.Log($"Hit at {hit.point}");
            }
        }
    }

    private IEnumerator OnReload()  // 재장전 코루틴
    {
        isReload = true;

        // 재장전 애니메이션, 사운드 재생
        animator.OnReload();
        PlaySound(audioClipReload);

        while (true)
        {
            // 사운드가 재생중이 아니면, 재장전 사운드 재생이 종료되었다는 뜻
            if (audioSource.isPlaying == false)
            {
                isReload = false;

                // 현재 탄 수를 최대로 설정하고, 바뀐 탄 수 정보를 Text UI에 업데이트
                weaponSetting.currentAmmo = weaponSetting.maxAmmo;
                onAmmoEvnet.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator OnflashEffect()
    {
        flashEffect.SetActive(true);    // 총구 이펙트 오브젝트 활성화

        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f);   // 무기의 공격속도보다 빠르게 설정

        flashEffect.SetActive(false);   // 총구 이펙트 오브젝트 비활성화
    }

    private IEnumerator OnTextEffect()  // 사격 시 텍스트 이펙트 코루틴
    {
        shotTextEffect.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        shotTextEffect.SetActive(false);
    }

    public override bool Shoot(Vector3 mouseScreenPos, out RaycastHit hit)
    {
        if (!base.Shoot(mouseScreenPos, out hit)) // WeaponBase의 Shoot 호출
        {
            return false;
        }

        // 적중 처리
        // tutorialEnemy와 enemy의 컴포넌트를 가져와 구분
        EnemyStatusManager enemy = hit.collider.GetComponent<EnemyStatusManager>();
        TutorialEnemyManager tutorialEnemy = hit.collider.GetComponent<TutorialEnemyManager>();
        // 둘 다 없으면 false 반환
        if (enemy == null && tutorialEnemy == null)
        {
            return false;
        }

        float damage = weaponSetting.singleDamage; // 기본 데미지
        switch (hit.collider.tag)
        {
            case "EnemySmall":
                //Debug.Log("singleRifle로 EnemySmall에게 데미지 적용 완료");
                break;
            case "EnemyMedium":
                damage += 15; // 추가 데미지
                // 추가 코인을 위한 criticalHit 증가
                if (enemy != null) enemy.criticalHit++;
                if (tutorialEnemy != null) tutorialEnemy.criticalHit++;
                //Debug.Log("singleRifle로 EnemyMedium에게 추가 데미지 적용 완료");
                break;
            case "EnemyBig":
                //Debug.Log("singleRifle로 EnemyBig에게 적 명중! 데미지 적용 완료");
                break;
            default:
                //Debug.Log("알 수 없는 적 태그");
                return false;
        }

        // 존재하는 쪽만 데미지 처리
        if (enemy != null) enemy.TakeDamage(damage);
        if (tutorialEnemy != null) tutorialEnemy.TakeDamage(damage);
        return true;
    }
}

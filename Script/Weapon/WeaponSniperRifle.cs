using System.Collections;   // 코루틴 사용
using UnityEngine;

// 게임에 존재하는 특수 돌격 소총의 '저격'(Sniper)사격에 대한 스크립트 입니다.
// 모든 무기에 관련한 스크립트는 Weapon을 앞에 붙여줍니다.
// WeaponBase를 상속받아 기본적인 무기 기능을 구현합니다.
public class WeaponSniperRifle : WeaponBase
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerHUD playerHUD;    // UI활성화를 위해 참조
    [SerializeField]    // Inspector에서 끌어서 사용
    private EMP emp;    // sniperRifle로 적에게 데미지를 입히면 EMP의 쿨타임이 감소되는 로직을 위한 참조

    [Header("Effects")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private GameObject flashEffect;  // 총구 이펙트

    [Header("Audio")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private AudioClip audioRapid;   // 연발 공격 사운드
    [SerializeField]    // Inspector에서 끌어서 사용
    private AudioClip audioClipReload;  // 재장전 사운드

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private float chargeProgress = 0f; // Sniper 차지 진행도 변수, HUD의 Slider UI에 사용

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isReloading = false;    // 재장전 중인지 여부, 사격 중 재장전을 방지하기 위한 변수
    
    void Awake()
    {
        base.Setup();   // WeaponBase의 Setup() 메소드 호출

        mainCamera = Camera.main; // 메인 카메라 캐싱
        audioSource = GetComponent<AudioSource>();  // AudioSource 가져오기

        weaponSetting.currentAmmo = weaponSetting.maxAmmo;  // 처음 탄 수는 최대로 설정
        SetCurrentDamage(70);  // WeaponBase의 SetCurrentDamage 함수를 통해 Sniper 타입 초기 데미지 설정, 괄호 안에 숫자가 초기 값
    }

    void OnEnable()
    {
        shotTextEffect.SetActive(false);    // 사격 텍스트 이펙트 비활성화
        flashEffect.SetActive(false);   // 총구 이펙트 비활성화
        onAmmoEvnet.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);   // 무기가 활성화될 때 해당 무기의 탄 수 정보를 갱신한다
    }

    void Update()
    {
        // 차징 중이고, 차징 준비가 되지 않았으며, 공격 주기(Time.time - 이전 공격시간이 공격속도보다 클 때)가 지났을 때 발동
        if (isCharging && !chargeReady && (Time.time - lastAttackTime > weaponSetting.attackRate))
        {
            // 차지 진행도 계산, 0.5초 동안 0에서 1로 증가
            chargeProgress = Mathf.Clamp01((Time.time - chargeStartTime) / 0.5f);
            // 차지 진행도에 따라 HUD 업데이트   
            playerHUD.UpdateSinperChargingHUD(chargeProgress, chargeReady);

            // 차지 준비 완료 시간(0.5초)이 지났다면 차지 준비 완료 상태로 변경
            if (Time.time - chargeStartTime >= 0.5f)
            {
                chargeReady = true; // 차지 준비 완료 상태로 변경하여, 차지 중지
                playerHUD.UpdateSinperChargingHUD(1f, chargeReady); // 차지가 끝난 UI를 재갱신
            }
        }

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
            if (!isCharging && (Time.time - lastAttackTime > weaponSetting.attackRate))
            {
                // Sniper는 반동 애니메이션 재생을 위해 isAiming을 따로 사용
                animator.SetAnimationBool("isAiming", true);
                // 처음 마우스를 눌렀을 때 차지 시작
                isCharging = true;
                chargeReady = false;
                chargeStartTime = Time.time;
            }
        }
    }

    private void ResetCharge()  // 차지 초기화
    {
        StartCoroutine(DelayTime());    // 자연스러운 애니메이션을 위한 DelayTime 코루틴 호출
        // 차지 관련 변수 초기화
        isCharging = false;
        chargeReady = false;
        chargeStartTime = 0f;
    }

    public override void StopWeaponAction(int type = 0)
    {
        // 마우스 왼쪽 클릭 (공격, 차지 게이지, HUD 초기화)
        if (type == 0 && chargeStartTime > 0)
        {
            OnAttack();
            ResetCharge();
            playerHUD.UpdateSinperChargingHUD(0, chargeReady);
        }
    }

    public override void StartReload()
    {
        // 현재 차지중이면 재장전 불가능
        if (isCharging == true) return;
        // 현재 재장전 중이면 재장전 불가능
        if (isReload == true) return;
        // 탄약이 다 차있으면 재장전 불가능
        if (weaponSetting.currentAmmo == weaponSetting.maxAmmo) return;

        // isReloading을 true로 설정하여, 사격 중 재장전 방지
        isReloading = true;
        // 무기 액션 도중에 'R'키를 눌러 재장전을 시도하면 무기 액션 종료 후 재장전
        ResetCharge();  // 차지 초기화
        playerHUD.UpdateSinperChargingHUD(0, chargeReady);  // 차지 HUD 초기화
        StopCoroutine("OnAttack");

        StartCoroutine("OnReload");
    }

    public void OnAttack()
    {
        if (Time.time - lastAttackTime > weaponSetting.attackRate)
        {
            // 공격주기가 되어야 공격할 수 있도록 하기 위해 현재 시간 저장
            lastAttackTime = Time.time;

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
            animator.SetAnimationBool("isAimShot", true);

            // 이펙트 재생
            StartCoroutine("OnflashEffect");
            StartCoroutine("OnTextEffect");
            // 발사 사운드 재생
            PlaySound(audioRapid);

            // 광선을 발사해 원하는 위치 공격
            Vector3 mouseScreenPos = Input.mousePosition;
            if (Shoot(mouseScreenPos, out RaycastHit hit)) // Shoot 호출
            {
                //Debug.Log($"Hit at {hit.point}");
            }
        }
    }

    public override bool Shoot(Vector3 mouseScreenPos, out RaycastHit hit)
    {
        Ray ray = GetMouseRay(mouseScreenPos);

        float sphereRadius = 0.1f; // 판정 반경
        float maxDistance = weaponSetting.attackDistance;

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan, 3f);

        // SphereCast로 가장 먼저 맞은 적 하나만 감지
        if (Physics.SphereCast(ray.origin, sphereRadius, ray.direction, out hit, maxDistance))
        {
            // tutorialEnemy와 enemy의 컴포넌트를 가져와 구분
            EnemyStatusManager enemy = hit.collider.GetComponent<EnemyStatusManager>();
            TutorialEnemyManager tutorialEnemy = hit.collider.GetComponent<TutorialEnemyManager>();
            if (enemy != null || tutorialEnemy != null)
            {
                // 데미지는 설정된 sniperDamge에 chargeProgress의 진행도를 곱하여 계산
                float damage = weaponSetting.sniperDamage * (1 + chargeProgress);

                switch (hit.collider.tag)
                {
                    case "EnemySmall":
                        //Debug.Log($"sniperRifle로 EnemySmall에게 데미지 적용 완료 {damage}");
                        break;
                    case "EnemyMedium":
                        //Debug.Log($"sniperRifle로 EnemyMedium에게 데미지 적용 완료 {damage}");
                        break;
                    case "EnemyBig":
                        damage += 200;  // 추가 데미지
                        // 추가 코인을 위한 criticalHit 증가
                        if (enemy != null) enemy.criticalHit++;
                        if (tutorialEnemy != null) tutorialEnemy.criticalHit++;
                        //Debug.Log($"sniperRifle로 EnemyBig에게 추가 데미지 적용 완료 {damage}");
                        break;
                    default:
                        break;
                }

                // 존재하는 쪽만 데미지 처리
                if (enemy != null) enemy.TakeDamage(damage);
                if (tutorialEnemy != null) tutorialEnemy.TakeDamage(damage);
                // 적에게 데미지를 입히면 EMP의 쿨타임 감소
                emp.empCoolTime -= 5;
                return true;
            }
        }

        hit = default;
        return false;
    }

    private IEnumerator DelayTime()
    {
        // 애니메이션이 자연스럽게 재생되도록 잠시 대기
        yield return new WaitForSeconds(0.5f);
        animator.SetAnimationBool("isAiming", false);
        animator.SetAnimationBool("isAimShot", false);
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
                // 재장전 변수 초기화
                isReload = false;
                isReloading = false; 

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
}

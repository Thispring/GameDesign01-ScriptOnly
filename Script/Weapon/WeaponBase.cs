using UnityEngine;

// 3종류의 무기를 하나의 타입으로 통일하기 위해 모든 무기가 상속받는 기반 클래스 입니다.
// 모든 무기에 관련한 스크립트는 Weapon을 앞에 붙여줍니다.
/*
사격 종류 별 명칭
단발 -> Single = 0
연발 -> Rapid = 1
저격 -> Sniper = 2
*/
public enum WeaponType { Single = 0, Rapid, Sniper, Rocket }   // 무기 종류에 대한 열거형

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { } // 무기의 탄수 정보가 바뀔 때 마다 외부에 있는 메소드 자동 호출 할 이벤트
// 변수 private -> protected으로 변경
public abstract class WeaponBase : MonoBehaviour    // abstract 추상 클래스로 정의
{
    // 외부에서 이벤트 함수 등록을 할 수 있도록 public 선언
    [HideInInspector]
    public AmmoEvent onAmmoEvnet = new AmmoEvent();

    [Header("WeaponBase")]
    [SerializeField]
    protected WeaponType weaponType;    // 무기 종류
    [SerializeField]
    protected WeaponSetting weaponSetting;  // 무기 설정

    [Header("Weapon Shooting")]
    protected Camera mainCamera; // 메인 카메라 참조

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    protected float lastAttackTime = 0; // 마지막 발사시간 체크용

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    protected bool isReload = false;    // 재장전 중인지 체크
    protected bool isAttack = false;    // 공격 여부 체크용

    [Header("Audio")]
    protected AudioSource audioSource;  // 사운드 재생 컴포넌트

    [Header("Animations")]
    [SerializeField]
    protected PlayerAnimatorController animator;    // 애니메이션 재생 제어

    [Header("Effects")]
    [SerializeField]
    protected GameObject shotTextEffect;

    [Header("Sniper Charge")]
    // Sniper의 차지샷 관련 변수, UI에 참고하여 값을 넣어야함
    protected float chargeStartTime = 0f; // 차지 시작 시간
    protected bool isCharging = false; // 차지 중인지 여부
    protected bool chargeReady = false; // 차지샷 준비 완료 여부

    [Header("Weapon Damage")]
    protected float singleDamage; // Single 타입의 데미지
    protected float rapidDamage;  // Rapid 타입의 데미지
    protected float sniperDamage; // Sniper 타입의 데미지

    // 외부에서 필요한 정보를 열람하기 위해 정의한 Get Property's
    public WeaponName WeaponName => weaponSetting.weaponName;
    public WeaponType WeaponType => weaponType; // 무기타입에 관한 Get
    public int CurrentAmmo => weaponSetting.currentAmmo; // 현재 탄약 수 Getter 추가
    public int MaxAmmo => weaponSetting.maxAmmo; // 최대 탄약 Getter 추가
    public float SingleDamage => weaponSetting.singleDamage; // Single 타입의 데미지 Getter 추가
    public float RapidDamage => weaponSetting.rapidDamage; // Rapid 타입의 데미지 Getter 추가
    public float SniperDamage => weaponSetting.sniperDamage; // Sniper 타입의 데미지 Getter 추가

    public abstract void StartWeaponAction(int type = 0);
    public abstract void StopWeaponAction(int type = 0);
    public abstract void StartReload();

    // 데미지 증가 메서드
    public void IncreaseDamage(int damageIncrease)
    {
        switch (weaponSetting.weaponName)
        {
            case WeaponName.Single:
                weaponSetting.singleDamage += damageIncrease; // WeaponSetting의 singleDamage 증가
                break;
            case WeaponName.Rapid:
                weaponSetting.rapidDamage += damageIncrease; // WeaponSetting의 rapidDamage 증가
                break;
            case WeaponName.Sniper:
                weaponSetting.sniperDamage += damageIncrease; // WeaponSetting의 sniperDamage 증가
                break;
            default:
                break;
        }
    }

    // shopManager에서 현재 무기 데미지를 불러오기 위한 메서드
    public float GetCurrentDamage()
    {
        return weaponSetting.weaponName switch
        {
            WeaponName.Single => weaponSetting.singleDamage,
            WeaponName.Rapid => weaponSetting.rapidDamage,
            WeaponName.Sniper => weaponSetting.sniperDamage,
            _ => 0
        };
    }

    // 현재 weaponName에 따른 데미지를 설정하는 메서드
    public void SetCurrentDamage(float damage)
    {
        switch (weaponSetting.weaponName)
        {
            case WeaponName.Single:
                weaponSetting.singleDamage = damage; // WeaponSetting의 singleDamage 설정
                break;
            case WeaponName.Rapid:
                weaponSetting.rapidDamage = damage; // WeaponSetting의 rapidDamage 설정
                break;
            case WeaponName.Sniper:
                weaponSetting.sniperDamage = damage; // WeaponSetting의 sniperDamage 설정
                break;
            default:
                break;
        }
    }

    // 마우스 입력을 받아 광선을 생성합니다.
    protected Ray GetMouseRay(Vector3 mouseScreenPos)
    {
        if (mainCamera == null)
        {
            return default;
        }
        return mainCamera.ScreenPointToRay(mouseScreenPos);
    }

    // mouseScreenPos을 기준으로 광선을 생성합니다.
    public virtual bool Shoot(Vector3 mouseScreenPos, out RaycastHit hit)
    {
        Ray ray = GetMouseRay(mouseScreenPos);

        // DrawRay를 통한 광선 확인
        Debug.DrawRay(ray.origin, ray.direction * weaponSetting.attackDistance, Color.yellow, 1.0f);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, weaponSetting.attackDistance))
        {
            return true;
        }

        hit = default;
        return false; // 적중하지 않음
    }

    public virtual void ResetWeaponState()  // 무기 교체를 위한 상태 초기화
    {
        isReload = false; // 재장전 상태 초기화
        isAttack = false; // 공격 상태 초기화
        StopWeaponAction(); // 무기 액션 종료
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        if (CurrentAmmo == 0)
        {
            audioSource.PlayOneShot(clip);  // 총알 마지막에 발사 및 장전 소리 중복
        }
        else
        {
            audioSource.Stop();  // 이전 소리를 강제로 멈추고
            audioSource.PlayOneShot(clip);  // 새로운 소리를 1번만 재생
        }
    }

    protected void Setup()
    {
        audioSource = GetComponent<AudioSource>();  // AudioSource 가져오기
        animator = GetComponent<PlayerAnimatorController>();    // PlayerAnimatorController의 Component 정보 가져오기
        if (animator == null)
        {
            Debug.LogError("PlayerAnimatorController가 설정되지 않았습니다.");
        }
    }
}

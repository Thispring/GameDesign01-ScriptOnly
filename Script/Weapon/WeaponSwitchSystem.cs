using UnityEngine;

// 무기 교체에 관한 스크립트 입니다.
public class WeaponSwitchSystem : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerShotController playerShotController;
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerHUD playerHUD;
    // 아래 4개의 스크립트는 Awake에서 GetComponent로 참조
    private WeaponSingleRifle weaponSingleRifle;
    private WeaponRapidRifle weaponRapidRifle;
    private WeaponSniperRifle weaponSniperRifle;
    private WeaponRocket weaponRocket;

    [Header("WeaponBase References")]
    [SerializeField]
    private WeaponBase[] weapons = new WeaponBase[4];   // 소지중인 무기 배열
    private WeaponBase currentWeapon;   // 현재 사용중인 무기
    private WeaponBase previousWeapon;  // 직전에 사용했던 무기

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private int currentWeaponIndex = 0; // 초기 무기는 Single(0)로 설정

    void Awake()
    {
        // 각 무기별 컴포넌트 가져오기
        weaponSingleRifle = GetComponent<WeaponSingleRifle>();
        weaponRapidRifle = GetComponent<WeaponRapidRifle>();
        weaponSniperRifle = GetComponent<WeaponSniperRifle>();
        weaponRocket = GetComponent<WeaponRocket>();

        // weapons 배열을 무기 순서대로 자동 할당
        weapons[0] = weaponSingleRifle;
        weapons[1] = weaponRapidRifle;
        weapons[2] = weaponSniperRifle;
        weapons[3] = weaponRocket;

        // 무기 정보 출력을 위해 현재 소지중인 모든 무기 이벤트 등록
        playerHUD.SetupAllWeapons(weapons);

        // 기본 무기 설정 (Single, 단발로 기본 설정)
        SwitchingWeapon(WeaponType.Single);
    }

    void Update()
    {
        if (Time.timeScale != 0)    // 게임이 일시정지 되지 않았을 때만 실행
            UpdateSwitch();
    }

    private void UpdateSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))   // 1번: SingleRifle
        {
            SwitchingWeapon((WeaponType)0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))  // 2번: RapidRifle
        {
            SwitchingWeapon((WeaponType)1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))  // 3번: SniperRifle
        {
            SwitchingWeapon((WeaponType)2);
        }
        else if (Input.GetKeyDown(KeyCode.Q))   // Q키: Rocket
        {
            SwitchingWeapon((WeaponType)3);
        }

        // 마우스 휠 입력 처리
        // 무기 순서는 Single(0) -> Rapid(1) -> Sniper(2) -> Rocket(3) 순서로 설정
        float scroll = Input.GetAxis("Mouse ScrollWheel");  // Input.GetAxis를 float 변수에 저장
        if (scroll > 0f)    // 위로 스크롤 시 scroll 값이 양수
        {
            // 마우스 휠 위로 스크롤: 다음 무기로 변경
            // 배열의 끝에서 다시 0으로 돌아오는 순환 구조를 위해 % 연산자를 사용
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            SwitchingWeapon((WeaponType)currentWeaponIndex);
        }
        else if (scroll < 0f)   // 아래로 스크롤 시 scroll 값이 음수
        {
            // 마우스 휠 아래로 스크롤: 이전 무기로 변경
            currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            SwitchingWeapon((WeaponType)currentWeaponIndex);
        }
    }

    // 무기 교체 함수
    // WeaponType 열거형을 사용하여 무기 종류를 지정
    private void SwitchingWeapon(WeaponType weaponType)
    {
        // 교체 가능한 무기가 없으면 종료
        if (weapons[(int)weaponType] == null)
        {
            return;
        }

        // 현재 사용중인 무기가 있으면 이전 무기 정보에 저장
        if (currentWeapon != null)
        {
            previousWeapon = currentWeapon;
        }

        // 무기 교체
        currentWeapon = weapons[(int)weaponType];   // weaponType을 int로 변환하여 weapons 배열에서 가져옴

        // 현재 사용중인 무기로 교체하려고 할 때 종료
        if (currentWeapon == previousWeapon)
        {
            return;
        }

        // 무기를 사용하는 PlayerController, PlayerHUD에 현재 무기 정보 전달
        playerShotController.SwitchingWeapon(currentWeapon);    // 사격을 담당하는 Controller의 실제 무기 변경
        playerHUD.SwitchingWeapon(currentWeapon);   // HUD를 통해 시각적으로 무기 변경 표시
    }
}

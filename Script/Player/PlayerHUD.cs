using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Player정보를 출력하는 HUD에 관련된 스크립트 입니다.
// 모든 Player에 관련한 스크립트는 Player을 앞에 붙여줍니다.
public class PlayerHUD : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    private WeaponBase weapon;
    [SerializeField]    // Inspector에서 끌어서 사용
    private EnemyMemoryPool enemyMemoryPool;

    [Header("WeaponName")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName; // 무기 이름

    [Header("CrossHair")]
    [SerializeField]
    private Image iamgeCrossHair;   // 조준점 이미지
    [SerializeField]
    private Sprite[] spritesCrossHair;  // 조준점에 사용되는 sprite 배열

    [Header("Sniper HUD")]  // '저격'과 관련된 UI
    [SerializeField]
    private Slider sniperChargeHUD; // 차지샷 Slider UI  
    [SerializeField]
    private Image scope;    // 조준경 UI

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI textAmmo;   // 현재/최대 탄 수 출력 text

    // Stage 및 Player에 관한 UI는 다른 캔버스에서 관리
    [Header("Stage Info")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private StageManager stageManager; // 스테이지 관리 스크립트
    [SerializeField]    // Inspector에서 끌어서 사용
    private TextMeshProUGUI textStage; // 스테이지 정보 출력 text

    [Header("Status Info")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerStatusManager playerStatusManager; // 플레이어 상태 관리 스크립트
    [SerializeField]    // Inspector에서 끌어서 사용
    private BaseStatus baseStatus; // 기지 상태 관리 스크립트
    [SerializeField]    // Inspector에서 끌어서 사용
    private TextMeshProUGUI missionText; // 미션 정보 출력 text

    [Header("Item Info")]
    [SerializeField]
    private TurretController turretController;
    [SerializeField]
    private TutorialTurretController tutorialTurretController;
    [SerializeField]
    private TextMeshProUGUI textTurret;
    [SerializeField]
    private Image turretImage;
    [SerializeField]
    private WeaponRocket weaponRocket;
    [SerializeField]
    private TextMeshProUGUI textRocketCount;
    [SerializeField]
    private Image rocketImage;
    [SerializeField]
    private EMP emp;
    [SerializeField]
    private TextMeshProUGUI textEMP;
    [SerializeField]
    private Image empImage;

    [Header("HP")]
    private int previousHP; // 이전 체력을 저장하는 변수
    private int basePreviousHP; // 기지 이전 체력을 저장하는 변수
    private int prevIncreaseCount = 0;  // 슬라이더 UI 크기변경 카운트
    public Slider playerHPSlider;
    public Slider baseHPSlider;

    [Header("Coin")]
    [SerializeField]
    private TextMeshProUGUI textCoin;

    [Header("Mission")]
    private float missionTimeCount = 5;

    void Awake()
    {
        missionTimeCount = 5;
        if (missionText != null) missionText.text = "10 스테이지까지 생존하세요!";
    }

    void Update()
    {
        if (missionText != null)
        {
            missionTimeCount -= Time.deltaTime;
            if (missionTimeCount <= 0)
            {
                missionText.text = ""; // 미션 텍스트 초기화
            } 
        }

        UpdateAmmoHUD(weapon.CurrentAmmo, weapon.MaxAmmo);  // 실시간으로 총알 개수 업데이트
        UpdateStageUI(); // 스테이지 UI 업데이트

        // 이전 체력과 현재 체력을 비교하여 UpdateHPHUD 호출
        int currentHP = playerStatusManager.CurrentHP; // PlayerStatusManager에서 현재 체력 가져오기

        if (previousHP != currentHP)
        {
            previousHP = currentHP; // 이전 체력을 현재 체력으로 갱신
            if (playerHPSlider != null)
            {
                playerHPSlider.maxValue = playerStatusManager.MaxHp;
                playerHPSlider.value = currentHP;
            }
        }

        if (playerStatusManager.increaseCount != prevIncreaseCount)
        {
            UpdatePlayerHPSliderUI(playerStatusManager.increaseCount);
            prevIncreaseCount = playerStatusManager.increaseCount;
        }

        // 기지 체력 업데이트
        int baseCurrentHP = baseStatus.BaseCurrentHP; // BaseStatus에서 현재 체력 가져오기

        if (basePreviousHP != baseCurrentHP) // 기지 체력이 바뀌었을 때
        {
            basePreviousHP = baseCurrentHP; // 이전 체력을 현재 체력으로 갱신
            // 슬라이더 초기화
            if (baseHPSlider != null)
            {
                baseHPSlider.maxValue = baseStatus.BaseMaxHP;
                baseHPSlider.value = baseCurrentHP;
            }
        }

        UpdateCoinHUD(playerStatusManager.coin);
        UpdateItemHUD();
    }

    // Player의 전체 체력이 증가할때 마다 슬라이더의 크기 증가
    private void UpdatePlayerHPSliderUI(int increaseCount)
    {
        if (playerHPSlider != null)
        {
            RectTransform rt = playerHPSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 기준값(초기값) 설정
                float basePosX = -497f;    // 초기 X 위치 (원하는 값으로 조정)
                float baseWidth = 590f; // 초기 Width (원하는 값으로 조정)
                float basePosY = rt.anchoredPosition.y;
                float baseHeight = rt.sizeDelta.y;

                // 증가분 계산
                float newPosX = basePosX + increaseCount * 15f;
                float newWidth = baseWidth + increaseCount * 30f;

                // 적용
                rt.anchoredPosition = new Vector2(newPosX, basePosY);
                rt.sizeDelta = new Vector2(newWidth, baseHeight);

                // maxValue와 value를 비례해서 갱신
                float hpRatio = (float)playerStatusManager.CurrentHP / playerStatusManager.MaxHp;
                playerHPSlider.maxValue = playerStatusManager.MaxHp;
                playerHPSlider.value = playerStatusManager.MaxHp * hpRatio;
            }
        }
    }

    private void UpdateCoinHUD(int currentCoin)
    {
        textCoin.text = $"Coin: {currentCoin}"; // 코인 UI 업데이트
    }

    private void UpdateStageUI() // 스테이지 UI 업데이트
    {
        if (enemyMemoryPool.isClearStage == true)
        {
            textStage.text = $"stage{stageManager.stageNumber} clear";
        }
        else
        {
            textStage.text = $"stage: " + stageManager.stageNumber.ToString();
        }
    }

    public void SetupAllWeapons(WeaponBase[] weapons)
    {
        for (int i = 0; i < weapons.Length; ++i)
        {
            if (weapons[3]) // 로켓 무기(3번 인덱스)만 처리
            {
                UpdateRocketHUD();
            }
            else if (weapons[i] != null) // 나머지 무기 처리
            {
                weapons[i].onAmmoEvnet.AddListener(UpdateAmmoHUD);
            }
        }
    }

    public void SwitchingWeapon(WeaponBase newWeapon)   // 무기 교체 
    {
        weapon = newWeapon;
        SetUpWeapon();
        // 무기 타입에 따라 조준점 이미지 변경
        switch (weapon.WeaponType)
        {
            case WeaponType.Single:
                iamgeCrossHair.sprite = spritesCrossHair[0]; // Sniper 조준점
                break;
            case WeaponType.Rapid:
                iamgeCrossHair.sprite = spritesCrossHair[1]; // Rapid 조준점
                break;
            case WeaponType.Sniper:
                iamgeCrossHair.sprite = spritesCrossHair[2]; // Single 조준점
                break;
            case WeaponType.Rocket:
                iamgeCrossHair.sprite = spritesCrossHair[3]; // Rocket 조준점
                break;
            default:
                Debug.LogWarning("알 수 없는 무기 타입입니다. 기본 조준점을 설정합니다.");
                iamgeCrossHair.sprite = spritesCrossHair[0]; // 기본 조준점
                break;
        }
    }

    private void SetUpWeapon()
    {
        textWeaponName.text = weapon.WeaponName.ToString(); // 텍스트 변경

        // 무기 타입 확인하여 차지샷 UI 활성화/비활성화
        sniperChargeHUD.gameObject.SetActive(weapon.WeaponType == WeaponType.Sniper);
        // 무기 타입 확인하여 scope UI 활성화/비활성화, 0408_줌 UI 미완성으로 임시 비활성화
        scope.gameObject.SetActive(weapon.WeaponName == WeaponName.Sniper);
    }

    private void UpdateAmmoHUD(int currentAmmo, int maxAmmo)    // 총알 정보를 UI에 표시하는 함수
    {
        if (weapon.WeaponName == WeaponName.Rocket)
        {
            if (weaponRocket.rocketCount > 0)
            {
                textAmmo.text = $"{weaponRocket.rocketCount}";
            }
            else
            {
                textAmmo.text = $"No Rocket";
            }
        }
        else
        {
            textAmmo.text = $"{currentAmmo}/{maxAmmo}";
        }
    }

    private void UpdateRocketHUD()
    {
        textAmmo.text = $"Rocket"; // 로켓 발사기 탄약 UI 업데이트
    }

    public void UpdateSinperChargingHUD(float chargeProgress, bool chargeReady)
    {
        sniperChargeHUD.value = 1;  // Slider의 벨류값으로 저격 가능 상태 표시
        if (chargeReady)
        {
            sniperChargeHUD.value = 1; // 차지 완료
        }
        else
        {
            sniperChargeHUD.value = chargeProgress; // 진행도 표시
        }
    }

    public void UpdateItemHUD()
    {
        // 터렛 쿨타임 업데이트
        if (turretController != null)
        {
            if (turretController.maxTurret > 0)
            {
                turretImage.fillAmount = 1f; // 쿨타임 완료 상태
                textTurret.text = $"{turretController.maxTurret}";
            }
            else
            {
                turretImage.fillAmount = 0;
                textTurret.text = $"{turretController.maxTurret}";
            }
        }
        else
        {
            turretImage.fillAmount = 0f; // 터렛이 없으면 Fill 값을 0으로 설정
        }

        if (tutorialTurretController != null)
        {
            if (tutorialTurretController.maxTurret > 0)
            {
                turretImage.fillAmount = 1f; // 쿨타임 완료 상태
                textTurret.text = $"{tutorialTurretController.maxTurret}";
            }
            else
            {
                turretImage.fillAmount = 0;
                textTurret.text = $"{tutorialTurretController.maxTurret}";
            }
        }
        else if (tutorialTurretController != null)
        {
            turretImage.fillAmount = 0f; // 터렛이 없으면 Fill 값을 0으로 설정
        }

        // 로켓 쿨타임 업데이트
        if (weaponRocket != null)
        {
            if (weaponRocket.rocketCount > 0)
            {
                rocketImage.fillAmount = 1f; // 쿨타임 완료 상태
                textRocketCount.text = $"{weaponRocket.rocketCount}"; // 로켓 개수 UI 업데이트
            }
            else
            {
                rocketImage.fillAmount = 0; // 쿨타임 완료 상태
                textRocketCount.text = $"{weaponRocket.rocketCount}"; // 로켓 개수 UI 업데이트
            }
        }
        else
        {
            rocketImage.fillAmount = 0f; // 로켓이 없으면 Fill 값을 0으로 설정
        }

        // EMP 쿨타임 업데이트
        if (emp != null)
        {
            float empCooldown = emp.empCoolTime; // EMP 쿨타임 가져오기
            float empMaxCooldown = 15; // EMP 최대 쿨타임 가져오기
            if (empCooldown <= 0)
            {
                empImage.fillAmount = 1f; // 쿨타임 완료 상태
                textEMP.text = "0";
            }
            else
            {
                empImage.fillAmount = 1f - (empCooldown / empMaxCooldown); // 쿨타임 진행 상태
                textEMP.text = $"{Mathf.Max(0, empCooldown):F0}"; // 소수점 1자리까지 표시
            }
        }
        else
        {
            empImage.fillAmount = 0f; // EMP가 없으면 Fill 값을 0으로 설정
        }
    }
}

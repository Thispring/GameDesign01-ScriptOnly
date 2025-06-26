using UnityEngine;
using TMPro;
using System.Collections;

// 플레이어가 코인을 통해 성장 할 수 있는 상점에 관한 스크립트입니다.
// 버튼을 통해 연결하며, 상점 기능은 총 10가지 입니다.
// 인게임 버튼 순서대로 변수 및 기능 순서 정렬 
public class ShopManager : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    public StageManager stageManager; // 스테이지 매니저 참조
    public PlayerStatusManager playerStatusManager; // 플레이어 상태 매니저 참조
    public BaseStatus baseStatus; // 기지 상태 참조 추가
    public WeaponRocket weaponRocket; // 로켓 참조 추가
    public WeaponSingleRifle singleWeapon;
    public WeaponRapidRifle rapidWeapon;
    public WeaponSniperRifle sniperWeapon;
    public EMP emp;
    public TurretController turretController;

    [Header("Count")]   // 강화 카운트, 되돌리기를 위해 사용
    public int sniperPowerUpCount = 0;
    public int rapidPowerUpCount = 0;
    public int singlePowerUpCount = 0;
    public int healCount = 0;
    public int increaseMaxHPCount = 0;
    public int repairBaseCount = 0;
    public int addRocketCount = 0;
    public int addTurretCount = 0;
    public int turretPowerUpCount = 0;
    public int empPowerUpCount = 0;

    [Header("Level")]    // 강화 레벨
    public int sniperPowerUpLevel = 1;
    public int rapidPowerUpLevel = 1;
    public int singlePowerUpLevel = 1;
    public int increaseMaxHPLevel = 1;
    public int turretPowerUpLevel = 1;
    public int empPowerUpLevel = 1;

    [Header("Power Up Value")]    // 강화 관련 수치
    private int singlePowerUp = 4;
    private int rapidPowerUp = 2;
    private int sniperPowerUp = 10;

    [Header("Coin")]    // 코인 관련 수치
    private int coinsToPay = 10; // 지불할 코인 수
    public int currrentCoin; // 현재 코인 수

    [Header("Temp Level")]  // 되돌리기를 고려한 임시 레벨
    private int tempSniperPowerUpLevel;
    private int tempRapidPowerUpLevel;
    private int tempSinglePowerUpLevel;
    private int tempIncreaseMaxHPLevel;
    private int tempTurretPowerUpLevel;
    private int tempEmpPowerUpLevel;

    [Header("Limit Count")] // 전체 회복 제한 변수
    private bool playerHPLimit = false;
    private bool baseHPLimit = false;

    [Header("Button Text")]
    public TextMeshProUGUI singleButtonText;
    public TextMeshProUGUI rapidButtonText;
    public TextMeshProUGUI sniperBottonText;
    public TextMeshProUGUI healButtonText;
    public TextMeshProUGUI maxHpButtonText;
    public TextMeshProUGUI repairBaseButtonText;
    public TextMeshProUGUI addTurretButtonText;
    public TextMeshProUGUI turretButtonText;
    public TextMeshProUGUI empButtonText;
    public TextMeshProUGUI rocketButtonText;

    [Header("Level Text")]
    public TextMeshProUGUI singleLevelText;
    public TextMeshProUGUI rapidLevelText;
    public TextMeshProUGUI sniperLevelText;
    public TextMeshProUGUI healLevelText;
    public TextMeshProUGUI maxHpLevelText;
    public TextMeshProUGUI turretLevelText;
    public TextMeshProUGUI empLevelText;

    [Header("Count Text")]
    public TextMeshProUGUI currentRocketText;
    public TextMeshProUGUI currentTurretText;

    [Header("Button Count Text")]
    public TextMeshProUGUI singleCountText;
    public TextMeshProUGUI rapidCountText;
    public TextMeshProUGUI sniperCountText;
    public TextMeshProUGUI healCountText;
    public TextMeshProUGUI maxHpCountText;
    public TextMeshProUGUI reapirBaseCountText;
    public TextMeshProUGUI addTurretCountText;
    public TextMeshProUGUI turretCountText;
    public TextMeshProUGUI empCountText;
    public TextMeshProUGUI rocketCountText;

    [Header("Warning Text")]
    public TextMeshProUGUI warningText;
    [SerializeField]
    private bool warningAddRocket = false;

    [Header("Sound")]
    public AudioClip shopButtonClickClip;
    public AudioClip warningButtonClickClip;
    public AudioClip buttonClickClip;
    [SerializeField]
    private AudioSource buttonAudioSource;

    void Awake()
    {
        sniperPowerUpLevel = 1;
        rapidPowerUpLevel = 1;
        singlePowerUpLevel = 1;

        warningAddRocket = false;
        warningText.text = "";
    }

    void Update()
    {
        ButtonHUD();
        LevelTextHUD();
        CountTextHUD();
        CurrentItemTextHUD();
    }

    public void saveCoin(int coin)
    {
        // coin은 이전 stage 코인
        currrentCoin = coin;
    }

    // 버튼 클릭 사운드 재생 함수
    private void PlayShopButtonSound()
    {
        if (shopButtonClickClip != null && buttonAudioSource != null)
            buttonAudioSource.PlayOneShot(shopButtonClickClip);
    }

    private void PlayWarningButtonSound()
    {
        if (warningButtonClickClip != null && buttonAudioSource != null)
            buttonAudioSource.PlayOneShot(warningButtonClickClip);
    }

    private void PlayButtonClickSound()
    {
        if (buttonClickClip != null && buttonAudioSource != null)
            buttonAudioSource.PlayOneShot(buttonClickClip);
    }

    private void ButtonHUD()
    {
        singleButtonText.text = $"[Single 사격 공격력 증가]\n현재 공격력: {singleWeapon.GetCurrentDamage()}\n\n(Coin: {coinsToPay * (tempSinglePowerUpLevel + 1)})";
        rapidButtonText.text = $"[Rapid 사격 공격력 증가]\n현재 공격력: {rapidWeapon.GetCurrentDamage()}\n\n(Coin: {coinsToPay * (tempRapidPowerUpLevel + 1)})";
        sniperBottonText.text = $"[Sniper 사격 공격력 증가]\n현재 공격력: {sniperWeapon.GetCurrentDamage()}\n\n(Coin: {coinsToPay * (tempSniperPowerUpLevel + 1)})";
        healButtonText.text = $"[전체 HP 회복]\n현재 HP: {PlayerStatusManager.Instance.CurrentHP}\n\n(Coin: {coinsToPay * (tempIncreaseMaxHPLevel + 1)})";
        maxHpButtonText.text = $"[최대 HP 증가]\n현재 최대 HP: {PlayerStatusManager.Instance.MaxHp}\n\n(Coin: {coinsToPay * (tempIncreaseMaxHPLevel + 1)})";
        repairBaseButtonText.text = $"[기지 HP 회복]\n현재 기지 HP: {baseStatus.BaseCurrentHP}\n\n(Coin: {coinsToPay})";
        addTurretButtonText.text = $"[터렛 추가 구매]\n현재 터렛 수: {turretController.maxTurret}\n\n(Coin: {coinsToPay * 3})";
        turretButtonText.text = $"[터렛 업그레이드]\n현재 터렛 HP: {turretController.turretMaxHP}\n현재 터렛 공격력: {turretController.turretDamage}\n\n(Coin: {coinsToPay * (tempTurretPowerUpLevel + 1) + 30})";
        empButtonText.text = $"[EMP 지속시간 증가]\n현재 EMP 지속시간: {emp.empStunTime}\n\n(Coin: {coinsToPay * (tempEmpPowerUpLevel + 1) + 20})";
        rocketButtonText.text = $"[로켓 추가 구매]\n현재 로켓 수: {weaponRocket.rocketCount}\n최대 보유 제한: 3개\n\n(Coin: {coinsToPay * 2})";
    }

    private void LevelTextHUD()
    {
        singleLevelText.text = $"Lv.{tempSinglePowerUpLevel}";
        rapidLevelText.text = $"Lv.{tempRapidPowerUpLevel}";
        sniperLevelText.text = $"Lv.{tempSniperPowerUpLevel}";
        healLevelText.text = $"Lv.{tempIncreaseMaxHPLevel}";
        maxHpLevelText.text = $"Lv.{tempIncreaseMaxHPLevel}";
        turretLevelText.text = $"Lv.{tempTurretPowerUpLevel}";
        empLevelText.text = $"Lv.{tempEmpPowerUpLevel}";
    }

    // 현재 아이템 보유 수치 Text 표시
    private void CurrentItemTextHUD()
    {
        currentRocketText.text = $"{weaponRocket.rocketCount}";
        currentTurretText.text = $"{turretController.maxTurret}";
    }

    private void CountTextHUD()
    {
        singleCountText.text = $"Count: {singlePowerUpCount}";
        rapidCountText.text = $"Count: {rapidPowerUpCount}";
        sniperCountText.text = $"Count: {sniperPowerUpCount}";
        healCountText.text = $"Count: {healCount}";
        maxHpCountText.text = $"Count: {increaseMaxHPCount}";
        reapirBaseCountText.text = $"Count: {repairBaseCount}";
        addTurretCountText.text = $"Count: {addTurretCount}";
        turretCountText.text = $"Count: {turretPowerUpCount}";
        empCountText.text = $"Count: {empPowerUpCount}";
        rocketCountText.text = $"Count: {addRocketCount}";
    }

    public void openShop()
    {
        tempSniperPowerUpLevel = sniperPowerUpLevel;
        tempRapidPowerUpLevel = rapidPowerUpLevel;
        tempSinglePowerUpLevel = singlePowerUpLevel;
        tempIncreaseMaxHPLevel = increaseMaxHPLevel;
        tempTurretPowerUpLevel = turretPowerUpLevel;
        tempEmpPowerUpLevel = empPowerUpLevel;
    }

    private IEnumerator WarningCoin()
    {
        warningText.text = "코인이 부족합니다.";
        yield return new WaitForSecondsRealtime(0.5f);  // 상점 열였을 때, Time.timeScale이 0이므로 Realtime 사용
        warningText.text = "";
    }

    public void SinglePowerUp()
    {
        int cost = coinsToPay * (tempSinglePowerUpLevel + 1);
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            singlePowerUpCount++;
            tempSinglePowerUpLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void RapidPowerUp()
    {
        int cost = coinsToPay * (tempRapidPowerUpLevel + 1);
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            rapidPowerUpCount++;
            tempRapidPowerUpLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void SniperPowerUp()
    {
        int cost = coinsToPay * (tempSniperPowerUpLevel + 1);
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            sniperPowerUpCount++;
            tempSniperPowerUpLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void Heal()
    {
        int cost = coinsToPay * (tempIncreaseMaxHPLevel + 1);
        if (playerStatusManager.coin >= cost && !playerHPLimit)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            healCount++;
            playerHPLimit = true;
        }
        else if (playerHPLimit)
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningHeal());
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    private IEnumerator WarningHeal()
    {
        warningText.text = "이미 회복 했습니다.";
        yield return new WaitForSecondsRealtime(0.5f);  // 상점 열였을 때, Time.timeScale이 0이므로 Realtime 사용
        warningText.text = "";
    }

    public void IncreaseMaxHP()
    {
        int cost = coinsToPay * (tempIncreaseMaxHPLevel + 1);
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            increaseMaxHPCount++;
            tempIncreaseMaxHPLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void AddTurret()
    {
        int cost = coinsToPay * 3;
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            addTurretCount++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void TurretPowerUp()
    {
        int cost = coinsToPay * (tempTurretPowerUpLevel + 1) + 30;
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            turretPowerUpCount++;
            tempTurretPowerUpLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void EMP_PowerUp()
    {
        int cost = coinsToPay * (tempEmpPowerUpLevel + 1) + 20;
        if (playerStatusManager.coin >= cost)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= cost;
            empPowerUpCount++;
            tempEmpPowerUpLevel++;
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    public void RepairBase()
    {
        if (playerStatusManager.coin >= coinsToPay && !baseHPLimit)
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= coinsToPay;
            repairBaseCount++;
            baseHPLimit = true;
        }
        else if (baseHPLimit)
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningRepair());
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    private IEnumerator WarningRepair()
    {
        warningText.text = "이미 수리 했습니다.";
        yield return new WaitForSecondsRealtime(0.5f);  // 상점 열였을 때, Time.timeScale이 0이므로 Realtime 사용
        warningText.text = "";
    }

    public void AddRocket()
    {
        int totalRocket = weaponRocket.rocketCount + addRocketCount;
        if (totalRocket >= 3)
        {
            PlayWarningButtonSound();
            warningAddRocket = true;
            StartCoroutine(WarningRocket());
            return;
        }
        else
        {
            warningAddRocket = false;
        }

        if (!warningAddRocket && playerStatusManager.coin >= (coinsToPay * 2))
        {
            PlayShopButtonSound();
            playerStatusManager.coin -= coinsToPay * 2;
            addRocketCount++;

            // 구매 후 3개 이상이 되는지 다시 체크
            if (weaponRocket.rocketCount + addRocketCount >= 3)
            {
                warningAddRocket = true;
            }
        }
        else if (warningAddRocket)
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningRocket());
        }
        else
        {
            PlayWarningButtonSound();
            StartCoroutine(WarningCoin());
        }
    }

    private IEnumerator WarningRocket()
    {
        warningText.text = "더 이상 로켓을 살 수 없습니다.";
        yield return new WaitForSecondsRealtime(0.5f);  // 상점 열였을 때, Time.timeScale이 0이므로 Realtime 사용
        warningText.text = "";
    }

    public void Reset()
    {
        PlayButtonClickSound();
        playerStatusManager.coin = currrentCoin;
        sniperPowerUpCount = 0;
        rapidPowerUpCount = 0;
        singlePowerUpCount = 0;
        healCount = 0;
        increaseMaxHPCount = 0;
        repairBaseCount = 0;
        addRocketCount = 0;
        addTurretCount = 0;
        turretPowerUpCount = 0;
        empPowerUpCount = 0;
        playerHPLimit = false;
        baseHPLimit = false;

        tempSniperPowerUpLevel = sniperPowerUpLevel;
        tempRapidPowerUpLevel = rapidPowerUpLevel;
        tempSinglePowerUpLevel = singlePowerUpLevel;
        tempIncreaseMaxHPLevel = increaseMaxHPLevel;
        tempTurretPowerUpLevel = turretPowerUpLevel;
        tempEmpPowerUpLevel = empPowerUpLevel;
    }

    public void ResumeGame()
    {
        PlayButtonClickSound();
        // 실제 효과 적용
        for (int i = 0; i < sniperPowerUpCount; i++)
            sniperWeapon.IncreaseDamage(sniperPowerUp * sniperPowerUpLevel);

        for (int i = 0; i < rapidPowerUpCount; i++)
            rapidWeapon.IncreaseDamage(rapidPowerUp * rapidPowerUpLevel);

        for (int i = 0; i < singlePowerUpCount; i++)
            singleWeapon.IncreaseDamage(singlePowerUp * singlePowerUpLevel);

        for (int i = 0; i < increaseMaxHPCount; i++)
        {
            PlayerStatusManager.Instance.IncreaseMaxHP(50);
        }

        for (int i = 0; i < healCount; i++)
            PlayerStatusManager.Instance.HealToMax();

        for (int i = 0; i < repairBaseCount; i++)
            baseStatus.HealToMax();

        for (int i = 0; i < addRocketCount; i++)
            weaponRocket.rocketCount += 1;

        for (int i = 0; i < addTurretCount; i++)
            turretController.maxTurret += 1;

        for (int i = 0; i < turretPowerUpCount; i++)
        {
            // 모든 활성화된 터렛을 찾아서 바로 업그레이드 반영되는지 확인
            Turret[] turrets = FindObjectsOfType<Turret>(true);
            foreach (var turret in turrets)
            {
                turret.maxTurretHP += 10 * turretPowerUpCount;
                turret.turretDamage += 5 * turretPowerUpCount;
            }

            // 터렛 업그레이드 정보 반영
            if (turretController != null)
            {
                // 함수는 hp, 공격력 순서 
                turretController.UpgradeTurret(10 * turretPowerUpCount, 5 * turretPowerUpCount); // 예시: 체력+10, 공격력+5
            }
        }

        for (int i = 0; i < empPowerUpCount; i++)
        {
            emp.empStunTime += empPowerUpCount;
        }

        // 임시 레벨을 실제 레벨에 반영
        sniperPowerUpLevel = tempSniperPowerUpLevel;
        rapidPowerUpLevel = tempRapidPowerUpLevel;
        singlePowerUpLevel = tempSinglePowerUpLevel;
        increaseMaxHPLevel = tempIncreaseMaxHPLevel;
        turretPowerUpLevel = tempTurretPowerUpLevel;
        empPowerUpLevel = tempEmpPowerUpLevel;

        // 카운트 초기화
        sniperPowerUpCount = 0;
        rapidPowerUpCount = 0;
        singlePowerUpCount = 0;
        healCount = 0;
        increaseMaxHPCount = 0;
        repairBaseCount = 0;
        addRocketCount = 0;
        addTurretCount = 0;
        turretPowerUpCount = 0;
        empPowerUpCount = 0;

        playerHPLimit = false;
        baseHPLimit = false;

        // 스테이지 진행
        stageManager.stageNumber += 1;
        stageManager.ResumeGame();
    }
}

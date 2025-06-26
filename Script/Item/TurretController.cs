using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 플레이어가 사용할 수 있는 Turret 설치에 관련된 스크립트입니다.
// 메모리풀로 관리하며, 최대 설치 가능 개수는 1개입니다.
public class TurretController : MonoBehaviour
{
    public static TurretController Instance { get; private set; } // 싱글톤 인스턴스

    [Header("Turret Events")]
    public UnityEvent<Transform> onTurretSpawned;
    public UnityEvent<Transform> onTurretDestroyed;

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    private MemoryPool turretMemoryPool;

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public float turretCoolTime = 2f;
    public int maxTurret = 1;   // 최대 터렛 보유 개수 

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool isCoolTime = false; // 쿨타임 체크용
    private bool usingTurret = false;   // 터렛을 사용했는지 여부
    public bool isActive = false;   // 현재 터렛이 활성화 되어있는지 여부
    private bool isKeyPressed = false;  // 터렛 테스트용

    [Header("TurretPrefab")]
    [SerializeField]
    private GameObject turretPrefab;
    [SerializeField]
    private GameObject turret;

    // 해당 스크립트에 필요한 Vector3 변수들
    [Header("Vector3")]
    private Vector3 turretPos = new Vector3(5, 0, 8.5f);

    // 활성화된 터렛 목록
    private List<Transform> activeTurrets = new List<Transform>();

    [Header("Turret Info")]
    public int turretMaxHP = 50;      // 터렛 최대 체력
    public int turretDamage = 10;     // 터렛 공격력

    void Awake()
    {
        // 소환 관련 변수 초기화
        maxTurret = 1;
        turretCoolTime = 2f;
        isCoolTime = false;

        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // MemoryPool을 이용해 터렛 생성
        turretMemoryPool = new MemoryPool(turretPrefab);

        GameObject item = turretMemoryPool.ActivatePoolItem();
        item.SetActive(false);
        turret = item;

        // 터렛 소환 이벤트 등록
        if (onTurretSpawned == null)
        {
            onTurretSpawned = new UnityEvent<Transform>();
        }

        if (onTurretDestroyed == null)
        {
            onTurretDestroyed = new UnityEvent<Transform>();
        }
    }

    void Update()
    {
        // W키로 터렛 사용
        if (!isActive && maxTurret > 0 && Input.GetKeyDown(KeyCode.W))
        {
            SetTurret();
        }

        // 터렛 소환 테스트 코드
        /*
        if (Input.GetKeyDown(KeyCode.W) && !isKeyPressed)
        {
            isKeyPressed = true;
            SetTurret();
        }
        else
        {
            Debug.Log("터렛 쿨타임 중입니다.");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            isKeyPressed = false;
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            DestroyTurret(turret);
        }
        */
    }

    private void SetTurret()
    {
        if (usingTurret == false)
        {
            if (turret.activeSelf == false)
            {
                maxTurret--;
                isCoolTime = false;
                isActive = true;
                turretCoolTime = 2f;
                turret.SetActive(true);
                turret.transform.position = turretPos;

                // 활성화된 터렛 목록에 추가
                activeTurrets.Add(turret.transform);

                // Enemy에게 Event 함수로 turret의 transform 전달
                NotifyEnemiesOnTurretSpawned(turret.transform);
            }
        }
    }

    public void DestorySign()
    {
        DestroyTurret(turret);
    }

    private void DestroyTurret(GameObject turret)
    {
        isActive = false;
        turret.SetActive(false);
        isKeyPressed = false;
        // 활성화된 터렛 목록에서 제거
        activeTurrets.Remove(turret.transform);

        // Enemy에게 Event 함수로 turret의 transform 전달
        NotifyEnemiesOnTurretDestroyed(turret.transform);
    }

    private void NotifyEnemiesOnTurretSpawned(Transform turretTransform)
    {
        foreach (EnemyFSM enemy in FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None))
        {
            enemy.OnTurretSpawned(turretTransform);
        }
    }

    private void NotifyEnemiesOnTurretDestroyed(Transform turretTransform)
    {
        foreach (EnemyFSM enemy in FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None))
        {
            enemy.OnTurretDestroyed(turretTransform);
        }
    }

    // 활성화된 터렛 목록 반환
    public List<Transform> GetActiveTurrets()
    {
        return activeTurrets;
    }

    // 터렛 업그레이드 함수
    public void UpgradeTurret(int hpIncrease, int damageIncrease)
    {
        turretMaxHP += hpIncrease;
        turretDamage += damageIncrease;
    }
}

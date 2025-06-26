using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 튜토리얼 환경에서 터렛사용을 위한 스크립트 입니다, 터렛의 활성화와 보유 개수에 제한을 두지 않습니다.
// 모든 튜토리얼에 관련한 스크립트는 Tutorial을 이름 앞에 붙여줍니다.
public class TutorialTurretController : MonoBehaviour
{
    public static TutorialTurretController Instance { get; private set; } // 싱글톤 인스턴스

    [Header("Turret Events")]
    public UnityEvent<Transform> onTutorialTurretSpawned;
    public UnityEvent<Transform> onTutorialTurretDestroyed;

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
    private bool isKeyPressed = false;  // 터렛 소환 활성화 체크용

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
        if (onTutorialTurretSpawned == null)
        {
            onTutorialTurretSpawned = new UnityEvent<Transform>();
        }

        if (onTutorialTurretDestroyed == null)
        {
            onTutorialTurretDestroyed = new UnityEvent<Transform>();
        }
    }

    void Update()
    {
        // W키로 터렛 사용
        // 튜토리얼에서는 W키를 반복적으로 사용해, 활성/비활성화 가능
        if (Input.GetKeyDown(KeyCode.W) && !isKeyPressed)
        {
            isKeyPressed = true;
            SetTurret();
        }
        else if (Input.GetKeyDown(KeyCode.W) && isKeyPressed)
        {
            DestroyTurret(turret);
            isKeyPressed = false;
        }
    }

    private void SetTurret()
    {
        if (usingTurret == false)
        {
            if (turret.activeSelf == false)
            {
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
        foreach (TutorialEnemyManager enemy in FindObjectsByType<TutorialEnemyManager>(FindObjectsSortMode.None))
        {
            if (enemy != null)
            {
                enemy.OnTurretSpawned(turretTransform);
            }
        }
    }

    private void NotifyEnemiesOnTurretDestroyed(Transform turretTransform)
    {
        foreach (TutorialEnemyManager enemy in FindObjectsByType<TutorialEnemyManager>(FindObjectsSortMode.None))
        {
            if (enemy != null)
            {
                enemy.OnTurretDestroyed(turretTransform);
            }
        }
    }

    // 활성화된 터렛 목록 반환
    public List<Transform> GetActiveTurrets()
    {
        return activeTurrets;
    }
}

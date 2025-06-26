using UnityEngine;

// 플레이어가 사용할 수 있는 로켓을 발사하는 아이템 스크립트입니다.
// 마우스를 사용하여 발사해야하기 때문에, WeaponBase를 상속받습니다.
public class WeaponRocket : WeaponBase
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private TutorialManager tutorialManager; // 튜토리얼에서 개수 무제한을 위한 참조

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public int rocketCount; // 로켓 발사 개수

    // 해당 스크립트에 필요한 프리팹, 게임오브젝트 
    [Header("Prefabs & GameObjects")]
    [SerializeField]
    private GameObject rocketPrefab; // 로켓 프리팹
    // 해당 스크립트에 필요한 Transform 변수들
    [Header("Transform")]
    [SerializeField]
    private Transform rocketSpawnPoint; // 로켓 발사 위치

    [Header("Audio")]
    [SerializeField]
    private AudioClip audioRocket;   // 로켓 발사 사운드

    void Awake()
    {
        rocketCount = 3;    // 로켓 발사 개수 초기화

        base.Setup();   // WeaponBase의 Setup() 메소드 호출
        mainCamera = Camera.main; // 메인 카메라 캐싱
        audioSource = GetComponent<AudioSource>();  // AudioSource 가져오기
    }

    void Update()
    {
        if (tutorialManager != null)
        {
            rocketCount = 3; // 튜토리얼 모드에서는 로켓 개수를 3으로 고정
        }
        else
        {
            //Debug.LogWarning("TutorialManager가 할당되지 않았습니다. 튜토리얼 모드가 아닐 경우 로켓 개수가 변경되지 않습니다.");
        }
    }

    public override void StartWeaponAction(int type = 0)
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseScreenPos = Input.mousePosition;
        // 마우스 위치로 로켓 발사
        FireRocket(mouseScreenPos);
        // 로켓 개수 감소
        rocketCount--;
    }

    private void FireRocket(Vector3 mouseScreenPos)
    {
        // 마우스 위치를 월드 좌표로 변환
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
        Vector3 targetPosition;

        targetPosition = ray.GetPoint(100f); // Ray의 100 유닛 앞 지점을 목표로 설정

        // 로켓의 방향 설정
        Vector3 direction = (targetPosition - rocketSpawnPoint.position).normalized;

        if (rocketPrefab != null)
        {
            // 설정된 프리팹을 사용하여 로켓 생성
            GameObject rocketObj = Instantiate(rocketPrefab, rocketSpawnPoint.position, Quaternion.LookRotation(direction));
            Rocket rocket = rocketObj.GetComponent<Rocket>();
            if (rocket != null)
            {
                rocket.Setup(direction);
            }
            else
            {
                //Debug.LogError("Rocket 컴포넌트를 찾을 수 없습니다. rocketPrefab에 Rocket 스크립트를 추가하세요.");
            }

            rocketPrefab.SetActive(true); // 로켓 활성화
        }
        else
        {
            //Debug.LogError("rocketPrefab이 설정되지 않았습니다. Inspector에서 rocketPrefab을 확인하세요.");
        }
        PlaySound(audioRocket); // 발사 사운드 재생
    }

    public override void StopWeaponAction(int type = 0)
    {
        // 기능을 사용하지 않지만, WeaponBase을 상속받고 있기에 삭제하지 않고 비워둡니다.
    }

    public override void StartReload()
    {
        // 기능을 사용하지 않지만, WeaponBase을 상속받고 있기에 삭제하지 않고 비워둡니다.
    }
}

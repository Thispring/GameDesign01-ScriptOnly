using UnityEngine;

// 새롭게 작동하는 Player의 사격 관련 스크립트 입니다.
// 고정시점뷰를 고려하여 마우스 입력을 통한 main Camera Ray를 쏘고, 그 방향으로 UI, GameObject를 이동시킵니다.
public class PlayerShotController : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private PlayerStatusManager playerStatusManager;  // Player의 상태를 관리하는 스크립트
    private WeaponRocket weaponRocket; // 로켓
    private WeaponBase weapon;  // 모든 무기가 상속받는 기반 클래스

    [Header("UI Elements")]
    public RectTransform aimUI;
    public RectTransform scopeUI;
    public Canvas canvas;

    [Header("Ray Settings")]
    public float maxRayDistance = 100f;

    [Header("Transform")]
    public Transform bulletSpawnPoint;

    [Header("Ray")]
    private Ray ray;
    private RaycastHit hitInfo;

    [Header("Camera Settings")]
    public Camera sniperCamera;
    private Vector3 cameraOriginPos;
    private bool isCameraMoved = false;
    public float cameraMoveSpeed = 5f; // 이동 속도

    void Awake()
    {
        playerStatusManager = GetComponent<PlayerStatusManager>();  // Player의 상태를 관리하는 스크립트
        weaponRocket = GetComponent<WeaponRocket>();
    }

    void Start()
    {
        cameraOriginPos = Camera.main.transform.position;
    }

    void Update()
    {
        if (Time.timeScale != 0)    // 게임이 일시정지 되지 않았을 때만 실행
            UpdateWeaponAction();   // 무기 액션 업데이트

        // Clamp the mouse position to the screen resolution (1920x1080)
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.x = Mathf.Clamp(mouseScreenPos.x, 0, 1920);
        mouseScreenPos.y = Mathf.Clamp(mouseScreenPos.y, 0, 1080);

        // 화면 끝에 닿았는지 체크 후, 해당 방향으로 이동하는 애니메이션 코드
        Vector3 cameraTargetOffset = Vector3.zero;
        if (mouseScreenPos.x <= 0)
            cameraTargetOffset.x = -1f;
        else if (mouseScreenPos.x >= 854)
            cameraTargetOffset.x = 1f;

        if (mouseScreenPos.y <= 0)
            cameraTargetOffset.y = -1f;
        else if (mouseScreenPos.y >= 480)
            cameraTargetOffset.y = 1f;

        if (cameraTargetOffset != Vector3.zero)
        {
            // 카메라를 부드럽게 이동
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                cameraOriginPos + cameraTargetOffset,
                Time.deltaTime * cameraMoveSpeed
            );
            isCameraMoved = true;
        }
        else if (isCameraMoved)
        {
            // 마우스가 다시 해상도 안으로 들어오면 원래 위치로 복귀
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                cameraOriginPos,
                Time.deltaTime * cameraMoveSpeed
            );
            // 복귀가 거의 완료되면 플래그 해제
            if (Vector3.Distance(Camera.main.transform.position, cameraOriginPos) < 0.01f)
                isCameraMoved = false;
        }

        // SniperRifle 망원 효과를 위한 로직
        if (weapon is WeaponSniperRifle)
        {
            ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            UpdateSniperCamera(mouseScreenPos);
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        }

        Vector3 targetPosition = ray.origin + ray.direction * 5f;
        Vector3 adjustedTargetPosition = new Vector3(targetPosition.x, targetPosition.y, 5f);

        // Check for collision with objects
        if (Physics.Raycast(ray, out hitInfo, maxRayDistance))
        {
            targetPosition = hitInfo.point;
            UpdateAimUI(hitInfo.point);
            UpdateBulletSpawnPoint(hitInfo.point); // Update bullet spawn point
        }
        else
        {
            targetPosition = ray.origin + ray.direction * maxRayDistance;
            Vector3 rayEndPoint = ray.origin + ray.direction * maxRayDistance;
            UpdateAimUI(rayEndPoint);
            UpdateBulletSpawnPoint(hitInfo.point); // Update bullet spawn point
        }
        // Fix the Z value of targetPosition to 5
        targetPosition.z = 5f;
    }

    private void UpdateSniperCamera(Vector3 mouseScreenPos)
    {
        // 마우스 포지션을 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        Vector3 sniperPosition = ray.origin + ray.direction * 10f; // z축 기준으로 적절한 거리 설정
        sniperPosition.z = 5f; // z축 고정

        // sniperCamera 위치 업데이트
        sniperCamera.transform.position = sniperPosition;

        // sniperCamera가 마우스 방향을 바라보도록 설정
        sniperCamera.transform.LookAt(ray.origin + ray.direction * maxRayDistance);
    }

    private void UpdateBulletSpawnPoint(Vector3 worldPosition)
    {
        // Convert world position to local position with Z fixed at 3
        Vector3 adjustedPosition = new Vector3(worldPosition.x, worldPosition.y, 3f);

        // Update the bullet spawn point position
        bulletSpawnPoint.position = adjustedPosition;
    }

    private void UpdateAimUI(Vector3 worldPosition)
    {
        // Convert world position to screen position
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // Convert screen position to local position within the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint);

        // Update the aim UI position
        aimUI.anchoredPosition = localPoint;
        scopeUI.anchoredPosition = localPoint;
    }

    void OnDrawGizmos()
    {
        if (ray.direction != Vector3.zero)
        {
            Gizmos.color = Color.red;

            // Draw the ray from the origin
            if (Physics.Raycast(ray, out hitInfo, maxRayDistance))
            {
                // Draw a line to the hit point
                Gizmos.DrawLine(ray.origin, hitInfo.point);

                // Draw a sphere at the hit point
                Gizmos.DrawSphere(hitInfo.point, 0.1f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(bulletSpawnPoint.position, hitInfo.point);
            }
            else
            {
                // Draw a line to the ray's end point
                Vector3 rayEndPoint = ray.origin + ray.direction * maxRayDistance;
                Gizmos.DrawLine(ray.origin, rayEndPoint);

                // Draw a sphere at the ray's end point
                Gizmos.DrawSphere(rayEndPoint, 0.1f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(bulletSpawnPoint.position, rayEndPoint);
            }
        }
    }

    private void UpdateWeaponAction()
    {
        // Single, Rapid 사격 처리
        if ((weapon is WeaponSingleRifle || weapon is WeaponRapidRifle) && Input.GetMouseButtonDown(0))
        {
            playerStatusManager.isShot = true;
            weapon.StartWeaponAction();
        }
        else if (Input.GetMouseButtonUp(0)) // 마우스 왼쪽 버튼을 뗐을 때
        {
            weapon.StopWeaponAction();
            playerStatusManager.isShot = false;
        }

        // Sniper 사격 처리
        if (weapon is WeaponSniperRifle && Input.GetMouseButton(0))
        {
            playerStatusManager.isShot = true;
            weapon.StartWeaponAction();
        }
        else if (weapon is WeaponSniperRifle && Input.GetMouseButtonUp(0))  // 왼쪽 버튼을 뗐을 때
        {
            weapon.StopWeaponAction();
            playerStatusManager.isShot = false;
        }

        // 재장전
        if (Input.GetKeyDown(KeyCode.R)) // 'R' 키 눌렀을 시 재장전
        {
            // WeaponRocket만 재장전 예외처리
            if (!(weapon is WeaponRocket))
            {
                weapon.StartReload();
                playerStatusManager.isShot = false;
            }
        }

        //** <Rocket은 Item으로 분류하여 따로 처리> **//
        if (weapon is WeaponRocket && weaponRocket.rocketCount > 0 && Input.GetMouseButtonDown(0))
        {
            // 로켓은 따로 엄폐 처리 X
            weapon.StartWeaponAction();
        }
    }

    public void SwitchingWeapon(WeaponBase newWeapon)
    {
        if (weapon != null)
        {
            weapon.StopWeaponAction(); // 기존 무기 액션 종료
        }

        weapon = newWeapon;
        weapon.ResetWeaponState(); // 새 무기의 상태 초기화
    }
}

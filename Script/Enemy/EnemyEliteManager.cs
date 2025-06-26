using UnityEngine;
using System;

// Elite 타입 Enemy를 관리하는 스크립트 입니다.
// 해당 Enemy가 소환 시, 모든 Normal 타입 Enemy가 무적이 됩니다.
// 해당 스크립트에서는 Normal 타입 Layer를 Ignore Raycast로 변경합니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyEliteManager : MonoBehaviour
{
    [Header("Layer")]
    private const string ignoreRaycastLayer = "Ignore Raycast";
    private const string enemyLayer = "Enemy";

    // 이벤트 선언
    public static event Action<bool> OnEliteShieldToggle;
    public static bool IsEliteActive { get; private set; } = false;

    void Update()
    {
        EnemyShield[] shields = FindObjectsOfType<EnemyShield>(true);
        foreach (var shield in shields)
        {
            // 부모와 자식 모두 레이어 변경
            SetLayerRecursively(shield.transform.root.gameObject, LayerMask.NameToLayer(ignoreRaycastLayer));
            shield.HandleShieldActiveCount(1);
        }
    }

    void OnEnable()
    {
        // 이벤트 호출(활성화)
        OnEliteShieldToggle?.Invoke(true);
        IsEliteActive = true;
    }

    void OnDisable()
    {
        // EnemyShield를 가진 모든 오브젝트 찾기
        EnemyShield[] shields = FindObjectsOfType<EnemyShield>(true);
        foreach (var shield in shields)
        {
            // 부모와 자식 모두 레이어 원복
            SetLayerRecursively(shield.transform.root.gameObject, LayerMask.NameToLayer(enemyLayer));
            shield.HandleShieldActiveCount(0);
        }
        // 이벤트 호출(비활성화)
        OnEliteShieldToggle?.Invoke(false);
        IsEliteActive = false;
    }

    // 부모와 자식 모두 레이어 변경
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

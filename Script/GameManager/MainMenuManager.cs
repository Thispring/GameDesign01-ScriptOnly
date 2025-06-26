using UnityEngine;

// 게임의 메인 메뉴를 관리하는 스크립트입니다.
// 캔버스 활성/비활성을 관리하며, scoreManager에게 MainMenuManager를 전달합니다.
// MainMenuManager는 'MainMenu' Scene에만 존재합니다.
public class MainMenuManager : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField]
    private Canvas startButtonCanvas;   // 시작 버튼 캔버스

    void Awake()
    {
        // 시작 버튼 캔버스 초기화
        startButtonCanvas.gameObject.SetActive(false);
    }

    // 버튼을 통해 캔버스를 활성화하는 함수
    public void OnStartButtonCanvas()
    {
        startButtonCanvas.gameObject.SetActive(true);
    }

    // 버튼을 통해 캔버스를 비활성화하는 함수
    public void OffStartButtonCanvas()
    {
        startButtonCanvas.gameObject.SetActive(false);
    }
}

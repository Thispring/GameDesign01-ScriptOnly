using UnityEngine;
using UnityEngine.UI;

// 게임의 설정에 관련된 스크립트 입니다.
public class GameSetting : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private StageManager stageManager;

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    private bool isPaused = false; // 게임 일시정지 상태 플래그
    public bool isSetOn = false;

    [Header("Canvas")]
    [SerializeField]
    private Canvas settingsCanvas; // 설정 UI 캔버스
    [SerializeField]
    private Canvas creditCanvas; // 크레딧 UI 

    [Header("Credit Image")]
    [SerializeField]
    private Sprite[] creditSprites; // 스프라이트 배열로 변경
    [SerializeField]
    private Image creditImage;      // 실제 이미지를 출력할 Image 컴포넌트
    private int currentCreditIndex = 0;   // 이미지 인덱스 변수

    void Awake()
    {
        settingsCanvas.gameObject.SetActive(false); // 설정 UI 비활성화
        if (creditCanvas != null) creditCanvas.gameObject.SetActive(false); // 크레딧 UI 비활성화

        // 시작 시 첫 이미지 출력
        ShowCreditImage(currentCreditIndex);
    }

    void Update()
    {
        if (stageManager != null)
        {
            // ESC 키를 눌렀을 때
            if ((stageManager.firstStage == false && Input.GetKeyDown(KeyCode.Escape)) || (stageManager.stageStart == true && Input.GetKeyDown(KeyCode.Escape)))
            {
                if (settingsCanvas.gameObject.activeSelf) // 설정 UI가 활성화되어 있으면
                {
                    CloseSettings(); // 설정 UI 닫기
                }
                else
                {
                    OpenSettings(); // 설정 UI 열기
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsCanvas.gameObject.activeSelf) // 설정 UI가 활성화되어 있으면
                {
                    CloseSettings(); // 설정 UI 닫기
                }
                else
                {
                    OpenSettings(); // 설정 UI 열기
                }
            }
        }
    }

    public void OpenSettings()
    {
        isSetOn = true;
        settingsCanvas.gameObject.SetActive(true); // 설정 UI 활성화
        TogglePause();
    }

    public void CloseSettings()
    {
        isSetOn = false;
        settingsCanvas.gameObject.SetActive(false); // 설정 UI 비활성화
        TogglePause();
    }

    public void OpenCredit()
    {
        if (creditCanvas != null)
        {
            creditCanvas.gameObject.SetActive(true); // 크레딧 UI 활성화
            TogglePause();
        }
    }

    public void CloseCredit()
    {
        if (creditCanvas != null)
        {
            creditCanvas.gameObject.SetActive(false); // 크레딧 UI 비활성화
            TogglePause();
        }
    }

    // 현재 인덱스의 스프라이트를 이미지에 출력
    private void ShowCreditImage(int index)
    {
        if (creditSprites == null || creditSprites.Length == 0 || creditImage == null)
            return;

        creditImage.sprite = creditSprites[index];
        creditImage.gameObject.SetActive(true);
    }

    // 다음 이미지 출력
    public void ShowNextCreditImage()
    {
        if (creditSprites == null || creditSprites.Length == 0)
            return;

        currentCreditIndex = (currentCreditIndex + 1) % creditSprites.Length;
        ShowCreditImage(currentCreditIndex);
    }

    // 이전 이미지 출력
    public void ShowPreviousCreditImage()
    {
        if (creditSprites == null || creditSprites.Length == 0)
            return;

        currentCreditIndex = (currentCreditIndex - 1 + creditSprites.Length) % creditSprites.Length;
        ShowCreditImage(currentCreditIndex);
    }

    private void TogglePause()
    {
        isPaused = !isPaused; // 일시정지 상태 전환
        Time.timeScale = isPaused ? 0 : 1; // 0이면 일시정지, 1이면 재개
    }

    public void Exit()
    {
        Application.Quit();
    }
}

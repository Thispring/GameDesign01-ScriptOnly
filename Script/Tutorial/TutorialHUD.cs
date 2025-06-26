using UnityEngine;
using UnityEngine.UI;

// Tutorial Scene에만 등장하는 HUD의 관리를 위한 스크립트 입니다.
// 모든 튜토리얼에 관련한 스크립트는 Tutorial을 이름 앞에 붙여줍니다.
public class TutorialHUD : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private GameSetting gameSetting; // 커서 보임을 위한 참조

    [Header("Canvas")]
    [SerializeField]
    private Canvas gameStartCanvas;
    [SerializeField]
    private Canvas tutorialCanvas;

    [Header("Image")]
    [SerializeField]
    private Sprite[] tutorialSprites; // 스프라이트 배열로 변경
    [SerializeField]
    private Image tutorialImage;      // 실제 이미지를 출력할 Image 컴포넌트

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private int currentTutorialIndex = 0;   // 이미지 인덱스 변수

    void Awake()
    {
        // 캔버스 비활성화
        gameStartCanvas.gameObject.SetActive(false);
        tutorialCanvas.gameObject.SetActive(false);
        // isSetOn을 false로 하여, 커서 숨김
        if (gameSetting != null)
            gameSetting.isSetOn = false;

        // 시작 시 첫 이미지 출력
        ShowTutorialImage(currentTutorialIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameStartCanvasOn();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            tutorialCanvasOn();
        }

        if (tutorialCanvas.gameObject.activeSelf && Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 오른쪽 화살표 키를 누르면 다음 이미지 출력
            ShowNextTutorialImage();
        }

        if (tutorialCanvas.gameObject.activeSelf && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 왼쪽 화살표 키를 누르면 이전 이미지 출력
            ShowPreviousTutorialImage();
        }

    }

    // 현재 인덱스의 스프라이트를 이미지에 출력
    private void ShowTutorialImage(int index)
    {
        if (tutorialSprites == null || tutorialSprites.Length == 0 || tutorialImage == null)
            return;

        tutorialImage.sprite = tutorialSprites[index];
        tutorialImage.gameObject.SetActive(true);
    }

    // 다음 이미지 출력
    public void ShowNextTutorialImage()
    {
        if (tutorialSprites == null || tutorialSprites.Length == 0)
            return;

        currentTutorialIndex = (currentTutorialIndex + 1) % tutorialSprites.Length;
        ShowTutorialImage(currentTutorialIndex);
    }

    // 이전 이미지 출력
    public void ShowPreviousTutorialImage()
    {
        if (tutorialSprites == null || tutorialSprites.Length == 0)
            return;

        currentTutorialIndex = (currentTutorialIndex - 1 + tutorialSprites.Length) % tutorialSprites.Length;
        ShowTutorialImage(currentTutorialIndex);
    }

    public void gameStartCanvasOn()
    {
        // G키를 누를 때마다 게임을 일시정지 후, gameStartCanvas의 활성/비활성 전환
        bool isActive = !gameStartCanvas.gameObject.activeSelf;
        gameStartCanvas.gameObject.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        // 커서 활성화
        if (gameSetting != null)
            gameSetting.isSetOn = isActive;
    }

    public void tutorialCanvasOn()
    {
        // T키를 누를 때마다 게임을 일시정지 후, tutorialCanvas의 활성/비활성 전환
        bool isActive = !tutorialCanvas.gameObject.activeSelf;
        tutorialCanvas.gameObject.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        // 커서 활성화
        if (gameSetting != null)
            gameSetting.isSetOn = isActive;
    }
}

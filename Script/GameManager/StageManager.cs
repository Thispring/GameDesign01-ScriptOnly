using UnityEngine;
using UnityEngine.Events;   // UnityEvent 사용을 위한 네임스페이스

// 게임의 스테이지를 관리하는 스크립트입니다.
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; } // 싱글톤 인스턴스 추가

    [Header("Events")]
    public UnityEvent onPauseGameEvent; // PauseGame 이벤트

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private SceneManager sceneManager; // 씬 매니저
    [SerializeField]
    private GameSetting gameSetting;

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public int stageNumber; // 현재 스테이지 번호
    public int gameScore = 0; // 게임 점수, enemy를 처치하여 얻은 killScore를 누적하여 저장합니다.

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    public bool stageStart = false; // 스테이지 시작 여부, false는 스테이지 시작 전, true는 스테이지 시작 후 입니다.
    public bool firstStage = false; // 1스테이지에서 설정 창 활성 용 

    // Player 관련 정보
    [Header("Player Info")] 
    [SerializeField]    // Inspector에서 끌어서 사용
    private PlayerStatusManager playerStatusManager; // PlayerStatusManager 스크립트의 인스턴스, 플레이어 상태 관리에 사용
    [SerializeField]    // Inspector에서 끌어서 사용
    private ShopManager shopManager;
    [SerializeField]    // Inspector에서 끌어서 사용
    private Canvas shopCanvas; // 상점 UI 캔버스

    [Header("Audio")]
    [SerializeField]
    private AudioSource stageAudio;
    [SerializeField]
    private AudioClip[] audioClips;
    private int lastStageAudioIndex = -1; // 마지막으로 재생한 오디오 인덱스 저장

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복된 인스턴스가 있으면 제거
            return;
        }
        Instance = this; // 싱글톤 인스턴스 설정

        // 스테이지 정보 초기화
        stageNumber = 1;
        stageStart = true;
        firstStage = false;
        shopCanvas.gameObject.SetActive(false);
        // UnityEvent에 PauseGame 함수 등록 
        onPauseGameEvent.AddListener(PauseGame);
    }

    void Update()
    {
        // 게임 승리 테스트 용 코드
        /*
        if (Input.GetKeyDown(KeyCode.N))
        {
            stageNumber = 9;
        }
        */
        int audioIndex = -1;

        switch (stageNumber)
        {
            case <= 3: // 스테이지 1~3
                audioIndex = 0;
                break;
            case >= 4 and <= 7: // 스테이지 4~7
                audioIndex = 1;
                break;
            case >= 8 and <= 10: // 스테이지 8~10
                audioIndex = 2;
                break;
            default:
                break;
        }

        // 오디오 클립이 바뀌어야 할 때만 교체 및 재생
        if (audioIndex != -1 && audioClips.Length > audioIndex)
        {
            if (lastStageAudioIndex != audioIndex)
            {
                stageAudio.Stop();
                stageAudio.clip = audioClips[audioIndex];
                stageAudio.Play();
                lastStageAudioIndex = audioIndex;
            }
        }

        // 게임이 시작되면 커서 비활성화
        if (!stageStart || gameSetting.isSetOn)
        {
            Cursor.visible = true;  // 커서 보임
        }
        else
        {
            Cursor.visible = false; // 커서 숨김
        }
    }

    public void PauseGame() // 스테이지 정지 함수
    {
        if (stageNumber >= 10)
        {
            sceneManager.VictoryScene();
        }

        if (stageStart == true) // 스테이지가 시작되었을 때만 발동
        {
            stageStart = false;
            Time.timeScale = 0; // 게임 일시정지
            shopCanvas.gameObject.SetActive(true);
            shopManager.saveCoin(playerStatusManager.coin); // coin을 매개변수로 전달
            shopManager.openShop();
            onPauseGameEvent.Invoke();
        }
    }

    public void ResumeGame()    // 스테이지 재개 함수
    {
        if (stageStart == false)    // 스테이지가 정지되었을 때만 발동
        {
            stageStart = true;
            Time.timeScale = 1; // 게임 재개
            shopCanvas.gameObject.SetActive(false);
            EnemyMemoryPool.onSpawnTileAction?.Invoke();    //  EnemyMemoryPool의 스폰타일 액션 호출
        }
    }
}

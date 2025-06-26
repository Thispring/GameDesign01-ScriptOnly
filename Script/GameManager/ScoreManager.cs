using UnityEngine;
using TMPro;

// 게임의 점수를 관리하는 스크립트 입니다.
public class ScoreManager : MonoBehaviour
{
    public static int Score { get; private set; } = 0; // 점수 초기화

    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private StageManager stageManager;
    [SerializeField]
    private PlayerStatusManager playerStatusManager;
    [SerializeField]
    private MainMenuManager mainMenu;

    [Header("High Score UI")]
    [SerializeField]
    private TextMeshProUGUI[] highScoreTexts = new TextMeshProUGUI[9];
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("High Score Canvas")]
    [SerializeField]
    private Canvas highScoreCanvas; // 최고 점수 UI 캔버스

    // InputField 추가
    [Header("High Score Name Input")]
    [SerializeField]
    private TMP_InputField highScoreNameInput;

    // 게임 점수 배열, static으로 설정
    private static int[] highScores = new int[10];

    // 점수 기록 이름 배열, static으로 설정
    private static string[] highScoreNames = new string[10];

    void Awake()
    {
        // 메인메뉴 Scene에서만 최고점수 불러오기
        if (mainMenu != null && highScoreTexts != null)
        {
            LoadHighScores();
        }

        if (highScoreCanvas != null)
        {
            highScoreCanvas.gameObject.SetActive(false);
        }

        // 점수 초기화
        if (stageManager != null && stageManager.stageNumber == 1)
        {
            Score = 0;
        }

        // TMP_InputField 엔터 이벤트 등록
        if (highScoreNameInput != null)
        {
            highScoreNameInput.onEndEdit.AddListener(OnNameInputEndEdit);
        }
    }

    void Update()
    {
        if (stageManager != null && playerStatusManager != null)
        {
            // 점수는 플레이어의 남은 코인 개수로 설정
            Score = playerStatusManager.coin;
        }

        if (scoreText != null)
        {
            // 점수 UI 업데이트
            scoreText.text = "Score: " + Score.ToString();
        }
    }

    // 엔터 입력 시 호출되는 함수
    private void OnNameInputEndEdit(string input)
    {
        // 엔터로 입력이 끝났을 때만 동작 (모바일 등에서 엔터가 아닌 경우도 있음)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || !string.IsNullOrEmpty(input))
        {
            SaveScoreWithName();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void SaveScoreWithName()
    {
        // 10번째에 점수 추가
        highScores[9] = Score;

        // 내림차순 정렬
        System.Array.Sort(highScores, (a, b) => b.CompareTo(a));

        // 정렬 후, 10번째(마지막) 점수가 새로 들어온 점수가 아니면(=새 점수가 상위 9위에 들어감)
        if (highScores[9] != Score)
        {
            // 기존 10번째(가장 낮은 점수) 이름을 null로 만들어 새로 입력받게 함
            highScoreNames[9] = null;
            PlayerPrefs.SetString("HighScoreName9", "");
        }

        // 이름 입력값 가져오기
        string inputName = highScoreNameInput != null ? highScoreNameInput.text : "NoName";

        // 점수가 들어간 위치에 이름 저장
        for (int i = 0; i < 9; i++)
        {
            if (highScores[i] == Score)
            {
                highScoreNames[i] = inputName;
                PlayerPrefs.SetString("HighScoreName" + i, inputName);
                break;
            }
        }

        // PlayerPrefs에 상위 9개만 저장
        for (int i = 0; i < 9; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, highScores[i]);
        }
        PlayerPrefs.Save();

        UpdateHighScoreUI();
    }

    public void LoadHighScores()
    {
        for (int i = 0; i < highScoreNames.Length; i++)
        {
            highScoreNames[i] = PlayerPrefs.GetString("HighScoreName" + i, "");
        }

        for (int i = 0; i < 9; i++)
        {
            highScores[i] = PlayerPrefs.GetInt("HighScore" + i, 0);
        }
        // 11번째는 항상 0으로 초기화
        highScores[9] = 0;
        UpdateHighScoreUI();
    }

    private void UpdateHighScoreUI()
    {
        for (int i = 0; i < highScoreTexts.Length; i++)
        {
            if (highScoreTexts[i] != null)
                highScoreTexts[i].text = $"{highScoreNames[i]}: {highScores[i]}";
        }
    }

    // 버튼용 함수
    public void ShowHighScoreUI()
    {
        highScoreCanvas.gameObject.SetActive(!highScoreCanvas.gameObject.activeSelf);
    }
}

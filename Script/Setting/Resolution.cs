using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// 게임의 해상도를 관리하는 스크립트입니다.
public class Resolution : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]    // Inspector에서 끌어서 사용
    private TextMeshProUGUI resolutionText;
    [SerializeField]
    private Button fullScreenButton;
    [SerializeField]
    private Button windowedButton;

    [Header("Sprite")]
    [SerializeField]
    private Sprite enabledSprite;
    [SerializeField]
    private Sprite disabledSprite;

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    public int setWidth = 1920; // 사용자 설정 너비
    public int setHeight = 1080; // 사용자 설정 높이

    // 해당 스크립트에 필요한 bool 변수들
    [Header("Flags")]
    [SerializeField]
    private bool allowFullScreen = true;

    void Awake()
    {
        // 해상도 텍스트 설정
        if (resolutionText != null)
        {
            resolutionText.text = "";
        }
    }

    void Start()
    {
        int deviceWidth = Display.main.systemWidth;
        int deviceHeight = Display.main.systemHeight;

        // 이전 모드 불러오기
        int savedMode = PlayerPrefs.GetInt("FullScreenMode", 1); // 기본값: 전체화면

        if (deviceWidth > setWidth || deviceHeight > setHeight)
        {
            Screen.SetResolution(setWidth, setHeight, false);
            Screen.fullScreenMode = FullScreenMode.Windowed;
            allowFullScreen = false;
            PlayerPrefs.SetInt("FullScreenMode", 0); // 강제로 창모드 저장
            PlayerPrefs.Save();
            SetFullScreenMode(false);
        }
        else
        {
#if UNITY_STANDALONE_OSX
            Screen.SetResolution(deviceWidth, deviceHeight, savedMode == 1);
#elif UNITY_STANDALONE_WIN
            Screen.SetResolution(deviceWidth, deviceHeight, savedMode == 1);
#else
            Screen.SetResolution(setWidth, setHeight, savedMode == 1);
#endif
            allowFullScreen = true;
            // 모드 적용
            if (savedMode == 1)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.fullScreen = true;
                SetFullScreenMode(true);
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                SetFullScreenMode(false);
            }
        }
    }

    public void SetFullScreen()
    {
        int deviceWidth = Display.main.systemWidth;
        int deviceHeight = Display.main.systemHeight;

        // 1920x1080을 넘어서는 기기에서만 전체화면 전환 허용
        if (deviceWidth > setWidth || deviceHeight > setHeight)
        {
            Screen.SetResolution(deviceWidth, deviceHeight, true);
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
            PlayerPrefs.SetInt("FullScreenMode", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // 그 외에는 전체화면 전환 불가 메시지
            StartCoroutine(FullScreenTextCoroutine());
        }
    }

    // 창모드, 전체화면 설정 함수 (버튼에 할당)
    public void SetFullScreenMode(bool isFullScreen)
    {
        if (!allowFullScreen && isFullScreen)
        {
            StartCoroutine(FullScreenTextCoroutine());
            return;
        }
        
        if (isFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
            if (fullScreenButton != null && enabledSprite != null)
                fullScreenButton.image.sprite = enabledSprite;
            if (windowedButton != null && disabledSprite != null)
                windowedButton.image.sprite = disabledSprite;
            PlayerPrefs.SetInt("FullScreenMode", 1); // 전체화면 저장
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
            if (windowedButton != null && enabledSprite != null)
                windowedButton.image.sprite = enabledSprite;
            if (fullScreenButton != null && disabledSprite != null)
                fullScreenButton.image.sprite = disabledSprite;
            PlayerPrefs.SetInt("FullScreenMode", 0); // 창모드 저장
        }
        PlayerPrefs.Save();
    }

    private IEnumerator FullScreenTextCoroutine()
    {
        if (resolutionText != null)
        {
            resolutionText.text = "전체화면으로 전환할 수 없습니다.";
            yield return new WaitForSeconds(2f);
            resolutionText.text = "";
        }
    }
}

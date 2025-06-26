using UnityEngine;

// 전체 게임의 Scene을 관리하는 스크립트입니다.
public class SceneManager : MonoBehaviour
{
    private bool isPaused = false; // 게임 일시정지 상태 플래그

    void Awake()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        // 발표용 일시정지 기능
        /*
        if (Input.GetKeyDown(KeyCode.P)) // P 키를 눌렀을 때
        {
            TogglePause(); // 일시정지 상태 전환
        }
        */
    }

    private void TogglePause()
    {
        isPaused = !isPaused; // 일시정지 상태 전환
        Time.timeScale = isPaused ? 0 : 1; // 0이면 일시정지, 1이면 재개
        Debug.Log(isPaused ? "게임 일시정지" : "게임 재개");
    }

    public void MainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // 지정된 씬 이름으로 전환
    }

    public void GameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // 지정된 씬 이름으로 전환
    }

    public void TutorialScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene"); // 지정된 씬 이름으로 전환
    }

    public void ToonIntro()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ToonIntro"); // 지정된 씬 이름으로 전환
    }

    public void DefeatScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DefeatScene"); // 지정된 씬 이름으로 전환
    }

    public void VictoryScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene"); // 지정된 씬 이름으로 전환
    }
}

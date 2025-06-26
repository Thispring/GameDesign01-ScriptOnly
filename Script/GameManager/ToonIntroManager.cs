using UnityEngine;

// 게임 시작 전, 인트로 화면을 관리하는 스크립트 입니다.
// 인트로 화면은 Unity Video Player를 사용하여 비디오를 재생합니다.
public class ToonIntroManager : MonoBehaviour
{
    private float timer = 25f;  // 비디오 시간에 맞춘 타이머 변수

    void Awake()
    {
        timer = 25f;    // 초기 타이머 설정
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // S 키를 눌렀을 때 스킵   
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // GameScene으로 전환
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime; // 타이머 감소
        }
        else
        {
            // 타이머가 0 이하가 되면 GameScene으로 전환
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}

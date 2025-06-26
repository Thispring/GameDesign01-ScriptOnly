using UnityEngine;
using System.Collections;

// 승리 시 애니메이션 출력을 위한 스크립트 입니다.
public class VictorySceneAnimator : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private Camera mainCamera;

    [Header("Canvas")]
    [SerializeField]
    private Canvas victoryCanvas;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip victoryAudio;

    void Start()
    {
        victoryCanvas.gameObject.SetActive(false);

        if (mainCamera == null)
            mainCamera = Camera.main;

        // 카메라 이동/회전 효과 시작
        StartCoroutine(CameraTransition());

        // 이전 씬에서 재생 중인 모든 오디오 정지
        foreach (var source in FindObjectsOfType<AudioSource>())
        {
            source.Stop();
        }

        audioSource.clip = victoryAudio;
        audioSource.Play();
    }

    // 카메라 이동 애니메이션
    private IEnumerator CameraTransition()
    {
        Vector3 targetPosition = new Vector3(-1, 1, 0);
        Quaternion targetRotation = Quaternion.Euler(0, -10, 0);

        float duration = 1.0f; // 전환 시간(초)
        float elapsed = 0f;

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 위치/회전 보정
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        yield return new WaitForSeconds(6f);
        victoryCanvas.gameObject.SetActive(true);
    }
}

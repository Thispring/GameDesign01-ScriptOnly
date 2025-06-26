using UnityEngine;
using System.Collections;

// 패배 시 애니메이션 출력을 위한 스크립트 입니다.
public class DefeatSceneAnimator : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private Camera mainCamera;

    [Header("Canvas")]
    [SerializeField]
    private Canvas defeatCanvas;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip defeatAudio;

    void Start()
    {
        defeatCanvas.gameObject.SetActive(false);

        if (mainCamera == null)
            mainCamera = Camera.main;

        // 카메라 이동/회전 효과 시작
        StartCoroutine(CameraTransition());

        // 이전 씬에서 재생 중인 모든 오디오 정지
        foreach (var source in FindObjectsOfType<AudioSource>())
        {
            source.Stop();
        }

        audioSource.clip = defeatAudio;
        audioSource.Play();
    }

    // 카메라 이동 애니메이션
    private IEnumerator CameraTransition()
    {
        Vector3 targetPosition = new Vector3(0, 1, -2.5f);
        Quaternion targetRotation = Quaternion.Euler(10, -4.29f, 0);

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

        yield return new WaitForSeconds(1f);
        defeatCanvas.gameObject.SetActive(true);
    }
}

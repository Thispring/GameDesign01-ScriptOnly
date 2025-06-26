using System.Collections;
using UnityEngine;

// Enemy의 Sound를 관리하는 스크립트 입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyAudioController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioSource moveAudioSource;
    [SerializeField]
    private AudioClip[] moveAudio;

    void Start()
    {
        // SoundEffect 조절을 위한 audioSource 등록
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null && moveAudioSource != null)
            soundManager.RegisterEnemyAudio(moveAudioSource);
    }

    public void PlayMoveSound()
    {
        StartCoroutine(EnemyMoveSound());
    }

    private IEnumerator EnemyMoveSound()
    {
        for (int i = 0; i < moveAudio.Length; i++)
        {
            moveAudioSource.PlayOneShot(moveAudio[i]);
            yield return new WaitForSeconds(1f);
        }
    }
}

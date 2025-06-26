using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 게임의 소리를 관리하는 스크립트 입니다.
public class SoundManager : MonoBehaviour
{
    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    // 게임 씬이 변경되어도 변경된 볼륨을 사용하기 위해 static 선언
    public static float SavedBGMVolume = 1f;
    public static float SavedSoundEffectVolume = 1f;

    [Header("BGM")]
    [SerializeField]
    private AudioSource bgmAudio;

    [Header("SoundEffect")]
    [SerializeField]
    private List<AudioSource> soundEffectAudios = new List<AudioSource>();

    [Header("UI")]
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider soundEffectSlider;

    void Start()
    {
        // 슬라이더가 존재할 때만 리스너 등록 및 값 동기화
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(BGMVolumeChange);
            bgmSlider.value = SavedBGMVolume;
        }

        if (soundEffectSlider != null)
        {
            soundEffectSlider.onValueChanged.AddListener(SoundEffectVolumeChange);
            soundEffectSlider.value = SavedSoundEffectVolume;
        }

        // 실제 오디오 소스 볼륨 적용
        bgmAudio.volume = SavedBGMVolume;
        foreach (var audio in soundEffectAudios)
        {
            if (audio != null)
                audio.volume = SavedSoundEffectVolume;
        }
    }

    private void BGMVolumeChange(float value)
    {
        bgmAudio.volume = value;
        SavedBGMVolume = value; // static 변수에 저장
    }

    private void SoundEffectVolumeChange(float value)
    {
        foreach (var audio in soundEffectAudios)
        {
            if (audio != null)
                audio.volume = value;
        }
        SavedSoundEffectVolume = value;
    }

    // 새로운 Enemy가 생성될 때 호출
    public void RegisterEnemyAudio(AudioSource enemyAudio)
    {
        if (!soundEffectAudios.Contains(enemyAudio))
            soundEffectAudios.Add(enemyAudio);
        enemyAudio.volume = SavedSoundEffectVolume;
    }
}

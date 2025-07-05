using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop = false;
    public AudioSource source;
}

public class SoundManager : MonoBehaviour
{
    [Header("사운드 설정")]
    [SerializeField] private Sound[] sounds;
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private bool enableSound = true;

    [Header("UI 사운드")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip emotionSelectSound;
    [SerializeField] private AudioClip badgeEarnedSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;

    [Header("배경음악")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.3f;
    [SerializeField] private bool playBackgroundMusic = true;

    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private AudioSource musicSource;
    private Dictionary<string, Sound> soundDictionary;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        soundDictionary = new Dictionary<string, Sound>();

        // 배경음악용 오디오 소스 생성
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;

        // 사운드 초기화
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

            soundDictionary[sound.name] = sound;
        }

        // 기본 UI 사운드 추가
        AddUISound("button_click", buttonClickSound);
        AddUISound("emotion_select", emotionSelectSound);
        AddUISound("badge_earned", badgeEarnedSound);
        AddUISound("success", successSound);
        AddUISound("error", errorSound);

        // 배경음악 시작
        if (playBackgroundMusic && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }

        LoadSoundSettings();
    }

    private void AddUISound(string name, AudioClip clip)
    {
        if (clip == null) return;

        Sound uiSound = new Sound
        {
            name = name,
            clip = clip,
            volume = 0.7f,
            pitch = 1f,
            loop = false
        };

        uiSound.source = gameObject.AddComponent<AudioSource>();
        uiSound.source.clip = uiSound.clip;
        uiSound.source.volume = uiSound.volume * masterVolume;
        uiSound.source.pitch = uiSound.pitch;
        uiSound.source.loop = uiSound.loop;

        soundDictionary[name] = uiSound;
    }

    /// <summary>
    /// 사운드 재생
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (!enableSound) return;

        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null)
            {
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"사운드를 찾을 수 없습니다: {soundName}");
        }
    }

    /// <summary>
    /// 사운드 정지
    /// </summary>
    public void StopSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }
    }

    /// <summary>
    /// 모든 사운드 정지
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }
    }

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (!enableSound || musicSource == null || backgroundMusic == null) return;

        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    /// <summary>
    /// 배경음악 정지
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// 배경음악 일시정지/재개
    /// </summary>
    public void PauseBackgroundMusic(bool pause)
    {
        if (musicSource == null) return;

        if (pause)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        // 모든 사운드 볼륨 업데이트
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * masterVolume;
            }
        }

        // 배경음악 볼륨 업데이트
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        SaveSoundSettings();
    }

    /// <summary>
    /// 음악 볼륨 설정
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        SaveSoundSettings();
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;

        if (!enabled)
        {
            StopAllSounds();
            StopBackgroundMusic();
        }
        else if (playBackgroundMusic)
        {
            PlayBackgroundMusic();
        }

        SaveSoundSettings();
    }

    /// <summary>
    /// 특정 사운드 볼륨 설정
    /// </summary>
    public void SetSoundVolume(string soundName, float volume)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            sound.volume = Mathf.Clamp01(volume);
            
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * masterVolume;
            }
        }
    }

    /// <summary>
    /// 페이드 인 효과와 함께 사운드 재생
    /// </summary>
    public void PlaySoundWithFadeIn(string soundName, float fadeTime = 1f)
    {
        if (!enableSound) return;

        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            StartCoroutine(FadeInSound(sound, fadeTime));
        }
    }

    /// <summary>
    /// 페이드 아웃 효과와 함께 사운드 정지
    /// </summary>
    public void StopSoundWithFadeOut(string soundName, float fadeTime = 1f)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            StartCoroutine(FadeOutSound(sound, fadeTime));
        }
    }

    private IEnumerator FadeInSound(Sound sound, float fadeTime)
    {
        if (sound.source == null) yield break;

        float targetVolume = sound.volume * masterVolume;
        sound.source.volume = 0f;
        sound.source.Play();

        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;
            sound.source.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }

        sound.source.volume = targetVolume;
    }

    private IEnumerator FadeOutSound(Sound sound, float fadeTime)
    {
        if (sound.source == null || !sound.source.isPlaying) yield break;

        float startVolume = sound.source.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;
            sound.source.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        sound.source.volume = 0f;
        sound.source.Stop();
        sound.source.volume = sound.volume * masterVolume;
    }

    /// <summary>
    /// 사운드 설정 저장
    /// </summary>
    private void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetInt("SoundEnabled", enableSound ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 사운드 설정 불러오기
    /// </summary>
    private void LoadSoundSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        enableSound = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSoundEnabled(enableSound);
    }

    #region UI 사운드 편의 메서드

    /// <summary>
    /// 버튼 클릭 사운드 재생
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySound("button_click");
    }

    /// <summary>
    /// 감정 선택 사운드 재생
    /// </summary>
    public void PlayEmotionSelect()
    {
        PlaySound("emotion_select");
    }

    /// <summary>
    /// 배지 획득 사운드 재생
    /// </summary>
    public void PlayBadgeEarned()
    {
        PlaySound("badge_earned");
    }

    /// <summary>
    /// 성공 사운드 재생
    /// </summary>
    public void PlaySuccess()
    {
        PlaySound("success");
    }

    /// <summary>
    /// 에러 사운드 재생
    /// </summary>
    public void PlayError()
    {
        PlaySound("error");
    }

    #endregion

    /// <summary>
    /// 현재 사운드 설정 정보 가져오기
    /// </summary>
    public string GetSoundInfo()
    {
        return $"마스터 볼륨: {masterVolume:F1}, 음악 볼륨: {musicVolume:F1}, 사운드 활성화: {enableSound}";
    }

    /// <summary>
    /// 사운드가 재생 중인지 확인
    /// </summary>
    public bool IsSoundPlaying(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            return sound.source != null && sound.source.isPlaying;
        }
        return false;
    }
}
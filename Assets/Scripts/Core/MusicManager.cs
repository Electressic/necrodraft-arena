using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [Header("Background Music")]
    public AudioSource musicAudioSource;
    public AudioClip backgroundMusic;
    
    [Header("Settings")]
    public bool playOnStart = true;
    public bool loopMusic = true;
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1f;
    
    private float targetVolume = 0.7f;
    private bool isFading = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SetupAudioSource();
    }
    
    void Start()
    {
        LoadVolumeFromSettings();
        
        if (playOnStart && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }
    
    private void SetupAudioSource()
    {
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        musicAudioSource.loop = loopMusic;
        musicAudioSource.playOnAwake = false;
        musicAudioSource.volume = 0f;
    }
    
    private void LoadVolumeFromSettings()
    {
        targetVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        
        if (!isFading)
        {
            musicAudioSource.volume = targetVolume;
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicAudioSource == null) return;
        
        musicAudioSource.clip = backgroundMusic;
        musicAudioSource.Play();
        
        if (fadeInDuration > 0)
        {
            FadeIn();
        }
        else
        {
            musicAudioSource.volume = targetVolume;
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicAudioSource == null) return;
        
        if (fadeOutDuration > 0)
        {
            FadeOut(() => musicAudioSource.Stop());
        }
        else
        {
            musicAudioSource.Stop();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Pause();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.UnPause();
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        targetVolume = volume;
        
        if (!isFading && musicAudioSource != null)
        {
            musicAudioSource.volume = targetVolume;
        }
    }
    
    public void ChangeBackgroundMusic(AudioClip newMusic, bool fadeTransition = true)
    {
        if (newMusic == null) return;
        
        if (fadeTransition && musicAudioSource.isPlaying)
        {
            FadeOut(() => {
                backgroundMusic = newMusic;
                PlayBackgroundMusic();
            });
        }
        else
        {
            backgroundMusic = newMusic;
            PlayBackgroundMusic();
        }
    }
    
    private void FadeIn()
    {
        if (musicAudioSource == null) return;
        
        StartCoroutine(FadeCoroutine(0f, targetVolume, fadeInDuration));
    }
    
    private void FadeOut(System.Action onComplete = null)
    {
        if (musicAudioSource == null) return;
        
        StartCoroutine(FadeCoroutine(musicAudioSource.volume, 0f, fadeOutDuration, onComplete));
    }
    
    private System.Collections.IEnumerator FadeCoroutine(float startVolume, float endVolume, float duration, System.Action onComplete = null)
    {
        isFading = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            if (musicAudioSource != null)
            {
                musicAudioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
            }
            
            yield return null;
        }
        
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = endVolume;
        }
        
        isFading = false;
        onComplete?.Invoke();
    }
    
    public void OnMusicVolumeChanged(float newVolume)
    {
        SetMusicVolume(newVolume);
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            PauseBackgroundMusic();
        else
            ResumeBackgroundMusic();
    }
} 
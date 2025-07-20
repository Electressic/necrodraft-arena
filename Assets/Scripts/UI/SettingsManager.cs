using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Volume Display Text")]
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    
    [Header("Audio Sources (Optional)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("UI Elements")]
    public Button applyButton;
    public Button resetButton;
    
    [Header("Button Sounds")]
    public AudioSource buttonAudioSource;
    public AudioClip buttonClickSound;
    
    [Header("Auto Save")]
    public bool autoSaveOnChange = false;
    
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    
    private float originalMasterVolume;
    private float originalMusicVolume;
    private float originalSfxVolume;
    
    private float unityDefaultMasterVolume;
    private float unityDefaultMusicVolume;
    private float unityDefaultSfxVolume;
    
    private bool isInitializing = true;
    
    void Start()
    {
        StoreUnityDefaults();
        
        SetupSliders();
        LoadSettings();
        SetupSliderListeners();
        SetupButtonListeners();
        SetupButtonAudio();
        
        isInitializing = false;
        UpdateApplyButtonState();
    }
    
    private void StoreUnityDefaults()
    {
        unityDefaultMasterVolume = masterVolumeSlider != null ? masterVolumeSlider.value : 80f;
        unityDefaultMusicVolume = musicVolumeSlider != null ? musicVolumeSlider.value : 50f;
        unityDefaultSfxVolume = sfxVolumeSlider != null ? sfxVolumeSlider.value : 70f;
        
        Debug.Log($"Stored Unity defaults: Master={unityDefaultMasterVolume}%, Music={unityDefaultMusicVolume}%, SFX={unityDefaultSfxVolume}%");
    }
    
    private void SetupButtonAudio()
    {
        if (buttonAudioSource == null)
        {
            buttonAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Created button audio source automatically");
        }
        
        if (buttonClickSound == null)
        {
            MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null && mainMenu.buttonClickSound != null)
            {
                buttonClickSound = mainMenu.buttonClickSound;
                Debug.Log("Using button click sound from MainMenuManager");
            }
            else
            {
                Debug.LogWarning("Button click sound not assigned and MainMenuManager not found or has no sound!");
            }
        }
    }
    
    private void SetupSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 100f;
            masterVolumeSlider.wholeNumbers = true;
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 100f;
            musicVolumeSlider.wholeNumbers = true;
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 100f;
            sfxVolumeSlider.wholeNumbers = true;
        }
    }
    
    private void SetupSliderListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }
    
    private void SetupButtonListeners()
    {
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyButtonClick);
            
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClick);
    }
    
    private void OnApplyButtonClick()
    {
        PlayButtonSound();
        SaveSettings();
    }
    
    private void OnResetButtonClick()
    {
        PlayButtonSound();
        ResetToDefaults();
    }
    
    private void PlayButtonSound()
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
        else
        {
            Debug.LogWarning($"Button audio missing - AudioSource: {(buttonAudioSource != null ? "OK" : "MISSING")}, ClickSound: {(buttonClickSound != null ? "OK" : "MISSING")}");
        }
    }
    
    private void OnMasterVolumeChanged(float percentageValue)
    {
        float normalizedValue = percentageValue / 100f;
        SetMasterVolume(normalizedValue);
        UpdateVolumeText(masterVolumeText, percentageValue);
        
        if (!isInitializing)
            UpdateApplyButtonState();
    }
    
    private void OnMusicVolumeChanged(float percentageValue)
    {
        float normalizedValue = percentageValue / 100f;
        SetMusicVolume(normalizedValue);
        UpdateVolumeText(musicVolumeText, percentageValue);
        
        if (!isInitializing)
            UpdateApplyButtonState();
    }
    
    private void OnSfxVolumeChanged(float percentageValue)
    {
        float normalizedValue = percentageValue / 100f;
        SetSFXVolume(normalizedValue);
        UpdateVolumeText(sfxVolumeText, percentageValue);
        
        if (!isInitializing)
            UpdateApplyButtonState();
    }
    
    private void UpdateVolumeText(TextMeshProUGUI textComponent, float percentageValue)
    {
        if (textComponent != null)
        {
            textComponent.text = $"{percentageValue:F0}%";
        }
    }
    
    private void UpdateApplyButtonState()
    {
        if (applyButton != null)
        {
            bool hasChanges = HasUnsavedChanges();
            applyButton.interactable = hasChanges;
            
            var colors = applyButton.colors;
            colors.normalColor = hasChanges ? Color.white : new Color32(128, 128, 128, 255); // Gray #808080
            applyButton.colors = colors;
            
            Debug.Log($"Apply button state: {(hasChanges ? "ENABLED" : "DISABLED")} - Has changes: {hasChanges}");
        }
    }
    
    private bool HasUnsavedChanges()
    {
        float currentMaster = masterVolumeSlider != null ? masterVolumeSlider.value : originalMasterVolume * 100f;
        float currentMusic = musicVolumeSlider != null ? musicVolumeSlider.value : originalMusicVolume * 100f;
        float currentSfx = sfxVolumeSlider != null ? sfxVolumeSlider.value : originalSfxVolume * 100f;
        
        bool masterChanged = !Mathf.Approximately(currentMaster, originalMasterVolume * 100f);
        bool musicChanged = !Mathf.Approximately(currentMusic, originalMusicVolume * 100f);
        bool sfxChanged = !Mathf.Approximately(currentSfx, originalSfxVolume * 100f);
        
        return masterChanged || musicChanged || sfxChanged;
    }
    
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("MasterVolume", dbValue);
        }
        
        AudioListener.volume = volume;
        
        if (autoSaveOnChange && !isInitializing)
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("MusicVolume", dbValue);
        }
        else if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        GameObject musicManagerObj = GameObject.Find("MusicManager");
        if (musicManagerObj != null)
        {
            musicManagerObj.SendMessage("OnMusicVolumeChanged", volume, SendMessageOptions.DontRequireReceiver);
        }
        
        if (autoSaveOnChange && !isInitializing)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("SFXVolume", dbValue);
        }
        else if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        
        if (autoSaveOnChange && !isInitializing)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }
    }
    
    public void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolumeSlider.value / 100f);
            
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value / 100f);
            
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider.value / 100f);
            
        PlayerPrefs.Save();
        
        originalMasterVolume = masterVolumeSlider != null ? masterVolumeSlider.value / 100f : originalMasterVolume;
        originalMusicVolume = musicVolumeSlider != null ? musicVolumeSlider.value / 100f : originalMusicVolume;
        originalSfxVolume = sfxVolumeSlider != null ? sfxVolumeSlider.value / 100f : originalSfxVolume;
        
        UpdateApplyButtonState();
        Debug.Log("Settings saved!");
    }
    
    public void LoadSettings()
    {
        isInitializing = true;
        
        if (masterVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, unityDefaultMasterVolume / 100f);
            masterVolumeSlider.value = savedVolume * 100f;
            originalMasterVolume = savedVolume;
            
            UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);
            SetMasterVolume(savedVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, unityDefaultMusicVolume / 100f);
            musicVolumeSlider.value = savedVolume * 100f;
            originalMusicVolume = savedVolume;
            
            UpdateVolumeText(musicVolumeText, musicVolumeSlider.value);
            SetMusicVolume(savedVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, unityDefaultSfxVolume / 100f);
            sfxVolumeSlider.value = savedVolume * 100f;
            originalSfxVolume = savedVolume;
            
            UpdateVolumeText(sfxVolumeText, sfxVolumeSlider.value);
            SetSFXVolume(savedVolume);
        }
        
        isInitializing = false;
        
        Debug.Log($"[SettingsManager] Loaded settings - Master: {(masterVolumeSlider?.value ?? 0):F0}%, Music: {(musicVolumeSlider?.value ?? 0):F0}%, SFX: {(sfxVolumeSlider?.value ?? 0):F0}%");
    }
    
    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = unityDefaultMasterVolume;
            UpdateVolumeText(masterVolumeText, unityDefaultMasterVolume);
            SetMasterVolume(unityDefaultMasterVolume / 100f);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = unityDefaultMusicVolume;
            UpdateVolumeText(musicVolumeText, unityDefaultMusicVolume);
            SetMusicVolume(unityDefaultMusicVolume / 100f);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = unityDefaultSfxVolume;
            UpdateVolumeText(sfxVolumeText, unityDefaultSfxVolume);
            SetSFXVolume(unityDefaultSfxVolume / 100f);
        }
        
        UpdateApplyButtonState();
        Debug.Log($"Settings reset to Unity defaults: Master={unityDefaultMasterVolume}%, Music={unityDefaultMusicVolume}%, SFX={unityDefaultSfxVolume}%");
    }
} 
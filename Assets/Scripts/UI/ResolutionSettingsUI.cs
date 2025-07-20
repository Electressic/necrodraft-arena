using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResolutionSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenModeDropdown;
    
    [Header("Button Sounds")]
    public AudioSource buttonAudioSource;
    public AudioClip buttonClickSound;
    
    private ResolutionManager resolutionManager;
    private Resolution[] availableResolutions;
    private bool isInitializing = true;

    private readonly Dictionary<string, Vector2Int[]> resolutionsByAspectRatio = new Dictionary<string, Vector2Int[]>
    {
        ["16:9"] = new Vector2Int[]
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160),
        },
        
        ["21:9"] = new Vector2Int[]
        {
            new Vector2Int(2560, 1080),
            new Vector2Int(3440, 1440),
        },
        
        ["32:9"] = new Vector2Int[]
        {
            new Vector2Int(3840, 1080),
            new Vector2Int(5120, 1440),
        }
    };
    
    private const string RESOLUTION_KEY = "ResolutionPreference";
    private const string FULLSCREEN_MODE_KEY = "FullscreenModePreference";
    
    void Start()
    {
        resolutionManager = FindFirstObjectByType<ResolutionManager>();
        
        SetupDropdowns();
        SetupEventListeners();
        LoadSettings();
        UpdateUI();
        
        isInitializing = false;
        
        Debug.Log("[ResolutionSettingsUI] Resolution settings UI initialized with aspect ratio filtering");
    }
    
    void SetupDropdowns()
    {
        SetupResolutionDropdown();
        SetupFullscreenModeDropdown();
    }
    
    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        List<Vector2Int> allResolutions = new List<Vector2Int>();
        
        foreach (var aspectRatioGroup in resolutionsByAspectRatio)
        {
            allResolutions.AddRange(aspectRatioGroup.Value);
        }
        
        availableResolutions = allResolutions.Distinct().Select(res => new Resolution
        {
            width = res.x,
            height = res.y,
            refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 }
        }).OrderBy(r => r.width).ToArray();
        
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            string aspectRatio = GetAspectRatioString(res.width, res.height);
            string qualityName = GetQualityName(res.width, res.height);
            
            string option = $"{res.width} x {res.height} ({qualityName})";
            if (!string.IsNullOrEmpty(aspectRatio) && aspectRatio != "16:9")
            {
                option += $" {aspectRatio}";
            }
            
            options.Add(option);
            
            if (res.width == Screen.width && res.height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    private string GetQualityName(int width, int height)
    {
        // Standard 16:9 resolutions
        if (width == 1280 && height == 720) return "720p";
        if (width == 1920 && height == 1080) return "1080p";
        if (width == 2560 && height == 1440) return "1440p";
        if (width == 3840 && height == 2160) return "4K";
        
        // Ultrawide variations
        if (width == 2560 && height == 1080) return "1080p";
        if (width == 3440 && height == 1440) return "1440p";
        if (width == 3840 && height == 1080) return "1080p";
        if (width == 5120 && height == 1440) return "1440p";
        
        return $"{height}p";
    }
    
    private string GetAspectRatioString(int width, int height)
    {
        float aspectRatio = (float)width / height;
        
        if (Mathf.Abs(aspectRatio - 16f/9f) < 0.05f) return "16:9";
        if (Mathf.Abs(aspectRatio - 21f/9f) < 0.05f) return "21:9";
        if (Mathf.Abs(aspectRatio - 32f/9f) < 0.05f) return "32:9";
        if (Mathf.Abs(aspectRatio - 4f/3f) < 0.05f) return "4:3";
        if (Mathf.Abs(aspectRatio - 16f/10f) < 0.05f) return "16:10";
        
        if (aspectRatio >= 3.0f) return "32:9";
        if (aspectRatio >= 2.2f) return "21:9";
        
        return "";
    }
    
    void SetupFullscreenModeDropdown()
    {
        if (fullscreenModeDropdown == null) return;
        
        fullscreenModeDropdown.ClearOptions();
        
        List<string> modeOptions = new List<string>
        {
            "Fullscreen",
            "Windowed"
        };
        
        fullscreenModeDropdown.AddOptions(modeOptions);
        
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            fullscreenModeDropdown.value = 1;
        }
        else
        {
            fullscreenModeDropdown.value = 0;
        }
    }
    
    void SetupEventListeners()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            
        if (fullscreenModeDropdown != null)
            fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
    }
    
    public void OnResolutionChanged(int resolutionIndex)
    {
        if (isInitializing || resolutionIndex >= availableResolutions.Length) return;
        
        PlayButtonSound();
        
        Resolution resolution = availableResolutions[resolutionIndex];
        
        Debug.Log($"[ResolutionSettingsUI] Changing resolution to: {resolution.width}x{resolution.height}");
        
        ForceCanvasScalerReset();
        
        if (resolutionManager != null)
        {
            resolutionManager.SetResolution(resolution.width, resolution.height);
        }
        else
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, 
                new RefreshRate() { numerator = 60, denominator = 1 });
        }
        
        StartCoroutine(DelayedCanvasRefresh());
        
        PlayerPrefs.SetString(RESOLUTION_KEY, $"{resolution.width}x{resolution.height}");
        PlayerPrefs.Save();
        
        Debug.Log($"[ResolutionSettingsUI] Resolution applied and saved: {resolution.width}x{resolution.height}");
    }
    
    private void ForceCanvasScalerReset()
    {
        CanvasScalerSetup[] canvasScalers = FindObjectsByType<CanvasScalerSetup>(FindObjectsSortMode.None);
        foreach (CanvasScalerSetup scaler in canvasScalers)
        {
            if (scaler != null)
            {
                scaler.SendMessage("ConfigureCanvasScaler", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    private System.Collections.IEnumerator DelayedCanvasRefresh()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ForceCanvasScalerReset();
        
        Debug.Log("[ResolutionSettingsUI] Canvas scalers refreshed after resolution change");
    }
    
    public void OnFullscreenModeChanged(int modeIndex)
    {
        if (isInitializing) return;
        
        PlayButtonSound();
        
        FullScreenMode mode = modeIndex == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        
        Debug.Log($"[ResolutionSettingsUI] Changing fullscreen mode to: {mode}");
        
        ForceCanvasScalerReset();
        
        if (resolutionManager != null)
        {
            resolutionManager.SetFullscreenMode(mode);
        }
        else
        {
            Screen.SetResolution(Screen.width, Screen.height, mode, 
                new RefreshRate() { numerator = 60, denominator = 1 });
        }
        
        StartCoroutine(DelayedCanvasRefresh());
        
        PlayerPrefs.SetInt(FULLSCREEN_MODE_KEY, modeIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"[ResolutionSettingsUI] Fullscreen mode applied and saved: {mode}");
    }
    
    void LoadSettings()
    {
        isInitializing = true;
        
        if (PlayerPrefs.HasKey(RESOLUTION_KEY))
        {
            string savedResolution = PlayerPrefs.GetString(RESOLUTION_KEY);
            string[] parts = savedResolution.Split('x');
            
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                for (int i = 0; i < availableResolutions.Length; i++)
                {
                    if (availableResolutions[i].width == width && availableResolutions[i].height == height)
                    {
                        if (resolutionDropdown != null)
                        {
                            resolutionDropdown.value = i;
                        }
                        break;
                    }
                }
            }
        }
        
        if (PlayerPrefs.HasKey(FULLSCREEN_MODE_KEY))
        {
            int savedMode = PlayerPrefs.GetInt(FULLSCREEN_MODE_KEY);
            if (fullscreenModeDropdown != null)
            {
                fullscreenModeDropdown.value = savedMode;
            }
        }
        
        isInitializing = false;
    }
    
    void UpdateUI()
    {
        if (resolutionDropdown != null)
        {
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                if (availableResolutions[i].width == Screen.width && 
                    availableResolutions[i].height == Screen.height)
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
            resolutionDropdown.RefreshShownValue();
        }
        
        if (fullscreenModeDropdown != null)
        {
            if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                fullscreenModeDropdown.value = 1;
            }
            else
            {
                fullscreenModeDropdown.value = 0;
            }
            fullscreenModeDropdown.RefreshShownValue();
        }
    }
    
    void PlayButtonSound()
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }
} 
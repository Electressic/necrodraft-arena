using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class ResolutionManager : MonoBehaviour
{
    [Header("Fixed Resolution Settings")]
    public int targetWidth = 1920;
    public int targetHeight = 1080;
    public int targetRefreshRate = 60;
    
    [Header("Windowed Mode Presets")]
    public List<Vector2Int> windowedResolutions = new List<Vector2Int>
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(960, 540),
        new Vector2Int(640, 360)
    };
    
    [Header("Pixel Perfect Settings")]
    public bool enablePixelPerfectFullscreen = true;
    public bool useIntegerScaling = true;
    public bool maintainAspectRatio = true;
    
    [Header("Performance")]
    public bool enableDynamicResolution = false;
    public int targetFrameRate = 60;
    public float minScaleFactor = 0.5f;
    public float maxScaleFactor = 1.0f;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    public static ResolutionManager Instance { get; private set; }
    
    private bool previousFullscreenState;
    private int preferredWidth;
    private int preferredHeight;
    private float currentScaleFactor = 1.0f;
    private RenderTexture renderTarget;
    private bool isUsingRenderScaling = false;
    
    public event System.Action OnResolutionChanged;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            PlayerPrefs.SetInt("FirstLaunch", 1);
            PlayerPrefs.SetInt("FullscreenModePreference", 0);
            PlayerPrefs.SetString("ResolutionPreference", "1920x1080");
            PlayerPrefs.Save();
            
            SetResolutionWithPixelPerfect(1920, 1080, FullScreenMode.FullScreenWindow);
            preferredWidth = 1920;
            preferredHeight = 1080;
            
            if (enableDebugLogging)
            {
                Debug.Log("[ResolutionManager] First launch detected - set to 1920x1080 Fullscreen Window");
            }
        }
        else
        {
            preferredWidth = PlayerPrefs.GetInt("PreferredWidth", targetWidth);
            preferredHeight = PlayerPrefs.GetInt("PreferredHeight", targetHeight);
            
            if (PlayerPrefs.HasKey("FullscreenModePreference"))
            {
                int savedMode = PlayerPrefs.GetInt("FullscreenModePreference");
                FullScreenMode mode = FullScreenMode.FullScreenWindow;
                switch (savedMode)
                {
                    case 0: mode = FullScreenMode.FullScreenWindow; break;
                    case 1: mode = FullScreenMode.ExclusiveFullScreen; break;
                    case 2: mode = FullScreenMode.Windowed; break;
                }
                
                SetResolutionWithPixelPerfect(preferredWidth, preferredHeight, mode);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[ResolutionManager] Loaded saved settings: {preferredWidth}x{preferredHeight} {mode}");
                }
            }
            else
            {   
                if (Screen.fullScreen)
                {
                    SetupFullscreen();
                }
                else
                {
                    SetupWindowed();
                }
            }
        }
        
        previousFullscreenState = Screen.fullScreen;
        
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Initialized - Current resolution: {Screen.width}x{Screen.height}, Fullscreen: {Screen.fullScreen}, Target FPS: {targetFrameRate}, Render Scaling: {isUsingRenderScaling}");
        }
        
        if (enableDynamicResolution)
        {
            InvokeRepeating("MonitorPerformance", 1f, 1f);
        }
    }
    
    void Update()
    {
        if (Screen.fullScreen != previousFullscreenState)
        {
            if (Screen.fullScreen)
            {
                SetupFullscreen();
            }
            else
            {
                SetupWindowed();
            }
            previousFullscreenState = Screen.fullScreen;
            OnResolutionChanged?.Invoke();
        }
    }
    
    private void SetResolutionWithPixelPerfect(int width, int height, FullScreenMode mode)
    {
        if (enablePixelPerfectFullscreen && mode != FullScreenMode.Windowed)
        {
            SetupPixelPerfectFullscreen(width, height, mode);
        }
        else
        {
            CleanupRenderTarget();
            Screen.SetResolution(width, height, mode, new RefreshRate() { numerator = (uint)targetRefreshRate, denominator = 1 });
            isUsingRenderScaling = false;
        }
    }
    
    private void SetupPixelPerfectFullscreen(int targetWidth, int targetHeight, FullScreenMode mode)
    {
        Resolution nativeRes = Screen.currentResolution;
        
        int scaleX = nativeRes.width / targetWidth;
        int scaleY = nativeRes.height / targetHeight;
        
        if (useIntegerScaling)
        {
            int scale = maintainAspectRatio ? Mathf.Min(scaleX, scaleY) : Mathf.Max(scaleX, scaleY);
            scale = Mathf.Max(1, scale);
            
            int scaledWidth = targetWidth * scale;
            int scaledHeight = targetHeight * scale;
            
            bool scalingMakesSense = IsScalingLogical(targetWidth, targetHeight, scale, nativeRes);
            
            if (scaledWidth <= nativeRes.width && scaledHeight <= nativeRes.height && scalingMakesSense)
            {
                SetupRenderTarget(targetWidth, targetHeight);
                Screen.SetResolution(scaledWidth, scaledHeight, mode, new RefreshRate() { numerator = (uint)targetRefreshRate, denominator = 1 });
                isUsingRenderScaling = true;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[ResolutionManager] Integer scaling: {targetWidth}x{targetHeight} -> {scaledWidth}x{scaledHeight} (scale: {scale}x)");
                }
                return;
            }
            else if (enableDebugLogging && !scalingMakesSense)
            {
                Debug.Log($"[ResolutionManager] Skipping integer scaling for {targetWidth}x{targetHeight} (scale: {scale}x) - would cause size inconsistencies");
            }
        }
        
        CleanupRenderTarget();
        Screen.SetResolution(targetWidth, targetHeight, mode, new RefreshRate() { numerator = (uint)targetRefreshRate, denominator = 1 });
        isUsingRenderScaling = false;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Standard fullscreen: {targetWidth}x{targetHeight}");
        }
    }
    
    private bool IsScalingLogical(int targetWidth, int targetHeight, int scale, Resolution nativeRes)
    {
        int effectiveWidth = targetWidth * scale;
        int effectiveHeight = targetHeight * scale;
        
        if (targetWidth == 1280 && targetHeight == 720)
        {
            int scale1080 = Mathf.Min(nativeRes.width / 1920, nativeRes.height / 1080);
            if (scale > scale1080 && scale1080 >= 1)
            {
                return false;
            }
        }
        
        if (effectiveWidth > nativeRes.width || effectiveHeight > nativeRes.height)
        {
            return false;
        }
        
        if (scale > 3)
        {
            return false;
        }
        
        return true;
    }
    
    private void SetupRenderTarget(int width, int height)
    {
        CleanupRenderTarget();
        
        renderTarget = new RenderTexture(width, height, 24);
        renderTarget.filterMode = FilterMode.Point;
        renderTarget.name = "PixelPerfectRenderTarget";
        
    }
    
    private void CleanupRenderTarget()
    {
        if (renderTarget != null)
        {
            renderTarget.Release();
            DestroyImmediate(renderTarget);
            renderTarget = null;
        }
    }
    
    private void SetupFullscreen()
    {
        int width = preferredWidth;
        int height = preferredHeight;
        
        if (!IsResolutionSupported(width, height))
        {
            Resolution[] resolutions = Screen.resolutions;
            if (resolutions.Length > 0)
            {
                Resolution bestRes = resolutions[resolutions.Length - 1];
                width = bestRes.width;
                height = bestRes.height;
                
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"[ResolutionManager] Preferred resolution {preferredWidth}x{preferredHeight} not supported, using {width}x{height}");
                }
            }
        }
        
        SetResolutionWithPixelPerfect(width, height, FullScreenMode.FullScreenWindow);
        
        PlayerPrefs.SetInt("PreferredWidth", width);
        PlayerPrefs.SetInt("PreferredHeight", height);
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Fullscreen setup: {width}x{height} @ {targetRefreshRate}Hz, Render Scaling: {isUsingRenderScaling}");
        }
    }
    
    private void SetupWindowed()
    {
        Vector2Int windowedRes = GetBestWindowedResolution();
        CleanupRenderTarget();
        Screen.SetResolution(windowedRes.x, windowedRes.y, FullScreenMode.Windowed, new RefreshRate() { numerator = (uint)targetRefreshRate, denominator = 1 });
        isUsingRenderScaling = false;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Windowed setup: {windowedRes.x}x{windowedRes.y} @ {targetRefreshRate}Hz");
        }
    }
    
    private Vector2Int GetBestWindowedResolution()
    {
        int targetWindowWidth = Screen.currentResolution.width * 2 / 3;
        int targetWindowHeight = Screen.currentResolution.height * 2 / 3;
        
        foreach (Vector2Int res in windowedResolutions)
        {
            if (res.x <= targetWindowWidth && res.y <= targetWindowHeight)
            {
                return res;
            }
        }
        
        return windowedResolutions[windowedResolutions.Count - 1];
    }
    
    private bool IsResolutionSupported(int width, int height)
    {
        Resolution[] resolutions = Screen.resolutions;
        foreach (Resolution res in resolutions)
        {
            if (res.width == width && res.height == height)
            {
                return true;
            }
        }
        return false;
    }
    
    void MonitorPerformance()
    {
        if (!enableDynamicResolution) return;
        
        float currentFPS = 1f / Time.deltaTime;
        float targetFPS = targetFrameRate;
        
        if (currentFPS < targetFPS * 0.9f && currentScaleFactor > minScaleFactor)
        {
            currentScaleFactor = Mathf.Max(currentScaleFactor - 0.1f, minScaleFactor);
            ApplyDynamicResolution();
            
            if (enableDebugLogging)
            {
                Debug.Log($"[ResolutionManager] Performance drop detected, reducing scale to {currentScaleFactor:F2}");
            }
        }
        else if (currentFPS > targetFPS * 1.1f && currentScaleFactor < maxScaleFactor)
        {
            currentScaleFactor = Mathf.Min(currentScaleFactor + 0.05f, maxScaleFactor);
            ApplyDynamicResolution();
            
            if (enableDebugLogging)
            {
                Debug.Log($"[ResolutionManager] Good performance, increasing scale to {currentScaleFactor:F2}");
            }
        }
    }
    
    void ApplyDynamicResolution()
    {
        int scaledWidth = Mathf.RoundToInt(preferredWidth * currentScaleFactor);
        int scaledHeight = Mathf.RoundToInt(preferredHeight * currentScaleFactor);
        
        SetResolutionWithPixelPerfect(scaledWidth, scaledHeight, Screen.fullScreenMode);
        OnResolutionChanged?.Invoke();
    }
    
    public void SetResolution(int width, int height)
    {
        preferredWidth = width;
        preferredHeight = height;
        
        if (Screen.fullScreen)
        {
            SetupFullscreen();
        }
        else
        {
            CleanupRenderTarget();
            Screen.SetResolution(width, height, FullScreenMode.Windowed, new RefreshRate() { numerator = (uint)targetRefreshRate, denominator = 1 });
            isUsingRenderScaling = false;
        }
        
        OnResolutionChanged?.Invoke();
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Resolution changed to: {width}x{height} @ {targetRefreshRate}Hz, Render Scaling: {isUsingRenderScaling}");
        }
    }
    
    public void SetFullscreenMode(FullScreenMode mode)
    {
        SetResolutionWithPixelPerfect(preferredWidth, preferredHeight, mode);
        PlayerPrefs.SetInt("FullscreenModePreference", (int)mode);
        OnResolutionChanged?.Invoke();
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ResolutionManager] Fullscreen mode changed to: {mode} @ {targetRefreshRate}Hz, Render Scaling: {isUsingRenderScaling}");
        }
    }
    
    public Resolution[] GetAvailableResolutions()
    {
        return Screen.resolutions;
    }
    
    public Vector2Int GetCurrentResolution()
    {
        return new Vector2Int(Screen.width, Screen.height);
    }
    
    public bool IsFullscreen()
    {
        return Screen.fullScreen;
    }
    
    public bool IsUsingRenderScaling()
    {
        return isUsingRenderScaling;
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("PreferredWidth", preferredWidth);
        PlayerPrefs.SetInt("PreferredHeight", preferredHeight);
        PlayerPrefs.Save();
        
        if (enableDebugLogging)
        {
            Debug.Log("[ResolutionManager] Settings saved");
        }
    }
    
    void OnDestroy()
    {
        CleanupRenderTarget();
    }
    
    void OnApplicationQuit()
    {
        CleanupRenderTarget();
    }
} 
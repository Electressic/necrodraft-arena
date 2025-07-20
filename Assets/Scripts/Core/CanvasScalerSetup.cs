using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerSetup : MonoBehaviour
{
    [Header("Reference Resolution")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("Scaling Settings")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;
    
    [Header("Pixel Art Settings")]
    public bool usePixelPerfectMode = false;
    public float pixelPerfectScaleFactor = 1.0f;
    public bool autoDetectPixelPerfect = false;
    
    [Header("Fullscreen Scaling")]
    public bool adaptToRenderScaling = false;
    public bool usePointFilteringForPixelArt = true;
    public bool forceConsistentScaling = true;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private bool wasUsingRenderScaling = false;
    
    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        ConfigureCanvasScaler();
        
        if (ResolutionManager.Instance != null)
        {
            ResolutionManager.Instance.OnResolutionChanged += OnResolutionChanged;
        }
    }
    
    void Start()
    {
        ConfigureCanvasScaler();
    }
    
    void OnDestroy()
    {
        if (ResolutionManager.Instance != null)
        {
            ResolutionManager.Instance.OnResolutionChanged -= OnResolutionChanged;
        }
    }
    
    void OnResolutionChanged()
    {
        ConfigureCanvasScaler();
    }
    
    void ConfigureCanvasScaler()
    {
        if (canvasScaler == null)
        {
            Debug.LogError("[CanvasScalerSetup] CanvasScaler component not found!");
            return;
        }
        
        if (forceConsistentScaling)
        {
            ConfigureConsistentScaling();
        }
        else
        {
            bool isUsingRenderScaling = ResolutionManager.Instance != null && ResolutionManager.Instance.IsUsingRenderScaling();
            
            bool shouldUsePixelPerfect = usePixelPerfectMode || (autoDetectPixelPerfect && isUsingRenderScaling);
            
            if (shouldUsePixelPerfect)
            {
                ConfigurePixelPerfectScaling(isUsingRenderScaling);
            }
            else
            {
                ConfigureStandardScaling();
            }
            
            if (wasUsingRenderScaling != isUsingRenderScaling)
            {
                wasUsingRenderScaling = isUsingRenderScaling;
                if (enableDebugLogging)
                {
                    Debug.Log($"[CanvasScalerSetup] Render scaling state changed: {isUsingRenderScaling}");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"[CanvasScalerSetup] Canvas scaler configured - Mode: {canvasScaler.uiScaleMode}, " +
                     $"Reference: {referenceResolution}, Scale Factor: {canvasScaler.scaleFactor:F2}, " +
                     $"Match: {canvasScaler.matchWidthOrHeight:F2}");
        }
    }
    
    void ConfigureConsistentScaling()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        canvasScaler.referencePixelsPerUnit = 100; // Unity default
        
        if (usePointFilteringForPixelArt)
        {
            SetCanvasFilterMode(FilterMode.Point);
        }
        
        if (enableDebugLogging)
        {
            Vector2Int currentRes = new Vector2Int(Screen.width, Screen.height);
            float effectiveScale = CalculateEffectiveScale(currentRes);
            Debug.Log($"[CanvasScalerSetup] Consistent scaling mode - Effective scale: {effectiveScale:F2} for {currentRes.x}x{currentRes.y}");
        }
    }
    
    void ConfigureStandardScaling()
    {   
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        
        canvasScaler.referencePixelsPerUnit = 100;
        
        if (usePointFilteringForPixelArt)
        {
            SetCanvasFilterMode(FilterMode.Point);
        }
    }
    
    void ConfigurePixelPerfectScaling(bool isUsingRenderScaling = false)
    {
        if (isUsingRenderScaling && adaptToRenderScaling)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            
            Vector2Int currentRes = ResolutionManager.Instance.GetCurrentResolution();
            float scaleFactorX = currentRes.x / referenceResolution.x;
            float scaleFactorY = currentRes.y / referenceResolution.y;
            float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);
            
            scaleFactor = Mathf.Max(1f, Mathf.Floor(scaleFactor));
            
            canvasScaler.scaleFactor = scaleFactor;
            canvasScaler.referencePixelsPerUnit = 1;
        }
        else
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasScaler.scaleFactor = pixelPerfectScaleFactor;
            canvasScaler.referencePixelsPerUnit = 1;
        }
        
        SetCanvasFilterMode(FilterMode.Point);
        
        if (enableDebugLogging)
        {
            Debug.Log($"[CanvasScalerSetup] Pixel perfect mode enabled with scale factor: {canvasScaler.scaleFactor}, Render scaling: {isUsingRenderScaling}");
        }
    }
    
    float CalculateEffectiveScale(Vector2Int screenSize)
    {
        float scaleX = screenSize.x / referenceResolution.x;
        float scaleY = screenSize.y / referenceResolution.y;
        return Mathf.Lerp(scaleX, scaleY, matchWidthOrHeight);
    }
    
    void SetCanvasFilterMode(FilterMode mode)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && usePointFilteringForPixelArt)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
} 
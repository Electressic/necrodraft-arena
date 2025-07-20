using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    [Header("Target Aspect Ratio")]
    public float targetAspectRatio = 16f / 9f; // 1.777... for 1920x1080
    
    [Header("Camera Settings")]
    public Camera targetCamera;
    public bool adjustOrthographicSize = true;
    public float baseOrthographicSize = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool enableDebugLogging = true;
    
    private float lastScreenWidth;
    private float lastScreenHeight;
    
    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
            
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
            
        if (targetCamera == null)
        {
            Debug.LogError("[AspectRatioController] No camera found! Please assign a camera.");
            enabled = false;
            return;
        }
        
        if (ResolutionManager.Instance != null)
        {
            ResolutionManager.Instance.OnResolutionChanged += OnResolutionChanged;
        }
        
        if (baseOrthographicSize <= 0 && targetCamera.orthographic)
        {
            baseOrthographicSize = targetCamera.orthographicSize;
        }
        
        targetCamera.rect = new Rect(0, 0, 1, 1);
        
        AdjustCameraForAspectRatio();
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
    
    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            OnResolutionChanged();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
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
        AdjustCameraForAspectRatio();
    }
    
    void AdjustCameraForAspectRatio()
    {
        if (targetCamera == null) return;
        
        float currentAspect = (float)Screen.width / Screen.height;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[AspectRatioController] Adjusting for aspect ratio - Current: {currentAspect:F3}, Target: {targetAspectRatio:F3}");
        }
        
        if (Mathf.Approximately(currentAspect, targetAspectRatio))
        {
            targetCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            
            if (adjustOrthographicSize && targetCamera.orthographic)
            {
                targetCamera.orthographicSize = baseOrthographicSize;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log("[AspectRatioController] Perfect aspect ratio match - using full screen");
            }
            return;
        }
        
        if (currentAspect > targetAspectRatio)
        {
            float inset = 1.0f - targetAspectRatio / currentAspect;
            targetCamera.rect = new Rect(inset / 2, 0.0f, 1.0f - inset, 1.0f);
            
            if (adjustOrthographicSize && targetCamera.orthographic)
            {
                targetCamera.orthographicSize = baseOrthographicSize;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[AspectRatioController] Pillarboxing applied - bars on sides, inset: {inset:F3}");
            }
        }
        else
        {
            float inset = 1.0f - currentAspect / targetAspectRatio;
            targetCamera.rect = new Rect(0.0f, inset / 2, 1.0f, 1.0f - inset);
            
            if (adjustOrthographicSize && targetCamera.orthographic)
            {
                targetCamera.orthographicSize = baseOrthographicSize * targetAspectRatio / currentAspect;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[AspectRatioController] Letterboxing applied - bars on top/bottom, inset: {inset:F3}");
            }
        }
    }
} 
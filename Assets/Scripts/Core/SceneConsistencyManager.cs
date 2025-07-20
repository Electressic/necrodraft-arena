using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SceneConsistencyManager : MonoBehaviour
{
    [Header("Reference Settings")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    public float matchWidthOrHeight = 0.5f;
    public float baseOrthographicSize = 5f;
    public float targetAspectRatio = 16f / 9f;
    
    void Awake()
    {
        SetupCanvas();
        SetupCamera();
        SetupAspectRatio();
    }
    
    void SetupCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.transform.parent != null) continue;
            
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            
            Debug.Log($"Canvas '{canvas.name}' configured with reference resolution {referenceResolution}");
        }
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found!");
            return;
        }
        
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = baseOrthographicSize;
        
        Debug.Log($"Main camera orthographic size set to {baseOrthographicSize}");
    }
    
    void SetupAspectRatio()
    {
        AspectRatioController controller = Camera.main?.GetComponent<AspectRatioController>();
        
        if (controller != null)
        {
            Debug.Log("Aspect Ratio Controller found and active");
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Apply Consistent Scene Settings")]
    static void ApplySettingsToCurrentScene()
    {
        GameObject manager = new GameObject("_SceneConsistencyManager");
        SceneConsistencyManager component = manager.AddComponent<SceneConsistencyManager>();
        
        component.SetupCanvas();
        component.SetupCamera();
        component.SetupAspectRatio();
        
        DestroyImmediate(manager);
        
        Debug.Log("Scene settings applied successfully!");
    }
#endif
}
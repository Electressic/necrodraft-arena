using UnityEngine;

public class WorldSpaceCanvasScaler : MonoBehaviour
{
    [Header("Reference Settings")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    public float baseScale = 0.01f;
    
    [Header("Camera Reference")]
    public Camera targetCamera;
    
    private Canvas canvas;
    private RectTransform canvasRect;
    
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = GetComponent<RectTransform>();
        
        if (targetCamera == null)
            targetCamera = Camera.main;
            
        ScaleCanvasForResolution();
    }
    
    void ScaleCanvasForResolution()
    {
        if (canvas.renderMode != RenderMode.WorldSpace) return;
        
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float referenceHeight = referenceResolution.y;
        
        float scaleFactor = (cameraHeight / referenceHeight) * baseScale;
        
        canvasRect.localScale = Vector3.one * scaleFactor;
        
        Debug.Log($"[WorldSpaceCanvasScaler] Camera height: {cameraHeight}, Scale: {scaleFactor}");
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class DraggablePartItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public PartData partData;
    public Image iconImage;
    
    // Private fields
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private MinionAssemblyManager assemblyManager;
    
    // Drag ghost/preview
    private GameObject dragGhost;
    private RectTransform ghostRectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Find canvas in parents
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        assemblyManager = FindFirstObjectByType<MinionAssemblyManager>();
    }
    
    public void Initialize(PartData part, MinionAssemblyManager manager)
    {
        partData = part;
        assemblyManager = manager;
        
        if (iconImage != null && part != null)
        {
            iconImage.sprite = part.icon;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (partData == null || canvas == null) return;
        
        CreateDragGhost();
        
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        
        if (assemblyManager != null && assemblyManager.enableDebugLogging)
            Debug.Log($"[DraggablePartItem] Started dragging {partData.partName}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null || ghostRectTransform == null) return;
        
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            null, // Use null for Screen Space - Overlay canvases
            out localPointerPosition))
        {
            ghostRectTransform.localPosition = localPointerPosition;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Safely restore original object state
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        GameObject dropTarget = eventData.pointerEnter;
        bool droppedSuccessfully = false;
        
        if (dropTarget != null)
        {
            MinionListEntryUI minionEntry = dropTarget.GetComponentInParent<MinionListEntryUI>();
            if (minionEntry != null)
            {
                droppedSuccessfully = minionEntry.TryEquipPart(partData);
            }
        }
        
        // Clean up drag ghost immediately to prevent reference errors
        DestroyDragGhost();
        
        if (assemblyManager != null && assemblyManager.enableDebugLogging)
        {
            if (droppedSuccessfully)
                Debug.Log($"[DraggablePartItem] Successfully equipped {partData.partName}");
            else
                Debug.Log($"[DraggablePartItem] Drag cancelled for {partData.partName}");
        }
    }
    
    void CreateDragGhost()
    {
        if (dragGhost != null || canvas == null) return;
        
        try
        {
            dragGhost = Instantiate(gameObject, canvas.transform);
            ghostRectTransform = dragGhost.GetComponent<RectTransform>();
            
            // Remove components that could cause issues
            DraggablePartItem ghostDraggable = dragGhost.GetComponent<DraggablePartItem>();
            if (ghostDraggable != null)
                DestroyImmediate(ghostDraggable);
            
            Button ghostButton = dragGhost.GetComponent<Button>();
            if (ghostButton != null)
            {
                ghostButton.interactable = false;
                ghostButton.onClick.RemoveAllListeners();
            }
            
            // Fix text layout BEFORE setting up canvas group
            FixGhostTextLayout();
            
            // Setup canvas group
            CanvasGroup ghostCanvasGroup = dragGhost.GetComponent<CanvasGroup>();
            if (ghostCanvasGroup == null)
                ghostCanvasGroup = dragGhost.AddComponent<CanvasGroup>();
            
            ghostCanvasGroup.alpha = 0.8f;
            ghostCanvasGroup.blocksRaycasts = false;
            
            // Scale for better visibility
            dragGhost.transform.localScale = Vector3.one * 1.1f;
            
            // Position at cursor
            Vector2 cursorPosition = GetCursorPosition();
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                cursorPosition, 
                null, // Use null for Screen Space - Overlay canvases
                out mousePos);
            ghostRectTransform.localPosition = mousePos;
            
            if (assemblyManager != null && assemblyManager.enableDebugLogging)
                Debug.Log($"[DraggablePartItem] Created drag ghost for {partData.partName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DraggablePartItem] Failed to create drag ghost: {e.Message}");
            DestroyDragGhost();
        }
    }
    
    void FixGhostTextLayout()
    {
        if (dragGhost == null) return;
        
        // Wait one frame to ensure the object is fully instantiated
        StartCoroutine(FixTextLayoutCoroutine());
    }
    
    System.Collections.IEnumerator FixTextLayoutCoroutine()
    {
        yield return null; // Wait one frame
        
        if (dragGhost == null) yield break;
        
        TextMeshProUGUI[] textComponents = dragGhost.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI textComponent in textComponents)
        {
            if (textComponent != null)
            {
                // Fix the text wrapping mode
                textComponent.textWrappingMode = TextWrappingModes.NoWrap; // Changed from Normal
                textComponent.overflowMode = TextOverflowModes.Overflow;
                
                // Ensure auto-sizing is disabled to prevent layout issues
                textComponent.enableAutoSizing = false;
                
                // Fix RectTransform - make sure it has proper width
                RectTransform textRect = textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // Set anchors to stretch
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(5, 5);
                    textRect.offsetMax = new Vector2(-5, -5);
                    
                    // Force a minimum width to prevent single-character lines
                    textRect.sizeDelta = new Vector2(Mathf.Max(200f, textRect.sizeDelta.x), textRect.sizeDelta.y);
                    
                    // Force immediate layout update
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
                }
                
                // Force mesh update
                textComponent.ForceMeshUpdate();
            }
        }
        
        // Final layout rebuild on the entire ghost
        if (ghostRectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ghostRectTransform);
        }
    }
    
    Vector2 GetCursorPosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }
    
    void DestroyDragGhost()
    {
        if (dragGhost != null)
        {
            // Remove from parent to prevent reference issues
            try
            {
                if (dragGhost.transform.parent != null)
                    dragGhost.transform.SetParent(null);
            }
            catch (System.Exception) 
            {
                // Object may already be destroyed, continue cleanup
            }
            
            Destroy(dragGhost);
            dragGhost = null;
        }
        
        ghostRectTransform = null;
    }
    
    void OnDestroy()
    {
        DestroyDragGhost();
    }
}
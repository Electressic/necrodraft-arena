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
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private MinionAssemblyManager assemblyManager;
    
    private GameObject dragGhost;
    private RectTransform ghostRectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
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
            null,
            out localPointerPosition))
        {
            ghostRectTransform.localPosition = localPointerPosition;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
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
            
            DraggablePartItem ghostDraggable = dragGhost.GetComponent<DraggablePartItem>();
            if (ghostDraggable != null)
                DestroyImmediate(ghostDraggable);
            
            Button ghostButton = dragGhost.GetComponent<Button>();
            if (ghostButton != null)
            {
                ghostButton.interactable = false;
                ghostButton.onClick.RemoveAllListeners();
            }
            
            FixGhostTextLayout();
            
            CanvasGroup ghostCanvasGroup = dragGhost.GetComponent<CanvasGroup>();
            if (ghostCanvasGroup == null)
                ghostCanvasGroup = dragGhost.AddComponent<CanvasGroup>();
            
            ghostCanvasGroup.alpha = 0.8f;
            ghostCanvasGroup.blocksRaycasts = false;
            
            dragGhost.transform.localScale = Vector3.one * 1.1f;
            
            Vector2 cursorPosition = GetCursorPosition();
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                cursorPosition, 
                null,
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

        StartCoroutine(FixTextLayoutCoroutine());
    }
    
    System.Collections.IEnumerator FixTextLayoutCoroutine()
    {
        yield return null;
        
        if (dragGhost == null) yield break;
        
        TextMeshProUGUI[] textComponents = dragGhost.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI textComponent in textComponents)
        {
            if (textComponent != null)
            {
                textComponent.textWrappingMode = TextWrappingModes.NoWrap;
                textComponent.overflowMode = TextOverflowModes.Overflow;
                
                textComponent.enableAutoSizing = false;
                
                RectTransform textRect = textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(5, 5);
                    textRect.offsetMax = new Vector2(-5, -5);
                    
                    textRect.sizeDelta = new Vector2(Mathf.Max(200f, textRect.sizeDelta.x), textRect.sizeDelta.y);
                    
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
                }
                
                textComponent.ForceMeshUpdate();
            }
        }
        
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
            try
            {
                if (dragGhost.transform.parent != null)
                    dragGhost.transform.SetParent(null);
            }
            catch (System.Exception) 
            {
                Debug.LogError($"[DraggablePartItem] Failed to destroy drag ghost {dragGhost.name}");
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
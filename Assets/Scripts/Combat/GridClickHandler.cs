using UnityEngine;
using UnityEngine.InputSystem;

public class GridClickHandler : MonoBehaviour
{
    [Header("References")]
    public CombatManager combatManager;
    public GridSpawnManager gridSpawnManager;
    
    [Header("Visual Feedback")]
    public Material highlightMaterial;
    private GameObject highlightQuad;
    
    // Input System references
    private Mouse mouse;
    private Camera mainCamera;
    
    void Start()
    {
        CreateHighlightQuad();
        
        // Get Input System mouse reference
        mouse = Mouse.current;
        if (mouse == null)
        {
            Debug.LogError("[GridClickHandler] No mouse detected in Input System!");
        }
        
        // Cache main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[GridClickHandler] No main camera found!");
        }
    }
    
    void CreateHighlightQuad()
    {
        // Create a visual highlight for valid positions
        highlightQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        highlightQuad.name = "GridHighlight";
        highlightQuad.transform.SetParent(transform);
        
        // Scale to match cell size exactly and position correctly
        highlightQuad.transform.localScale = Vector3.one * gridSpawnManager.cellSize * 0.95f;
        highlightQuad.transform.rotation = Quaternion.identity; // Ensure no rotation
        
        // Set up material
        Renderer renderer = highlightQuad.GetComponent<Renderer>();
        if (highlightMaterial != null)
        {
            renderer.material = highlightMaterial;
        }
        else
        {
            // Create default highlight material with better visibility
            Material defaultHighlight = new Material(Shader.Find("Sprites/Default"));
            defaultHighlight.color = new Color(0, 1, 0, 0.4f); // Slightly more opaque green
            defaultHighlight.renderQueue = 3000; // Render on top
            renderer.material = defaultHighlight;
        }
        
        // Remove collider to avoid interference
        Collider collider = highlightQuad.GetComponent<Collider>();
        if (collider != null)
            DestroyImmediate(collider);
        
        highlightQuad.SetActive(false);
    }
    
    void Update()
    {
        if (combatManager.currentState == CombatManager.CombatState.Preview)
        {
            HandleMouseInput();
        }
        else
        {
            if (highlightQuad != null)
                highlightQuad.SetActive(false);
        }
    }
    
    void HandleMouseInput()
    {
        if (mouse == null || mainCamera == null) return;
        
        // Get mouse position using new Input System
        Vector2 mouseScreenPos = mouse.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mouseWorldPos.z = 0; // Ensure Z is 0 for 2D
        
        Vector2Int gridPos = gridSpawnManager.WorldToGridPosition(mouseWorldPos);
        
        // Show highlight if hovering over valid position
        if (IsValidPosition(gridPos))
        {
            Vector3 worldPos = gridSpawnManager.grid[gridPos.x, gridPos.y].worldPosition;
            // Ensure highlight is positioned exactly at grid center with slight Z offset for visibility
            highlightQuad.transform.position = new Vector3(worldPos.x, worldPos.y, -0.1f);
            highlightQuad.SetActive(true);
        }
        else
        {
            highlightQuad.SetActive(false);
        }
        
        // Handle clicks using new Input System
        if (mouse.leftButton.wasPressedThisFrame)
        {
            HandleLeftClick(mouseWorldPos, gridPos);
        }
        else if (mouse.rightButton.wasPressedThisFrame)
        {
            HandleRightClick();
        }
    }
    
    void HandleLeftClick(Vector3 worldPos, Vector2Int gridPos)
    {
        Debug.Log($"[GridClickHandler] Left click at world position: {worldPos}, grid position: {gridPos}");
        
        // First check if we clicked on a preview indicator
        Collider2D clickedCollider = Physics2D.OverlapPoint(worldPos);
        if (clickedCollider != null)
        {
            PreviewUnitClickHandler clickHandler = clickedCollider.GetComponent<PreviewUnitClickHandler>();
            if (clickHandler != null)
            {
                Debug.Log($"[GridClickHandler] Clicked on preview unit {clickHandler.previewIndex}");
                combatManager.SelectMinionForRepositioning(clickHandler.previewIndex);
                return;
            }
        }
        
        // If we have a selected minion and clicked on a valid grid position
        if (combatManager.selectedMinionForRepositioning != -1 && IsValidPosition(gridPos))
        {
            if (!IsCellOccupiedByOtherMinion(gridPos, combatManager.selectedMinionForRepositioning))
            {
                Debug.Log($"[GridClickHandler] Moving minion to position {gridPos}");
                combatManager.MoveMinionToPosition(gridPos);
            }
            else
            {
                Debug.Log($"[GridClickHandler] Position {gridPos} is occupied by another minion");
            }
        }
        else
        {
            Debug.Log($"[GridClickHandler] Click ignored - selected minion: {combatManager.selectedMinionForRepositioning}, valid position: {IsValidPosition(gridPos)}");
        }
    }
    
    void HandleRightClick()
    {
        Debug.Log("[GridClickHandler] Right click detected");
        // Cancel selection
        if (combatManager.selectedMinionForRepositioning != -1)
        {
            combatManager.DeselectMinion();
        }
    }
    
    bool IsValidPosition(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= gridSpawnManager.gridWidth || 
            gridPos.y < 0 || gridPos.y >= gridSpawnManager.gridHeight)
            return false;
            
        return gridSpawnManager.grid[gridPos.x, gridPos.y].zone == GridSpawnManager.GridZone.PlayerZone;
    }
    
    bool IsCellOccupiedByOtherMinion(Vector2Int gridPos, int requestingMinionIndex)
    {
        return combatManager.IsCellOccupiedByOtherMinion(gridPos, requestingMinionIndex);
    }
} 
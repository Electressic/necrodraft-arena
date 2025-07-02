using UnityEngine;

public class PreviewUnitClickHandler : MonoBehaviour
{
    [Header("References")]
    public CombatManager combatManager;
    public int previewIndex;
    
    [Header("Visual Feedback")]
    public Material selectedMaterial;
    private Renderer unitRenderer;
    private Material originalMaterial;
    private bool isSelected = false;
    
    void Start()
    {
        // Ensure we have a 2D collider for mouse detection
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            
            // Size the collider to match the sprite bounds
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                collider.size = spriteRenderer.sprite.bounds.size;
            }
            else
            {
                collider.size = Vector2.one; // Default size
            }
        }
        
        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer != null)
        {
            originalMaterial = unitRenderer.material;
        }
        
        Debug.Log($"[PreviewUnitClickHandler] Initialized for preview unit {previewIndex}");
    }
    
    void OnMouseDown()
    {
        Debug.Log($"[PreviewUnitClickHandler] Mouse down on preview unit {previewIndex}. Current state: {combatManager.currentState}");
        
        if (combatManager != null && combatManager.currentState == CombatManager.CombatState.Preview)
        {
            combatManager.SelectMinionForRepositioning(previewIndex);
        }
    }
    
    void OnMouseEnter()
    {
        // Visual feedback when hovering
        if (combatManager.currentState == CombatManager.CombatState.Preview && !isSelected)
        {
            if (unitRenderer != null && selectedMaterial != null)
            {
                // Slightly brighten the unit when hovering
                Color currentColor = unitRenderer.material.color;
                unitRenderer.material.color = currentColor * 1.2f;
            }
        }
    }
    
    void OnMouseExit()
    {
        // Remove hover effect
        if (combatManager.currentState == CombatManager.CombatState.Preview && !isSelected)
        {
            if (unitRenderer != null && originalMaterial != null)
            {
                unitRenderer.material.color = originalMaterial.color;
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (unitRenderer != null)
        {
            if (selected && selectedMaterial != null)
            {
                unitRenderer.material = selectedMaterial;
            }
            else if (!selected && originalMaterial != null)
            {
                unitRenderer.material = originalMaterial;
            }
        }
        
        Debug.Log($"[PreviewUnitClickHandler] Unit {previewIndex} selection state: {selected}");
    }
    
    void OnValidate()
    {
        // Auto-assign combat manager if not set
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
        }
    }
} 
using UnityEngine;
using System.Collections.Generic;

public class MinionVisual : MonoBehaviour
{
    [Header("Visual Setup")]
    public SpriteRenderer baseSpriteRenderer;
    
    [Header("Part Layers")]
    public Transform partContainer;
    
    [Header("Layer Order")]
    public int baseLayerOrder = 0;
    public int headLayerOrder = 4;
    public int torsoLayerOrder = 1;
    public int armsLayerOrder = 2;
    public int legsLayerOrder = 3;
    
    [Header("Debug")]
    public bool enableDebugLogging = false;
    
    // Part renderers
    private Dictionary<PartData.PartType, SpriteRenderer> partRenderers = new Dictionary<PartData.PartType, SpriteRenderer>();
    private Minion currentMinion;
    
    void Awake()
    {
        // Get or create base sprite renderer
        if (baseSpriteRenderer == null)
            baseSpriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create part container if it doesn't exist
        if (partContainer == null)
        {
            GameObject container = new GameObject("PartContainer");
            container.transform.SetParent(transform, false);
            partContainer = container.transform;
        }
        
        // Initialize part renderers
        InitializePartRenderers();
    }
    
    void InitializePartRenderers()
    {
        // Create sprite renderers for each part type
        CreatePartRenderer(PartData.PartType.Head, headLayerOrder);
        CreatePartRenderer(PartData.PartType.Torso, torsoLayerOrder);
        CreatePartRenderer(PartData.PartType.Arms, armsLayerOrder);
        CreatePartRenderer(PartData.PartType.Legs, legsLayerOrder);
        
        // Set base sprite layer order
        if (baseSpriteRenderer != null)
            baseSpriteRenderer.sortingOrder = baseLayerOrder;
    }
    
    void CreatePartRenderer(PartData.PartType partType, int layerOrder)
    {
        // Create child GameObject for this part
        GameObject partObj = new GameObject($"{partType}Part");
        partObj.transform.SetParent(partContainer, false);
        
        // Add sprite renderer
        SpriteRenderer partRenderer = partObj.AddComponent<SpriteRenderer>();
        partRenderer.sortingOrder = layerOrder;
        
        // Store reference
        partRenderers[partType] = partRenderer;
        
        // Start hidden
        partRenderer.sprite = null;
        partRenderer.color = Color.white;
        
        if (enableDebugLogging)
            Debug.Log($"[MinionVisual] Created {partType} renderer with layer order {layerOrder}");
    }
    
    public void UpdateVisuals(Minion minion)
    {
        if (minion == null) return;
        
        currentMinion = minion;
        
        // Update base sprite
        UpdateBaseSprite(minion);
        
        // Update each part
        UpdatePartSprite(PartData.PartType.Head, minion.headPart);
        UpdatePartSprite(PartData.PartType.Torso, minion.torsoPart);
        UpdatePartSprite(PartData.PartType.Arms, minion.armsPart);
        UpdatePartSprite(PartData.PartType.Legs, minion.legsPart);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionVisual] Updated visuals for {minion.minionName}");
    }
    
    void UpdateBaseSprite(Minion minion)
    {
        if (baseSpriteRenderer != null && minion.baseData != null)
        {
            if (minion.baseData.baseSprite != null)
            {
                baseSpriteRenderer.sprite = minion.baseData.baseSprite;
            }
            else
            {
                // Use a default base if no base sprite
                baseSpriteRenderer.sprite = null;
                baseSpriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Transparent grey base
            }
        }
    }
    
    void UpdatePartSprite(PartData.PartType partType, PartData partData)
    {
        if (!partRenderers.ContainsKey(partType)) return;
        
        SpriteRenderer partRenderer = partRenderers[partType];
        
        if (partData != null && partData.icon != null)
        {
            // Show the part sprite
            partRenderer.sprite = partData.icon;
            partRenderer.color = GetPartColor(partData);
            
            if (enableDebugLogging)
                Debug.Log($"[MinionVisual] Equipped {partData.partName} to {partType} slot");
        }
        else
        {
            // Hide the part if not equipped
            partRenderer.sprite = null;
            
            if (enableDebugLogging)
                Debug.Log($"[MinionVisual] Removed part from {partType} slot");
        }
    }
    
    Color GetPartColor(PartData partData)
    {
        // You can customize this to show rarity, theme, or other visual effects
        Color baseColor = Color.white;
        
        // Subtle rarity tinting
        Color rarityTint = partData.GetRarityColor();
        return Color.Lerp(baseColor, rarityTint, 0.1f); // Very subtle tint
    }
    
    public void EquipPart(PartData.PartType partType, PartData partData)
    {
        if (currentMinion != null)
        {
            currentMinion.EquipPart(partData);
            UpdatePartSprite(partType, partData);
        }
    }
    
    public void UnequipPart(PartData.PartType partType)
    {
        if (currentMinion != null)
        {
            currentMinion.UnequipPart(partType);
            UpdatePartSprite(partType, null);
        }
    }
    
    // Method to refresh visuals when minion data changes
    public void RefreshVisuals()
    {
        if (currentMinion != null)
            UpdateVisuals(currentMinion);
    }
    
    // Method to apply visual effects (for combat feedback)
    public void ApplyVisualEffect(string effectName, float duration = 1f)
    {
        switch (effectName.ToLower())
        {
            case "damage":
                StartCoroutine(DamageFlashEffect());
                break;
            case "heal":
                StartCoroutine(HealGlowEffect());
                break;
            case "critical":
                StartCoroutine(CriticalFlashEffect());
                break;
        }
    }
    
    System.Collections.IEnumerator DamageFlashEffect()
    {
        Color originalColor = baseSpriteRenderer.color;
        baseSpriteRenderer.color = Color.red;
        
        foreach (var partRenderer in partRenderers.Values)
        {
            if (partRenderer.sprite != null)
                partRenderer.color = Color.red;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        baseSpriteRenderer.color = originalColor;
        foreach (var kvp in partRenderers)
        {
            if (kvp.Value.sprite != null)
                kvp.Value.color = GetPartColor(currentMinion?.GetEquippedPart(kvp.Key));
        }
    }
    
    System.Collections.IEnumerator HealGlowEffect()
    {
        Color healColor = Color.green;
        
        for (int i = 0; i < 3; i++)
        {
            baseSpriteRenderer.color = healColor;
            foreach (var partRenderer in partRenderers.Values)
            {
                if (partRenderer.sprite != null)
                    partRenderer.color = healColor;
            }
            
            yield return new WaitForSeconds(0.1f);
            
            baseSpriteRenderer.color = Color.white;
            foreach (var kvp in partRenderers)
            {
                if (kvp.Value.sprite != null)
                    kvp.Value.color = GetPartColor(currentMinion?.GetEquippedPart(kvp.Key));
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    System.Collections.IEnumerator CriticalFlashEffect()
    {
        Color critColor = Color.yellow;
        
        for (int i = 0; i < 3; i++)
        {
            baseSpriteRenderer.color = critColor;
            foreach (var partRenderer in partRenderers.Values)
            {
                if (partRenderer.sprite != null)
                    partRenderer.color = critColor;
            }
            
            yield return new WaitForSeconds(0.05f);
            
            baseSpriteRenderer.color = Color.white;
            foreach (var kvp in partRenderers)
            {
                if (kvp.Value.sprite != null)
                    kvp.Value.color = GetPartColor(currentMinion?.GetEquippedPart(kvp.Key));
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }
} 
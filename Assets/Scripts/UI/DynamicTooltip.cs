using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DynamicTooltip : MonoBehaviour
{
    [Header("Tooltip UI")]
    public RectTransform tooltipPanel;
    public TMPro.TextMeshProUGUI tooltipText;
    public Image backgroundImage;
    
    [Header("Positioning")]
    public Vector2 offset = new Vector2(15, -15);
    public float padding = 10f;
    public float minDistanceFromCursor = 20f;
    
    [Header("Tooltip Size")]
    public Vector2 tooltipSize = new Vector2(300, 150);
    public bool autoResize = true;
    
    public static DynamicTooltip Instance { get; private set; }
    
    private Canvas parentCanvas;
    private RectTransform canvasRect;
    
    private Vector2 lastMousePosition;
    private bool isTooltipActive = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        
        MakeTooltipNonInteractive();
        HideTooltip();
    }
    
    void MakeTooltipNonInteractive()
    {
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }
        
        TMPro.TextMeshProUGUI[] texts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (TMPro.TextMeshProUGUI text in texts)
        {
            text.raycastTarget = false;
        }
        
        Graphic[] graphics = GetComponentsInChildren<Graphic>();
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }
    }
    
    void Update()
    {
        if (isTooltipActive && tooltipPanel.gameObject.activeInHierarchy)
        {
            Vector2 currentMousePosition = GetMousePosition();
            
            if (Vector2.Distance(currentMousePosition, lastMousePosition) > 5f)
            {
                lastMousePosition = currentMousePosition;
                SetTooltipPosition(currentMousePosition);
            }
        }
    }
    
    public void ShowTooltip(PartData part, Vector2 worldPosition)
    {
        if (part == null) return;
        
        string tooltipContent = GenerateTooltipContent(part);
        tooltipText.text = tooltipContent;
        
        if (autoResize)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipText.rectTransform);
            
            Vector2 preferredSize = tooltipText.GetPreferredValues();
            tooltipPanel.sizeDelta = new Vector2(
                Mathf.Min(preferredSize.x + padding * 2, tooltipSize.x), 
                Mathf.Min(preferredSize.y + padding * 2, tooltipSize.y)
            );
        }
        else
        {
            tooltipPanel.sizeDelta = tooltipSize;
        }
        
        lastMousePosition = worldPosition;
        
        tooltipPanel.gameObject.SetActive(true);
        isTooltipActive = true;
        
        SetTooltipPosition(worldPosition);
        tooltipPanel.SetAsLastSibling();
    }
    
    public void ShowTooltip(PartData part)
    {
        Vector2 mousePosition = GetMousePosition();
        ShowTooltip(part, mousePosition);
    }
    
    public void HideTooltip()
    {
        isTooltipActive = false;
        tooltipPanel.gameObject.SetActive(false);
    }
    
    Vector2 GetMousePosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        else
        {
            return Vector2.zero;
        }
    }
    
    void SetTooltipPosition(Vector2 screenPosition)
    {
        if (parentCanvas == null || canvasRect == null) return;
        
        Vector2 localCursorPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, parentCanvas.worldCamera, out localCursorPosition);
        
        Vector2 tooltipSize = tooltipPanel.sizeDelta;
        
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float halfWidth = canvasWidth / 2;
        float halfHeight = canvasHeight / 2;
        
        Vector2 bestPosition = CalculateBestTooltipPosition(localCursorPosition, tooltipSize, halfWidth, halfHeight);
        
        tooltipPanel.localPosition = bestPosition;
    }
    
    Vector2 CalculateBestTooltipPosition(Vector2 cursorPos, Vector2 tooltipSize, float halfWidth, float halfHeight)
    {
        Vector2 bestPosition;
        
        bestPosition = new Vector2(cursorPos.x + minDistanceFromCursor, cursorPos.y - tooltipSize.y / 2);
        if (bestPosition.x + tooltipSize.x <= halfWidth - 5 &&
            bestPosition.y >= -halfHeight + 5 &&
            bestPosition.y + tooltipSize.y <= halfHeight - 5)
        {
            return bestPosition;
        }
        
        bestPosition = new Vector2(cursorPos.x - tooltipSize.x - minDistanceFromCursor, cursorPos.y - tooltipSize.y / 2);
        if (bestPosition.x >= -halfWidth + 5 &&
            bestPosition.y >= -halfHeight + 5 &&
            bestPosition.y + tooltipSize.y <= halfHeight - 5)
        {
            return bestPosition;
        }
        
        bestPosition = new Vector2(cursorPos.x - tooltipSize.x / 2, cursorPos.y + minDistanceFromCursor);
        if (bestPosition.y + tooltipSize.y <= halfHeight - 5 &&
            bestPosition.x >= -halfWidth + 5 &&
            bestPosition.x + tooltipSize.x <= halfWidth - 5)
        {
            return bestPosition;
        }
        
        bestPosition = new Vector2(cursorPos.x - tooltipSize.x / 2, cursorPos.y - tooltipSize.y - minDistanceFromCursor);
        if (bestPosition.y >= -halfHeight + 5 &&
            bestPosition.x >= -halfWidth + 5 &&
            bestPosition.x + tooltipSize.x <= halfWidth - 5)
        {
            return bestPosition;
        }
        
        bestPosition = new Vector2(cursorPos.x + minDistanceFromCursor, cursorPos.y - tooltipSize.y / 2);
        
        bestPosition.x = Mathf.Clamp(bestPosition.x, -halfWidth + 5, halfWidth - tooltipSize.x - 5);
        bestPosition.y = Mathf.Clamp(bestPosition.y, -halfHeight + 5, halfHeight - tooltipSize.y - 5);
        
        return bestPosition;
    }
    
    string GenerateTooltipContent(PartData part)
    {
        if (part == null) return "";
        
        Color rarityColor = part.GetRarityColor();
        string tooltipContent = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{part.partName}</color></b>";
        tooltipContent += $"\n<size=10><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()}] {part.GetThemeText()} {part.type}</color></size>";
        
        if (part.stats != null && part.stats.HasAnyStats())
        {
            List<string> statsList = new List<string>();
            
            if (part.stats.hp > 0) statsList.Add($"<color=green>+{part.stats.hp}% HP</color>");
            if (part.stats.attack > 0) statsList.Add($"<color=red>+{part.stats.attack}% Attack</color>");
            if (part.stats.defense > 0) statsList.Add($"<color=orange>+{part.stats.defense}% Defense</color>");
            if (part.stats.critChance > 0) statsList.Add($"<color=yellow>+{part.stats.critChance}% Crit Chance</color>");
            if (part.stats.critDamage > 0) statsList.Add($"<color=yellow>+{part.stats.critDamage}% Crit Damage</color>");
            if (part.stats.armorPen > 0) statsList.Add($"<color=purple>+{part.stats.armorPen}% Armor Pen</color>");
            
            string statsText = string.Join(" | ", statsList);
            
            if (statsList.Count > 0)
            {
                tooltipContent += $"\n\n<b>Stats:</b>\n{statsText}";
            }
        }
        else
        {
            tooltipContent += $"\n\n<b>Stats:</b>\n<color=green>+{part.GetHPBonus()} HP</color>\n<color=red>+{part.GetAttackBonus()} Attack</color>";
        }
        
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            string abilityDescription = part.GetAbilityDescription();
            if (!string.IsNullOrEmpty(abilityDescription))
            {
                tooltipContent += $"\n\n<b><color=darkTurquoise>Special Ability:</color></b>\n<color=cyan>{abilityDescription}</color>";
            }
        }
        
        if (!string.IsNullOrEmpty(part.description))
        {
            tooltipContent += $"\n\n<color=darkSlateGray><i>{part.description}</i></color>";
        }
        
        return tooltipContent;
    }
} 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MinionListEntryUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TMPro.TMP_InputField nicknameInput;
    public Button headSlotButton;
    public Button torsoSlotButton;
    public Button armsSlotButton;
    public Button legsSlotButton;
    
    [Header("Slot Text References")]
    public TMPro.TextMeshProUGUI headSlotText;
    public TMPro.TextMeshProUGUI torsoSlotText;
    public TMPro.TextMeshProUGUI armsSlotText;
    public TMPro.TextMeshProUGUI legsSlotText;
    
    [Header("Visual Feedback")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color validDropColor = Color.green;
    public Color invalidDropColor = Color.red;
    
    [Header("Progression UI")]
    public TMPro.TextMeshProUGUI levelText;          // NEW: Display minion level
    public UnityEngine.UI.Slider experienceBar;     // NEW: XP progress bar
    public TMPro.TextMeshProUGUI experienceText;    // NEW: XP numbers (optional)
    
    [Header("Tooltip System")]
    public GameObject tooltipPanel;  // Panel to show detailed stats on hover
    public TMPro.TextMeshProUGUI tooltipText;  // Text component for tooltip
    
    // Private fields
    private Minion representedMinion;
    private MinionAssemblyManager manager;
    private Dictionary<PartData.PartType, Button> slotButtons;
    private Dictionary<PartData.PartType, TMPro.TextMeshProUGUI> slotTexts;
    private PartData draggedPart = null;
    
    void Awake()
    {
        // Initialize dictionaries for easy access
        slotButtons = new Dictionary<PartData.PartType, Button>
        {
            { PartData.PartType.Head, headSlotButton },
            { PartData.PartType.Torso, torsoSlotButton },
            { PartData.PartType.Arms, armsSlotButton },
            { PartData.PartType.Legs, legsSlotButton }
        };
        
        slotTexts = new Dictionary<PartData.PartType, TMPro.TextMeshProUGUI>
        {
            { PartData.PartType.Head, headSlotText },
            { PartData.PartType.Torso, torsoSlotText },
            { PartData.PartType.Arms, armsSlotText },
            { PartData.PartType.Legs, legsSlotText }
        };
        
        // Get background image if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }
    
    public void Initialize(Minion minionData, MinionAssemblyManager assemblyManager)
    {
        representedMinion = minionData;
        manager = assemblyManager;
        
        // Setup nickname input
        if (nicknameInput != null)
        {
            nicknameInput.text = minionData.minionName;
            nicknameInput.onEndEdit.AddListener(OnNicknameChanged);
        }
        
        // Setup slot button listeners (for unequipping) AND hover tooltips
        foreach (var slot in slotButtons)
        {
            PartData.PartType partType = slot.Key;
            Button button = slot.Value;
            
            if (button != null)
            {
                button.onClick.AddListener(() => OnSlotClicked(partType));
                
                // Add hover event triggers for tooltips
                var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                // Add pointer enter event
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((eventData) => ShowTooltip(partType));
                trigger.triggers.Add(pointerEnter);
                
                // Add pointer exit event
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) => HideTooltip());
                trigger.triggers.Add(pointerExit);
            }
        }
        
        // Hide tooltip initially
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        
        // Initial refresh
        RefreshDisplay();
    }
    
    // Drag and Drop Implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Add null check at the start
        if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
        
        // Check if something is being dragged
        if (eventData.dragging && eventData.pointerDrag != null)
        {
            DraggablePartItem draggedItem = eventData.pointerDrag.GetComponent<DraggablePartItem>();
            if (draggedItem != null && draggedItem.partData != null)
            {
                draggedPart = draggedItem.partData;
                UpdateVisualFeedback(true);
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Add null check at the start
        if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
        
        draggedPart = null;
        UpdateVisualFeedback(false);
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Add null check at the start
        if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
        
        if (eventData.pointerDrag != null)
        {
            DraggablePartItem draggedItem = eventData.pointerDrag.GetComponent<DraggablePartItem>();
            if (draggedItem != null && draggedItem.partData != null)
            {
                TryEquipPart(draggedItem.partData);
            }
        }
        
        draggedPart = null;
        UpdateVisualFeedback(false);
    }
    
    public bool TryEquipPart(PartData part)
    {
        if (part == null || representedMinion == null || manager == null) 
            return false;
        
        // Check if we can equip this part type
        PartData currentPart = representedMinion.GetEquippedPart(part.type);
        
        // Tell the manager to handle the equipping logic
        manager.OnMinionSlotClicked(representedMinion, part.type, part);
        
        return true; // Always return true for now, let manager handle validation
    }
    
    void UpdateVisualFeedback(bool isDragOver)
    {
        if (backgroundImage == null) return;
        
        if (!isDragOver)
        {
            backgroundImage.color = normalColor;
            ClearSlotHighlights();
            return;
        }
        
        if (draggedPart == null)
        {
            backgroundImage.color = highlightColor;
            return;
        }
        
        // Check if the dragged part can be equipped
        bool canEquip = representedMinion.GetEquippedPart(draggedPart.type) != draggedPart;
        backgroundImage.color = canEquip ? validDropColor : invalidDropColor;
        
        // Highlight the target slot
        HighlightTargetSlot(draggedPart.type, canEquip);
    }
    
    void HighlightTargetSlot(PartData.PartType partType, bool isValid)
    {
        // Clear all highlights first
        ClearSlotHighlights();
        
        // Highlight the target slot
        if (slotButtons.ContainsKey(partType))
        {
            Button targetButton = slotButtons[partType];
            if (targetButton != null)
            {
                ColorBlock colors = targetButton.colors;
                colors.normalColor = isValid ? validDropColor : invalidDropColor;
                targetButton.colors = colors;
            }
        }
    }
    
    void ClearSlotHighlights()
    {
        foreach (var kvp in slotButtons)
        {
            Button button = kvp.Value;
            if (button != null)
            {
                PartData equippedPart = representedMinion.GetEquippedPart(kvp.Key);
                
                ColorBlock colors = button.colors;
                if (equippedPart != null)
                {
                    // Use rarity color for equipped parts
                    Color rarityColor = equippedPart.GetRarityColor();
                    rarityColor.a = 0.7f;
                    colors.normalColor = rarityColor;
                }
                else
                {
                    colors.normalColor = new Color32(128, 128, 128, 255); // Empty - Gray #808080
                }
                button.colors = colors;
            }
        }
    }
    
    public void RefreshDisplay()
    {
        if (representedMinion == null) return;
        
        // Update each slot display
        foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
        {
            UpdateSlotDisplay(partType);
        }
        
        // Update progression display
        UpdateProgressionDisplay();
        
        // Ensure normal background color
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }
    
    void UpdateProgressionDisplay()
    {
        if (representedMinion == null) return;
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Level {representedMinion.level}";
        }
        
        // Update experience bar
        if (experienceBar != null)
        {
            float progress = representedMinion.GetExperienceProgress();
            experienceBar.value = progress;
        }
        
        // Update experience text (optional detailed view)
        if (experienceText != null)
        {
            experienceText.text = representedMinion.GetExperienceText();
        }
    }
    
    void UpdateSlotDisplay(PartData.PartType partType)
    {
        if (!slotTexts.ContainsKey(partType)) return;
        
        TMPro.TextMeshProUGUI slotText = slotTexts[partType];
        Button slotButton = slotButtons[partType];
        if (slotText == null || slotButton == null) return;
        
        PartData equippedPart = representedMinion.GetEquippedPart(partType);
        
        if (equippedPart != null)
        {
            // Get rarity color
            Color rarityColor = equippedPart.GetRarityColor();
            
            // Show MINIMAL text - just part name and rarity
            string slotDisplayText = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{equippedPart.partName}</color></b>";
            slotDisplayText += $"\n<size=8><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{equippedPart.GetRarityText()}]</color></size>";
            
            slotText.text = slotDisplayText;
            slotText.color = Color.black;
            
            // Change button color based on rarity
            ColorBlock colors = slotButton.colors;
            Color buttonColor = rarityColor;
            buttonColor.a = 0.7f; // More visible than inventory
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonColor * 1.3f;
            slotButton.colors = colors;
        }
        else
        {
            slotText.text = $"<b>{partType}</b>\n<color=grey>Empty</color>";
            slotText.color = new Color32(128, 128, 128, 255); // Gray #808080
            
            // Change button color to indicate it's empty
            ColorBlock colors = slotButton.colors;
            colors.normalColor = new Color32(128, 128, 128, 255); // Gray #808080
            colors.highlightedColor = Color.white;
            slotButton.colors = colors;
        }
    }
    
    void OnNicknameChanged(string newName)
    {
        if (representedMinion != null && !string.IsNullOrEmpty(newName))
        {
            representedMinion.minionName = newName;
            
            if (manager != null)
            {
                manager.OnMinionDataChanged(representedMinion);
            }
        }
    }
    
    void OnSlotClicked(PartData.PartType partType)
    {
        if (manager != null)
        {
            manager.OnMinionSlotClicked(representedMinion, partType);
        }
    }
    
    void ShowTooltip(PartData.PartType partType)
    {
        if (tooltipPanel != null && tooltipText != null && representedMinion != null)
        {
            PartData equippedPart = representedMinion.GetEquippedPart(partType);
            if (equippedPart != null)
            {
                tooltipPanel.SetActive(true);
                
                // Create detailed tooltip text
                Color rarityColor = equippedPart.GetRarityColor();
                string tooltipContent = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{equippedPart.partName}</color></b>";
                tooltipContent += $"\n<size=10><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{equippedPart.GetRarityText()}]</color></size>";
                
                // Show detailed stats
                if (equippedPart.stats.HasAnyStats())
                {
                    List<string> statsList = new List<string>();
                    if (equippedPart.stats.health > 0) statsList.Add($"<color=green>+{equippedPart.stats.health*100:F0}% Health</color>");
                    if (equippedPart.stats.attack > 0) statsList.Add($"<color=red>+{equippedPart.stats.attack*100:F0}% Attack</color>");
                    if (equippedPart.stats.defense > 0) statsList.Add($"<color=orange>+{equippedPart.stats.defense*100:F0}% Defense</color>");
                    if (equippedPart.stats.attackSpeed > 0) statsList.Add($"<color=yellow>+{equippedPart.stats.attackSpeed*100:F0}% Attack Speed</color>");
                    if (equippedPart.stats.critChance > 0) statsList.Add($"<color=yellow>+{equippedPart.stats.critChance*100:F0}% Crit Chance</color>");
                    if (equippedPart.stats.moveSpeed > 0) statsList.Add($"<color=#00FFFF>+{equippedPart.stats.moveSpeed*100:F0}% Move Speed</color>");
                    
                    tooltipContent += $"\n\n<b>Stats:</b>\n{string.Join("\n", statsList)}";
                }
                else
                {
                    // Fallback to legacy system
                    tooltipContent += $"\n\n<b>Stats:</b>\n<color=green>+{equippedPart.GetHPBonus()} HP</color>\n<color=red>+{equippedPart.GetAttackBonus()} Attack</color>";
                }
                
                // Show ability if it exists
                if (equippedPart.specialAbility != PartData.SpecialAbility.None)
                {
                    tooltipContent += $"\n\n<b><color=orange>Special Ability:</color></b>\n<color=orange>{equippedPart.specialAbility}</color>";
                }
                
                tooltipText.text = tooltipContent;
            }
        }
    }
    
    void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
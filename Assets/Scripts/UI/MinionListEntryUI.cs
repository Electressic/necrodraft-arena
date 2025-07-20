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
    public TMPro.TextMeshProUGUI levelText;
    public UnityEngine.UI.Slider experienceBar;
    public TMPro.TextMeshProUGUI experienceText;
    
    [Header("Dynamic Tooltip")]
    
    private Minion representedMinion;
    private MinionAssemblyManager manager;
    private Dictionary<PartData.PartType, Button> slotButtons;
    private Dictionary<PartData.PartType, TMPro.TextMeshProUGUI> slotTexts;
    private PartData draggedPart = null;
    
    void Awake()
    {
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
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }
    
    public void Initialize(Minion minionData, MinionAssemblyManager assemblyManager)
    {
        representedMinion = minionData;
        manager = assemblyManager;
        
        if (nicknameInput != null)
        {
            nicknameInput.text = minionData.minionName;
            nicknameInput.onEndEdit.AddListener(OnNicknameChanged);
        }
        
        foreach (var slot in slotButtons)
        {
            PartData.PartType partType = slot.Key;
            Button button = slot.Value;
            
            if (button != null)
            {
                button.onClick.AddListener(() => OnSlotClicked(partType));
                
                var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((eventData) => ShowTooltip(partType));
                trigger.triggers.Add(pointerEnter);
                
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) => HideTooltip());
                trigger.triggers.Add(pointerExit);
            }
        }
        
        RefreshDisplay();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
        
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
        if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
        
        draggedPart = null;
        UpdateVisualFeedback(false);
    }
    
    public void OnDrop(PointerEventData eventData)
    {
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
        
        PartData currentPart = representedMinion.GetEquippedPart(part.type);
        
        manager.OnMinionSlotClicked(representedMinion, part.type, part);
        
        return true;
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
        
        bool canEquip = representedMinion.GetEquippedPart(draggedPart.type) != draggedPart;
        backgroundImage.color = canEquip ? validDropColor : invalidDropColor;
        
        HighlightTargetSlot(draggedPart.type, canEquip);
    }
    
    void HighlightTargetSlot(PartData.PartType partType, bool isValid)
    {
        ClearSlotHighlights();
        
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
                    Color rarityColor = equippedPart.GetRarityColor();
                    rarityColor.a = 0.7f;
                    colors.normalColor = rarityColor;
                }
                else
                {
                    colors.normalColor = new Color32(128, 128, 128, 255);
                }
                button.colors = colors;
            }
        }
    }
    
    public void RefreshDisplay()
    {
        if (representedMinion == null) return;

        foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
        {
            UpdateSlotDisplay(partType);
        }
        
        UpdateProgressionDisplay();
        
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }
    
    void UpdateProgressionDisplay()
    {
        if (representedMinion == null) return;
        
        if (levelText != null)
        {
            levelText.text = $"Level {representedMinion.level}";
        }
        
        if (experienceBar != null)
        {
            float progress = representedMinion.GetExperienceProgress();
            experienceBar.value = progress;
        }
        
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
            Color rarityColor = equippedPart.GetRarityColor();
            
            string slotDisplayText = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{equippedPart.partName}</color></b>";
            slotDisplayText += $"\n<size=8><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{equippedPart.GetRarityText()}]</color></size>";
            
            slotText.text = slotDisplayText;
            slotText.color = Color.black;
            
            ColorBlock colors = slotButton.colors;
            Color buttonColor = rarityColor;
            buttonColor.a = 0.7f;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonColor * 1.3f;
            slotButton.colors = colors;
        }
        else
        {
            slotText.text = $"<b>{partType}</b>\n<color=grey>Empty</color>";
            slotText.color = new Color32(128, 128, 128, 255);
            
            ColorBlock colors = slotButton.colors;
            colors.normalColor = new Color32(128, 128, 128, 255);
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
        if (representedMinion != null)
        {
            PartData equippedPart = representedMinion.GetEquippedPart(partType);
            if (equippedPart != null)
            {
                if (DynamicTooltip.Instance != null)
                {
                    DynamicTooltip.Instance.ShowTooltip(equippedPart);
                }
                else
                {
                    Debug.LogWarning("[MinionListEntryUI] Dynamic tooltip instance not found! Make sure DynamicTooltip is in the scene and active.");
                }
            }
        }
    }
    
    void HideTooltip()
    {
        if (DynamicTooltip.Instance != null)
        {
            DynamicTooltip.Instance.HideTooltip();
        }
    }
}
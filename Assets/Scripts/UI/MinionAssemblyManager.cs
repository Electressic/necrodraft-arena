using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinionAssemblyManager : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI minionStatsText;
    public Transform inventoryContentPanel;  // The grid container for inventory
    public Transform partSlotsPanel;         // The grid container for equipped parts
    public Button continueButton;
    public Button backButton;
    
    [Header("Minion Data")]
    public MinionData defaultMinionData;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Private fields
    private Minion currentMinion;
    private List<GameObject> inventoryItems = new List<GameObject>();
    private List<GameObject> equippedSlotItems = new List<GameObject>();
    
    void Start()
    {
        InitializeMinion();
        SetupUI();
        RefreshInventoryDisplay();
        RefreshEquippedPartsDisplay();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Assembly scene initialized");
    }
    
    void InitializeMinion()
    {
        if (defaultMinionData != null)
        {
            currentMinion = new Minion(defaultMinionData);
        }
        else
        {
            Debug.LogError("[MinionAssemblyManager] No default minion data assigned!");
        }
    }
    
    void SetupUI()
    {
        // Set up navigation buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToGameplay);
        if (backButton != null)
            backButton.onClick.AddListener(BackToCardSelection);
        
        // Update title
        if (titleText != null)
            titleText.text = "Assemble Your Minion";
    }
    
    void RefreshInventoryDisplay()
    {
        // Clear existing inventory items
        ClearInventoryDisplay();
        
        // Get all available parts from inventory
        List<PartData> availableParts = PlayerInventory.GetAllParts();
        
        // Filter out parts that are currently equipped
        List<PartData> unequippedParts = new List<PartData>();
        foreach (PartData part in availableParts)
        {
            if (currentMinion.GetEquippedPart(part.type) != part)
            {
                unequippedParts.Add(part);
            }
        }
        
        if (unequippedParts.Count == 0)
        {
            CreateInventoryEmptyMessage();
            return;
        }
        
        // Create grid items for each unequipped part
        foreach (PartData part in unequippedParts)
        {
            CreateInventoryGridItem(part);
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Refreshed inventory display with {unequippedParts.Count} unequipped parts");
    }
    
    void RefreshEquippedPartsDisplay()
    {
        // Clear existing equipped part displays
        ClearEquippedPartsDisplay();
        
        // Create slot items for each part type
        CreateEquippedPartSlot(PartData.PartType.Head);
        CreateEquippedPartSlot(PartData.PartType.Torso);
        CreateEquippedPartSlot(PartData.PartType.Arms);
        CreateEquippedPartSlot(PartData.PartType.Legs);
    }
    
    void CreateInventoryEmptyMessage()
    {
        GameObject messageItem = CreateGridItem("No parts available\nCollect parts from card selection!", Color.yellow);
        
        // Disable button for message
        Button button = messageItem.GetComponent<Button>();
        if (button != null)
            button.interactable = false;
            
        messageItem.transform.SetParent(inventoryContentPanel, false);
        inventoryItems.Add(messageItem);
    }
    
    void CreateInventoryGridItem(PartData part)
    {
        string partText = $"<b>{part.partName}</b>\n({part.type})\n+{part.hpBonus} HP\n+{part.attackBonus} ATK";
        GameObject item = CreateGridItem(partText, Color.white);
        
        // Add single-click functionality (simple approach)
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => EquipPartFromInventory(part));
        }
        
        item.transform.SetParent(inventoryContentPanel, false);
        inventoryItems.Add(item);
    }
    
    void CreateEquippedPartSlot(PartData.PartType partType)
    {
        PartData equippedPart = currentMinion.GetEquippedPart(partType);
        
        string slotText;
        Color textColor;
        
        if (equippedPart != null)
        {
            slotText = $"<b>{equippedPart.partName}</b>\n({partType})\n+{equippedPart.hpBonus} HP\n+{equippedPart.attackBonus} ATK";
            textColor = Color.white;
        }
        else
        {
            slotText = $"{partType}\nSlot\n\n(Empty)";
            textColor = Color.gray;
        }
        
        GameObject slotItem = CreateGridItem(slotText, textColor);
        
        // Add click to unequip functionality
        if (equippedPart != null)
        {
            Button button = slotItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => UnequipPartToInventory(equippedPart));
            }
        }
        else
        {
            // Disable button for empty slots
            Button button = slotItem.GetComponent<Button>();
            if (button != null)
                button.interactable = false;
        }
        
        slotItem.transform.SetParent(partSlotsPanel, false);
        equippedSlotItems.Add(slotItem);
    }
    
    GameObject CreateGridItem(string text, Color textColor)
    {
        // Create the item container
        GameObject item = new GameObject("GridItem");
        
        // Add button component
        Button button = item.AddComponent<Button>();
        
        // Add image component for background
        Image itemImage = item.AddComponent<Image>();
        itemImage.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        
        // Add text child
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(item.transform);
        textChild.transform.localScale = Vector3.one;
        
        TMPro.TextMeshProUGUI textComponent = textChild.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 10;
        textComponent.color = textColor;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TMPro.TextWrappingModes.Normal; // FIXED: Updated property
        
        // Set up text positioning to fill the button
        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);  // Small padding
        textRect.offsetMax = new Vector2(-5, -5);
        
        return item;
    }
    
    void EquipPartFromInventory(PartData part)
    {
        if (part == null || currentMinion == null) return;
        
        // Check if slot is occupied and warn player
        PartData currentPart = currentMinion.GetEquippedPart(part.type);
        if (currentPart != null)
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Replacing {currentPart.partName} with {part.partName} in {part.type} slot");
            
            // The old part stays in PlayerInventory (it was collected, so player keeps it)
        }
        
        // Equip the new part
        currentMinion.EquipPart(part);
        
        // Refresh displays
        RefreshInventoryDisplay();
        RefreshEquippedPartsDisplay();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Equipped {part.partName} to {part.type} slot");
    }
    
    void UnequipPartToInventory(PartData part)
    {
        if (part == null || currentMinion == null) return;
        
        // Unequip the part
        currentMinion.UnequipPart(part.type);
        
        // Part remains in PlayerInventory, just refresh displays
        RefreshInventoryDisplay();
        RefreshEquippedPartsDisplay();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Unequipped {part.partName} from {part.type} slot");
    }
    
    void ClearInventoryDisplay()
    {
        foreach (GameObject item in inventoryItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        inventoryItems.Clear();
    }
    
    void ClearEquippedPartsDisplay()
    {
        foreach (GameObject item in equippedSlotItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        equippedSlotItems.Clear();
    }
    
    void UpdateMinionDisplay()
    {
        if (currentMinion == null) return;
        
        // Update stats display
        if (minionStatsText != null)
        {
            string statsText = $"<size=24><b>{currentMinion.minionName}</b></size>\n\n" +
                              $"<color=#00ff00>HP:</color> {currentMinion.totalHP} <color=#888888>(Base: {currentMinion.baseData.baseHP})</color>\n" +
                              $"<color=#ff5555>ATK:</color> {currentMinion.totalAttack} <color=#888888>(Base: {currentMinion.baseData.baseAttack})</color>\n\n" +
                              $"<color=#ffff00>Parts Equipped:</color> {currentMinion.GetEquippedPartsCount()}/4";
            
            minionStatsText.text = statsText;
        }
    }
    
    public void ContinueToGameplay()
    {
        // Store the assembled minion for gameplay
        MinionManager.SetCurrentMinion(currentMinion);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Proceeding to gameplay with minion: {currentMinion.totalHP} HP, {currentMinion.totalAttack} ATK");
        
        SceneManager.LoadScene("Gameplay");
    }
    
    public void BackToCardSelection()
    {
        SceneManager.LoadScene("CardSelection");
    }
}

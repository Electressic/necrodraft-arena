using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinionAssemblyManager : MonoBehaviour
{    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI titleText;
    // Remove this line: public TMPro.TextMeshProUGUI minionStatsText;
    // Remove this line: public Transform partSlotsPanel;         
    public Transform inventoryContentPanel;  // The grid container for inventory (right side)
    public Button continueButton;
    public Button backButton;
    public Button cardSelectionButton;  // NEW: Button to open card selection overlay
    public TMPro.TextMeshProUGUI selectedPartText; // Drag a Text element here
    
    [Header("Card Selection Overlay")]
    public CardSelectionOverlay cardSelectionOverlay;

    [Header("Minion List UI")]  // NEW SECTION
    public Transform minionListContent;      // The content area for minion entries (left side)
    public GameObject minionEntryPrefab;     // Prefab for individual minion entries

    [Header("Minion Creation UI")]
    public Transform minionSelectorPanel;    // Keep for backwards compatibility, can remove later
    public Button createMinionButton;        
    public TMPro.TMP_InputField minionNameInput; // Input field for naming minions
    
    [Header("Minion Data")]
    public MinionData defaultMinionData;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Private fields
    private List<Minion> minionRoster = new List<Minion>();
    private Minion currentMinion;
    private int selectedMinionIndex = -1;
    private List<GameObject> inventoryItems = new List<GameObject>();
    private List<GameObject> minionSelectorItems = new List<GameObject>();
    private PartData selectedPartForEquipping = null;
    private GameObject selectedPartUI = null;
    
    void Start()
    {
        InitializeFromMinionManager();
        SetupUI();
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
        
        // Initialize card selection button state based on wave progression
        UpdateCardSelectionButtonState();
        
        // Subscribe to minion progression events
        MinionManager.OnMinionUnlocked += OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp += OnMinionLeveledUp;
        MinionManager.OnRosterChanged += UpdateMinionDisplay;
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Assembly scene initialized");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        MinionManager.OnMinionUnlocked -= OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp -= OnMinionLeveledUp;
        MinionManager.OnRosterChanged -= UpdateMinionDisplay;
    }
    
    // Called when a new minion slot is unlocked
    void OnMinionUnlocked(int newMaxMinions)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] New minion slot unlocked! Now have {newMaxMinions} max minions for wave {GameData.GetCurrentWave()}");
        
        // Show notification to player (for now just debug, but this could trigger a popup)
        string notificationMessage = $"MINION UNLOCKED! You can now field {newMaxMinions} minions.";
        Debug.Log($"[MinionAssemblyManager] NOTIFICATION: {notificationMessage}");
        
        // Refresh UI to show the new minion creation button state
        RefreshMinionSelector();
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
    }
    
    // Called when a minion levels up
    void OnMinionLeveledUp(Minion minion, int newLevel)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] 🎉 {minion.minionName} reached level {newLevel}!");
        
        // Show level-up notification (for now just enhanced debug)
        string notificationMessage = $"🎉 {minion.minionName} LEVEL UP! Now level {newLevel}";
        Debug.Log($"[MinionAssemblyManager] LEVEL UP: {notificationMessage}");
        
        // Refresh UI to show updated level and stats
        RefreshMinionListDisplay();
        RefreshInventoryDisplay();
    }
    
    void InitializeFromMinionManager()
    {
        // Get roster from MinionManager
        minionRoster = MinionManager.GetMinionRoster();
        selectedMinionIndex = MinionManager.GetSelectedMinionIndex();
        
        // If no minions exist, create the starting minion from selected class
        if (minionRoster.Count == 0)
        {
            CreateStartingMinion();
        }
        
        // Set current minion
        if (selectedMinionIndex >= 0 && selectedMinionIndex < minionRoster.Count)
        {
            currentMinion = minionRoster[selectedMinionIndex];
        }
        else if (minionRoster.Count > 0)
        {
            selectedMinionIndex = 0;
            currentMinion = minionRoster[0];
            MinionManager.SetSelectedMinionIndex(0);
        }
    }

    void CreateStartingMinion()
    {
        NecromancerClass selectedClass = GameData.GetSelectedClass();
        
        if (selectedClass != null && selectedClass.startingMinionType != null)
        {
            // Create minion from class starting type
            string startingName = $"{selectedClass.className} Minion";
            Minion newMinion = new Minion(selectedClass.startingMinionType);
            newMinion.minionName = startingName; // Set name after creation
            
            // Auto-equip the class starting parts
            AutoEquipStartingParts(newMinion, selectedClass);
            
            // Add to roster
            MinionManager.AddMinion(newMinion);
            minionRoster = MinionManager.GetMinionRoster(); // Refresh local copy
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created starting minion: {startingName}");
        }
        else
        {
            // Fallback to default minion if no class selected
            CreateNewMinion("Minion 1");
        }
    }
    
    void AutoEquipStartingParts(Minion minion, NecromancerClass necroClass)
    {
        if (necroClass.startingParts == null) return;
        
        int partsEquipped = 0;
        
        // Try to equip each starting part to the appropriate slot
        foreach (PartData part in necroClass.startingParts)
        {
            if (part != null)
            {
                // Check if this part slot is already filled
                PartData currentPart = minion.GetEquippedPart(part.type);
                if (currentPart == null)
                {
                    // Equip the part
                    minion.EquipPart(part);
                    partsEquipped++;
                    
                    if (enableDebugLogging)
                        Debug.Log($"[MinionAssemblyManager] Auto-equipped {part.partName} to {minion.minionName}");
                }
            }
        }
        
        // Force stats recalculation after auto-equipping
        minion.CalculateStats();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Auto-equipped {partsEquipped} starting parts to {minion.minionName}. Final stats: {minion.totalHP} HP, {minion.totalAttack} ATK");
    }

    void SetupUI()
    {
        // Set up navigation buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToGameplay);
        if (backButton != null)
            backButton.onClick.AddListener(BackToClassSelection);
        if (cardSelectionButton != null)
            cardSelectionButton.onClick.AddListener(ShowCardSelection);
        
        // Set up create minion button
        if (createMinionButton != null)
            createMinionButton.onClick.AddListener(OnCreateMinionClicked);
        
        // Update title
        if (titleText != null)
            titleText.text = "Assemble Your Minions";
            
        // Set up card selection overlay events
        SetupCardSelectionOverlay();
        
        // Refresh minion selector
        RefreshMinionSelector();
    }      void RefreshInventoryDisplay()
    {
        // Clear existing inventory items
        ClearInventoryDisplay();
        
        // Get all available parts from inventory
        List<PartData> availableParts = PlayerInventory.GetAllParts();
        
        // Count how many of each part type we have and how many are equipped
        Dictionary<PartData, int> partCounts = new Dictionary<PartData, int>();
        Dictionary<PartData, int> equippedCounts = new Dictionary<PartData, int>();
        
        // Count total parts
        foreach (PartData part in availableParts)
        {
            if (!partCounts.ContainsKey(part))
                partCounts[part] = 0;
            partCounts[part]++;
        }
        
        // Count equipped parts across all minions
        foreach (Minion minion in minionRoster)
        {
            foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
            {
                PartData equippedPart = minion.GetEquippedPart(partType);
                if (equippedPart != null)
                {
                    if (!equippedCounts.ContainsKey(equippedPart))
                        equippedCounts[equippedPart] = 0;
                    equippedCounts[equippedPart]++;
                }
            }
        }
        
        // Show ALL parts (whether equipped or not) with proper counts
        bool hasAnyParts = false;
        foreach (var kvp in partCounts)
        {
            PartData part = kvp.Key;
            int totalCount = kvp.Value;
            int equippedCount = equippedCounts.ContainsKey(part) ? equippedCounts[part] : 0;
            
            // Always create an inventory item, even if all are equipped
            CreateInventoryGridItem(part, totalCount, equippedCount);
            hasAnyParts = true;
        }
        
        if (!hasAnyParts)
        {
            CreateInventoryEmptyMessage();
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Refreshed inventory display");
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
    void CreateInventoryGridItem(PartData part, int totalCount, int equippedCount)
    {
        int unequippedCount = totalCount - equippedCount;
        
        // Get rarity color for part name
        Color rarityColor = part.GetRarityColor();
        
        // Create simplified, readable part text
        string partText = $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}><b>{part.partName}</b></color>";
        if (unequippedCount > 1)
        {
            partText += $" x{unequippedCount}";
        }
        if (equippedCount > 0)
        {
            partText += $" ({equippedCount} equipped)";
        }
        
        partText += $"\n<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()}]</color>";
        partText += $"\n<color=green>+{part.GetHPBonus()} HP</color>  <color=red>+{part.GetAttackBonus()} ATK</color>";
        
        // Add ability name only (not full description) if it exists
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            partText += $"\n<color=orange><b>{part.specialAbility}</b></color>";
        }
        
        // Choose appropriate text color based on availability
        Color textColor = Color.white;
        if (unequippedCount == 0)
        {
            textColor = Color.gray; // All equipped, none available
            partText += "\n<color=red>[All Equipped]</color>";
        }
        else if (equippedCount > 0)
        {
            textColor = new Color(1f, 1f, 0.8f); // Some equipped, some available
        }
        
        GameObject item = CreateGridItem(partText, textColor);
        
        // Add rarity-colored background
        Image itemImage = item.GetComponent<Image>();
        if (itemImage != null)
        {
            Color bgColor = rarityColor;
            bgColor.a = 0.15f; // Very subtle background
            itemImage.color = bgColor;
        }
        
        // Only add DraggablePartItem if we have unequipped copies
        if (unequippedCount > 0)
        {
            DraggablePartItem dragComponent = item.AddComponent<DraggablePartItem>();
            dragComponent.Initialize(part, this);
            
            // Keep the button click as fallback
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipPartFromInventory(part));
            }
        }
        else
        {
            // Disable the button if no parts available
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
        
        item.transform.SetParent(inventoryContentPanel, false);
        inventoryItems.Add(item);
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
        textComponent.fontSize = 12;
        textComponent.color = textColor;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TMPro.TextWrappingModes.Normal;
        
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
        
        // Check if we have this part available in inventory (unequipped)
        int availableCount = GetAvailablePartCount(part); // Use our helper method
        int equippedCount = GetTotalEquippedCount(part);
        int unequippedCount = availableCount - equippedCount;
        
        if (unequippedCount <= 0)
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] No unequipped {part.partName} available in inventory!");
            return;
        }
        
        // Check if this slot is occupied and handle accordingly
        PartData currentPart = currentMinion.GetEquippedPart(part.type);
        if (currentPart != null)
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Replacing {currentPart.partName} with {part.partName} in {part.type} slot");
            
            // Unequip current part first (it goes back to available inventory)
            currentMinion.UnequipPart(part.type);
        }
        
        // Equip the new part
        currentMinion.EquipPart(part);
        
        // Refresh displays (removed RefreshEquippedPartsDisplay)
        RefreshInventoryDisplay();
        RefreshMinionSelector();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Equipped {part.partName} to {currentMinion.minionName}'s {part.type} slot");
    }
    
    // Add helper method to count equipped parts across all minions
    int GetTotalEquippedCount(PartData part)
    {
        int count = 0;
        foreach (Minion minion in minionRoster)
        {
            foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
            {
                PartData equippedPart = minion.GetEquippedPart(partType);
                if (equippedPart == part)
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    void UnequipPartToInventory(PartData part)
    {
        if (part == null || currentMinion == null) return;
        
        // Unequip the part
        currentMinion.UnequipPart(part.type);
        
        // Force stats recalculation
        currentMinion.CalculateStats();
        
        // Part remains in PlayerInventory, just refresh displays
        RefreshInventoryDisplay();
        RefreshMinionSelector();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Unequipped {part.partName} from {part.type} slot. New stats: {currentMinion.totalHP} HP, {currentMinion.totalAttack} ATK");
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
    
    void UpdateMinionDisplay()
    {
        if (currentMinion == null) return;
        
        // Since minionStatsText is removed, we don't need this method for now
        // The individual minion entries will show their own stats
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Updated display for {currentMinion.minionName}");
    }
      public void ContinueToGameplay()
    {
        // Validate that we have at least one minion
        if (minionRoster.Count == 0)
        {
            Debug.LogWarning("[MinionAssemblyManager] Cannot proceed - no minions created!");
            return;
        }
        
        // For the prototype, we'll use all created minions in gameplay
        // The MinionManager already has the full roster
        
        if (enableDebugLogging)
        {
            Debug.Log($"[MinionAssemblyManager] Proceeding to gameplay with {minionRoster.Count} minions:");
            foreach (Minion minion in minionRoster)
            {
                Debug.Log($"  - {minion.minionName}: {minion.totalHP} HP, {minion.totalAttack} ATK, {minion.GetEquippedPartsCount()}/4 parts");
            }
        }
        
        // Mark that we've completed the first wave setup if this is the initial run
        if (GameData.IsFirstWave())
        {
            // Don't complete the first wave yet - that happens after combat
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] First wave minion assembly complete. Proceeding to Wave 1 combat.");
        }
        
        SceneManager.LoadScene("Gameplay");
    }
      public void BackToClassSelection()
    {
        // Save minion selection before leaving
        MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Returning to class selection");
        
        // Return to class selection scene
        SceneManager.LoadScene("ClassSelection");
    }
    
    public void ShowCardSelection()
    {
        if (cardSelectionOverlay == null)
        {
            Debug.LogWarning("[MinionAssemblyManager] Card selection overlay not assigned!");
            return;
        }
        
        // Check if card selection is available (only after first wave)
        if (GameData.IsFirstWave())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Card selection not available - complete first wave first");
            return;
        }
        
        // Check if already selected a part this session
        if (cardSelectionOverlay.HasSelectedPartThisSession())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Card selection blocked - already selected a part this session");
            return;
        }
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Showing card selection overlay");
        
        cardSelectionOverlay.ShowOverlay("Choose a Body Part", "Select one card to add to your collection");
        
        // Update button state
        UpdateCardSelectionButtonState();
    }
    
    void SetupCardSelectionOverlay()
    {
        if (cardSelectionOverlay != null)
        {
            // Subscribe to overlay events
            cardSelectionOverlay.OnPartSelected += OnPartSelectedFromOverlay;
            cardSelectionOverlay.OnOverlayClosed += OnCardSelectionClosed;
            cardSelectionOverlay.OnOverlayHidden += OnCardSelectionHidden;
            cardSelectionOverlay.OnOverlayResumed += OnCardSelectionResumed;
            
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Card selection overlay events set up");
        }
    }
    
    void OnPartSelectedFromOverlay(PartData selectedPart)
    {
        if (selectedPart == null) return;
        
        // Add the selected part to inventory
        PlayerInventory.AddPart(selectedPart);
        
        // Refresh the inventory display
        RefreshInventoryDisplay();
        
        // Update button state to greyed out
        UpdateCardSelectionButtonState();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Added {selectedPart.partName} to inventory from overlay");
    }
    
    void OnCardSelectionClosed()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Card selection overlay closed");
    }
    
    void OnCardSelectionHidden()
    {
        // Update button text to show "Resume Card Selection"
        UpdateCardSelectionButtonState();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Card selection overlay temporarily hidden");
    }
    
    void OnCardSelectionResumed()
    {
        // Update button text back to normal
        UpdateCardSelectionButtonState();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Card selection overlay resumed");
    }
    
        void UpdateCardSelectionButtonState()
    {
        if (cardSelectionButton == null) return;
        
        bool isFirstWave = GameData.IsFirstWave();
        bool hasSelected = cardSelectionOverlay != null && cardSelectionOverlay.HasSelectedPartThisSession();
        
        // Button availability logic:
        // - Disabled if it's the first wave (no card selection until after first wave)
        // - Disabled if already selected a part this session
        bool buttonEnabled = !isFirstWave && !hasSelected;
        cardSelectionButton.interactable = buttonEnabled;
        
        // Update button text and color
        TMPro.TextMeshProUGUI buttonText = cardSelectionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (isFirstWave)
            {
                buttonText.text = "Complete Wave 1 First";
                buttonText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Dark grey
            }
            else if (hasSelected)
            {
                buttonText.text = "Part Selected";
                buttonText.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Light grey
            }
            else
            {
                buttonText.text = "Get New Parts";
                buttonText.color = Color.white; // Normal color
            }
        }
    }
     
     // Public method to reset card selection for new wave/session
     public void ResetCardSelectionForNewWave()
     {
         if (cardSelectionOverlay != null)
         {
             cardSelectionOverlay.ResetSessionState();
             UpdateCardSelectionButtonState();
             
             if (enableDebugLogging)
                 Debug.Log("[MinionAssemblyManager] Card selection reset for new wave");
         }
     }
      
      // Minion Management Methods
    void CreateNewMinion(string minionName = "")
    {
        if (!MinionManager.CanAddMoreMinions())
        {
            Debug.LogWarning($"[MinionAssemblyManager] Cannot create more minions! Max: {MinionManager.GetMaxMinions()}");
            return;
        }
        
        if (defaultMinionData == null)
        {
            Debug.LogError("[MinionAssemblyManager] No default minion data assigned!");
            return;
        }
        
        // Generate name if none provided
        if (string.IsNullOrEmpty(minionName))
        {
            minionName = $"Minion {MinionManager.GetMinionCount() + 1}";
        }
        
        // Create new minion
        Minion newMinion = new Minion(defaultMinionData);
        newMinion.minionName = minionName;
        
        // Add to manager
        if (MinionManager.AddMinion(newMinion))
        {
            // Update local roster and select new minion
            minionRoster = MinionManager.GetMinionRoster();
            selectedMinionIndex = minionRoster.Count - 1;
            currentMinion = newMinion;
            MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
            
            // Refresh UI
            RefreshMinionSelector();
            RefreshInventoryDisplay();
            UpdateMinionDisplay();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created new minion: {minionName}");
        }
    }
    
    void SelectMinion(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            selectedMinionIndex = index;
            currentMinion = minionRoster[index];
            MinionManager.SetSelectedMinionIndex(index);
            
            // Refresh displays for the new minion
            RefreshInventoryDisplay();
            UpdateMinionDisplay();
            RefreshMinionSelector(); // Update button states
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Selected minion: {currentMinion.minionName}");
        }
    }
      void OnCreateMinionClicked()
    {
        string minionName = "";
        if (minionNameInput != null && !string.IsNullOrEmpty(minionNameInput.text))
        {
            minionName = minionNameInput.text;
            minionNameInput.text = ""; // Clear input after use
        }
        
        CreateNewMinion(minionName);
    }
    
    void RefreshMinionSelector()
    {
        // If using new system, refresh minion list display
        if (minionListContent != null && minionEntryPrefab != null)
        {
            RefreshMinionListDisplay();
        }
        else
        {
            // Keep old system as fallback
            ClearMinionSelector();
            for (int i = 0; i < minionRoster.Count; i++)
            {
                CreateMinionSelectorButton(i);
            }
        }
        
        // Update create button state
        if (createMinionButton != null)
        {
            createMinionButton.interactable = MinionManager.CanAddMoreMinions();
            TMPro.TextMeshProUGUI buttonText = createMinionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Create Minion ({MinionManager.GetMinionCount()}/{MinionManager.GetMaxMinions()})";
            }
        }
    }
    
    // Add these missing methods for backwards compatibility
    void ClearMinionSelector()
    {
        foreach (GameObject item in minionSelectorItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        minionSelectorItems.Clear();
    }

    void CreateMinionSelectorButton(int index)
    {
        if (index < 0 || index >= minionRoster.Count) return;
        
        Minion minion = minionRoster[index];
        string buttonText = $"{minion.minionName}\nHP: {minion.totalHP} | ATK: {minion.totalAttack}\nParts: {minion.GetEquippedPartsCount()}/4";
        
        GameObject buttonItem = CreateGridItem(buttonText, index == selectedMinionIndex ? Color.green : Color.white);
        
        Button button = buttonItem.GetComponent<Button>();
        if (button != null)
        {
            int buttonIndex = index; // Capture for closure
            button.onClick.AddListener(() => SelectMinion(buttonIndex));
        }
        
        buttonItem.transform.SetParent(minionSelectorPanel, false);
        minionSelectorItems.Add(buttonItem);
    }

    // Add the missing callback methods for MinionListEntryUI
    public void OnMinionDataChanged(Minion minion)
    {
        // Called when minion name changes
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Minion data changed: {minion.minionName}");
    }

    public void OnMinionSlotClicked(Minion minion, PartData.PartType partType)
    {
        // Set this minion as current for inventory purposes
        SetCurrentMinion(minion);
        
        PartData equippedPart = minion.GetEquippedPart(partType);
        
        if (equippedPart != null)
        {
            // Unequip if something is equipped
            UnequipPartToInventory(equippedPart);
        }
        else if (selectedPartForEquipping != null)
        {
            // Equip the selected part if slot is empty and we have a part selected
            if (selectedPartForEquipping.type == partType)
            {
                EquipPartFromInventory(selectedPartForEquipping);
                DeselectPart(); // Clear selection after equipping
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Cannot equip {selectedPartForEquipping.type} part to {partType} slot");
            }
        }
        else
        {
            // No part selected, just log the slot click
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Empty {partType} slot clicked for {minion.minionName}. Select a part from inventory first.");
        }
    }

    // Add overloaded method for drag operations
    public void OnMinionSlotClicked(Minion minion, PartData.PartType partType, PartData specificPart = null)
    {
        // Set this minion as current for inventory purposes
        SetCurrentMinion(minion);
        
        PartData equippedPart = minion.GetEquippedPart(partType);
        
        if (specificPart != null)
        {
            // This is a drag operation with a specific part
            if (specificPart.type != partType)
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Cannot equip {specificPart.type} part to {partType} slot");
                return;
            }
            
            // Check if we have unequipped copies available
            int availableCount = GetAvailablePartCount(specificPart); // Use our helper method instead
            int equippedCount = GetTotalEquippedCount(specificPart);
            int unequippedCount = availableCount - equippedCount;
            
            if (unequippedCount <= 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] No unequipped {specificPart.partName} available!");
                return;
            }
            
            // Equip the dragged part
            EquipPartFromInventory(specificPart);
        }
        else if (equippedPart != null)
        {
            // This is a click operation to unequip
            UnequipPartToInventory(equippedPart);
        }
        else
        {
            // Empty slot clicked without dragging
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Empty {partType} slot clicked for {minion.minionName}. Drag a part here to equip.");
        }
    }

    void SetCurrentMinion(Minion minion)
    {
        for (int i = 0; i < minionRoster.Count; i++)
        {
            if (minionRoster[i] == minion)
            {
                currentMinion = minion;
                selectedMinionIndex = i;
                MinionManager.SetSelectedMinionIndex(i);
                
                // Refresh inventory to show correct availability
                RefreshInventoryDisplay();
                break;
            }
        }
    }

    void RefreshMinionListDisplay()
    {
        // Clear existing minion entries
        ClearMinionListDisplay();
        
        // Create an entry for each minion
        for (int i = 0; i < minionRoster.Count; i++)
        {
            CreateMinionListEntry(minionRoster[i]);
        }
    }
    
    void CreateMinionListEntry(Minion minion)
    {
        if (minionEntryPrefab == null || minionListContent == null) return;
        
        GameObject entryInstance = Instantiate(minionEntryPrefab, minionListContent);
        MinionListEntryUI entryUI = entryInstance.GetComponent<MinionListEntryUI>();
        
        if (entryUI != null)
        {
            entryUI.Initialize(minion, this);
            minionSelectorItems.Add(entryInstance); // Reuse this list for cleanup
        }
    }
    
    void ClearMinionListDisplay()
    {
        foreach (GameObject item in minionSelectorItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        minionSelectorItems.Clear();
    }

    // Update the OnInventoryItemClicked method
    void OnInventoryItemClicked(PartData part)
    {
        if (part == null) return;
        
        // If clicking the same part, deselect it
        if (selectedPartForEquipping == part)
        {
            DeselectPart();
            return;
        }
        
        // Select this part for equipping
        SelectPartForEquipping(part);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Selected {part.partName} for equipping");
    }

    void SelectPartForEquipping(PartData part)
    {
        // Clear previous selection
        DeselectPart();
        
        // Set new selection
        selectedPartForEquipping = part;
        
        // Update UI feedback
        if (selectedPartText != null)
        {
            selectedPartText.text = $"Selected: {part.partName}\nClick on an empty {part.type} slot to equip";
            selectedPartText.gameObject.SetActive(true);
        }
        
        UpdateInventoryHighlights();
    }

    void DeselectPart()
    {
        selectedPartForEquipping = null;
        selectedPartUI = null;
        
        // Clear UI feedback
        if (selectedPartText != null)
        {
            selectedPartText.gameObject.SetActive(false);
        }
        
        UpdateInventoryHighlights();
    }

    void UpdateInventoryHighlights()
    {
        // Update inventory item visuals to show selection
        foreach (GameObject inventoryItem in inventoryItems)
        {
            Image itemImage = inventoryItem.GetComponent<Image>();
            if (itemImage != null)
            {
                // Reset to default color or highlight if selected
                itemImage.color = (inventoryItem == selectedPartUI) ? Color.yellow : Color.white;
            }
        }
    }

    // Add this helper method if PlayerInventory doesn't have the right GetPartCount method
    int GetAvailablePartCount(PartData part)
    {
        List<PartData> allParts = PlayerInventory.GetAllParts();
        int count = 0;
        foreach (PartData inventoryPart in allParts)
        {
            if (inventoryPart == part || inventoryPart.partName == part.partName)
            {
                count++;
            }
        }
        return count;
    }
}

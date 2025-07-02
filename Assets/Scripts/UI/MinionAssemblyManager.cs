using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinionAssemblyManager : MonoBehaviour
{    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI minionCountText;        // "Your Minions (1/5)" text
    // REMOVED: public Transform minionCardsContainer;   // DEPRECATED: Old card system - use list system instead
    public Button addMinionButton;                       // + button to add new minions
    public Transform inventoryIconsContainer;            // Grid container for part icons (bottom)
    public Button continueButton;
    public Button backButton;
    public Button cardSelectionButton;
    
    [Header("Card Selection Overlay")]
    public CardSelectionOverlay cardSelectionOverlay;

    [Header("Minion List UI")]  // NEW SECTION
    public Transform minionListContent;      // The content area for minion entries (left side)
    public GameObject minionEntryPrefab;     // Prefab for individual minion entries

    [Header("Minion Creation UI")]
    public Button createMinionButton;        
    
    [Header("Prefabs")]
    // REMOVED: public GameObject minionCardPrefab;     // DEPRECATED: Old card system - use minionEntryPrefab instead
    public GameObject partIconPrefab;                   // Simple part icon prefab for inventory

    [Header("Minion Data")]
    public MinionData defaultMinionData;
    
    [Header("Bone Weaver Bonus Parts")]
    public PartData vampiricClaws;
    public PartData vampiricFangs;
    public PartData berserkerGreaves;
    public PartData berserkerSpine;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    [Header("Part Type Sprites")]
    public Sprite headSprite;
    public Sprite torsoSprite;
    public Sprite armsSprite;
    public Sprite legsSprite;
    
    [Header("Inventory Tooltip System")]
    public GameObject inventoryTooltipPanel;  // Panel to show detailed stats on hover
    public TMPro.TextMeshProUGUI inventoryTooltipText;  // Text component for tooltip
    
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
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Starting assembly manager with new scroll layout");
        
        // Initialize from MinionManager - this creates starting minion if needed
        InitializeFromMinionManager();
        
        // Load minion roster from MinionManager
        minionRoster = MinionManager.GetMinionRoster();
        
        // Set up UI
        if (addMinionButton != null)
        {
            addMinionButton.onClick.AddListener(OnAddMinionClicked);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToGameplay);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
        
        if (cardSelectionButton != null)
        {
            cardSelectionButton.onClick.AddListener(ShowCardSelection);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Card selection button click event connected");
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogError("[MinionAssemblyManager] ✗ Card selection button is NULL! Check inspector assignment.");
        }
        
        // Setup card selection overlay events
        SetupCardSelectionOverlay();
        
        // Hide inventory tooltip initially
        if (inventoryTooltipPanel != null)
            inventoryTooltipPanel.SetActive(false);
        
        // Refresh displays
        RefreshInventoryDisplay();
        // RefreshMinionCardDisplay(); // DEPRECATED: Use list system instead
        UpdateMinionCountDisplay();
        UpdateMinionDisplay();
        
        // Initialize card selection button state based on wave progression
        UpdateCardSelectionButtonState();
        
        // Subscribe to minion progression events
        MinionManager.OnMinionUnlocked += OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp += OnMinionLeveledUp;
        MinionManager.OnRosterChanged += UpdateMinionDisplay;
        
        // Subscribe to inventory events for UI updates
        PlayerInventory.OnPartAdded += OnPartAddedToInventory;
        PlayerInventory.OnInventoryCleared += OnInventoryCleared;
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Assembly scene initialized");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        MinionManager.OnMinionUnlocked -= OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp -= OnMinionLeveledUp;
        MinionManager.OnRosterChanged -= UpdateMinionDisplay;
        
        // Unsubscribe from inventory events
        PlayerInventory.OnPartAdded -= OnPartAddedToInventory;
        PlayerInventory.OnInventoryCleared -= OnInventoryCleared;
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
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] InitializeFromMinionManager called");
        
        // Get roster from MinionManager
        minionRoster = MinionManager.GetMinionRoster();
        selectedMinionIndex = MinionManager.GetSelectedMinionIndex();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Found {minionRoster.Count} existing minions in MinionManager");
        
        // If no minions exist, create the starting minion from selected class
        if (minionRoster.Count == 0)
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] No minions found, creating starting minion");
            CreateStartingMinion();
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Using existing minions: {string.Join(", ", minionRoster.ConvertAll(m => m.minionName))}");
        }
        
        // Set current minion
        if (selectedMinionIndex >= 0 && selectedMinionIndex < minionRoster.Count)
        {
            currentMinion = minionRoster[selectedMinionIndex];
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Selected existing minion at index {selectedMinionIndex}: {currentMinion.minionName}");
        }
        else if (minionRoster.Count > 0)
        {
            selectedMinionIndex = 0;
            currentMinion = minionRoster[0];
            MinionManager.SetSelectedMinionIndex(0);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Defaulted to first minion: {currentMinion.minionName}");
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
            
            // SPECIAL: Check for Bone Weaver to give bonus starter set
            if (selectedClass.className == "Bone Weaver")
            {
                AutoEquipBoneWeaverBonusParts(newMinion);
            }
            else
            {
                // Auto-equip the class starting parts
                AutoEquipStartingParts(newMinion, selectedClass);
            }
            
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
    
    void AutoEquipBoneWeaverBonusParts(Minion minion)
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Equipping special Bone Weaver starter set.");

        PartData[] bonusParts = { vampiricFangs, berserkerSpine, vampiricClaws, berserkerGreaves };
        foreach (PartData part in bonusParts)
        {
            if (part != null)
            {
                if (minion.GetEquippedPart(part.type) == null)
                {
                    minion.EquipPart(part);
                    if (enableDebugLogging)
                        Debug.Log($"[MinionAssemblyManager] Bone Weaver equipped bonus part: {part.partName}");
                }
            }
            else
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"[MinionAssemblyManager] A Bone Weaver bonus part is not assigned in the Inspector!");
            }
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

    // This method is now obsolete and replaced by UI setup in Start()
    /*
    void SetupUI()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToGameplay);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToClassSelection);
        }
        if (createMinionButton != null)
        {
            createMinionButton.onClick.AddListener(OnCreateMinionClicked);
        }

        if (titleText != null)
        {
            titleText.text = "Assemble Your Minions";
        }
    }
    */
    
    void RefreshInventoryDisplay()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] RefreshInventoryDisplay called");
        
        // Clear existing inventory items
        ClearInventoryDisplay();
        
        // Get all available parts
        List<PartData> availableParts = PlayerInventory.GetAllParts();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Found {availableParts.Count} parts in inventory");
        
        if (availableParts.Count == 0)
        {
            CreateInventoryEmptyMessage();
            return;
        }
        
        // Group parts by type and count
        Dictionary<PartData, int> partCounts = new Dictionary<PartData, int>();
        foreach (PartData part in availableParts)
        {
            if (partCounts.ContainsKey(part))
                partCounts[part]++;
            else
                partCounts[part] = 1;
        }
        
        // Create icon-based inventory items
        foreach (var kvp in partCounts)
        {
            PartData part = kvp.Key;
            int totalCount = kvp.Value;
            int equippedCount = GetTotalEquippedCount(part);
            
            CreateInventoryIconItem(part, totalCount, equippedCount);
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Refreshed inventory display with {partCounts.Count} unique parts as icons");
    }
    
    void CreateInventoryEmptyMessage()
    {
        GameObject messageItem = CreateGridItem("No parts available\nCollect parts from card selection!", Color.yellow);
        
        // Disable button for message
        Button button = messageItem.GetComponent<Button>();
        if (button != null)
            button.interactable = false;
            
        messageItem.transform.SetParent(inventoryIconsContainer, false);
        inventoryItems.Add(messageItem);
    }
    void CreateInventoryIconItem(PartData part, int totalCount, int equippedCount)
    {
        if (inventoryIconsContainer == null)
        {
            Debug.LogError("[MinionAssemblyManager] inventoryIconsContainer is null! Check UI references in inspector.");
            return;
        }
        
        int unequippedCount = totalCount - equippedCount;
        
        // Create simple icon-based item using prefab or simple setup
        GameObject iconItem = CreatePartIcon(part, unequippedCount, equippedCount);
        
        // Only add draggable functionality if we have unequipped copies
        if (unequippedCount > 0)
        {
            DraggablePartItem dragComponent = iconItem.AddComponent<DraggablePartItem>();
            dragComponent.Initialize(part, this);
            
            // Add click listener as fallback
            Button button = iconItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipPartFromInventory(part));
            }
        }
        else
        {
            // Disable interaction if no parts available
            Button button = iconItem.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
            
            // Gray out the icon
            Image iconImage = iconItem.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = new Color32(128, 128, 128, 255); // Gray #808080
            }
        }
        
        iconItem.transform.SetParent(inventoryIconsContainer, false);
        inventoryItems.Add(iconItem);
    }
    
    GameObject CreatePartIcon(PartData part, int unequippedCount, int equippedCount)
    {
        GameObject iconItem = new GameObject($"PartIcon_{part.partName}", typeof(RectTransform));
        
        // Add button component
        Button button = iconItem.AddComponent<Button>();
        
        // Add main icon image
        Image iconImage = iconItem.AddComponent<Image>();
        
        // Use part type sprite first, then part.icon, then fallback to color
        Sprite partSprite = GetPartTypeSprite(part.type);
        if (partSprite != null)
        {
            iconImage.sprite = partSprite;
            iconImage.color = Color.white; // Keep full color for sprite
        }
        else if (part.icon != null)
        {
            iconImage.sprite = part.icon;
            iconImage.color = Color.white; // Keep full color for sprite
        }
        else
        {
            // Use rarity-colored background if no icon (last resort)
            iconImage.color = new Color32(128, 128, 128, 255); // Gray #808080
        }
        
        // Set up RectTransform
        RectTransform iconRect = iconItem.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(64, 64); // Square icon size
        
        // Add hover tooltip functionality
        var trigger = iconItem.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = iconItem.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        // Add pointer enter event
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => ShowInventoryTooltip(part));
        trigger.triggers.Add(pointerEnter);
        
        // Add pointer exit event  
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => HideInventoryTooltip());
        trigger.triggers.Add(pointerExit);
        
        // Set up click functionality
        button.onClick.AddListener(() => OnInventoryItemClicked(part));
        
        // Add count text if multiple copies
        if (unequippedCount + equippedCount > 1)
        {
            GameObject countTextObj = new GameObject("CountText", typeof(RectTransform));
            countTextObj.transform.SetParent(iconItem.transform, false);
            
            // Create separate background object for the count
            GameObject countBgObj = new GameObject("CountBackground", typeof(RectTransform));
            countBgObj.transform.SetParent(countTextObj.transform, false);
            countBgObj.transform.SetAsFirstSibling(); // Behind text
            
            // Add background circle for count on separate object
            Image countBg = countBgObj.AddComponent<Image>();
            countBg.color = new Color(0, 0, 0, 0.7f);
            
            // Set background size and position
            RectTransform countBgRect = countBgObj.GetComponent<RectTransform>();
            countBgRect.anchorMin = Vector2.zero;
            countBgRect.anchorMax = Vector2.one;
            countBgRect.offsetMin = Vector2.zero;
            countBgRect.offsetMax = Vector2.zero;
            
            TMPro.TextMeshProUGUI countText = countTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            countText.text = unequippedCount.ToString();
            countText.fontSize = 12;
            countText.color = Color.white;
            countText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Position in bottom-right corner
            RectTransform countRect = countTextObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.7f, 0f);
            countRect.anchorMax = new Vector2(1f, 0.3f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
        }
        
        return iconItem;
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
        // Use ONLY the list system - remove card system to eliminate duplicates
        RefreshMinionListDisplay();  // Keep this - uses minionEntryPrefab
        // RefreshMinionCardDisplay();  // REMOVE this - causes duplicates
        UpdateMinionCountDisplay();
    }

    void UpdateMinionCountDisplay()
    {
        if (minionCountText != null)
        {
            int currentCount = minionRoster.Count;
            int maxMinions = MinionManager.GetMaxMinions();
            minionCountText.text = $"Your Minions ({currentCount}/{maxMinions})";
        }
        
        // Show/hide add button based on available slots
        if (addMinionButton != null)
        {
            int maxMinions = MinionManager.GetMaxMinions();
            addMinionButton.gameObject.SetActive(minionRoster.Count < maxMinions);
        }
    }

    // DEPRECATED CARD SYSTEM - Commented out to eliminate duplicates
    // Use RefreshMinionListDisplay() instead
    /*
    void RefreshMinionCardDisplay()
    {
        // Clear existing minion cards
        ClearMinionCardDisplay();
        
        // Create cards for each minion
        for (int i = 0; i < minionRoster.Count; i++)
        {
            CreateMinionCard(minionRoster[i], i);
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Refreshed minion card display with {minionRoster.Count} minions");
    }

    void ClearMinionCardDisplay()
    {
        if (minionCardsContainer == null) return;
        
        foreach (Transform child in minionCardsContainer)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    void CreateMinionCard(Minion minion, int index)
    {
        if (minionCardsContainer == null)
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] minionCardsContainer is null!");
            return;
        }
        
        // Create basic card structure (you'll replace this with your Aseprite-created prefab later)
        GameObject minionCard = CreateBasicMinionCard(minion, index);
        
        minionCard.transform.SetParent(minionCardsContainer, false);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Created minion card for {minion.minionName}");
    }
    */

    GameObject CreateBasicMinionCard(Minion minion, int index)
    {
        // Create UI GameObject with RectTransform (instead of regular GameObject)
        GameObject card = new GameObject($"MinionCard_{minion.minionName}", typeof(RectTransform));
        
        // Add card background
        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.8f, 0.7f, 0.6f, 0.9f); // Parchment-like color
        
        // Set card size (RectTransform is automatically added with typeof(RectTransform))
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(120, 180); // Tall card format
        
        // Add name field at top
        GameObject nameField = new GameObject("NameField", typeof(RectTransform));
        nameField.transform.SetParent(card.transform, false);
        
        // Add Image component first for UI input field background
        Image nameFieldBg = nameField.AddComponent<Image>();
        nameFieldBg.color = new Color(1f, 1f, 1f, 0.8f);
        
        TMPro.TMP_InputField nameInput = nameField.AddComponent<TMPro.TMP_InputField>();
        nameInput.text = minion.minionName;
        nameInput.onEndEdit.AddListener((newName) => OnMinionNameChanged(minion, newName));
        
        RectTransform nameRect = nameField.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.1f, 0.8f);
        nameRect.anchorMax = new Vector2(0.85f, 0.95f); // Leave space for remove button
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Add remove button at top-right
        GameObject removeBtn = new GameObject("RemoveButton", typeof(RectTransform));
        removeBtn.transform.SetParent(card.transform, false);
        
        Button removeButton = removeBtn.AddComponent<Button>();
        removeButton.onClick.AddListener(() => RemoveMinionCard(index));
        
        Image removeBtnImg = removeBtn.AddComponent<Image>();
        removeBtnImg.color = Color.red;
        
        RectTransform removeRect = removeBtn.GetComponent<RectTransform>();
        removeRect.anchorMin = new Vector2(0.85f, 0.85f);
        removeRect.anchorMax = new Vector2(0.95f, 0.95f);
        removeRect.offsetMin = Vector2.zero;
        removeRect.offsetMax = Vector2.zero;
        
        // Add text to remove button
        GameObject removeText = new GameObject("RemoveText", typeof(RectTransform));
        removeText.transform.SetParent(removeBtn.transform, false);
        TMPro.TextMeshProUGUI removeTextComp = removeText.AddComponent<TMPro.TextMeshProUGUI>();
        removeTextComp.text = "×";
        removeTextComp.color = Color.white;
        removeTextComp.fontSize = 16;
        removeTextComp.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform removeTextRect = removeText.GetComponent<RectTransform>();
        removeTextRect.anchorMin = Vector2.zero;
        removeTextRect.anchorMax = Vector2.one;
        removeTextRect.offsetMin = Vector2.zero;
        removeTextRect.offsetMax = Vector2.zero;
        
        // Add 2x2 part slots grid
        CreatePartSlotsGrid(card, minion);
        
        // Add stats display at bottom
        CreateStatsDisplay(card, minion);
        
        return card;
    }

    void CreatePartSlotsGrid(GameObject card, Minion minion)
    {
        GameObject slotsContainer = new GameObject("PartSlots", typeof(RectTransform));
        slotsContainer.transform.SetParent(card.transform, false);
        
        RectTransform slotsRect = slotsContainer.GetComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0.1f, 0.3f);
        slotsRect.anchorMax = new Vector2(0.9f, 0.7f);
        slotsRect.offsetMin = Vector2.zero;
        slotsRect.offsetMax = Vector2.zero;
        
        // Add Grid Layout Group for 2x2 layout
        GridLayoutGroup grid = slotsContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(40, 40);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        
        // Create slots for each part type
        PartData.PartType[] partTypes = { PartData.PartType.Head, PartData.PartType.Torso, 
                                         PartData.PartType.Arms, PartData.PartType.Legs };
        
        foreach (PartData.PartType partType in partTypes)
        {
            CreatePartSlot(slotsContainer, minion, partType);
        }
    }

    void CreatePartSlot(GameObject parent, Minion minion, PartData.PartType partType)
    {
        GameObject slot = new GameObject($"{partType}Slot", typeof(RectTransform));
        slot.transform.SetParent(parent.transform, false);
        
        Button slotButton = slot.AddComponent<Button>();
        slotButton.onClick.AddListener(() => OnMinionSlotClicked(minion, partType));
        
        Image slotImg = slot.AddComponent<Image>();
        
        PartData equippedPart = minion.GetEquippedPart(partType);
        if (equippedPart != null)
        {
            if (equippedPart.icon != null)
            {
                slotImg.sprite = equippedPart.icon;
            }
            else
            {
                slotImg.color = equippedPart.GetRarityColor();
            }
        }
        else
        {
            slotImg.color = new Color32(128, 128, 128, 255); // Empty slot - Gray #808080
        }
    }

    void CreateStatsDisplay(GameObject card, Minion minion)
    {
        GameObject statsContainer = new GameObject("StatsDisplay", typeof(RectTransform));
        statsContainer.transform.SetParent(card.transform, false);
        
        RectTransform statsRect = statsContainer.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.05f);
        statsRect.anchorMax = new Vector2(0.9f, 0.25f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI statsText = statsContainer.AddComponent<TMPro.TextMeshProUGUI>();
        statsText.fontSize = 8;
        statsText.color = Color.black;
        statsText.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Calculate and display total stats
        string statsDisplay = $"HP: {minion.totalHP}\n";
        statsDisplay += $"ATK: {minion.totalAttack}\n";
        statsDisplay += $"Level: {minion.level}";
        
        statsText.text = statsDisplay;
    }

    void OnMinionNameChanged(Minion minion, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            minion.minionName = newName;
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Renamed minion to {newName}");
        }
    }

    void RemoveMinionCard(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            string minionName = minionRoster[index].minionName;
            MinionManager.RemoveMinion(index);
            minionRoster = MinionManager.GetMinionRoster(); // Refresh local copy
            
            // RefreshMinionCardDisplay(); // DEPRECATED: Use list system instead
            UpdateMinionCountDisplay();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Removed minion: {minionName}");
        }
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
    
    public void BackToMainMenu()
    {
        // Save minion selection before leaving
        MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Returning to main menu");
        
        // Return to main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowCardSelection()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] 🎯 ShowCardSelection() called - Button was clicked!");
        
        if (cardSelectionOverlay == null)
        {
            Debug.LogError("[MinionAssemblyManager] ✗ Card selection overlay not assigned!");
            return;
        }
        
        // Check if card selection is available (only after first wave)
        if (GameData.IsFirstWave())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✗ Card selection not available - complete first wave first");
            return;
        }
        
        // Check if already selected a part this session
        if (cardSelectionOverlay.HasSelectedPartThisSession())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✗ Card selection blocked - already selected a part this session");
            return;
        }
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] ✓ Showing card selection overlay");
        
        cardSelectionOverlay.ShowOverlay("Choose a Body Part", "Select one card to add to your collection");
        
        // Update button state
        UpdateCardSelectionButtonState();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] ✓ Card selection overlay should now be visible");
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
                Debug.Log("[MinionAssemblyManager] ✓ Card selection overlay events connected successfully");
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogError("[MinionAssemblyManager] ✗ CardSelectionOverlay reference is NULL! Check inspector assignments.");
        }
    }
    
    void OnPartSelectedFromOverlay(PartData selectedPart)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] ✓ OnPartSelectedFromOverlay called with: {(selectedPart != null ? selectedPart.partName : "NULL")}");
        
        if (selectedPart == null) 
        {
            if (enableDebugLogging)
                Debug.LogError("[MinionAssemblyManager] ✗ Selected part is NULL!");
            return;
        }
        
        // Add the selected part to inventory
        PlayerInventory.AddPart(selectedPart);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] ✓ Added {selectedPart.partName} to PlayerInventory. Total parts: {PlayerInventory.GetPartCount()}");
        
        // Refresh the inventory display
        RefreshInventoryDisplay();
        
        // Update button state to greyed out
        UpdateCardSelectionButtonState();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] ✓ Inventory refresh completed after adding {selectedPart.partName}");
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
        int currentWave = GameData.GetCurrentWave();
        
        // Debug logging to understand current state
        if (enableDebugLogging)
        {
            Debug.Log($"[MinionAssemblyManager] Button state check - Wave: {currentWave}, IsFirstWave: {isFirstWave}, HasSelected: {hasSelected}");
        }
        
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
                buttonText.text = $"Complete Wave {currentWave} First";
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
         
         // Auto-show card selection after completing a wave (except first wave)
         if (!GameData.IsFirstWave() && GameData.GetCurrentWave() > 1)
         {
             StartCoroutine(DelayedAutoShowCardSelection());
         }
     }
     
     // Coroutine to show card selection automatically after a short delay
     System.Collections.IEnumerator DelayedAutoShowCardSelection()
     {
         yield return new WaitForSeconds(0.5f); // Brief delay for scene transition
         
         if (enableDebugLogging)
             Debug.Log("[MinionAssemblyManager] Auto-showing card selection after wave completion");
             
         ShowCardSelection();
     }
     
     // Debug method to manually complete wave 1 for testing
     [ContextMenu("Debug: Complete Current Wave")]
     public void DebugCompleteWave()
     {
         if (enableDebugLogging)
             Debug.Log($"[MinionAssemblyManager] DEBUG: Manually completing wave {GameData.GetCurrentWave()}");
             
         GameData.CompleteWave();
         UpdateCardSelectionButtonState();
         
         if (enableDebugLogging)
             Debug.Log($"[MinionAssemblyManager] DEBUG: Wave completed, now on wave {GameData.GetCurrentWave()}");
     }
     
     // Debug method to force show card selection for testing
     [ContextMenu("Debug: Force Show Card Selection")]
     public void DebugForceShowCardSelection()
     {
         if (enableDebugLogging)
             Debug.Log($"[MinionAssemblyManager] DEBUG: Force showing card selection (bypassing wave check)");
             
         // Reset session state to allow new selection
         if (cardSelectionOverlay != null)
         {
             cardSelectionOverlay.ResetSessionState();
         }
         
         ShowCardSelection();
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
        // Create new minion with auto-generated name
        CreateNewMinion("");
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

    // Legacy method - disabled since minionSelectorPanel was removed
    void CreateMinionSelectorButton(int index)
    {
        // This method is part of the old UI system and is no longer used
        // The new system uses RefreshMinionListDisplay() instead
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] CreateMinionSelectorButton called but this method is deprecated");
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
        if (minionEntryPrefab == null)
        {
            Debug.LogError("[MinionAssemblyManager] minionEntryPrefab is null! Check UI references in inspector.");
            return;
        }
        
        if (minionListContent == null)
        {
            Debug.LogError("[MinionAssemblyManager] minionListContent is null! Check UI references in inspector.");
            return;
        }
        
        GameObject entryInstance = Instantiate(minionEntryPrefab, minionListContent);
        MinionListEntryUI entryUI = entryInstance.GetComponent<MinionListEntryUI>();
        
        if (entryUI != null)
        {
            entryUI.Initialize(minion, this);
            minionSelectorItems.Add(entryInstance); // Reuse this list for cleanup
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created minion list entry for: {minion.minionName}");
        }
        else
        {
            Debug.LogError("[MinionAssemblyManager] minionEntryPrefab doesn't have MinionListEntryUI component!");
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
        selectedPartForEquipping = part;
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Selected {part.partName} for equipping.");
    }

    void DeselectPart()
    {
        selectedPartForEquipping = null;
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

    // Add this helper method to the class
    Sprite GetCardSpriteForRarity(PartData.PartRarity rarity)
    {
        // Reference the same sprites used in CardSelectionOverlay
        if (cardSelectionOverlay != null)
        {
            switch (rarity)
            {
                case PartData.PartRarity.Common: return cardSelectionOverlay.commonCardSprite;
                case PartData.PartRarity.Rare: return cardSelectionOverlay.rareCardSprite;
                case PartData.PartRarity.Epic: return cardSelectionOverlay.epicCardSprite;
                default: return cardSelectionOverlay.commonCardSprite;
            }
        }
        return null;
    }

    // Keep this method for backward compatibility with other parts of the code
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

    public void OnAddMinionClicked()
    {
        if (MinionManager.CanAddMoreMinions())
        {
            // Create new basic minion with required data
            if (defaultMinionData == null)
            {
                if (enableDebugLogging)
                    Debug.LogError("[MinionAssemblyManager] Cannot create minion - defaultMinionData is null!");
                return;
            }
            
            Minion newMinion = new Minion(defaultMinionData);
            newMinion.minionName = $"Minion {minionRoster.Count + 1}";
            
            bool success = MinionManager.AddMinion(newMinion);
            if (success)
            {
                minionRoster = MinionManager.GetMinionRoster(); // Refresh local copy
                UpdateMinionDisplay(); // Use centralized update method
                
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Added new minion: {newMinion.minionName}");
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[MinionAssemblyManager] Cannot add more minions! Max: {MinionManager.GetMaxMinions()}");
        }
    }

    // Subscribe to inventory events for UI updates
    void OnPartAddedToInventory(PartData part)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[MinionAssemblyManager] Part added to inventory: {part.partName}");
            Debug.Log($"[MinionAssemblyManager] Total parts in inventory: {PlayerInventory.GetPartCount()}");
            Debug.Log($"[MinionAssemblyManager] Inventory container reference: {(inventoryIconsContainer != null ? "✓ Valid" : "✗ NULL")}");
        }
        
        // Force a complete refresh with delay to ensure proper timing
        StartCoroutine(DelayedInventoryRefresh());
    }
    
    // Add coroutine for delayed refresh to handle timing issues
    System.Collections.IEnumerator DelayedInventoryRefresh()
    {
        yield return new UnityEngine.WaitForEndOfFrame();
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Performing delayed inventory refresh");
        RefreshInventoryDisplay();
    }

    void OnInventoryCleared()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Inventory cleared");
        RefreshInventoryDisplay();
    }

    // Public method for testing inventory system
    [ContextMenu("Test Inventory Refresh")]
    public void TestInventoryRefresh()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[MinionAssemblyManager] Manual inventory refresh test triggered");
            Debug.Log($"[MinionAssemblyManager] PlayerInventory part count: {PlayerInventory.GetPartCount()}");
            Debug.Log($"[MinionAssemblyManager] Inventory container: {(inventoryIconsContainer != null ? inventoryIconsContainer.name : "NULL")}");
        }
        RefreshInventoryDisplay();
    }

    // Helper method to get the correct sprite for a part type
    Sprite GetPartTypeSprite(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return headSprite;
            case PartData.PartType.Torso:
                return torsoSprite;
            case PartData.PartType.Arms:
                return armsSprite;
            case PartData.PartType.Legs:
                return legsSprite;
            default:
                return null;
        }
    }

    // Inventory tooltip methods
    void ShowInventoryTooltip(PartData part)
    {
        if (inventoryTooltipPanel != null && inventoryTooltipText != null && part != null)
        {
            inventoryTooltipPanel.SetActive(true);
            
            // Create detailed tooltip text
            Color rarityColor = part.GetRarityColor();
            string tooltipContent = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{part.partName}</color></b>";
            tooltipContent += $"\n<size=10><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()}]</color></size>";
            
            // Show detailed stats
            if (part.stats.HasAnyStats())
            {
                List<string> statsList = new List<string>();
                if (part.stats.health > 0) statsList.Add($"<color=green>+{part.stats.health*100:F0}% Health</color>");
                if (part.stats.attack > 0) statsList.Add($"<color=red>+{part.stats.attack*100:F0}% Attack</color>");
                if (part.stats.defense > 0) statsList.Add($"<color=orange>+{part.stats.defense*100:F0}% Defense</color>");
                if (part.stats.attackSpeed > 0) statsList.Add($"<color=yellow>+{part.stats.attackSpeed*100:F0}% Attack Speed</color>");
                if (part.stats.critChance > 0) statsList.Add($"<color=yellow>+{part.stats.critChance*100:F0}% Crit Chance</color>");
                if (part.stats.moveSpeed > 0) statsList.Add($"<color=#00FFFF>+{part.stats.moveSpeed*100:F0}% Move Speed</color>");
                
                tooltipContent += $"\n\n<b>Stats:</b>\n{string.Join("\n", statsList)}";
            }
            else
            {
                // Fallback to legacy system
                tooltipContent += $"\n\n<b>Stats:</b>\n<color=green>+{part.GetHPBonus()} HP</color>\n<color=red>+{part.GetAttackBonus()} Attack</color>";
            }
            
            // Show ability if it exists
            if (part.specialAbility != PartData.SpecialAbility.None)
            {
                tooltipContent += $"\n\n<b><color=orange>Special Ability:</color></b>\n<color=orange>{part.specialAbility}</color>";
            }
            
            // Show availability info
            int totalCount = GetAvailablePartCount(part);
            int equippedCount = GetTotalEquippedCount(part);
            int availableCount = totalCount - equippedCount;
            
            tooltipContent += $"\n\n<b>Availability:</b>";
            tooltipContent += $"\n<color=white>Total: {totalCount}</color>";
            tooltipContent += $"\n<color=white>Equipped: {equippedCount}</color>";
            tooltipContent += $"\n<color={(availableCount > 0 ? "green" : "red")}>Available: {availableCount}</color>";
            
            inventoryTooltipText.text = tooltipContent;
        }
    }
    
    void HideInventoryTooltip()
    {
        if (inventoryTooltipPanel != null)
        {
            inventoryTooltipPanel.SetActive(false);
        }
    }
}

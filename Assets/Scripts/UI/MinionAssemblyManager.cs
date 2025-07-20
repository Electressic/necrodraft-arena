using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MinionAssemblyManager : MonoBehaviour
{    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI minionCountText;
    public Button addMinionButton;
    public Transform inventoryIconsContainer;
    public Button continueButton;
    public Button backButton;
    
    [Header("Minion Navigation UI")]
    public Button previousMinionButton;
    public Button nextMinionButton;
    public Button removeMinionButton;
    public Transform minionDisplayArea;

    [Header("Minion List UI")]
    public GameObject minionEntryPrefab;

    [Header("Part Management UI")]
    public Button deleteSelectedPartsButton;
    public Button equipSelectedPartButton;
    public TMPro.TextMeshProUGUI selectedPartInfoText;
    
    [Header("Prefabs")]
    public GameObject partIconPrefab;

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
    
    [Header("Rarity Background Frames")]
    public Sprite commonFrame;
    public Sprite uncommonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    
    [Header("Dynamic Tooltip")]
    public DynamicTooltip dynamicTooltip;
    
    [Header("Merge System UI")]
    public Button mergeModeToggleButton;
    public GameObject mergePreviewPopup;
    public Button confirmMergeButton;
    public Button cancelMergeButton;
    public TMPro.TextMeshProUGUI mergePreviewText;
    public Image mergePreviewImage;
    public Image inventoryOutline;
    
    [Header("Wave Preview")]
    public WavePreviewManager wavePreviewManager;  // Reference to wave preview system
    
    // Private fields
    private List<Minion> minionRoster = new List<Minion>();
    private Minion currentMinion;
    private int selectedMinionIndex = -1;
    private List<GameObject> inventoryItems = new List<GameObject>();
    private List<GameObject> minionSelectorItems = new List<GameObject>();
    
    private HashSet<PartData> selectedParts = new HashSet<PartData>();
    private Dictionary<PartData, GameObject> partToUI = new Dictionary<PartData, GameObject>();
    
    private bool isMergeMode = false;
    private HashSet<PartData> mergeSelectedParts = new HashSet<PartData>();
    private PartData.PartType? mergeSlotType = null;
    private PartData.PartRarity? mergeRarity = null;
    
    // Part visual state colors
    private static readonly Color EQUIPPED_COLOR = new Color(0.6f, 0.6f, 0.6f, 0.8f);
    private static readonly Color SELECTED_COLOR = new Color(0.8f, 0.8f, 0.2f, 0.8f);
    private static readonly Color NORMAL_COLOR = Color.white;
    
    void Start()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Starting assembly manager with new scroll layout");
        
        // Initialize from MinionManager - this creates starting minion if needed
        InitializeFromMinionManager();
        
        // Auto-show card selection if we just completed combat
        if (GameData.JustCompletedCombat())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Just completed combat - will auto-show card selection");
            StartCoroutine(DelayedAutoShowCardSelectionOnStart());
        }
        
        // Load minion roster from MinionManager
        SyncMinionRosterFromManager();
        
        // Set up UI
        if (addMinionButton != null)
        {
            addMinionButton.onClick.AddListener(OnAddMinionClicked);
        }
        
        // Set up navigation buttons
        if (previousMinionButton != null)
        {
            previousMinionButton.onClick.AddListener(OnPreviousMinionClicked);
        }
        
        if (nextMinionButton != null)
        {
            nextMinionButton.onClick.AddListener(OnNextMinionClicked);
        }
        
        if (removeMinionButton != null)
        {
            removeMinionButton.onClick.AddListener(OnRemoveMinionClicked);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToGameplay);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
        
        // NEW: Setup part management buttons
        if (deleteSelectedPartsButton != null)
        {
            deleteSelectedPartsButton.onClick.AddListener(DeleteSelectedParts);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Delete selected parts button connected");
        }
        
        if (equipSelectedPartButton != null)
        {
            equipSelectedPartButton.onClick.AddListener(EquipSelectedPart);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Equip selected part button connected");
        }
        
        // NEW: Setup merge system buttons
        if (mergeModeToggleButton != null)
        {
            mergeModeToggleButton.onClick.AddListener(ToggleMergeMode);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Merge mode toggle button connected");
        }
        
        if (confirmMergeButton != null)
        {
            confirmMergeButton.onClick.AddListener(ConfirmMerge);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Confirm merge button connected");
        }
        
        if (cancelMergeButton != null)
        {
            cancelMergeButton.onClick.AddListener(CancelMerge);
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✓ Cancel merge button connected");
        }
        
        
        // Find the dynamic tooltip system - prefer singleton, fallback to search
        if (dynamicTooltip == null)
        {
            if (DynamicTooltip.Instance != null)
            {
                dynamicTooltip = DynamicTooltip.Instance;
            }
            else
            {
                dynamicTooltip = FindAnyObjectByType<DynamicTooltip>();
            }
        }
        
        ClearMinionListDisplay();
        
        RefreshInventoryDisplay();
        UpdateMinionCountDisplay();
        UpdateMinionDisplay();
        
        
        MinionManager.OnMinionUnlocked += OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp += OnMinionLeveledUp;
        MinionManager.OnRosterChanged += OnRosterChanged;
        
        PlayerInventory.OnPartAdded += OnPartAddedToInventory;
        PlayerInventory.OnInventoryCleared += OnInventoryCleared;
        
        SaveSystem.OnGameLoaded += OnGameLoaded;
        
        InitializeWavePreview();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Assembly scene initialized");
    }
    
    void OnDestroy()
    {
        MinionManager.OnMinionUnlocked -= OnMinionUnlocked;
        MinionManager.OnMinionLeveledUp -= OnMinionLeveledUp;
        MinionManager.OnRosterChanged -= OnRosterChanged;
        
        PlayerInventory.OnPartAdded -= OnPartAddedToInventory;
        PlayerInventory.OnInventoryCleared -= OnInventoryCleared;
        
        SaveSystem.OnGameLoaded -= OnGameLoaded;
    }
    
    void SyncMinionRosterFromManager()
    {
        minionRoster = MinionManager.GetMinionRoster();
        selectedMinionIndex = MinionManager.GetSelectedMinionIndex();
        
        if (selectedMinionIndex >= 0 && selectedMinionIndex < minionRoster.Count)
        {
            currentMinion = minionRoster[selectedMinionIndex];
        }
        else
        {
            currentMinion = null;
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Synced roster from manager: {minionRoster.Count} minions, selected index: {selectedMinionIndex}");
    }
    
    void OnRosterChanged()
    {
        SyncMinionRosterFromManager();
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
    }
    
    void OnGameLoaded()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Game loaded from save - refreshing UI");
        
        SyncMinionRosterFromManager();
        
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
        UpdateMinionCountDisplay();
        
        ForceRefreshEquippedPartsDisplay();
        
        UpdateWavePreview();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] UI refreshed after loading from save");
    }
    
    void ForceRefreshEquippedPartsDisplay()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Force refreshing equipped parts display");
        
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Equipped parts display refreshed");
    }
    

    
    void OnMinionUnlocked(int newMaxMinions)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] New minion slot unlocked! Now have {newMaxMinions} max minions for wave {GameData.GetCurrentWave()}");
        
        string notificationMessage = $"MINION UNLOCKED! You can now field {newMaxMinions} minions.";
        Debug.Log($"[MinionAssemblyManager] NOTIFICATION: {notificationMessage}");
        
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
    }
    
    void OnMinionLeveledUp(Minion minion, int newLevel)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] 🎉 {minion.minionName} reached level {newLevel}!");
        
        string notificationMessage = $"🎉 {minion.minionName} LEVEL UP! Now level {newLevel}";
        Debug.Log($"[MinionAssemblyManager] LEVEL UP: {notificationMessage}");
        
        UpdateMinionDisplay();
        RefreshInventoryDisplay();
    }
    
    void InitializeFromMinionManager()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] InitializeFromMinionManager called");
        
        minionRoster = MinionManager.GetMinionRoster();
        selectedMinionIndex = MinionManager.GetSelectedMinionIndex();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Found {minionRoster.Count} existing minions in MinionManager");
        
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
            string startingName = $"{selectedClass.className} Minion";
            Minion newMinion = new Minion(selectedClass.startingMinionType);
            newMinion.minionName = startingName;
            
            if (selectedClass.className == "Bone Weaver")
            {
                AutoEquipBoneWeaverBonusParts(newMinion);
            }
            else
            {
                AutoEquipStartingParts(newMinion, selectedClass);
            }
            
            MinionManager.AddMinion(newMinion);
            minionRoster = MinionManager.GetMinionRoster();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created starting minion: {startingName}");
            
            RefreshInventoryDisplay();
        }
        else
        {
            CreateNewMinion("Minion 1");
            
            RefreshInventoryDisplay();
        }
    }
    
    void AutoEquipBoneWeaverBonusParts(Minion minion)
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Generating Bone Weaver starter parts with new system.");

        PartData headPart = GenerateStarterPart(PartData.PartType.Head, PartData.PartTheme.Bone, PartData.PartRarity.Common, "Sharpened Skull");
        
        PartData bodyPart = GenerateStarterPart(PartData.PartType.Torso, PartData.PartTheme.Bone, PartData.PartRarity.Common, "Reinforced Ribcage");
        
        PartData armsPart = GenerateStarterPart(PartData.PartType.Arms, PartData.PartTheme.Bone, PartData.PartRarity.Common, "Battle-worn Bones");
        
        PartData legsPart = GenerateStarterPart(PartData.PartType.Legs, PartData.PartTheme.Bone, PartData.PartRarity.Common, "Lightweight Legs");
        
        if (headPart != null) PlayerInventory.AddPart(headPart);
        if (bodyPart != null) PlayerInventory.AddPart(bodyPart);
        if (armsPart != null) PlayerInventory.AddPart(armsPart);
        if (legsPart != null) PlayerInventory.AddPart(legsPart);
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Added {4} Bone Weaver starter parts to inventory");
        
        if (minion.GetEquippedPart(PartData.PartType.Head) == null && headPart != null)
        {
            minion.EquipPart(headPart);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Bone Weaver equipped starter head: {headPart.partName} - {headPart.stats.GetStatsText()}");
        }
        
        if (minion.GetEquippedPart(PartData.PartType.Arms) == null && armsPart != null)
        {
            minion.EquipPart(armsPart);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Bone Weaver equipped starter arms: {armsPart.partName} - {armsPart.stats.GetStatsText()}");
        }
        
        if (minion.GetEquippedPart(PartData.PartType.Torso) == null && bodyPart != null)
        {
            minion.EquipPart(bodyPart);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Bone Weaver equipped starter torso: {bodyPart.partName} - {bodyPart.stats.GetStatsText()}");
        }
        
        if (minion.GetEquippedPart(PartData.PartType.Legs) == null && legsPart != null)
        {
            minion.EquipPart(legsPart);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Bone Weaver equipped starter legs: {legsPart.partName} - {legsPart.stats.GetStatsText()}");
        }
    }
    
    PartData GenerateStarterPart(PartData.PartType partType, PartData.PartTheme theme, PartData.PartRarity rarity, string baseName)
    {
        PartData newPart = ScriptableObject.CreateInstance<PartData>();
        newPart.partName = baseName;
        newPart.type = partType;
        newPart.theme = theme;
        newPart.rarity = rarity;
        
        PartData.SpecialAbility ability;
        int abilityLevel;
        newPart.stats = PartStatsGenerator.GenerateWithAbility(theme, rarity, partType, out ability, out abilityLevel);
        newPart.specialAbility = ability;
        newPart.abilityLevel = abilityLevel;
        newPart.abilityRole = GetAbilityRole(ability);
        
        newPart.description = $"A starter {theme.ToString().ToLower()} {partType.ToString().ToLower()} crafted for precision and speed.";
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Generated starter part: {newPart.partName} ({rarity}) - {newPart.stats.GetStatsText()} | Ability: {ability} L{abilityLevel}");
        
        return newPart;
    }
    
    PartData.AbilityRole GetAbilityRole(PartData.SpecialAbility ability)
    {
        return ability switch
        {
            PartData.SpecialAbility.Taunt or PartData.SpecialAbility.ShieldWall or PartData.SpecialAbility.DamageSharing => PartData.AbilityRole.Guardian,
            PartData.SpecialAbility.Flanking or PartData.SpecialAbility.FocusFire or PartData.SpecialAbility.Momentum => PartData.AbilityRole.Assault,
            PartData.SpecialAbility.RangeAttack or PartData.SpecialAbility.Overwatch or PartData.SpecialAbility.Hunter => PartData.AbilityRole.Marksman,
            PartData.SpecialAbility.Healing or PartData.SpecialAbility.Inspiration or PartData.SpecialAbility.BattleCry => PartData.AbilityRole.Support,
            PartData.SpecialAbility.Mobility or PartData.SpecialAbility.PhaseStep or PartData.SpecialAbility.Confuse => PartData.AbilityRole.Trickster,
            _ => PartData.AbilityRole.None
        };
    }

    void AutoEquipStartingParts(Minion minion, NecromancerClass necroClass)
    {
        if (necroClass.startingParts == null) return;
        
        int partsEquipped = 0;
        int maxStartingParts = 4; 
        
        foreach (PartData part in necroClass.startingParts)
        {
            if (part != null)
            {
                if (part.rarity == PartData.PartRarity.Epic)
                {
                    if (enableDebugLogging)
                        Debug.Log($"[MinionAssemblyManager] Converting epic starting part {part.partName} to uncommon with new stats");
                    
                    PartData updatedPart = GenerateStarterPart(part.type, part.theme, PartData.PartRarity.Uncommon, part.partName);
                    PlayerInventory.AddPart(updatedPart);
                }
                else
                {
                    PlayerInventory.AddPart(part);
                }
            }
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Added {necroClass.startingParts.Length} starting parts to inventory");
        
        foreach (PartData part in necroClass.startingParts)
        {
            if (part != null && partsEquipped < maxStartingParts)
            {
                PartData currentPart = minion.GetEquippedPart(part.type);
                if (currentPart == null)
                {
                    if (part.rarity == PartData.PartRarity.Epic)
                    {
                        PartData updatedPart = GenerateStarterPart(part.type, part.theme, PartData.PartRarity.Uncommon, part.partName);
                        minion.EquipPart(updatedPart);
                    }
                    else
                    {
                        minion.EquipPart(part);
                    }
                    
                    partsEquipped++;
                    
                    if (enableDebugLogging)
                        Debug.Log($"[MinionAssemblyManager] Auto-equipped {part.partName} to {minion.minionName}");
                }
            }
        }
        
        minion.CalculateStats();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Auto-equipped {partsEquipped} starting parts to {minion.minionName}. Final stats: {minion.totalHP} HP, {minion.totalAttack} ATK");
    }
    
    void RefreshInventoryDisplay()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] RefreshInventoryDisplay called");
        
        ClearInventoryDisplay();
        
        List<PartData> availableParts = PlayerInventory.GetAllParts();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Found {availableParts.Count} parts in inventory");
        
        if (availableParts.Count == 0)
        {
            CreateInventoryEmptyMessage();
            return;
        }
        
        Dictionary<PartData, int> partCounts = new Dictionary<PartData, int>();
        foreach (PartData part in availableParts)
        {
            if (partCounts.ContainsKey(part))
                partCounts[part]++;
            else
                partCounts[part] = 1;
        }
        
        foreach (var kvp in partCounts)
        {
            PartData part = kvp.Key;
            int totalCount = kvp.Value;
            int equippedCount = GetTotalEquippedCount(part);
            
            CreateInventoryIconItem(part, totalCount, equippedCount);
        }
        
        UpdateAllPartVisuals();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Refreshed inventory display with {partCounts.Count} unique parts as icons");
    }
    
    void CreateInventoryEmptyMessage()
    {
        GameObject messageItem = CreateGridItem("No parts available\nCollect parts from card selection!", Color.yellow);
        
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
        
        GameObject iconItem = CreatePartIcon(part, unequippedCount, equippedCount);
        
        if (PlayerInventory.IsNewPart(part))
        {
            AddNewPartIndicator(iconItem);
        }
        
        partToUI[part] = iconItem;
        
        if (unequippedCount > 0 && !isMergeMode)
        {
            DraggablePartItem dragComponent = iconItem.AddComponent<DraggablePartItem>();
            dragComponent.Initialize(part, this);
            
            Button button = iconItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnInventoryPartClicked(part));
            }
        }
        else if (unequippedCount > 0 && isMergeMode)
        {
            Button button = iconItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnInventoryPartClicked(part));
            }
        }
        else
        {
            Button button = iconItem.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }

        }
        
        iconItem.transform.SetParent(inventoryIconsContainer, false);
        inventoryItems.Add(iconItem);
    }
    
    void AddNewPartIndicator(GameObject iconItem)
    {
        GameObject asteriskObj = new GameObject("NewPartIndicator", typeof(RectTransform));
        asteriskObj.transform.SetParent(iconItem.transform, false);
        
        TMPro.TextMeshProUGUI asteriskText = asteriskObj.AddComponent<TMPro.TextMeshProUGUI>();
        asteriskText.text = "*";
        asteriskText.color = Color.yellow;
        asteriskText.fontSize = 24;
        asteriskText.fontStyle = TMPro.FontStyles.Bold;
        asteriskText.alignment = TMPro.TextAlignmentOptions.TopRight;
        
        RectTransform asteriskRect = asteriskObj.GetComponent<RectTransform>();
        asteriskRect.anchorMin = new Vector2(1, 1);
        asteriskRect.anchorMax = new Vector2(1, 1);
        asteriskRect.pivot = new Vector2(1, 1);
        asteriskRect.anchoredPosition = new Vector2(-5, -5);
        asteriskRect.sizeDelta = new Vector2(20, 20);
    }
    
    GameObject CreatePartIcon(PartData part, int unequippedCount, int equippedCount)
    {
        GameObject iconItem = new GameObject($"PartIcon_{part.partName}", typeof(RectTransform));
        
        Button button = iconItem.AddComponent<Button>();
        
        RectTransform iconRect = iconItem.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(64, 64);
        
        GameObject backgroundFrame = new GameObject("BackgroundFrame", typeof(RectTransform));
        backgroundFrame.transform.SetParent(iconItem.transform, false);
        backgroundFrame.transform.SetAsFirstSibling();
        
        Image backgroundImage = backgroundFrame.AddComponent<Image>();
        Sprite rarityFrame = GetRarityBackgroundFrame(part.rarity);
        if (rarityFrame != null)
        {
            backgroundImage.sprite = rarityFrame;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = GetRarityColor(part.rarity);
        }
        
        RectTransform backgroundRect = backgroundFrame.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        GameObject partIconObj = new GameObject("PartIcon", typeof(RectTransform));
        partIconObj.transform.SetParent(iconItem.transform, false);
        
        Image partIconImage = partIconObj.AddComponent<Image>();
        
        Sprite partSprite = GetPartTypeSprite(part.type);
        if (partSprite != null)
        {
            partIconImage.sprite = partSprite;
        }
        else if (part.icon != null)
        {
            partIconImage.sprite = part.icon;
        }
        else
        {
            partIconImage.color = new Color32(128, 128, 128, 255);
        }
        
        if (equippedCount > 0)
        {
            partIconImage.color = EQUIPPED_COLOR;
        }
        else
        {
            partIconImage.color = NORMAL_COLOR;
        }
        
        RectTransform partIconRect = partIconObj.GetComponent<RectTransform>();
        partIconRect.anchorMin = new Vector2(0.1f, 0.1f);
        partIconRect.anchorMax = new Vector2(0.9f, 0.9f);
        partIconRect.offsetMin = Vector2.zero;
        partIconRect.offsetMax = Vector2.zero;
        
        var trigger = iconItem.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = iconItem.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => OnPartHoverEnter(part, iconItem));
        trigger.triggers.Add(pointerEnter);
        
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => OnPartHoverExit(part));
        trigger.triggers.Add(pointerExit);
        
        if (unequippedCount + equippedCount > 1)
        {
            GameObject countTextObj = new GameObject("CountText", typeof(RectTransform));
            countTextObj.transform.SetParent(iconItem.transform, false);
            
            GameObject countBgObj = new GameObject("CountBackground", typeof(RectTransform));
            countBgObj.transform.SetParent(countTextObj.transform, false);
            countBgObj.transform.SetAsFirstSibling();
            
            Image countBg = countBgObj.AddComponent<Image>();
            countBg.color = new Color(0, 0, 0, 0.7f);
            
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
        
        int availableCount = GetAvailablePartCount(part);
        int equippedCount = GetTotalEquippedCount(part);
        int unequippedCount = availableCount - equippedCount;
        
        if (unequippedCount <= 0)
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] No unequipped {part.partName} available!");
            return;
        }
        
        PartData currentPart = currentMinion.GetEquippedPart(part.type);
        if (currentPart != null)
        {
            PlayerInventory.AddPart(currentPart);
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Returned {currentPart.partName} to inventory");
        }
        
        PlayerInventory.RemovePart(part);
        
        currentMinion.EquipPart(part);
        
        currentMinion.CalculateStats();
        
        SaveSystem.AutoSave();
        
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
        
        UpdateAllPartVisuals();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Equipped {part.partName} to {part.type} slot. New stats: {currentMinion.totalHP} HP, {currentMinion.totalAttack} ATK");
    }
    
    int GetTotalEquippedCount(PartData part)
    {
        int count = 0;
        List<Minion> currentRoster = MinionManager.GetMinionRoster();
        
        foreach (Minion minion in currentRoster)
        {
            foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
            {
                PartData equippedPart = minion.GetEquippedPart(partType);
                if (equippedPart != null && part != null && equippedPart.partName == part.partName)
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
        
        currentMinion.UnequipPart(part.type);
        
        currentMinion.CalculateStats();
        
        SaveSystem.AutoSave();
        
        RefreshInventoryDisplay();
        UpdateMinionDisplay();
        
        UpdateAllPartVisuals();
        
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
        
        partToUI.Clear();
    }
    
    void UpdateMinionDisplay()
    {
        RefreshSingleMinionDisplay();
        UpdateMinionCountDisplay();
        UpdateNavigationButtons();
    }

    void UpdateMinionCountDisplay()
    {
        if (minionCountText != null)
        {
            int currentCount = MinionManager.GetMinionCount();
            int maxMinions = MinionManager.GetMaxMinions();
            minionCountText.text = $"Your Minions ({currentCount}/{maxMinions})";
        }
        
        if (addMinionButton != null)
        {
            int maxMinions = MinionManager.GetMaxMinions();
            addMinionButton.gameObject.SetActive(MinionManager.GetMinionCount() < maxMinions);
        }
    }
     
     public void ContinueToGameplay()
    {
        if (minionRoster.Count == 0)
        {
            Debug.LogWarning("[MinionAssemblyManager] Cannot proceed - no minions created!");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"[MinionAssemblyManager] Proceeding to gameplay with {minionRoster.Count} minions:");
            foreach (Minion minion in minionRoster)
            {
                Debug.Log($"  - {minion.minionName}: {minion.totalHP} HP, {minion.totalAttack} ATK, {minion.GetEquippedPartsCount()}/4 parts");
            }
        }
        
        if (GameData.IsFirstWave())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] First wave minion assembly complete. Proceeding to Wave 1 combat.");
        }
        
        SceneManager.LoadScene("Gameplay");
    }
      public void BackToClassSelection()
    {
        MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Returning to class selection");
        
        SceneManager.LoadScene("ClassSelection");
    }
    
    public void BackToMainMenu()
    {
        MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Returning to main menu");
        
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowCardSelection()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] 🎯 ShowCardSelection() called - Button was clicked!");
        
        if (GameData.IsFirstWave())
        {
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] ✗ Card selection not available - complete first wave first");
            return;
        }
        
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] ✓ Showing card selection overlay");

    }
     
     void AutoAddWaveRewardParts()
     {
         if (enableDebugLogging)
             Debug.Log("[MinionAssemblyManager] Auto-adding 3 parts to inventory after wave completion");
         
         int currentWave = GameData.GetCurrentWave();
         NecromancerClass playerClass = GameData.GetSelectedClass();
         List<PartData> generatedParts = DynamicPartGenerator.GenerateCardSelection(currentWave, playerClass, 3);

         foreach (PartData part in generatedParts)
         {
             if (part != null)
             {
                 PlayerInventory.AddPart(part);
                 if (enableDebugLogging)
                     Debug.Log($"[MinionAssemblyManager] Auto-added {part.partName} to inventory");
             }
         }
         
         SaveSystem.AutoSave();
         
         RefreshInventoryDisplay();
         
         string notificationMessage = $"Wave {currentWave - 1} Complete! Added 3 new parts to inventory.";
         Debug.Log($"[MinionAssemblyManager] NOTIFICATION: {notificationMessage}");
         
         if (enableDebugLogging)
             Debug.Log($"[MinionAssemblyManager] Auto-added {generatedParts.Count} parts to inventory. Total parts: {PlayerInventory.GetPartCount()}");
     }
     
     System.Collections.IEnumerator DelayedAutoShowCardSelectionOnStart()
     {
         yield return new WaitForSeconds(0.1f);
         
         if (enableDebugLogging)
             Debug.Log("[MinionAssemblyManager] Auto-showing card selection after returning from combat");
         
         GameData.ClearCombatCompletedFlag();
         
         AutoAddWaveRewardParts();
     }
     

     
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
        
        if (string.IsNullOrEmpty(minionName))
        {
            minionName = $"Minion {MinionManager.GetMinionCount() + 1}";
        }
        
        Minion newMinion = new Minion(defaultMinionData);
        newMinion.minionName = minionName;
        
        if (MinionManager.AddMinion(newMinion))
        {
            minionRoster = MinionManager.GetMinionRoster();
            selectedMinionIndex = minionRoster.Count - 1;
            currentMinion = newMinion;
            MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
            
            RefreshInventoryDisplay();
            UpdateMinionDisplay();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created new minion: {minionName}");
        }
    }
    
    void SelectMinion(int index)
    {
        List<Minion> currentRoster = MinionManager.GetMinionRoster();
        
        if (index >= 0 && index < currentRoster.Count)
        {
            selectedMinionIndex = index;
            currentMinion = currentRoster[index];
            MinionManager.SetSelectedMinionIndex(index);
            
            RefreshInventoryDisplay();
            UpdateMinionDisplay();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Selected minion: {currentMinion.minionName}");
        }
    }

    public void OnMinionDataChanged(Minion minion)
    {
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Minion data changed: {minion.minionName}");
    }

    public void OnMinionSlotClicked(Minion minion, PartData.PartType partType)
    {
        SetCurrentMinion(minion);
        
        PartData equippedPart = minion.GetEquippedPart(partType);
        
        if (equippedPart != null)
        {
            UnequipPartToInventory(equippedPart);
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Empty {partType} slot clicked for {minion.minionName}. Select a part from inventory and use the Equip button.");
        }
    }

    public void OnMinionSlotClicked(Minion minion, PartData.PartType partType, PartData specificPart = null)
    {
        SetCurrentMinion(minion);
        
        PartData equippedPart = minion.GetEquippedPart(partType);
        
        if (specificPart != null)
        {
            if (specificPart.type != partType)
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Cannot equip {specificPart.type} part to {partType} slot");
                return;
            }
            
            int availableCount = GetAvailablePartCount(specificPart);
            int equippedCount = GetTotalEquippedCount(specificPart);
            int unequippedCount = availableCount - equippedCount;
            
            if (unequippedCount <= 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] No unequipped {specificPart.partName} available!");
                return;
            }
            
            EquipPartFromInventory(specificPart);
        }
        else if (equippedPart != null)
        {
            UnequipPartToInventory(equippedPart);
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Empty {partType} slot clicked for {minion.minionName}. Drag a part here to equip.");
        }
    }

    void SetCurrentMinion(Minion minion)
    {   
        List<Minion> currentRoster = MinionManager.GetMinionRoster();
        
        for (int i = 0; i < currentRoster.Count; i++)
        {
            if (currentRoster[i] == minion)
            {
                currentMinion = minion;
                selectedMinionIndex = i;
                MinionManager.SetSelectedMinionIndex(i);
                
                RefreshInventoryDisplay();
                break;
            }
        }
    }

    void RefreshSingleMinionDisplay()
    {
        ClearMinionDisplayArea();
        
        if (currentMinion != null && minionDisplayArea != null)
        {
            CreateSingleMinionDisplay(currentMinion);
        }
    }
    
    void ClearMinionDisplayArea()
    {
        if (minionDisplayArea != null)
        {
            for (int i = minionDisplayArea.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(minionDisplayArea.GetChild(i).gameObject);
            }
        }
    }
    
    void CreateSingleMinionDisplay(Minion minion)
    {
        if (minionEntryPrefab == null)
        {
            Debug.LogError("[MinionAssemblyManager] minionEntryPrefab is null! Check UI references in inspector.");
            return;
        }
        
        if (minionDisplayArea == null)
        {
            Debug.LogError("[MinionAssemblyManager] minionDisplayArea is null! Check UI references in inspector.");
            return;
        }
        
        GameObject entryInstance = Instantiate(minionEntryPrefab, minionDisplayArea);
        MinionListEntryUI entryUI = entryInstance.GetComponent<MinionListEntryUI>();
        
        if (entryUI != null)
        {
            entryUI.Initialize(minion, this);
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Created single minion display for: {minion.minionName}");
        }
        else
        {
            Debug.LogError("[MinionAssemblyManager] minionEntryPrefab doesn't have MinionListEntryUI component!");
        }
    }

    void RefreshMinionListDisplay()
    {
        ClearMinionListDisplay();
        
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
        
        
        GameObject entryInstance = Instantiate(minionEntryPrefab);
        MinionListEntryUI entryUI = entryInstance.GetComponent<MinionListEntryUI>();
        
        if (entryUI != null)
        {
            entryUI.Initialize(minion, this);
            minionSelectorItems.Add(entryInstance);
            
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

    GameObject CreateGridItem(string text, Color textColor)
    {
        GameObject item = new GameObject("GridItem");
        
        Button button = item.AddComponent<Button>();
        
        Image itemImage = item.AddComponent<Image>();
        itemImage.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);

        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(item.transform);
        textChild.transform.localScale = Vector3.one;
        
        TMPro.TextMeshProUGUI textComponent = textChild.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 12;
        textComponent.color = textColor;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TMPro.TextWrappingModes.Normal;
        
        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
        
        return item;
    }

    public void OnAddMinionClicked()
    {
        if (MinionManager.CanAddMoreMinions())
        {
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
                minionRoster = MinionManager.GetMinionRoster();
                selectedMinionIndex = minionRoster.Count - 1;
                currentMinion = newMinion;
                MinionManager.SetSelectedMinionIndex(selectedMinionIndex);
                UpdateMinionDisplay();
                UpdateNavigationButtons();
                
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
    
    public void OnPreviousMinionClicked()
    {
        int currentSelectedIndex = MinionManager.GetSelectedMinionIndex();
        if (currentSelectedIndex > 0)
        {
            SelectMinion(currentSelectedIndex - 1);
            UpdateNavigationButtons();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Navigated to previous minion: {currentMinion.minionName}");
        }
    }
    
    public void OnNextMinionClicked()
    {
        int currentSelectedIndex = MinionManager.GetSelectedMinionIndex();
        int currentRosterCount = MinionManager.GetMinionCount();
        if (currentSelectedIndex < currentRosterCount - 1)
        {
            SelectMinion(currentSelectedIndex + 1);
            UpdateNavigationButtons();
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Navigated to next minion: {currentMinion.minionName}");
        }
    }
    
    public void OnRemoveMinionClicked()
    {
        int currentRosterCount = MinionManager.GetMinionCount();
        int currentSelectedIndex = MinionManager.GetSelectedMinionIndex();
        
        if (currentRosterCount > 1)
        {
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Removing minion: {currentMinion.minionName}");
            
            ReturnEquippedPartsToInventory(currentMinion);
                
            MinionManager.RemoveMinion(currentSelectedIndex);
            
            SyncMinionRosterFromManager();
            
            UpdateMinionDisplay();
            UpdateNavigationButtons();
            RefreshInventoryDisplay();
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] Cannot remove last minion!");
        }
    }
    
    void ReturnEquippedPartsToInventory(Minion minion)
    {
        if (minion == null) return;
        
        int partsReturned = 0;
        
        foreach (PartData.PartType partType in System.Enum.GetValues(typeof(PartData.PartType)))
        {
            PartData equippedPart = minion.GetEquippedPart(partType);
            if (equippedPart != null)
            {
                PlayerInventory.AddPart(equippedPart);
                partsReturned++;
                
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Returned {equippedPart.partName} ({partType}) to inventory");
            }
        }
        
        if (enableDebugLogging)
                         Debug.Log($"[MinionAssemblyManager] Returned {partsReturned} parts to inventory from {minion.minionName}");
     }

    
    void UpdateNavigationButtons()
    {
        int currentRosterCount = MinionManager.GetMinionCount();
        int currentSelectedIndex = MinionManager.GetSelectedMinionIndex();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] UpdateNavigationButtons: selectedIndex={currentSelectedIndex}, rosterCount={currentRosterCount}");
        
        if (previousMinionButton != null)
        {
            bool canGoPrevious = (currentSelectedIndex > 0);
            previousMinionButton.interactable = canGoPrevious;
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Previous button: {(canGoPrevious ? "ENABLED" : "DISABLED")}");
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] previousMinionButton is NULL! Check inspector assignment.");
        }
        
        if (nextMinionButton != null)
        {
            bool canGoNext = (currentSelectedIndex < currentRosterCount - 1);
            nextMinionButton.interactable = canGoNext;
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Next button: {(canGoNext ? "ENABLED" : "DISABLED")}");
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] nextMinionButton is NULL! Check inspector assignment.");
        }
        
        if (removeMinionButton != null)
        {
            bool canRemove = (currentRosterCount > 1);
            removeMinionButton.interactable = canRemove;
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Remove button: {(canRemove ? "ENABLED" : "DISABLED")}");
        }
        
        if (addMinionButton != null)
        {
            bool canAdd = MinionManager.CanAddMoreMinions();
            addMinionButton.interactable = canAdd;
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Add button: {(canAdd ? "ENABLED" : "DISABLED")}");
        }
        
        UpdateMinionCountDisplay();
    }

    void OnPartAddedToInventory(PartData part)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[MinionAssemblyManager] Part added to inventory: {part.partName}");
            Debug.Log($"[MinionAssemblyManager] Total parts in inventory: {PlayerInventory.GetPartCount()}");
            Debug.Log($"[MinionAssemblyManager] Inventory container reference: {(inventoryIconsContainer != null ? "✓ Valid" : "✗ NULL")}");
        }
        
        StartCoroutine(DelayedInventoryRefresh());
    }
    
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

    Sprite GetRarityBackgroundFrame(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common:
                return commonFrame;
            case PartData.PartRarity.Uncommon:
                return uncommonFrame;
            case PartData.PartRarity.Rare:
                return rareFrame;
            case PartData.PartRarity.Epic:
                return epicFrame;
            default:
                return commonFrame;
        }
    }

    Color GetRarityColor(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common:
                return new Color32(128, 128, 128, 255); // Gray
            case PartData.PartRarity.Uncommon:
                return new Color32(0, 255, 0, 255); // Green
            case PartData.PartRarity.Rare:
                return new Color32(0, 100, 255, 255); // Blue
            case PartData.PartRarity.Epic:
                return new Color32(255, 0, 255, 255); // Purple
            default:
                return new Color32(128, 128, 128, 255); // Gray fallback
        }
    }

    void OnPartHoverEnter(PartData part, GameObject iconItem)
    {
        ShowDynamicTooltip(part);
        
        if (PlayerInventory.IsNewPart(part))
        {
            PlayerInventory.MarkPartAsSeen(part);
            RemoveNewPartIndicator(iconItem);
        }
    }
    
    void OnPartHoverExit(PartData part)
    {
        HideDynamicTooltip();
    }
    
    void RemoveNewPartIndicator(GameObject iconItem)
    {
        Transform asteriskChild = iconItem.transform.Find("NewPartIndicator");
        if (asteriskChild != null)
        {
            DestroyImmediate(asteriskChild.gameObject);
        }
    }
    
    void ShowDynamicTooltip(PartData part)
    {
        if (part != null)
        {
            DynamicTooltip tooltipToUse = dynamicTooltip;
            if (tooltipToUse == null)
            {
                tooltipToUse = DynamicTooltip.Instance;
            }
            if (tooltipToUse == null)
            {
                tooltipToUse = FindAnyObjectByType<DynamicTooltip>();
            }
            
            if (tooltipToUse != null)
            {
                tooltipToUse.ShowTooltip(part);
                
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Showing dynamic tooltip for {part.partName}");
            }
            else
            {
                if (enableDebugLogging)
                    Debug.LogWarning("[MinionAssemblyManager] Dynamic tooltip not found! Make sure DynamicTooltip GameObject is in the scene and active.");
            }
        }
    }
    
    void HideDynamicTooltip()
    {
        DynamicTooltip tooltipToUse = dynamicTooltip ?? DynamicTooltip.Instance;
        if (tooltipToUse != null)
        {
            tooltipToUse.HideTooltip();
        }
    }
    
    void OnInventoryPartClicked(PartData part)
    {
        if (part == null) return;
        
        if (isMergeMode)
        {
            HandleMergeModeClick(part);
            return;
        }
        
        if (selectedParts.Contains(part))
        {
            selectedParts.Remove(part);
            UpdatePartVisual(part, false);
        }
        else
        {
            selectedParts.Add(part);
            UpdatePartVisual(part, true);
        }
        
        UpdatePartManagementUI();
        
        UpdateAllPartVisuals();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Part selection changed. Selected: {selectedParts.Count} parts");
    }
    
    void HandleMergeModeClick(PartData part)
    {
        if (mergeSelectedParts.Contains(part))
        {
            mergeSelectedParts.Remove(part);
            UpdatePartVisual(part, false);
            
            if (mergeSelectedParts.Count == 0)
            {
                mergeSlotType = null;
                mergeRarity = null;
            }
        }
        else
        {
            if (CanAddToMergeSelection(part))
            {
                mergeSelectedParts.Add(part);
                UpdatePartVisual(part, true);
                
                if (mergeSelectedParts.Count == 1)
                {
                    mergeSlotType = part.type;
                    mergeRarity = part.rarity;
                }
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log($"[MinionAssemblyManager] Cannot merge {part.partName} - incompatible with current selection");
                return;
            }
        }
        
        UpdateMergeUI();
        
        UpdateAllPartVisuals();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Merge selection changed. Selected: {mergeSelectedParts.Count} parts");
    }
    
    bool CanAddToMergeSelection(PartData part)
    {
        if (mergeSelectedParts.Count >= 3) return false;
        
        if (!IsPartAvailableForMerging(part)) return false;
        
        if (mergeSelectedParts.Count == 0) return true;
        
        return part.type == mergeSlotType && part.rarity == mergeRarity;
    }
    
    bool IsPartAvailableForMerging(PartData part)
    {
        foreach (Minion minion in minionRoster)
        {
            if (minion.GetEquippedPart(part.type) == part)
            {
                return false;
            }
        }
        return true;
    }
    
    void UpdatePartVisual(PartData part, bool isSelected)
    {
        if (partToUI.TryGetValue(part, out GameObject uiObject))
        {
            Transform partIconTransform = uiObject.transform.Find("PartIcon");
            if (partIconTransform != null)
            {
                Image partIconImage = partIconTransform.GetComponent<Image>();
                if (partIconImage != null)
                {
                    if (isSelected)
                    {
                        partIconImage.color = SELECTED_COLOR;
                    }
                    else
                    {
                        partIconImage.color = NORMAL_COLOR;
                    }
                }
            }
        }
    }
    
    void UpdateAllPartVisuals()
    {
        foreach (var kvp in partToUI)
        {
            PartData part = kvp.Key;
            GameObject uiObject = kvp.Value;
            
            if (uiObject == null) continue;
            
            Transform partIconTransform = uiObject.transform.Find("PartIcon");
            if (partIconTransform != null)
            {
                Image partIconImage = partIconTransform.GetComponent<Image>();
                if (partIconImage != null)
                {
                    Color targetColor = NORMAL_COLOR;
                    
                    if (isMergeMode)
                    {
                        if (mergeSelectedParts.Contains(part))
                        {
                            targetColor = SELECTED_COLOR;
                        }
                        else if (!CanAddToMergeSelection(part) || !IsPartAvailableForMerging(part))
                        {
                            targetColor = EQUIPPED_COLOR;
                        }
                    }
                    else
                    {
                        if (selectedParts.Contains(part))
                        {
                            targetColor = SELECTED_COLOR;
                        }
                        else if (GetTotalEquippedCount(part) > 0)
                        {
                            targetColor = EQUIPPED_COLOR;
                        }
                    }
                    
                    partIconImage.color = targetColor;
                }
            }
        }
    }
    
    void UpdatePartManagementUI()
    {
        if (deleteSelectedPartsButton != null)
        {
            deleteSelectedPartsButton.interactable = selectedParts.Count > 0;
        }
        
        if (equipSelectedPartButton != null)
        {
            equipSelectedPartButton.interactable = selectedParts.Count == 1 && currentMinion != null;
        }
        
        if (selectedPartInfoText != null)
        {
            if (selectedParts.Count == 0)
            {
                selectedPartInfoText.text = "No parts selected";
            }
            else if (selectedParts.Count == 1)
            {
                PartData part = selectedParts.First();
                selectedPartInfoText.text = $"Selected: {part.partName} ({part.GetRarityText()})";
            }
            else
            {
                selectedPartInfoText.text = $"Selected: {selectedParts.Count} parts";
            }
        }
    }
    
    void DeleteSelectedParts()
    {
        if (selectedParts.Count == 0) return;
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Deleting {selectedParts.Count} selected parts");
        
        foreach (PartData part in selectedParts)
        {
            PlayerInventory.RemovePart(part);
        }
        
        selectedParts.Clear();
        partToUI.Clear();
        
        RefreshInventoryDisplay();
        
        UpdatePartManagementUI();
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Parts deleted and inventory refreshed");
    }
    
    void EquipSelectedPart()
    {
        if (selectedParts.Count != 1 || currentMinion == null) return;
        
        PartData part = selectedParts.First();
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Equipping {part.partName} to {currentMinion.minionName}");
        
        EquipPartFromInventory(part);
        
        selectedParts.Clear();
        UpdatePartManagementUI();
        
        RefreshInventoryDisplay();
        
        UpdateMinionDisplay();
    }
    
    void ToggleMergeMode()
    {
        isMergeMode = !isMergeMode;
        
        if (isMergeMode)
        {
            EnterMergeMode();
        }
        else
        {
            ExitMergeMode();
        }
        
        if (enableDebugLogging)
            Debug.Log($"[MinionAssemblyManager] Merge mode {(isMergeMode ? "ENABLED" : "DISABLED")}");
    }
    
    void EnterMergeMode()
    {
        selectedParts.Clear();
        UpdatePartManagementUI();
        
        if (mergeModeToggleButton != null)
        {
            TMPro.TextMeshProUGUI buttonText = mergeModeToggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Exit Merge Mode";
        }
        
        if (inventoryOutline != null)
        {
            inventoryOutline.gameObject.SetActive(true);
        }
        
        if (mergePreviewPopup != null)
        {
            mergePreviewPopup.SetActive(false);
        }
        
        UpdateMergeUI();
        
        UpdateAllPartVisuals();
        
        DisableDragComponentsInMergeMode();
    }
    
    void ExitMergeMode()
    {
        mergeSelectedParts.Clear();
        mergeSlotType = null;
        mergeRarity = null;
        
        foreach (var kvp in partToUI)
        {
            if (kvp.Value != null)
            {
                UpdatePartVisual(kvp.Key, false);
            }
        }
        
        if (mergeModeToggleButton != null)
        {
            TMPro.TextMeshProUGUI buttonText = mergeModeToggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Merge Mode";
        }
        
        if (inventoryOutline != null)
        {
            inventoryOutline.gameObject.SetActive(false);
        }

        if (mergePreviewPopup != null)
        {
            mergePreviewPopup.SetActive(false);
        }
        
        UpdateMergeUI();
        
        UpdateAllPartVisuals();
        
        DisableDragComponentsInMergeMode();
    }
    
    void DisableDragComponentsInMergeMode()
    {
        foreach (var kvp in partToUI)
        {
            if (kvp.Value != null)
            {
                DraggablePartItem dragComponent = kvp.Value.GetComponent<DraggablePartItem>();
                if (dragComponent != null)
                {
                    dragComponent.enabled = !isMergeMode;
                }
            }
        }
    }
    
    void UpdateMergeUI()
    {
        UpdateAllPartVisuals();
        
        if (mergePreviewPopup != null)
        {
            bool showPreview = mergeSelectedParts.Count == 3;
            mergePreviewPopup.SetActive(showPreview);
            
            if (showPreview)
            {
                UpdateMergePreview();
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] Merge preview popup is null - check UI references");
        }
    }
    
    void UpdateMergePartVisuals()
    {
        foreach (var kvp in partToUI)
        {
            PartData part = kvp.Key;
            GameObject uiObject = kvp.Value;
            
            if (uiObject == null) continue;
            
            Transform partIconTransform = uiObject.transform.Find("PartIcon");
            if (partIconTransform != null)
            {
                Image partIconImage = partIconTransform.GetComponent<Image>();
                if (partIconImage != null)
                {
                    if (mergeSelectedParts.Contains(part))
                    {
                        partIconImage.color = SELECTED_COLOR;
                    }
                    else if (CanAddToMergeSelection(part) && IsPartAvailableForMerging(part))
                    {
                        partIconImage.color = NORMAL_COLOR;
                    }
                    else
                    {
                        partIconImage.color = EQUIPPED_COLOR;
                    }
                }
            }
        }
    }

    void UpdateMergePreview()
    {
        if (mergeSelectedParts.Count != 3) return;
        
        PartData mergedPart = GenerateMergedPart();
        
        if (mergePreviewText != null)
        {
            string previewText = $"Merge Result:\n";
            previewText += $"{mergedPart.partName}\n";
            previewText += $"{mergedPart.GetRarityText()} {mergedPart.type}\n\n";
            
            if (mergedPart.stats.HasAnyStats())
            {
                previewText += "Stats:\n";
                if (mergedPart.stats.hp > 0) previewText += $"+{mergedPart.stats.hp} HP\n";
                if (mergedPart.stats.attack > 0) previewText += $"+{mergedPart.stats.attack} ATK\n";
                if (mergedPart.stats.defense > 0) previewText += $"+{mergedPart.stats.defense} DEF\n";
                if (mergedPart.stats.critChance > 0) previewText += $"+{mergedPart.stats.critChance}% Crit\n";
                if (mergedPart.stats.critDamage > 0) previewText += $"+{mergedPart.stats.critDamage}% Crit DMG\n";
                if (mergedPart.stats.armorPen > 0) previewText += $"+{mergedPart.stats.armorPen} Armor Pen\n";
            }
            
            string abilityDesc = mergedPart.GetAbilityDescription();
            if (!string.IsNullOrEmpty(abilityDesc))
            {
                previewText += $"\nAbility:\n{abilityDesc}";
            }
            
            mergePreviewText.text = previewText;
        }
        
        if (mergePreviewImage != null)
        {
            Sprite partSprite = GetPartTypeSprite(mergedPart.type);
            if (partSprite != null)
            {
                mergePreviewImage.sprite = partSprite;
                mergePreviewImage.color = mergedPart.GetRarityColor();
            }
        }
    }
    
    PartData GenerateMergedPart()
    {
        if (mergeSelectedParts.Count != 3) return null;
        
        PartData basePart = mergeSelectedParts.First();
        
        PartData mergedPart = ScriptableObject.CreateInstance<PartData>();
        mergedPart.partName = $"Merged {basePart.type}";
        mergedPart.type = basePart.type;
        mergedPart.theme = basePart.theme;
        mergedPart.icon = basePart.icon;
        
        switch (basePart.rarity)
        {
            case PartData.PartRarity.Common:
                mergedPart.rarity = PartData.PartRarity.Uncommon;
                break;
            case PartData.PartRarity.Uncommon:
                mergedPart.rarity = PartData.PartRarity.Rare;
                break;
            case PartData.PartRarity.Rare:
                mergedPart.rarity = PartData.PartRarity.Epic;
                break;
            case PartData.PartRarity.Epic:
                mergedPart.rarity = PartData.PartRarity.Epic;
                break;
        }
        
        mergedPart.stats = CalculateMergedStats();
        
        mergedPart.specialAbility = DetermineMergedAbility();
        mergedPart.abilityLevel = DetermineMergedAbilityLevel();
        
        return mergedPart;
    }
    
    PartData.PartStats CalculateMergedStats()
    {
        PartData.PartStats mergedStats = new PartData.PartStats();
        
        foreach (PartData part in mergeSelectedParts)
        {
            mergedStats.hp += part.stats.hp;
            mergedStats.attack += part.stats.attack;
            mergedStats.defense += part.stats.defense;
            mergedStats.critChance += part.stats.critChance;
            mergedStats.critDamage += part.stats.critDamage;
            mergedStats.armorPen += part.stats.armorPen;
        }
        
        float efficiencyBonus = GetRarityEfficiencyBonus(mergeRarity.Value);
        mergedStats.hp = Mathf.RoundToInt(mergedStats.hp * efficiencyBonus);
        mergedStats.attack = Mathf.RoundToInt(mergedStats.attack * efficiencyBonus);
        mergedStats.defense = Mathf.RoundToInt(mergedStats.defense * efficiencyBonus);
        mergedStats.critChance = Mathf.RoundToInt(mergedStats.critChance * efficiencyBonus);
        mergedStats.critDamage = Mathf.RoundToInt(mergedStats.critDamage * efficiencyBonus);
        mergedStats.armorPen = Mathf.RoundToInt(mergedStats.armorPen * efficiencyBonus);
        
        return mergedStats;
    }
    
    float GetRarityEfficiencyBonus(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 1.1f;
            case PartData.PartRarity.Uncommon: return 1.2f;
            case PartData.PartRarity.Rare: return 1.3f;
            default: return 1.0f;
        }
    }
    
    PartData.SpecialAbility DetermineMergedAbility()
    {
        Dictionary<PartData.SpecialAbility, int> abilityCounts = new Dictionary<PartData.SpecialAbility, int>();
        
        foreach (PartData part in mergeSelectedParts)
        {
            if (part.specialAbility != PartData.SpecialAbility.None)
            {
                if (abilityCounts.ContainsKey(part.specialAbility))
                    abilityCounts[part.specialAbility]++;
                else
                    abilityCounts[part.specialAbility] = 1;
            }
        }
        
        if (abilityCounts.Count == 1)
        {
            return abilityCounts.Keys.First();
        }
        
        if (abilityCounts.Count > 1)
        {
            return abilityCounts.OrderByDescending(x => x.Value).First().Key;
        }
        
        if (Random.Range(0f, 1f) < 0.25f)
        {
            return GetRandomAbilityForSlot(mergeSlotType.Value);
        }
        
        return PartData.SpecialAbility.None;
    }
    
    PartData.SpecialAbility GetRandomAbilityForSlot(PartData.PartType slotType)
    {
        switch (slotType)
        {
            case PartData.PartType.Head:
                return (PartData.SpecialAbility)Random.Range(1, 5); // Overwatch, Hunter, Focus Fire, Confuse
            case PartData.PartType.Torso:
                return (PartData.SpecialAbility)Random.Range(5, 9); // Taunt, Shield Wall, Damage Sharing, Battle Cry
            case PartData.PartType.Arms:
                return (PartData.SpecialAbility)Random.Range(9, 13); // Range Attack, Flanking, Momentum, Inspiration
            case PartData.PartType.Legs:
                return (PartData.SpecialAbility)Random.Range(13, 16); // Mobility, Phase Step, Healing
            default:
                return PartData.SpecialAbility.None;
        }
    }
    
    int DetermineMergedAbilityLevel()
    {
        if (mergeSelectedParts.Count != 3) return 1;
        
        int totalLevel = 0;
        int abilityCount = 0;
        
        foreach (PartData part in mergeSelectedParts)
        {
            if (part.specialAbility != PartData.SpecialAbility.None)
            {
                totalLevel += part.abilityLevel;
                abilityCount++;
            }
        }
        
        if (abilityCount == 0) return 1;

        int averageLevel = Mathf.RoundToInt((float)totalLevel / abilityCount) + 1;
        return Mathf.Min(averageLevel, 3);
    }
    
    void ConfirmMerge()
    {
        if (mergeSelectedParts.Count != 3) return;
        
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Confirming merge operation");
        
        PartData mergedPart = GenerateMergedPart();
        
        if (mergedPart != null)
        {
            foreach (PartData part in mergeSelectedParts)
            {
                PlayerInventory.RemovePart(part);
            }
            
            PlayerInventory.AddPart(mergedPart);
            
            if (enableDebugLogging)
                Debug.Log($"[MinionAssemblyManager] Successfully merged parts into {mergedPart.partName}");
        }
        
        mergeSelectedParts.Clear();
        mergeSlotType = null;
        mergeRarity = null;
        
        if (mergePreviewPopup != null)
        {
            mergePreviewPopup.SetActive(false);
        }
        
        RefreshInventoryDisplay();
        
        UpdateMinionDisplay();
        
        if (isMergeMode)
        {
            UpdateMergeUI();
        }
    }
    
    void CancelMerge()
    {
        if (enableDebugLogging)
            Debug.Log("[MinionAssemblyManager] Cancelling merge operation");
        
        mergeSelectedParts.Clear();
        mergeSlotType = null;
        mergeRarity = null;

        foreach (var kvp in partToUI)
        {
            UpdatePartVisual(kvp.Key, false);
        }
        
        if (mergePreviewPopup != null)
        {
            mergePreviewPopup.SetActive(false);
        }
        
        UpdateMergeUI();
    }
    
    void InitializeWavePreview()
    {
        if (wavePreviewManager == null)
        {
            wavePreviewManager = FindAnyObjectByType<WavePreviewManager>();
        }
        
        if (wavePreviewManager != null)
        {
            wavePreviewManager.UpdateWavePreview();
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Wave preview initialized");
        }
        else
        {
            if (enableDebugLogging)
                Debug.LogWarning("[MinionAssemblyManager] WavePreviewManager not found in scene");
        }
    }
    
    void UpdateWavePreview()
    {
        if (wavePreviewManager != null)
        {
            wavePreviewManager.RefreshPreview();
            if (enableDebugLogging)
                Debug.Log("[MinionAssemblyManager] Wave preview updated");
        }
    }
}

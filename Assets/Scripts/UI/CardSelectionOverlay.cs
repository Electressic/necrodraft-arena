using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CardSelectionOverlay : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup overlayCanvasGroup;
    public Button[] cardButtons = new Button[3];
    public TMPro.TextMeshProUGUI[] cardTexts = new TMPro.TextMeshProUGUI[3];
    public Button continueButton;
    public Button hideButton;  // Button to temporarily hide overlay
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI instructionsText;
    public Image backgroundDim;
    
    [Header("Card Background Sprites")]
    public Sprite commonCardSprite;
    public Sprite rareCardSprite;
    public Sprite epicCardSprite;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    public AnimationCurve fadeAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    

    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Events
    public System.Action<PartData> OnPartSelected;
    public System.Action OnOverlayClosed;
    public System.Action OnOverlayHidden;  // When temporarily hidden
    public System.Action OnOverlayResumed; // When shown again after hiding
    
    // Private fields
    private PartData[] currentCards = new PartData[3];
    private PartData selectedPart;
    private int selectedCardIndex = -1;
    private bool isAnimating = false;
    private bool isTemporarilyHidden = false;
    private bool hasSelectedPartThisSession = false;
    
    void Awake()
    {
        // Ensure overlay starts hidden
        if (overlayCanvasGroup == null)
            overlayCanvasGroup = GetComponent<CanvasGroup>();
            
        // Start hidden but keep GameObject active
        InitializeAsHidden();
        SetupButtonListeners();
        LoadCardBackgroundSprites();
        SetupBackgroundDim();
    }
    
    void LoadCardBackgroundSprites()
    {
        // Load card background sprites from Resources
        if (commonCardSprite == null)
            commonCardSprite = Resources.Load<Sprite>("UI_Card_Background");
        if (rareCardSprite == null)
            rareCardSprite = Resources.Load<Sprite>("UI_Card_Background_Rare");
        if (epicCardSprite == null)
            epicCardSprite = Resources.Load<Sprite>("UI_Card_Background_Epic");
            
        if (enableDebugLogging)
        {
            Debug.Log($"[CardSelectionOverlay] Loaded card backgrounds - Common: {(commonCardSprite != null ? "✓" : "✗")}, Rare: {(rareCardSprite != null ? "✓" : "✗")}, Epic: {(epicCardSprite != null ? "✓" : "✗")}");
        }
    }
    
    void SetupBackgroundDim()
    {
        if (backgroundDim != null)
        {
            // Set background dim to dark semi-transparent
            backgroundDim.color = new Color(0f, 0f, 0f, 0.7f); // Dark background instead of white
        }
    }
    
    void InitializeAsHidden()
    {
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
        }
        // Keep GameObject active - just make it invisible
    }
    
    void SetupButtonListeners()
    {
        // Set up card button listeners
        for (int i = 0; i < cardButtons.Length; i++)
        {
            int cardIndex = i; // Capture for closure
            cardButtons[i].onClick.AddListener(() => SelectCard(cardIndex));
        }
        
        // Set up action buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ConfirmSelection);
        if (hideButton != null)
            hideButton.onClick.AddListener(TemporarilyHideOverlay);
    }
    
    public void ShowOverlay(string title = "Choose a Body Part", string instructions = "Select one card to add to your collection")
    {
        if (isAnimating) return;
        
        // Check if resuming from temporary hide
        if (isTemporarilyHidden)
        {
            ResumeOverlay();
            return;
        }
        
        // Update UI text
        if (titleText != null)
            titleText.text = title;
        if (instructionsText != null)
            instructionsText.text = instructions;
            
        // Draw new cards (only if starting fresh)
        DrawCards();
        
        // Reset selection state
        selectedPart = null;
        selectedCardIndex = -1;
        hasSelectedPartThisSession = false;
        UpdateUIState();
        
        // Show with animation
        StartCoroutine(FadeInOverlay());
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionOverlay] Showing overlay");
    }
    
    // New method for debug testing - shows overlay with specific cards
    public void ShowOverlayWithTestCards(PartData[] testCards, string title = "Debug Test", string instructions = "Select any card")
    {
        if (isAnimating) return;
        
        // Check if resuming from temporary hide
        if (isTemporarilyHidden)
        {
            ResumeOverlay();
            return;
        }
        
        // Update UI text
        if (titleText != null)
            titleText.text = title;
        if (instructionsText != null)
            instructionsText.text = instructions;
            
        // Set specific test cards instead of drawing random ones
        SetTestCards(testCards);
        
        // Reset selection state
        selectedPart = null;
        selectedCardIndex = -1;
        hasSelectedPartThisSession = false;
        UpdateUIState();
        
        // Show with animation
        StartCoroutine(FadeInOverlay());
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionOverlay] Showing overlay with test cards: {string.Join(", ", testCards.Select(p => p != null ? $"{p.partName} ({p.rarity})" : "null"))}");
    }
    
    // Helper method to set specific cards and update display
    void SetTestCards(PartData[] testCards)
    {
        // Set the cards
        for (int i = 0; i < currentCards.Length; i++)
        {
            currentCards[i] = i < testCards.Length ? testCards[i] : null;
        }
        
        // Update the display for all cards
        for (int i = 0; i < currentCards.Length; i++)
        {
            UpdateCardDisplay(i);
        }
    }
    
    public void HideOverlay()
    {
        if (isAnimating) return;
        
        // This is a permanent close, reset temporary hide state
        isTemporarilyHidden = false;
        StartCoroutine(FadeOutOverlay());
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionOverlay] Hiding overlay");
    }
    
    public void TemporarilyHideOverlay()
    {
        if (isAnimating) return;
        
        isTemporarilyHidden = true;
        StartCoroutine(FadeOutOverlay(isTemporary: true));
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionOverlay] Temporarily hiding overlay");
    }
    
    public void ResumeOverlay()
    {
        if (isAnimating) return;
        
        isTemporarilyHidden = false;
        StartCoroutine(FadeInOverlay());
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionOverlay] Resuming overlay");
        
        OnOverlayResumed?.Invoke();
    }
    
    IEnumerator FadeInOverlay()
    {
        isAnimating = true;
        SetOverlayVisible(true, immediate: true);
        
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            float alpha = fadeAnimationCurve.Evaluate(progress);
            
            overlayCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        overlayCanvasGroup.alpha = 1f;
        isAnimating = false;
    }
    
    IEnumerator FadeOutOverlay(bool isTemporary = false)
    {
        isAnimating = true;
        
        float timer = 0f;
        float startAlpha = overlayCanvasGroup.alpha;
        
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            float alpha = startAlpha * (1f - fadeAnimationCurve.Evaluate(progress));
            
            overlayCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.interactable = false;
        overlayCanvasGroup.blocksRaycasts = false;
        
        isAnimating = false;
        
        if (isTemporary)
        {
            OnOverlayHidden?.Invoke();
        }
        else
        {
            OnOverlayClosed?.Invoke();
        }
    }
    
    void SetOverlayVisible(bool visible, bool immediate = false)
    {
        if (overlayCanvasGroup == null) return;
        
        if (visible)
        {
            // Set up for showing
            overlayCanvasGroup.alpha = immediate ? 1f : 0f;
            overlayCanvasGroup.interactable = immediate;
            overlayCanvasGroup.blocksRaycasts = immediate;
        }
        else
        {
            // Set up for hiding
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
        }
        // Keep GameObject always active - use CanvasGroup for visibility
    }
    
    void DrawCards()
    {
        // Generate dynamic parts based on current wave and player class
        int currentWave = GetCurrentWave();
        NecromancerClass playerClass = GetCurrentPlayerClass();
        
        // Generate 3 unique random parts using dynamic system
        List<PartData> generatedParts = DynamicPartGenerator.GenerateCardSelection(currentWave, playerClass, 3);
        
        if (generatedParts.Count < 3)
        {
            Debug.LogError("[CardSelectionOverlay] Failed to generate enough parts!");
            return;
        }
        
        // Set the generated parts as current cards
        for (int i = 0; i < 3; i++)
        {
            currentCards[i] = i < generatedParts.Count ? generatedParts[i] : null;
            UpdateCardDisplay(i);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"[CardSelectionOverlay] Generated cards for wave {currentWave}: {currentCards[0]?.partName}, {currentCards[1]?.partName}, {currentCards[2]?.partName}");
        }
    }
    
    // Helper method to get current wave
    int GetCurrentWave()
    {
        return GameData.GetCurrentWave();
    }
    
    // Helper method to get current player class
    NecromancerClass GetCurrentPlayerClass()
    {
        return GameData.GetSelectedClass();
    }
    
    void UpdateCardDisplay(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardTexts.Length || currentCards[cardIndex] == null)
            return;
            
        PartData part = currentCards[cardIndex];
        
        // Set the card background sprite directly on the button's Image component
        Image cardImage = cardButtons[cardIndex].GetComponent<Image>();
        if (cardImage != null)
        {
            Sprite spriteToUse = GetSpriteForRarity(part.rarity);
            if (spriteToUse != null)
            {
                cardImage.sprite = spriteToUse;
            }
        }
        
        // Get rarity color for highlighting
        Color rarityColor = part.GetRarityColor();
        
        // Use the new stats system instead of legacy hpBonus/attackBonus
        string statsText = "";
        if (part.stats.HasAnyStats())
        {
            // Build stats display with better formatting
            List<string> statsList = new List<string>();
            if (part.stats.health > 0) statsList.Add($"<color=green>HP:</color> +{part.stats.health}");
            if (part.stats.attack > 0) statsList.Add($"<color=red>ATK:</color> +{part.stats.attack}");
            if (part.stats.defense > 0) statsList.Add($"<color=orange>DEF:</color> +{part.stats.defense}");
            if (part.stats.attackSpeed > 0) statsList.Add($"<color=yellow>AS:</color> +{(part.stats.attackSpeed*100):F0}%");
            if (part.stats.critChance > 0) statsList.Add($"<color=yellow>CRIT:</color> +{(part.stats.critChance*100):F0}%");
            if (part.stats.critDamage > 0) statsList.Add($"<color=yellow>CD:</color> +{(part.stats.critDamage*100):F0}%");
            if (part.stats.moveSpeed > 0) statsList.Add($"<color=#00FFFF>SPD:</color> +{(part.stats.moveSpeed*100):F0}%");
            if (part.stats.range > 0) statsList.Add($"<color=#FF00FF>RNG:</color> +{(part.stats.range*100):F0}%");
            
            // Format based on number of stats for better readability
            if (statsList.Count <= 2)
            {
                // Few stats: keep on one line
                statsText = string.Join("  ", statsList);
            }
            else if (statsList.Count <= 4)
            {
                // Medium stats: split into two lines
                int half = statsList.Count / 2;
                string firstLine = string.Join("  ", statsList.GetRange(0, half));
                string secondLine = string.Join("  ", statsList.GetRange(half, statsList.Count - half));
                statsText = $"{firstLine}\n{secondLine}";
            }
            else
            {
                // Many stats: use multiple lines
                statsText = string.Join("  ", statsList.GetRange(0, 3)) + "\n" + 
                           string.Join("  ", statsList.GetRange(3, statsList.Count - 3));
            }
        }
        else
        {
            // Fallback to legacy system if new stats are empty
            statsText = $"<color=green>HP:</color> +{part.GetHPBonus()}  <color=red>ATK:</color> +{part.GetAttackBonus()}";
        }
        
        // Format card text display with rarity and abilities
        string cardText = $"<size=18><b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{part.partName}</color></b></size>\n" +
                         $"<size=12><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()}]</color></size>\n\n" +
                         $"<color=yellow>Type:</color> {part.type}\n\n" +
                         $"{statsText}\n\n";
        
        // Add special ability if it exists (smaller text)
        string abilityDesc = part.GetAbilityDescription();
        if (!string.IsNullOrEmpty(abilityDesc))
        {
            cardText += $"<size=10><color=orange><b>{abilityDesc}</b></color></size>\n\n";
        }
        
        cardText += $"<size=10><i>{part.description}</i></size>";
        
        cardTexts[cardIndex].text = cardText;
        
        // Reset button colors to be more subtle since we're using background sprites
        ColorBlock colors = cardButtons[cardIndex].colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cardButtons[cardIndex].colors = colors;
    }
    
    Sprite GetSpriteForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return commonCardSprite;
            case PartData.PartRarity.Rare: return rareCardSprite;
            case PartData.PartRarity.Epic: return epicCardSprite;
            default: return commonCardSprite;
        }
    }
    
    public void SelectCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= currentCards.Length || currentCards[cardIndex] == null)
            return;
            
        selectedPart = currentCards[cardIndex];
        selectedCardIndex = cardIndex;
        
        // Visual feedback - highlight selected card
        UpdateCardSelectionVisuals();
        UpdateUIState();
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionOverlay] Selected: {selectedPart.partName}");
    }
    
    void UpdateCardSelectionVisuals()
    {
        for (int i = 0; i < cardButtons.Length; i++)
        {
            ColorBlock colors = cardButtons[i].colors;
            
            if (i == selectedCardIndex)
            {
                // Highlight selected card with bright green border effect
                colors.normalColor = new Color(0.8f, 1f, 0.8f, 1f);
                colors.highlightedColor = new Color(0.9f, 1f, 0.9f, 1f);
                colors.pressedColor = new Color(0.7f, 0.9f, 0.7f, 1f);
                colors.selectedColor = new Color(0.8f, 1f, 0.8f, 1f);
            }
            else
            {
                // Reset to normal colors for non-selected cards
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                colors.selectedColor = Color.white;
            }
            
            cardButtons[i].colors = colors;
        }
    }
    
    void UpdateUIState()
    {
        // Show/hide continue button based on selection
        if (continueButton != null)
            continueButton.gameObject.SetActive(selectedPart != null);
            
        // Update hide button text based on state
        if (hideButton != null)
        {
            TMPro.TextMeshProUGUI hideButtonText = hideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (hideButtonText != null)
                hideButtonText.text = "Check Minions";
        }
    }
    
    public void ConfirmSelection()
    {
        if (selectedPart == null)
        {
            Debug.LogWarning("[CardSelectionOverlay] No part selected!");
            return;
        }
        
        hasSelectedPartThisSession = true;
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionOverlay] Confirmed selection: {selectedPart.partName}");
        
        OnPartSelected?.Invoke(selectedPart);
        HideOverlay();
    }
    
    // Public method to check if a part has been selected this session
    public bool HasSelectedPartThisSession()
    {
        return hasSelectedPartThisSession;
    }
    
    // Public method to reset the session state (call this when starting new wave/session)
    public void ResetSessionState()
    {
        hasSelectedPartThisSession = false;
        isTemporarilyHidden = false;
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionOverlay] Session state reset");
    }
    
    List<PartData> GetAllAvailableParts()
    {
        List<PartData> parts = new List<PartData>();
        
        // Load all PartData assets from the Resources folder
        // This loads from Assets/Resources/ folder
        PartData[] resourceParts = Resources.LoadAll<PartData>("");
        parts.AddRange(resourceParts);
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionOverlay] Loaded {parts.Count} available parts dynamically from Resources folder");
        
        return parts;
    }
} 
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
    public Image[] cardIcons = new Image[3]; 
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
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionOverlay] 🎯 ShowOverlay() called! Title: '{title}', IsAnimating: {isAnimating}, IsHidden: {isTemporarilyHidden}");
        
        if (isAnimating) 
        {
            if (enableDebugLogging)
                Debug.Log("[CardSelectionOverlay] ✗ Cannot show - currently animating");
            return;
        }
        
        // Check if resuming from temporary hide
        if (isTemporarilyHidden)
        {
            if (enableDebugLogging)
                Debug.Log("[CardSelectionOverlay] ↗️ Resuming from temporary hide");
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
            Debug.Log("[CardSelectionOverlay] ✓ Starting fade-in animation");
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

        // Set the card background sprite
        Image cardImage = cardButtons[cardIndex].GetComponent<Image>();
        if (cardImage != null)
        {
            Sprite spriteToUse = GetSpriteForRarity(part.rarity);
            if (spriteToUse != null)
            {
                cardImage.sprite = spriteToUse;
            }
        }

        // Set the card icon if available
        if (cardIcons != null && cardIndex < cardIcons.Length && cardIcons[cardIndex] != null)
        {
            if (part.icon != null)
            {
                cardIcons[cardIndex].sprite = part.icon;
                cardIcons[cardIndex].color = Color.white;
            }
            else
            {
                // Use a default icon or hide if no icon available
                cardIcons[cardIndex].color = Color.clear;
            }
        }

        // Get rarity color for highlighting
        Color rarityColor = part.GetRarityColor();

        // Build comprehensive stats display using new stats system
        string statsText = "";
        if (part.stats.HasAnyStats())
        {
            List<string> statsList = new List<string>();
            if (part.stats.health > 0) statsList.Add($"<color=green>HP: +{part.stats.health*100:F0}%</color>");
            if (part.stats.attack > 0) statsList.Add($"<color=red>ATK: +{part.stats.attack*100:F0}%</color>");
            if (part.stats.defense > 0) statsList.Add($"<color=orange>DEF: +{part.stats.defense*100:F0}%</color>");
            if (part.stats.attackSpeed > 0) statsList.Add($"<color=yellow>AS: +{(part.stats.attackSpeed*100):F0}%</color>");
            if (part.stats.critChance > 0) statsList.Add($"<color=yellow>CRIT: +{(part.stats.critChance*100):F0}%</color>");
            if (part.stats.moveSpeed > 0) statsList.Add($"<color=#00FFFF>SPD: +{(part.stats.moveSpeed*100):F0}%</color>");

            // Format stats in 2 columns for better readability
            if (statsList.Count <= 3)
            {
                statsText = string.Join("\n", statsList);
            }
            else
            {
                int half = (statsList.Count + 1) / 2;
                string leftColumn = string.Join("\n", statsList.GetRange(0, half));
                string rightColumn = string.Join("\n", statsList.GetRange(half, statsList.Count - half));
                statsText = $"{leftColumn}     {rightColumn}";
            }
        }
        else
        {
            // Fallback to legacy system
            statsText = $"<color=green>HP: +{part.GetHPBonus()}</color>\n<color=red>ATK: +{part.GetAttackBonus()}</color>";
        }

        // Create cleaner card text layout with sections
        string cardText = $"<b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{part.partName}</color></b>\n" +
                         $"<size=10><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()} {part.type}]</color></size>\n\n" +
                         $"{statsText}\n";

        // Add special ability if it exists
        string abilityDesc = part.GetAbilityDescription();
        if (!string.IsNullOrEmpty(abilityDesc))
        {
            cardText += $"\n<size=9><color=orange><b>{abilityDesc}</b></color></size>";
        }

        cardTexts[cardIndex].text = cardText;

        // Reset button colors
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
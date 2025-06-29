using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
        // Get all available parts from Resources folder dynamically
        List<PartData> allParts = GetAllAvailableParts();
        
        if (allParts.Count < 3)
        {
            Debug.LogError("[CardSelectionOverlay] Not enough parts available! Need at least 3.");
            return;
        }
        
        // Create a shuffled copy of all parts
        List<PartData> shuffledParts = new List<PartData>(allParts);
        
        // Draw 3 unique random parts
        for (int i = 0; i < 3; i++)
        {
            if (shuffledParts.Count > 0)
            {
                int randomIndex = Random.Range(0, shuffledParts.Count);
                currentCards[i] = shuffledParts[randomIndex];
                shuffledParts.RemoveAt(randomIndex); // Prevent duplicates
                
                UpdateCardDisplay(i);
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"[CardSelectionOverlay] Drew cards: {currentCards[0]?.partName}, {currentCards[1]?.partName}, {currentCards[2]?.partName}");
        }
    }
    
    void UpdateCardDisplay(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardTexts.Length || currentCards[cardIndex] == null)
            return;
            
        PartData part = currentCards[cardIndex];
        
        // Get rarity color for highlighting
        Color rarityColor = part.GetRarityColor();
        
        // Format card text display with rarity and abilities
        string cardText = $"<size=18><b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{part.partName}</color></b></size>\n" +
                         $"<size=12><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>[{part.GetRarityText()}]</color></size>\n\n" +
                         $"<color=yellow>Type:</color> {part.type}\n\n" +
                         $"<color=green>HP:</color> +{part.hpBonus}  " +
                         $"<color=red>ATK:</color> +{part.attackBonus}\n\n";
        
        // Add special ability if it exists (smaller text)
        string abilityDesc = part.GetAbilityDescription();
        if (!string.IsNullOrEmpty(abilityDesc))
        {
            cardText += $"<size=10><color=orange><b>{abilityDesc}</b></color></size>\n\n";
        }
        
        cardText += $"<size=10><i>{part.description}</i></size>";
        
        cardTexts[cardIndex].text = cardText;
        
        // Set initial rarity background color
        ColorBlock colors = cardButtons[cardIndex].colors;
        Color buttonColor = rarityColor;
        buttonColor.a = 0.3f; // Subtle background
        
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonColor * 1.2f;
        colors.pressedColor = buttonColor * 0.8f;
        
        cardButtons[cardIndex].colors = colors;
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
                colors.normalColor = Color.green;
                colors.highlightedColor = new Color(0.8f, 1f, 0.8f);
                colors.pressedColor = new Color(0.7f, 0.9f, 0.7f);
                colors.selectedColor = Color.green;
            }
            else if (currentCards[i] != null)
            {
                // Preserve rarity colors for non-selected cards
                Color rarityColor = currentCards[i].GetRarityColor();
                rarityColor.a = 0.3f; // Keep subtle background
                
                colors.normalColor = rarityColor;
                colors.highlightedColor = rarityColor * 1.2f;
                colors.pressedColor = rarityColor * 0.8f;
                colors.selectedColor = rarityColor;
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
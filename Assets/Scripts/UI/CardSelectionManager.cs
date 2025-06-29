using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public Button[] cardButtons = new Button[3];
    public TMPro.TextMeshProUGUI[] cardTexts = new TMPro.TextMeshProUGUI[3];
    public Button continueButton;
    public TMPro.TextMeshProUGUI titleText;
    
    [Header("Part Data")]
    public List<PartData> allParts = new List<PartData>();
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Private fields
    private PartData[] currentCards = new PartData[3];
    private PartData selectedPart;
    private int selectedCardIndex = -1;
    
    void Start()
    {
        InitializeUI();
        DrawCards();
        SetupButtonListeners();
        
        if (enableDebugLogging)
            Debug.Log("[CardSelectionManager] Card selection scene initialized");
    }
    
    void InitializeUI()
    {
        // Hide continue button initially
        continueButton.gameObject.SetActive(false);
        
        // Update title for prototype (repeating Wave 1)
        if (titleText != null)
            titleText.text = "Choose a Body Part - Prepare for Battle";
    }
    
    void DrawCards()
    {
        if (allParts.Count < 3)
        {
            Debug.LogError("[CardSelectionManager] Not enough parts in allParts list! Need at least 3.");
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
            Debug.Log($"[CardSelectionManager] Drew cards: {currentCards[0]?.partName}, {currentCards[1]?.partName}, {currentCards[2]?.partName}");
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
    
    void SetupButtonListeners()
    {
        // Set up card button listeners
        for (int i = 0; i < cardButtons.Length; i++)
        {
            int cardIndex = i; // Capture for closure
            cardButtons[i].onClick.AddListener(() => SelectCard(cardIndex));
        }
        
        // Set up continue button
        continueButton.onClick.AddListener(ContinueToGameplay);
    }
    
    public void SelectCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= currentCards.Length || currentCards[cardIndex] == null)
            return;
            
        selectedPart = currentCards[cardIndex];
        selectedCardIndex = cardIndex;
        
        // Visual feedback - highlight selected card
        UpdateCardSelectionVisuals();
        
        // Show continue button
        continueButton.gameObject.SetActive(true);
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionManager] Selected: {selectedPart.partName}");
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
            else
            {
                // Default for empty cards
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
                colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
                colors.selectedColor = Color.white;
            }
            
            cardButtons[i].colors = colors;
        }
    }
    
    public void ContinueToGameplay()
    {
        if (selectedPart == null)
        {
            Debug.LogWarning("[CardSelectionManager] No part selected!");
            return;
        }
        
        // Store the selected part in player inventory
        PlayerInventory.AddPart(selectedPart);
        
        if (enableDebugLogging)
            Debug.Log($"[CardSelectionManager] Added {selectedPart.partName} to inventory. Proceeding to minion assembly.");
        
        SceneManager.LoadScene("MinionAssembly");
    }
    
    // Helper method to get current wave (can be expanded later)
    int GetCurrentWave()
    {
        // For now, return a simple wave counter
        // Later this can be retrieved from a GameManager or save system
        return PlayerInventory.GetPartCount() + 1;
    }
}
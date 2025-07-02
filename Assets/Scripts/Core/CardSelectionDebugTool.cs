using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class CardSelectionDebugTool : MonoBehaviour
{
    [Header("Debug UI")]
    public Button testRandomButton;
    public Button testCommonOnlyButton;
    public Button testRareOnlyButton;
    public Button testEpicOnlyButton;
    public Button testMixedButton;
    public Button regenerateEpicButton;
    
    [Header("References")]
    public CardSelectionOverlay cardSelectionOverlay;
    
    [Header("Debug Settings")]
    public bool enableDebugLogging = true;
    public Key quickTestKey = Key.T; // Press T to quickly test random
    
    // Private fields
    private List<PartData> allParts;
    private List<PartData> commonParts;
    private List<PartData> rareParts;
    private List<PartData> epicParts;
    
    void Start()
    {
        LoadAllParts();
        SetupDebugButtons();
        
        if (enableDebugLogging)
        {
            Debug.Log($"[CardSelectionDebugTool] Initialized - Parts: {allParts.Count} total (Common: {commonParts.Count}, Rare: {rareParts.Count}, Epic: {epicParts.Count})");
            Debug.Log($"[CardSelectionDebugTool] CardSelectionOverlay reference: {(cardSelectionOverlay != null ? "✓ Connected" : "✗ Missing")}");
            Debug.Log($"[CardSelectionDebugTool] Quick test key: T");
        }
    }
    
    void Update()
    {
        // Quick test with keyboard using new Input System
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (enableDebugLogging)
                Debug.Log($"[CardSelectionDebugTool] T key pressed - triggering random test");
            TestRandomCards();
        }
    }
    
    void LoadAllParts()
    {
        // Generate sample parts using dynamic system for testing
        allParts = new List<PartData>();
        commonParts = new List<PartData>();
        rareParts = new List<PartData>();
        epicParts = new List<PartData>();
        
        // Generate sample parts of each rarity for testing with proper wave matching
        for (int i = 0; i < 5; i++) // 5 of each rarity for testing variety
        {
            // Common parts - use wave 1-3 for proper common generation
            PartData commonPart = DynamicPartGenerator.GenerateRandomPart(Random.Range(1, 4)); 
            commonPart.rarity = PartData.PartRarity.Common;
            // Regenerate stats to match common rarity properly
            commonPart.stats = PartStatsGenerator.Generate(commonPart.theme, PartData.PartRarity.Common, commonPart.type);
            commonPart.description = GenerateEnhancedDescription(commonPart);
            commonParts.Add(commonPart);
            allParts.Add(commonPart);
            
            // Rare parts - use wave 8-12 for proper rare generation
            PartData rarePart = DynamicPartGenerator.GenerateRandomPart(Random.Range(8, 13)); 
            rarePart.rarity = PartData.PartRarity.Rare;
            // Regenerate stats to match rare rarity properly
            rarePart.stats = PartStatsGenerator.Generate(rarePart.theme, PartData.PartRarity.Rare, rarePart.type);
            rarePart.description = GenerateEnhancedDescription(rarePart);
            rareParts.Add(rarePart);
            allParts.Add(rarePart);
            
            // Epic parts - use wave 15+ for proper epic generation
            PartData epicPart = DynamicPartGenerator.GenerateRandomPart(Random.Range(15, 20)); 
            epicPart.rarity = PartData.PartRarity.Epic;
            // Regenerate stats to match epic rarity properly
            epicPart.stats = PartStatsGenerator.Generate(epicPart.theme, PartData.PartRarity.Epic, epicPart.type);
            epicPart.description = GenerateEnhancedDescription(epicPart);
            epicParts.Add(epicPart);
            allParts.Add(epicPart);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("[CardSelectionDebugTool] Generated dynamic test parts with proper stats for debugging");
            
            // Log some epic part examples
            var epicExample = epicParts.FirstOrDefault();
            if (epicExample != null)
            {
                Debug.Log($"[CardSelectionDebugTool] Epic example: {epicExample.partName} - HP: {epicExample.stats.health*100:F0}%, ATK: {epicExample.stats.attack*100:F0}%");
            }
        }
    }
    
    void SetupDebugButtons()
    {
        if (testRandomButton != null)
            testRandomButton.onClick.AddListener(TestRandomCards);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] TestRandomButton reference is missing!");
            
        if (testCommonOnlyButton != null)
            testCommonOnlyButton.onClick.AddListener(TestCommonOnly);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] TestCommonOnlyButton reference is missing!");
            
        if (testRareOnlyButton != null)
            testRareOnlyButton.onClick.AddListener(TestRareOnly);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] TestRareOnlyButton reference is missing!");
            
        if (testEpicOnlyButton != null)
            testEpicOnlyButton.onClick.AddListener(TestEpicOnly);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] TestEpicOnlyButton reference is missing!");
            
        if (testMixedButton != null)
            testMixedButton.onClick.AddListener(TestMixed);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] TestMixedButton reference is missing!");
            
        if (regenerateEpicButton != null)
            regenerateEpicButton.onClick.AddListener(RegenerateEpicParts);
        else if (enableDebugLogging)
            Debug.LogWarning("[CardSelectionDebugTool] RegenerateEpicButton reference is missing!");
    }
    
    public void TestRandomCards()
    {
        if (enableDebugLogging)
            Debug.Log("[CardSelectionDebugTool] TestRandomCards called");
            
        if (allParts.Count < 3)
        {
            Debug.LogError("[CardSelectionDebugTool] Not enough parts to test!");
            return;
        }
        
        // Create a shuffled copy
        List<PartData> shuffled = new List<PartData>(allParts);
        for (int i = 0; i < shuffled.Count; i++)
        {
            var temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        // Get first 3
        List<PartData> testCards = shuffled.Take(3).ToList();
        ShowTestCards(testCards, "Testing Random Parts");
    }
    
    public void TestCommonOnly()
    {
        if (commonParts.Count < 3)
        {
            Debug.LogWarning("[CardSelectionDebugTool] Not enough common parts! Using available ones.");
            ShowTestCards(GetRandomParts(commonParts, 3), "Testing Common Parts Only");
        }
        else
        {
            ShowTestCards(GetRandomParts(commonParts, 3), "Testing Common Parts Only");
        }
    }
    
    public void TestRareOnly()
    {
        if (rareParts.Count < 3)
        {
            Debug.LogWarning("[CardSelectionDebugTool] Not enough rare parts! Using available ones.");
            ShowTestCards(GetRandomParts(rareParts, 3), "Testing Rare Parts Only");
        }
        else
        {
            ShowTestCards(GetRandomParts(rareParts, 3), "Testing Rare Parts Only");
        }
    }
    
    public void TestEpicOnly()
    {
        if (epicParts.Count < 3)
        {
            Debug.LogWarning("[CardSelectionDebugTool] Not enough epic parts! Using available ones.");
            ShowTestCards(GetRandomParts(epicParts, 3), "Testing Epic Parts Only");
        }
        else
        {
            ShowTestCards(GetRandomParts(epicParts, 3), "Testing Epic Parts Only");
        }
    }
    
    public void TestMixed()
    {
        // Force one of each rarity if possible
        List<PartData> mixedCards = new List<PartData>();
        
        if (commonParts.Count > 0)
            mixedCards.Add(commonParts[Random.Range(0, commonParts.Count)]);
        if (rareParts.Count > 0)
            mixedCards.Add(rareParts[Random.Range(0, rareParts.Count)]);
        if (epicParts.Count > 0)
            mixedCards.Add(epicParts[Random.Range(0, epicParts.Count)]);
            
        // Fill remaining slots if needed
        while (mixedCards.Count < 3 && allParts.Count > 0)
        {
            PartData randomPart = allParts[Random.Range(0, allParts.Count)];
            if (!mixedCards.Contains(randomPart))
                mixedCards.Add(randomPart);
        }
        
        ShowTestCards(mixedCards, "Testing Mixed Rarities (C/R/E)");
    }
    
    public void RegenerateEpicParts()
    {
        if (enableDebugLogging)
            Debug.Log("[CardSelectionDebugTool] Regenerating Epic parts with new budget system");
        
        // Generate fresh Epic parts using the new budget system
        List<PartData> newEpicParts = new List<PartData>();
        
        for (int i = 0; i < 3; i++)
        {
            // Generate new Epic part with wave 18-19 (high epic waves)
            PartData newEpicPart = DynamicPartGenerator.GenerateRandomPart(Random.Range(17, 20));
            newEpicPart.rarity = PartData.PartRarity.Epic;
            
            // Ensure proper epic stats (regenerate with correct rarity)
            newEpicPart.stats = PartStatsGenerator.Generate(newEpicPart.theme, PartData.PartRarity.Epic, newEpicPart.type);
            
            // Add enhanced description
            newEpicPart.description = GenerateEnhancedDescription(newEpicPart);
            
            newEpicParts.Add(newEpicPart);
            
            // Log the stats for debugging
            if (enableDebugLogging)
            {
                Debug.Log($"[CardSelectionDebugTool] Generated Epic Part {i+1}: {newEpicPart.partName}");
                Debug.Log($"  Stats: {newEpicPart.stats.GetStatsText()}");
                Debug.Log($"  Stat Count: {CountNonZeroStats(newEpicPart.stats)}");
                Debug.Log($"  Has ability: {newEpicPart.specialAbility != PartData.SpecialAbility.None}");
            }
        }
        
        ShowTestCards(newEpicParts, "NEW Epic Parts (Budget System)");
    }
    
    private int CountNonZeroStats(PartData.PartStats stats)
    {
        int count = 0;
        if (stats.health > 0) count++;
        if (stats.attack > 0) count++;
        if (stats.defense > 0) count++;
        if (stats.attackSpeed > 0) count++;
        if (stats.critChance > 0) count++;
        if (stats.critDamage > 0) count++;
        if (stats.moveSpeed > 0) count++;
        if (stats.range > 0) count++;
        return count;
    }
    
    List<PartData> GetRandomParts(List<PartData> sourceList, int count)
    {
        if (sourceList.Count == 0) return new List<PartData>();
        
        List<PartData> result = new List<PartData>();
        List<PartData> shuffled = new List<PartData>(sourceList);
        
        for (int i = 0; i < count && i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(0, shuffled.Count);
            result.Add(shuffled[randomIndex]);
            shuffled.RemoveAt(randomIndex);
        }
        
        return result;
    }
    
    void ShowTestCards(List<PartData> testCards, string title)
    {
        if (cardSelectionOverlay == null)
        {
            Debug.LogError("[CardSelectionDebugTool] CardSelectionOverlay reference is missing!");
            return;
        }
        
        // Convert to array and ensure we have exactly 3 cards
        PartData[] cardArray = new PartData[3];
        for (int i = 0; i < 3; i++)
        {
            cardArray[i] = i < testCards.Count ? testCards[i] : null;
        }
        
        // Use the new method that bypasses random card generation
        cardSelectionOverlay.ShowOverlayWithTestCards(cardArray, title, "Debug Test - Select any card");
        
        if (enableDebugLogging)
        {
            string cardInfo = string.Join(", ", testCards.Select(p => $"{p.partName} ({p.rarity})"));
            Debug.Log($"[CardSelectionDebugTool] Showing test cards: {cardInfo}");
        }
    }
    
    void OnGUI()
    {
        // Simple on-screen debug info
        if (enableDebugLogging)
        {
            GUI.Box(new Rect(10, 10, 300, 120), "Card Selection Debug Tool");
            GUI.Label(new Rect(20, 35, 280, 20), $"Total Parts: {allParts.Count}");
            GUI.Label(new Rect(20, 55, 280, 20), $"Common: {commonParts.Count} | Rare: {rareParts.Count} | Epic: {epicParts.Count}");
            GUI.Label(new Rect(20, 75, 280, 20), $"Press '{quickTestKey}' for quick random test");
            GUI.Label(new Rect(20, 95, 280, 20), "Use debug buttons to test specific rarities");
        }
    }
    
    private string GenerateEnhancedDescription(PartData part)
    {
        string[] themeDescriptions = part.theme switch
        {
            PartData.PartTheme.Skeleton => new[] { 
                "Crafted from ancient bones, these parts offer speed and precision.",
                "Bleached by time, these skeletal remains whisper of forgotten battles.",
                "Light as air but deadly sharp, these bones remember how to fight.",
                "Ancient calcium forged into unholy purpose.",
                "The hollow echo of death given form."
            },
            PartData.PartTheme.Zombie => new[] { 
                "Putrid flesh that refuses to die, offering resilience through decay.",
                "Rotting meat that grows stronger through corruption.", 
                "Festering tissue that regenerates from pure spite.",
                "Undead flesh that feeds on violence.",
                "Decaying matter animated by dark hunger."
            },
            PartData.PartTheme.Ghost => new[] { 
                "Ethereal essence bound between worlds, shifting between solid and spirit.",
                "Spectral matter that phases through reality itself.",
                "Ghostly remnants of powerful souls seeking purpose.",
                "Ectoplasmic energy given semi-solid form.",
                "Spirit essence that defies natural law."
            },
            _ => new[] { "A mysterious undead component of unknown origin." }
        };
        
        string rarityFlavor = part.rarity switch
        {
            PartData.PartRarity.Common => "A basic but reliable component.",
            PartData.PartRarity.Uncommon => "Enhanced through dark rituals.",
            PartData.PartRarity.Rare => "Infused with powerful necromantic energy.",
            PartData.PartRarity.Epic => "Legendary artifact of immense power.",
            _ => ""
        };
        
        string baseDesc = themeDescriptions[Random.Range(0, themeDescriptions.Length)];
        
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            return $"{baseDesc} {rarityFlavor}";
        }
        else
        {
            return $"{baseDesc} {rarityFlavor}";
        }
    }
} 
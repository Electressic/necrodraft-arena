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
        
        // Generate sample parts of each rarity for testing
        for (int i = 0; i < 5; i++) // 5 of each rarity for testing variety
        {
            PartData commonPart = DynamicPartGenerator.GenerateRandomPart(1); // Wave 1 = more common
            commonPart.rarity = PartData.PartRarity.Common; // Force rarity for testing
            commonParts.Add(commonPart);
            allParts.Add(commonPart);
            
            PartData rarePart = DynamicPartGenerator.GenerateRandomPart(3); // Wave 3 = more rare
            rarePart.rarity = PartData.PartRarity.Rare; // Force rarity for testing
            rareParts.Add(rarePart);
            allParts.Add(rarePart);
            
            PartData epicPart = DynamicPartGenerator.GenerateRandomPart(5); // Wave 5 = more epic
            epicPart.rarity = PartData.PartRarity.Epic; // Force rarity for testing  
            epicParts.Add(epicPart);
            allParts.Add(epicPart);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("[CardSelectionDebugTool] Generated dynamic test parts for debugging");
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
            // Generate new Epic part with wave 19 (forces Epic rarity)
            PartData newEpicPart = DynamicPartGenerator.GenerateRandomPart(19);
            newEpicPart.rarity = PartData.PartRarity.Epic; // Force Epic for testing
            newEpicParts.Add(newEpicPart);
            
            // Log the stats for debugging
            if (enableDebugLogging)
            {
                Debug.Log($"[CardSelectionDebugTool] Generated Epic Part {i+1}: {newEpicPart.partName}");
                Debug.Log($"  Stats: {newEpicPart.stats.GetStatsText()}");
                Debug.Log($"  Stat Count: {CountNonZeroStats(newEpicPart.stats)}");
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
} 
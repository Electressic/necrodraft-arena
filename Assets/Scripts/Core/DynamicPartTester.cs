using UnityEngine;

public class DynamicPartTester : MonoBehaviour
{
    [Header("Part Generation Testing")]
    [SerializeField] private PartData.PartTheme testTheme = PartData.PartTheme.Skeleton;
    [SerializeField] private PartData.PartRarity testRarity = PartData.PartRarity.Common;
    [SerializeField] private PartData.PartType testPartType = PartData.PartType.Head;
    [SerializeField] private PartData.SpecialAbility testAbility = PartData.SpecialAbility.CriticalStrike;
    
    [Header("Generated Part Preview")]
    [SerializeField] private PartData generatedPart;
    
    [Header("Testing Controls")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool useInspectorSettingsForBatch = true;
    
    [ContextMenu("Generate Test Part")]
    void GenerateTestPart()
    {
        // Create a new part instance (this won't save to disk)
        PartData newPart = ScriptableObject.CreateInstance<PartData>();
        
        // Set basic properties
        newPart.partName = GetGeneratedPartName();
        newPart.type = testPartType;
        newPart.theme = testTheme;
        newPart.rarity = testRarity;
        newPart.specialAbility = testAbility;
        
        // Generate dynamic stats
        newPart.GenerateRandomStats();
        
        // Store for inspection
        generatedPart = newPart;
        
        // Log results
        if (enableDebugLogging)
        {
            Debug.Log($"[DynamicPartTester] Generated Part: {newPart.GetFullDescription()}");
            LogStatsBreakdown(newPart);
        }
    }
    
    [ContextMenu("Generate 5 Test Parts")]
    void GenerateFiveTestParts()
    {
        Debug.Log("=== GENERATING 5 TEST PARTS ===");
        
        if (useInspectorSettingsForBatch)
        {
            Debug.Log($"Using Inspector Settings: {testTheme} {testRarity} {testPartType} with {testAbility}");
        }
        
        for (int i = 0; i < 5; i++)
        {
            if (!useInspectorSettingsForBatch)
            {
                // Randomize properties for variety
                testTheme = (PartData.PartTheme)Random.Range(0, 3);
                testRarity = (PartData.PartRarity)Random.Range(0, 4); // Now includes Uncommon
                testPartType = (PartData.PartType)Random.Range(0, 4);
                testAbility = (PartData.SpecialAbility)Random.Range(1, 9); // Skip None
            }
            // If useInspectorSettingsForBatch is true, it will use the inspector values
            
            GenerateTestPart();
        }
        
        Debug.Log("=== GENERATION COMPLETE ===");
    }
    
    [ContextMenu("Generate 5 Random Parts")]
    void GenerateFiveRandomParts()
    {
        Debug.Log("=== GENERATING 5 RANDOM PARTS ===");
        
        bool originalSetting = useInspectorSettingsForBatch;
        useInspectorSettingsForBatch = false; // Force randomization
        
        GenerateFiveTestParts();
        
        useInspectorSettingsForBatch = originalSetting; // Restore setting
    }
    
    [ContextMenu("Test Set Bonus Requirements")]
    void TestSetBonusRequirements()
    {
        Debug.Log("=== TESTING SET BONUS REQUIREMENTS ===");
        
        // Create a test minion
        Minion testMinion = new Minion(CreateTestMinionData());
        
        Debug.Log($"Base minion: {testMinion.GetDetailedStatsText()}");
        Debug.Log($"Set bonuses: {testMinion.GetSetBonusesSummary()}");
        
        // Test with 1 part (should NOT activate set bonus)
        PartData singlePart = CreateTestPart(PartData.SpecialAbility.CriticalStrike);
        testMinion.EquipPart(singlePart);
        Debug.Log($"After 1 CriticalStrike part: {testMinion.GetSetBonusesSummary()}");
        
        // Test with 2 parts (SHOULD activate set bonus)
        PartData secondPart = CreateTestPart(PartData.SpecialAbility.CriticalStrike);
        secondPart.type = PartData.PartType.Torso;
        testMinion.EquipPart(secondPart);
        Debug.Log($"After 2 CriticalStrike parts: {testMinion.GetSetBonusesSummary()}");
        
        Debug.Log("=== SET BONUS TEST COMPLETE ===");
    }
    
    private string GetGeneratedPartName()
    {
        string[] themeAdjectives = { "Bone", "Flesh", "Spectral" };
        string[] partNames = { "Head", "Torso", "Arms", "Legs" };
        string[] rarityPrefix = { "", "Sturdy", "Superior", "Legendary" }; // Common, Uncommon, Rare, Epic
        
        string themeAdj = themeAdjectives[(int)testTheme];
        string partName = partNames[(int)testPartType];
        string prefix = rarityPrefix[(int)testRarity];
        
        return prefix != "" ? $"{prefix} {themeAdj} {partName}" : $"{themeAdj} {partName}";
    }
    
    private void LogStatsBreakdown(PartData part)
    {
        Debug.Log($"Theme: {part.theme} | Rarity: {part.rarity} | Set Bonus: {part.specialAbility}");
        Debug.Log($"Stats: {part.stats.GetStatsText()}");
        
        // Show budget system analysis
        int statBudget = GetStatBudgetForRarity(part.rarity);
        int statCount = CountNonZeroStats(part.stats);
        int usedBudget = CalculateUsedBudget(part.stats);
        
        Debug.Log($"Budget Analysis - Target: {statBudget} points, Used: {usedBudget} points, Stats: {statCount}, Efficiency: {(float)usedBudget/statBudget:P0}");
    }
    
    private int GetStatBudgetForRarity(PartData.PartRarity rarity)
    {
        return rarity switch
        {
            PartData.PartRarity.Common => 12,
            PartData.PartRarity.Uncommon => 20,
            PartData.PartRarity.Rare => 32,
            PartData.PartRarity.Epic => 50,
            _ => 12
        };
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
    
    private int CalculateUsedBudget(PartData.PartStats stats)
    {
        // Calculate budget usage based on stat costs (matching PartStatsGenerator)
        int budget = 0;
        budget += Mathf.RoundToInt(stats.health / 0.05f * 1);    // Convert 5% per point back to points (1 point per 5% HP)
        budget += Mathf.RoundToInt(stats.attack / 0.04f * 1);    // Convert 4% per point back to points (1 point per 4% ATK)
        budget += Mathf.RoundToInt(stats.defense / 0.03f * 2);   // Convert 3% per point back to points (2 points per 3% DEF)
        budget += Mathf.RoundToInt(stats.attackSpeed * 100 * 1); // 1 point per 1%
        budget += Mathf.RoundToInt(stats.critChance * 100 * 2);  // 2 points per 1%
        budget += Mathf.RoundToInt(stats.critDamage * 100 * 1);  // 1 point per 1%
        budget += Mathf.RoundToInt(stats.moveSpeed * 100 * 1);   // 1 point per 1%
        budget += Mathf.RoundToInt(stats.range * 100 * 2);       // 2 points per 1%
        return budget;
    }
    
    private PartData CreateTestPart(PartData.SpecialAbility ability)
    {
        PartData part = ScriptableObject.CreateInstance<PartData>();
        part.partName = $"Test {ability} Part";
        part.type = PartData.PartType.Head;
        part.theme = PartData.PartTheme.Skeleton;
        part.rarity = PartData.PartRarity.Common;
        part.specialAbility = ability;
        part.GenerateRandomStats();
        return part;
    }
    
    private MinionData CreateTestMinionData()
    {
        // Create a minimal test minion data
        MinionData testData = ScriptableObject.CreateInstance<MinionData>();
        testData.minionName = "Test Minion";
        testData.baseHP = 15;
        testData.baseAttack = 8;
        return testData;
    }
    
    void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[DynamicPartTester] Ready! Right-click this component and use context menu to test generation.");
        }
    }
} 
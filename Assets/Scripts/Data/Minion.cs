using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Minion
{
    [Header("Identity")]
    public string minionName;
    public MinionData baseData;
    
    [Header("Equipment Slots")]
    public PartData headPart;
    public PartData torsoPart;
    public PartData armsPart;
    public PartData legsPart;
    
    [Header("Calculated Core Stats")]
    public int totalHP;
    public int totalAttack;
    public int totalDefense;
    
    [Header("Calculated Combat Stats")]
    public float totalAttackSpeed = 1.0f;
    public float totalCritChance = 0.0f;
    public float totalCritDamage = 1.5f; // Base 150% crit damage
    
    [Header("Calculated Movement Stats")]
    public float totalMoveSpeed = 1.0f;
    public float totalRange = 1.0f;
    
    [Header("Active Set Bonus Abilities")]
    public List<PartData.SpecialAbility> activeAbilities = new List<PartData.SpecialAbility>();
    
    [Header("Experience & Leveling")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100; // Base XP for level 2
    public int totalExperienceSpent = 0; // Track total XP for stat calculations
    
    // Constructor
    public Minion(MinionData data)
    {
        baseData = data;
        minionName = data.minionName;
        CalculateStats();
    }
    
    public void EquipPart(PartData part)
    {
        if (part == null) return;
        
        switch (part.type)
        {
            case PartData.PartType.Head:
                headPart = part;
                break;
            case PartData.PartType.Torso:
                torsoPart = part;
                break;
            case PartData.PartType.Arms:
                armsPart = part;
                break;
            case PartData.PartType.Legs:
                legsPart = part;
                break;
        }
        
        CalculateStats();
    }
    
    public void UnequipPart(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                headPart = null;
                break;
            case PartData.PartType.Torso:
                torsoPart = null;
                break;
            case PartData.PartType.Arms:
                armsPart = null;
                break;
            case PartData.PartType.Legs:
                legsPart = null;
                break;
        }
        
        CalculateStats();
    }
    
    public PartData GetEquippedPart(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head: return headPart;
            case PartData.PartType.Torso: return torsoPart;
            case PartData.PartType.Arms: return armsPart;
            case PartData.PartType.Legs: return legsPart;
            default: return null;
        }
    }
    
    public void CalculateStats()
    {
        // Start with base stats and add a global survivability bonus
        totalHP = baseData.baseHP + 10; // GLOBAL BUFF: +10 Base HP
        totalAttack = baseData.baseAttack + 2; // GLOBAL BUFF: +2 Base Attack
        totalDefense = 5; // No base defense
        
        // Reset calculated stats to base values
        totalAttackSpeed = 1.0f;
        totalCritChance = 0.0f;
        totalCritDamage = 1.5f; // Base 150% crit damage
        totalMoveSpeed = 1.0f;
        totalRange = 1.0f;
        
        // Apply level bonuses first (flat bonuses)
        ApplyLevelBonuses();
        
        // Store base+level stats before part multipliers
        int baseHP = totalHP;
        int baseAttack = totalAttack;
        int baseDefense = totalDefense;
        
        // Clear and recalculate abilities
        activeAbilities.Clear();
        
        // Reset multiplier stats before part processing
        totalAttackSpeed = 1.0f;
        totalCritChance = 0.0f;
        totalCritDamage = 1.5f;
        totalMoveSpeed = 1.0f;
        totalRange = 1.0f;
        
        // Process each equipped part for multiplier bonuses
        float hpMultiplier = 1.0f;
        float attackMultiplier = 1.0f;
        float defenseMultiplier = 1.0f;
        
        ProcessPartMultipliers(headPart, ref hpMultiplier, ref attackMultiplier, ref defenseMultiplier);
        ProcessPartMultipliers(torsoPart, ref hpMultiplier, ref attackMultiplier, ref defenseMultiplier);
        ProcessPartMultipliers(armsPart, ref hpMultiplier, ref attackMultiplier, ref defenseMultiplier);
        ProcessPartMultipliers(legsPart, ref hpMultiplier, ref attackMultiplier, ref defenseMultiplier);
        
        // Apply multipliers to base+level stats
        totalHP = Mathf.RoundToInt(baseHP * hpMultiplier);
        totalAttack = Mathf.RoundToInt(baseAttack * attackMultiplier);
        totalDefense = Mathf.RoundToInt(baseDefense * defenseMultiplier);
        
        // Apply set bonus abilities based on counts (2+ parts required)
        CalculateSetBonuses();
    }
    
    private void ProcessPartMultipliers(PartData part, ref float hpMult, ref float atkMult, ref float defMult)
    {
        if (part == null) return;
        
        // Migrate legacy stats if needed
        part.MigrateLegacyStats();
        
        // Add percentage multipliers
        hpMult += part.stats.health;
        atkMult += part.stats.attack;
        defMult += part.stats.defense;
        
        // Add other multiplier-based stats
        totalAttackSpeed += part.stats.attackSpeed;
        totalCritChance += part.stats.critChance;
        totalCritDamage += part.stats.critDamage;
        totalMoveSpeed += part.stats.moveSpeed;
        totalRange += part.stats.range;
        
        // Track special abilities for set bonus calculation
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            activeAbilities.Add(part.specialAbility);
        }
    }
    
    private void CalculateSetBonuses()
    {
        // Count occurrences of each ability
        Dictionary<PartData.SpecialAbility, int> abilityCounts = new Dictionary<PartData.SpecialAbility, int>();
        
        foreach (var ability in activeAbilities)
        {
            if (ability != PartData.SpecialAbility.None)
            {
                if (!abilityCounts.ContainsKey(ability))
                    abilityCounts[ability] = 0;
                abilityCounts[ability]++;
            }
        }
        
        // Clear active abilities and rebuild with only qualifying set bonuses
        activeAbilities.Clear();
        
        // Apply set bonuses only if 2+ parts of same type
        foreach (var kvp in abilityCounts)
        {
            if (kvp.Value >= 2) // REQUIRE 2+ PARTS FOR SET BONUS
            {
                activeAbilities.Add(kvp.Key);
                ApplySetBonusEffect(kvp.Key, kvp.Value);
            }
        }
    }
    
    private void ApplySetBonusEffect(PartData.SpecialAbility ability, int count)
    {
        // Apply immediate stat modifications for set bonus abilities
        switch (ability)
        {
            case PartData.SpecialAbility.Swift:
                // Swift: +100% move speed, -25% HP (only applies if 2+ Swift parts)
                totalMoveSpeed += 1.0f; // Additional +100% move speed
                totalHP = Mathf.RoundToInt(totalHP * 0.75f); // -25% HP penalty
                break;
                
            case PartData.SpecialAbility.Berserker:
                // Berserker gets stronger with more parts
                float berserkerBonus = 0.5f + (count - 2) * 0.25f; // 50% + 25% per extra part
                // This will be applied during combat when HP < 50%
                break;
                
            // Other abilities don't modify base stats directly
            // They're handled during combat (Vampiric, Armored, etc.)
        }
    }
    
    private void ApplyLevelBonuses()
    {
        if (level <= 1) return; // No bonuses at level 1
        
        PartData.PartStats levelBonus = GetLevelBonusStats();
        
        // Add level bonuses to total stats (level bonuses are still flat, cast from float)
        totalHP += Mathf.RoundToInt(levelBonus.health);
        totalAttack += Mathf.RoundToInt(levelBonus.attack);
        totalDefense += Mathf.RoundToInt(levelBonus.defense);
        totalAttackSpeed += levelBonus.attackSpeed;
        totalCritChance += levelBonus.critChance;
        totalCritDamage += levelBonus.critDamage;
        totalMoveSpeed += levelBonus.moveSpeed;
        totalRange += levelBonus.range;
    }
    
    public bool HasSetBonus(PartData.SpecialAbility ability)
    {
        return activeAbilities.Contains(ability);
    }
    
    public int GetAbilityCount(PartData.SpecialAbility ability)
    {
        int count = 0;
        foreach (var equippedPart in GetAllEquippedParts())
        {
            if (equippedPart?.specialAbility == ability)
                count++;
        }
        return count;
    }
    
    public bool IsSlotEmpty(PartData.PartType partType)
    {
        return GetEquippedPart(partType) == null;
    }
    
    public int GetEquippedPartsCount()
    {
        int count = 0;
        if (headPart != null) count++;
        if (torsoPart != null) count++;
        if (armsPart != null) count++;
        if (legsPart != null) count++;
        return count;
    }
    
    public List<PartData> GetAllEquippedParts()
    {
        List<PartData> parts = new List<PartData>();
        if (headPart != null) parts.Add(headPart);
        if (torsoPart != null) parts.Add(torsoPart);
        if (armsPart != null) parts.Add(armsPart);
        if (legsPart != null) parts.Add(legsPart);
        return parts;
    }
    
    public string GetSetBonusesSummary()
    {
        if (activeAbilities.Count == 0) return "No set bonuses active";
        
        List<string> bonusDescriptions = new List<string>();
        
        foreach (var ability in activeAbilities)
        {
            int count = GetAbilityCount(ability);
            string tierText = count >= 4 ? " (Max)" : count == 3 ? " (II)" : " (I)";
            bonusDescriptions.Add(ability.ToString() + tierText);
        }
        
        return string.Join(", ", bonusDescriptions);
    }
    
    public string GetDetailedStatsText()
    {
        // Level and experience info first
        string result = GetExperienceText();
        if (level > 1)
        {
            PartData.PartStats levelBonus = GetLevelBonusStats();
            string bonusText = levelBonus.GetStatsText();
            if (!bonusText.Contains("No stat bonuses"))
                result += $"\nLevel Bonus: {bonusText}";
        }
        
        // Current stats
        string stats = $"\nHP: {totalHP} | ATK: {totalAttack}";
        
        if (totalDefense > 0)
            stats += $" | DEF: {totalDefense}";
            
        if (totalAttackSpeed != 1.0f)
            stats += $" | ATK Speed: {totalAttackSpeed:F1}x";
            
        if (totalCritChance > 0)
            stats += $" | Crit: {(totalCritChance*100):F0}%";
            
        if (totalCritDamage != 1.5f)
            stats += $" | Crit DMG: {(totalCritDamage*100):F0}%";
            
        if (totalMoveSpeed != 1.0f)
            stats += $" | Speed: {totalMoveSpeed:F1}x";
            
        if (totalRange != 1.0f)
            stats += $" | Range: {totalRange:F1}x";
            
        return result + stats;
    }
    
    // Backwards compatibility
    public float totalMoveSpeedMultiplier => totalMoveSpeed;
    
    public string GetAbilitiesSummary()
    {
        return GetSetBonusesSummary();
    }
    
    // ==================== EXPERIENCE & LEVELING SYSTEM ====================
    
    /// <summary>
    /// Add experience to this minion and handle level-ups
    /// </summary>
    public bool AddExperience(int xpGained)
    {
        if (xpGained <= 0) return false;
        
        experience += xpGained;
        totalExperienceSpent += xpGained;
        
        // Check for level up
        bool leveledUp = false;
        while (experience >= experienceToNextLevel && level < 20) // Level cap at 20
        {
            experience -= experienceToNextLevel;
            level++;
            leveledUp = true;
            
            // Increase XP requirement for next level (25% increase per level)
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.25f);
            
            Debug.Log($"[Minion] {minionName} leveled up to level {level}! Next level requires {experienceToNextLevel} XP.");
        }
        
        if (leveledUp)
        {
            // Recalculate stats with level bonuses
            CalculateStats();
        }
        
        return leveledUp;
    }
    
    /// <summary>
    /// Get total stat bonus from level progression
    /// </summary>
    public PartData.PartStats GetLevelBonusStats()
    {
        PartData.PartStats levelBonus = new PartData.PartStats();
        
        if (level <= 1) return levelBonus; // No bonus at level 1
        
        // Each level provides increasing stat bonuses
        int levelsGained = level - 1;
        
        // Progressive stat growth - early levels give more basic stats, later levels give more advanced stats
        if (levelsGained > 0)
        {
            // Basic stats (every level)
            levelBonus.health = levelsGained * 2;        // +2 HP per level
            levelBonus.attack = levelsGained * 1;        // +1 ATK per level
            levelBonus.defense = levelsGained * 1;       // +1 DEF per level
        }
        
        if (levelsGained >= 3)
        {
            // Advanced stats (starting at level 4)
            int advancedLevels = levelsGained - 2;
            levelBonus.attackSpeed = advancedLevels * 0.05f;   // +5% attack speed per level after 3
            levelBonus.moveSpeed = advancedLevels * 0.03f;     // +3% move speed per level after 3
        }
        
        if (levelsGained >= 5)
        {
            // Elite stats (starting at level 6)
            int eliteLevels = levelsGained - 4;
            levelBonus.critChance = eliteLevels * 0.02f;       // +2% crit chance per level after 5
            levelBonus.range = eliteLevels * 0.05f;            // +5% range per level after 5
        }
        
        if (levelsGained >= 8)
        {
            // Master stats (starting at level 9)
            int masterLevels = levelsGained - 7;
            levelBonus.critDamage = masterLevels * 0.05f;      // +5% crit damage per level after 8
        }
        
        return levelBonus;
    }
    
    /// <summary>
    /// Get experience progress as percentage for UI display
    /// </summary>
    public float GetExperienceProgress()
    {
        if (level >= 20) return 1.0f; // Max level
        return (float)experience / experienceToNextLevel;
    }
    
    /// <summary>
    /// Get experience display text for UI
    /// </summary>
    public string GetExperienceText()
    {
        if (level >= 20) return $"Level {level} (MAX)";
        return $"Level {level} ({experience}/{experienceToNextLevel} XP)";
    }
}
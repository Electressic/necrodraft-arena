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
        // Start with base stats
        totalHP = baseData.baseHP;
        totalAttack = baseData.baseAttack;
        totalDefense = 0; // No base defense
        
        // Reset calculated stats to base values
        totalAttackSpeed = 1.0f;
        totalCritChance = 0.0f;
        totalCritDamage = 1.5f; // Base 150% crit damage
        totalMoveSpeed = 1.0f;
        totalRange = 1.0f;
        
        // Clear and recalculate abilities
        activeAbilities.Clear();
        
        // Process each equipped part
        ProcessPart(headPart);
        ProcessPart(torsoPart);
        ProcessPart(armsPart);
        ProcessPart(legsPart);
        
        // Apply set bonus abilities based on counts (2+ parts required)
        CalculateSetBonuses();
    }
    
    private void ProcessPart(PartData part)
    {
        if (part == null) return;
        
        // Migrate legacy stats if needed
        part.MigrateLegacyStats();
        
        // Add all stat bonuses from the part (using new format)
        totalHP += part.stats.health;
        totalAttack += part.stats.attack;
        totalDefense += part.stats.defense;
        
        // Add multiplier-based stats
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
        string stats = $"HP: {totalHP} | ATK: {totalAttack}";
        
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
            
        return stats;
    }
    
    // Backwards compatibility
    public float totalMoveSpeedMultiplier => totalMoveSpeed;
    
    public string GetAbilitiesSummary()
    {
        return GetSetBonusesSummary();
    }
}
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
    
    [Header("Calculated Stats")]
    public int totalHP;
    public int totalAttack;
    public float totalMoveSpeedMultiplier = 1f;
    
    [Header("Active Special Abilities")]
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
        totalMoveSpeedMultiplier = 1f;
        
        // Clear and recalculate abilities
        activeAbilities.Clear();
        
        // Process each equipped part
        ProcessPart(headPart);
        ProcessPart(torsoPart);
        ProcessPart(armsPart);
        ProcessPart(legsPart);
    }
    
    private void ProcessPart(PartData part)
    {
        if (part == null) return;
        
        // Add basic stat bonuses
        totalHP += part.hpBonus;
        totalAttack += part.attackBonus;
        
        // Add special ability to active list
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            activeAbilities.Add(part.specialAbility);
            
            // Apply immediate stat modifications for certain abilities
            switch (part.specialAbility)
            {
                case PartData.SpecialAbility.Swift:
                    totalMoveSpeedMultiplier *= 2f; // +100% move speed
                    totalHP = Mathf.RoundToInt(totalHP * 0.75f); // -25% HP
                    break;
            }
        }
    }
    
    public bool HasAbility(PartData.SpecialAbility ability)
    {
        return activeAbilities.Contains(ability);
    }
    
    public int GetAbilityCount(PartData.SpecialAbility ability)
    {
        int count = 0;
        foreach (var activeAbility in activeAbilities)
        {
            if (activeAbility == ability) count++;
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
    
    public string GetAbilitiesSummary()
    {
        if (activeAbilities.Count == 0) return "No special abilities";
        
        List<string> abilityNames = new List<string>();
        foreach (var ability in activeAbilities)
        {
            if (ability != PartData.SpecialAbility.None)
                abilityNames.Add(ability.ToString());
        }
        
        return string.Join(", ", abilityNames);
    }
}
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
    public int totalCritChance = 0;     // Crit chance percentage (0-100)
    public int totalCritDamage = 50;    // Crit damage bonus percentage (base 50% = 150% total)
    public int totalArmorPen = 0;       // Armor penetration
    

    
    [Header("Active Abilities")]
    public List<PartData.SpecialAbility> activeAbilities = new List<PartData.SpecialAbility>();
    public Dictionary<PartData.SpecialAbility, int> abilityLevels = new Dictionary<PartData.SpecialAbility, int>();
    
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
        totalHP = baseData.baseHP + 10;
        totalAttack = baseData.baseAttack + 2;
        totalDefense = 5;
        
        totalCritChance = 0;
        totalCritDamage = 50;
        totalArmorPen = 0;
        

        
        ApplyLevelBonuses();
        
        int baseHP = totalHP;
        int baseAttack = totalAttack;
        int baseDefense = totalDefense;
        
        activeAbilities.Clear();
        
        ProcessPartStats(headPart);
        ProcessPartStats(torsoPart);
        ProcessPartStats(armsPart);
        ProcessPartStats(legsPart);
        
        CalculateAbilities();
    }
    
    private void ProcessPartStats(PartData part)
    {
        if (part == null) return;
        
        part.MigrateLegacyStats();
        
        totalHP += part.stats.hp;
        totalAttack += part.stats.attack;
        totalDefense += part.stats.defense;
        totalCritChance += part.stats.critChance;
        totalCritDamage += part.stats.critDamage;
        totalArmorPen += part.stats.armorPen;
        

        
        if (part.specialAbility != PartData.SpecialAbility.None)
        {
            activeAbilities.Add(part.specialAbility);
            
            if (!abilityLevels.ContainsKey(part.specialAbility) || abilityLevels[part.specialAbility] < part.abilityLevel)
            {
                abilityLevels[part.specialAbility] = part.abilityLevel;
            }
        }
    }
    
    private void CalculateAbilities()
    {
        var uniqueAbilities = new HashSet<PartData.SpecialAbility>(activeAbilities);
        activeAbilities.Clear();
        activeAbilities.AddRange(uniqueAbilities);
        
        foreach (var ability in activeAbilities)
        {
            if (ability != PartData.SpecialAbility.None)
            {
                ApplyAbilityPassiveEffect(ability);
            }
        }
    }
    
    private void ApplyAbilityPassiveEffect(PartData.SpecialAbility ability)
    {   
        int level = abilityLevels.ContainsKey(ability) ? abilityLevels[ability] : 1;
        
        switch (ability)
        {
            case PartData.SpecialAbility.Inspiration:
                totalAttack += level;
                break;
                
        }
    }
    
    private void ApplyLevelBonuses()
    {
        if (level <= 1) return;
        
        PartData.PartStats levelBonus = GetLevelBonusStats();
        
        totalHP += levelBonus.hp;
        totalAttack += levelBonus.attack;
        totalDefense += levelBonus.defense;
        totalCritChance += levelBonus.critChance;
        totalCritDamage += levelBonus.critDamage;
        totalArmorPen += levelBonus.armorPen;
    }
    
    public bool HasAbility(PartData.SpecialAbility ability)
    {
        return activeAbilities.Contains(ability);
    }
    
    public int GetAbilityLevel(PartData.SpecialAbility ability)
    {
        return abilityLevels.ContainsKey(ability) ? abilityLevels[ability] : 0;
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
    
    public string GetAbilitiesSummary()
    {
        if (activeAbilities.Count == 0) return "No abilities active";
        
        List<string> abilityDescriptions = new List<string>();
        
        foreach (var ability in activeAbilities)
        {
            int level = GetAbilityLevel(ability);
            string levelText = level > 1 ? $" L{level}" : "";
            abilityDescriptions.Add(ability.ToString() + levelText);
        }
        
        return string.Join(", ", abilityDescriptions);
    }
    
    public string GetDetailedStatsText()
    {
        string result = GetExperienceText();
        if (level > 1)
        {
            PartData.PartStats levelBonus = GetLevelBonusStats();
            string bonusText = levelBonus.GetStatsText();
            if (!bonusText.Contains("No stat bonuses"))
                result += $"\nLevel Bonus: {bonusText}";
        }
        
        string stats = $"\nHP: {totalHP} | ATK: {totalAttack}";
        
        if (totalDefense > 0)
            stats += $" | DEF: {totalDefense}";
            
        if (totalCritChance > 0)
            stats += $" | Crit: {totalCritChance}%";
            
        if (totalCritDamage != 50)
            stats += $" | Crit DMG: +{totalCritDamage}%";
            
        if (totalArmorPen > 0)
            stats += $" | Armor Pen: {totalArmorPen}";
            
        return result + stats;
    }
    
    public bool HasSetBonus(PartData.SpecialAbility ability) => HasAbility(ability);
    public string GetSetBonusesSummary() => GetAbilitiesSummary();
    
    
    public bool AddExperience(int xpGained)
    {
        if (xpGained <= 0) return false;
        
        experience += xpGained;
        totalExperienceSpent += xpGained;
        
        bool leveledUp = false;
        while (experience >= experienceToNextLevel && level < 20)
        {
            experience -= experienceToNextLevel;
            level++;
            leveledUp = true;
            
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.25f);
            
            Debug.Log($"[Minion] {minionName} leveled up to level {level}! Next level requires {experienceToNextLevel} XP.");
        }
        
        if (leveledUp)
        {   
            CalculateStats();
        }
        
        return leveledUp;
    }
    
    public PartData.PartStats GetLevelBonusStats()
    {
        PartData.PartStats levelBonus = new PartData.PartStats();
        
        if (level <= 1) return levelBonus;
        
        int levelsGained = level - 1;
        
        if (levelsGained > 0)
        {
            levelBonus.hp = levelsGained * 2;
            levelBonus.attack = levelsGained * 1;
            levelBonus.defense = levelsGained * 1;
        }
        
        if (levelsGained >= 5)
        {
            int eliteLevels = levelsGained - 4;
            levelBonus.critChance = eliteLevels * 2;
        }
        
        if (levelsGained >= 8)
        {
            int masterLevels = levelsGained - 7;
            levelBonus.critDamage = masterLevels * 5;
        }
        
        if (levelsGained >= 12)
        {
            int legendaryLevels = levelsGained - 11;
            levelBonus.armorPen = legendaryLevels;
        }
        
        return levelBonus;
    }
    
    public float GetExperienceProgress()
    {
        if (level >= 20) return 1.0f;
        return (float)experience / experienceToNextLevel;
    }
    
    public string GetExperienceText()
    {
        if (level >= 20) return $"Level {level} (MAX)";
        return $"Level {level} ({experience}/{experienceToNextLevel} XP)";
    }
}
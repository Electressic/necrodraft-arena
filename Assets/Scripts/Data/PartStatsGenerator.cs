using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PartStatsGenerator
{
    // Stat point costs - how many budget points each stat type costs per unit
    private const int HEALTH_COST = 1;          // 1 point per +5% HP
    private const int ATTACK_COST = 1;          // 1 point per +4% Attack
    private const int DEFENSE_COST = 2;         // 2 points per +3% Defense (more valuable)
    private const int ATTACK_SPEED_COST = 1;    // 1 point per +1% attack speed
    private const int CRIT_CHANCE_COST = 2;     // 2 points per +1% crit chance (valuable)
    private const int CRIT_DAMAGE_COST = 1;     // 1 point per +1% crit damage
    private const int MOVE_SPEED_COST = 1;      // 1 point per +1% move speed
    private const int RANGE_COST = 2;           // 2 points per +1% range (valuable)
    
    /// <summary>
    /// Generate stats for a part using the budget allocation system
    /// </summary>
    public static PartData.PartStats Generate(PartData.PartTheme theme, PartData.PartRarity rarity, PartData.PartType partType)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        // Get total budget for this rarity
        int totalBudget = GetStatBudgetForRarity(rarity);
        
        // Get primary and secondary stat pools for this part type
        StatType[] primaryPool = GetPrimaryStatPool(partType);
        StatType[] secondaryPool = GetSecondaryStatPool(partType);
        
        // Calculate budget split between primary and secondary pools
        float primaryRatio = GetPrimaryPoolRatio(rarity);
        int primaryBudget = Mathf.RoundToInt(totalBudget * primaryRatio);
        int secondaryBudget = totalBudget - primaryBudget;
        
        // Allocate stats from pools
        AllocateStatsFromPool(stats, primaryPool, primaryBudget);
        AllocateStatsFromPool(stats, secondaryPool, secondaryBudget);
        
        // Debug logging for Epic parts
        if (rarity == PartData.PartRarity.Epic)
        {
            Debug.Log($"[PartStatsGenerator] Epic {partType} - Primary budget: {primaryBudget}, Secondary budget: {secondaryBudget}");
            Debug.Log($"[PartStatsGenerator] Primary pool: [{string.Join(", ", primaryPool)}], Secondary pool: [{string.Join(", ", secondaryPool)}]");
        }
        
        // Apply theme-based adjustments (slight boosts to thematic stats)
        ApplyThemeAdjustments(stats, theme);
        
        // Ensure we have the correct number of stats for the rarity
        EnsureStatCountForRarity(stats, rarity);
        
        return stats;
    }
    
    /// <summary>
    /// Generate a generic part with bonus budget (no primary/secondary pool restrictions)
    /// </summary>
    public static PartData.PartStats GenerateGeneric(PartData.PartRarity rarity)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        // Generic parts get +25% budget
        int totalBudget = Mathf.RoundToInt(GetStatBudgetForRarity(rarity) * 1.25f);
        
        // All stats available for generic parts
        StatType[] allStats = { StatType.Health, StatType.Attack, StatType.Defense, 
                               StatType.AttackSpeed, StatType.CritChance, StatType.CritDamage, 
                               StatType.MoveSpeed, StatType.Range };
        
        // Allocate stats efficiently across all types
        AllocateStatsFromPool(stats, allStats, totalBudget);
        
        // Ensure proper stat count
        EnsureStatCountForRarity(stats, rarity);
        
        return stats;
    }
    
    private enum StatType
    {
        Health, Attack, Defense, AttackSpeed, CritChance, CritDamage, MoveSpeed, Range
    }
    
    private static int GetStatBudgetForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 12;
            case PartData.PartRarity.Uncommon: return 20;
            case PartData.PartRarity.Rare: return 32;
            case PartData.PartRarity.Epic: return 50;
            default: return 12;
        }
    }
    
    private static float GetPrimaryPoolRatio(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 0.90f;    // 90% primary, 10% secondary
            case PartData.PartRarity.Uncommon: return 0.75f;  // 75% primary, 25% secondary
            case PartData.PartRarity.Rare: return 0.60f;      // 60% primary, 40% secondary
            case PartData.PartRarity.Epic: return 0.50f;      // 50% primary, 50% secondary
            default: return 0.90f;
        }
    }
    
    private static StatType[] GetPrimaryStatPool(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return new[] { StatType.CritChance, StatType.CritDamage, StatType.Range };
            case PartData.PartType.Torso:
                return new[] { StatType.Health, StatType.Defense };
            case PartData.PartType.Arms:
                return new[] { StatType.Attack, StatType.AttackSpeed, StatType.CritChance };
            case PartData.PartType.Legs:
                return new[] { StatType.MoveSpeed, StatType.AttackSpeed };
            default:
                return new[] { StatType.Health, StatType.Attack };
        }
    }
    
    private static StatType[] GetSecondaryStatPool(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return new[] { StatType.AttackSpeed, StatType.MoveSpeed, StatType.Health };
            case PartData.PartType.Torso:
                return new[] { StatType.Attack, StatType.MoveSpeed, StatType.AttackSpeed };
            case PartData.PartType.Arms:
                return new[] { StatType.Health, StatType.Defense, StatType.Range, StatType.MoveSpeed };
            case PartData.PartType.Legs:
                return new[] { StatType.Health, StatType.Defense, StatType.Range, StatType.CritChance };
            default:
                return new[] { StatType.Health, StatType.Attack, StatType.Defense };
        }
    }
    
    private static void AllocateStatsFromPool(PartData.PartStats stats, StatType[] pool, int budget)
    {
        if (budget <= 0 || pool.Length == 0) return;
        
        // Randomly distribute budget across available stats in the pool
        List<StatType> availableStats = pool.ToList();
        int remainingBudget = budget;
        
        while (remainingBudget > 0 && availableStats.Count > 0)
        {
            // Pick a random stat from available pool
            StatType selectedStat = availableStats[Random.Range(0, availableStats.Count)];
            
            // Calculate how much we can afford to put into this stat
            int statCost = GetStatCost(selectedStat);
            int maxAffordable = remainingBudget / statCost;
            
            if (maxAffordable > 0)
            {
                // Make smaller, more varied investments (prefer 1-10 point investments)
                int maxInvestment = Mathf.Min(maxAffordable, 15); // Cap at reasonable amounts
                int investment = Random.Range(1, maxInvestment + 1);
                
                ApplyStatInvestment(stats, selectedStat, investment);
                remainingBudget -= investment * statCost;
            }
            else
            {
                // Can't afford this stat anymore, remove it from options
                availableStats.Remove(selectedStat);
            }
        }
    }
    
    private static int GetStatCost(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return HEALTH_COST;
            case StatType.Attack: return ATTACK_COST;
            case StatType.Defense: return DEFENSE_COST;
            case StatType.AttackSpeed: return ATTACK_SPEED_COST;
            case StatType.CritChance: return CRIT_CHANCE_COST;
            case StatType.CritDamage: return CRIT_DAMAGE_COST;
            case StatType.MoveSpeed: return MOVE_SPEED_COST;
            case StatType.Range: return RANGE_COST;
            default: return 1;
        }
    }
    
    private static void ApplyStatInvestment(PartData.PartStats stats, StatType statType, int investment)
    {
        switch (statType)
        {
            case StatType.Health:
                stats.health += investment * 0.05f; // 1 investment = 5% HP bonus
                break;
            case StatType.Attack:
                stats.attack += investment * 0.04f; // 1 investment = 4% Attack bonus
                break;
            case StatType.Defense:
                stats.defense += investment * 0.03f; // 1 investment = 3% Defense bonus (more valuable per point since cost is 2)
                break;
            case StatType.AttackSpeed:
                stats.attackSpeed += investment * 0.01f; // 1 investment = 1% (0.01)
                break;
            case StatType.CritChance:
                stats.critChance += investment * 0.01f; // 1 investment = 1% (0.01)
                break;
            case StatType.CritDamage:
                stats.critDamage += investment * 0.01f; // 1 investment = 1% (0.01)
                break;
            case StatType.MoveSpeed:
                stats.moveSpeed += investment * 0.01f; // 1 investment = 1% (0.01)
                break;
            case StatType.Range:
                stats.range += investment * 0.01f; // 1 investment = 1% (0.01)
                break;
        }
    }
    
    private static void ApplyThemeAdjustments(PartData.PartStats stats, PartData.PartTheme theme)
    {
        // Small thematic bonuses to make themes feel distinct
        switch (theme)
        {
            case PartData.PartTheme.Skeleton:
                // Slight boost to speed/precision stats
                if (stats.moveSpeed > 0) stats.moveSpeed += 0.01f;
                if (stats.critChance > 0) stats.critChance += 0.01f;
                break;
            case PartData.PartTheme.Zombie:
                // Slight boost to durability stats
                if (stats.health > 0) stats.health += 0.02f; // +2% health bonus
                if (stats.defense > 0) stats.defense += 0.02f; // +2% defense bonus
                break;
            case PartData.PartTheme.Ghost:
                // Slight boost to magical/balanced stats
                if (stats.critDamage > 0) stats.critDamage += 0.01f;
                if (stats.range > 0) stats.range += 0.01f;
                break;
        }
    }
    
    private static void EnsureStatCountForRarity(PartData.PartStats stats, PartData.PartRarity rarity)
    {
        int currentStatCount = CountNonZeroStats(stats);
        int targetMinStats = GetMinStatsForRarity(rarity);
        int targetMaxStats = GetMaxStatsForRarity(rarity);
        
        // If we have too few stats, add some basic ones
        while (currentStatCount < targetMinStats)
        {
            AddRandomBasicStat(stats);
            currentStatCount = CountNonZeroStats(stats);
        }
        
        // If we have too many stats, we accept it (budget allocation can be efficient)
        // Only remove stats if we're way over the limit
        while (currentStatCount > targetMaxStats + 1)
        {
            RemoveWeakestStat(stats);
            currentStatCount = CountNonZeroStats(stats);
        }
    }
    
    private static int GetMinStatsForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 1;
            case PartData.PartRarity.Uncommon: return 2;
            case PartData.PartRarity.Rare: return 2;
            case PartData.PartRarity.Epic: return 3;
            default: return 1;
        }
    }
    
    private static int GetMaxStatsForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 2;
            case PartData.PartRarity.Uncommon: return 2;
            case PartData.PartRarity.Rare: return 3;
            case PartData.PartRarity.Epic: return 4;
            default: return 2;
        }
    }
    
    private static int CountNonZeroStats(PartData.PartStats stats)
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
    
    private static void AddRandomBasicStat(PartData.PartStats stats)
    {
        // Add a basic stat if missing
        StatType[] basicStats = { StatType.Health, StatType.Attack, StatType.Defense };
        
        foreach (StatType stat in basicStats)
        {
            if (GetStatValue(stats, stat) == 0)
            {
                ApplyStatInvestment(stats, stat, Random.Range(1, 4));
                return;
            }
        }
        
        // If all basic stats exist, add a small amount to a random one
        StatType randomBasic = basicStats[Random.Range(0, basicStats.Length)];
        ApplyStatInvestment(stats, randomBasic, 1);
    }
    
    private static void RemoveWeakestStat(PartData.PartStats stats)
    {
        // Find the stat with the lowest value and remove it
        StatType weakestStat = StatType.Health;
        float weakestValue = float.MaxValue;
        
        if (stats.health > 0 && stats.health < weakestValue) { weakestStat = StatType.Health; weakestValue = stats.health; }
        if (stats.attack > 0 && stats.attack < weakestValue) { weakestStat = StatType.Attack; weakestValue = stats.attack; }
        if (stats.defense > 0 && stats.defense < weakestValue) { weakestStat = StatType.Defense; weakestValue = stats.defense; }
        if (stats.attackSpeed > 0 && stats.attackSpeed < weakestValue) { weakestStat = StatType.AttackSpeed; weakestValue = stats.attackSpeed; }
        if (stats.critChance > 0 && stats.critChance < weakestValue) { weakestStat = StatType.CritChance; weakestValue = stats.critChance; }
        if (stats.critDamage > 0 && stats.critDamage < weakestValue) { weakestStat = StatType.CritDamage; weakestValue = stats.critDamage; }
        if (stats.moveSpeed > 0 && stats.moveSpeed < weakestValue) { weakestStat = StatType.MoveSpeed; weakestValue = stats.moveSpeed; }
        if (stats.range > 0 && stats.range < weakestValue) { weakestStat = StatType.Range; weakestValue = stats.range; }
        
        // Remove the weakest stat
        SetStatValue(stats, weakestStat, 0);
    }
    
    private static float GetStatValue(PartData.PartStats stats, StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return stats.health;
            case StatType.Attack: return stats.attack;
            case StatType.Defense: return stats.defense;
            case StatType.AttackSpeed: return stats.attackSpeed;
            case StatType.CritChance: return stats.critChance;
            case StatType.CritDamage: return stats.critDamage;
            case StatType.MoveSpeed: return stats.moveSpeed;
            case StatType.Range: return stats.range;
            default: return 0;
        }
    }
    
    private static void SetStatValue(PartData.PartStats stats, StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.Health: stats.health = value; break;
            case StatType.Attack: stats.attack = value; break;
            case StatType.Defense: stats.defense = value; break;
            case StatType.AttackSpeed: stats.attackSpeed = value; break;
            case StatType.CritChance: stats.critChance = value; break;
            case StatType.CritDamage: stats.critDamage = value; break;
            case StatType.MoveSpeed: stats.moveSpeed = value; break;
            case StatType.Range: stats.range = value; break;
        }
    }
} 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PartStatsGenerator
{
    // Stat point costs per unit according to design document
    private const int HP_COST = 1;              // 1 point per 1 HP
    private const int ATTACK_COST = 1;          // 1 point per 1 Attack
    private const int DEFENSE_COST = 2;         // 2 points per 1 Defense (more valuable)
    private const int CRIT_CHANCE_COST = 2;     // 2 points per 1% crit chance
    private const int CRIT_DAMAGE_COST = 1;     // 1 point per 1% crit damage
    private const int ARMOR_PEN_COST = 3;       // 3 points per 1 armor penetration
    
    public static PartData.PartStats Generate(PartData.PartTheme theme, PartData.PartRarity rarity, PartData.PartType partType)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        int totalBudget = GetStatBudgetForRarity(rarity);
        
        if (totalBudget <= 0)
        {
            Debug.LogWarning($"[PartStatsGenerator] Invalid budget {totalBudget} for {rarity}! Using minimum budget.");
            totalBudget = 8; 
        }
        
        StatType[] primaryPool = GetPrimaryStatPool(partType);
        StatType[] secondaryPool = GetSecondaryStatPool(partType);
        
        if (primaryPool.Length == 0 && secondaryPool.Length == 0)
        {
            Debug.LogWarning($"[PartStatsGenerator] No stat pools available for {partType}! Using default stats.");
            stats.hp = totalBudget / 2;
            stats.attack = totalBudget / 2;
            return stats;
        }
        
        float primaryRatio = GetPrimaryPoolRatio(rarity);
        int primaryBudget = Mathf.RoundToInt(totalBudget * primaryRatio);
        int secondaryBudget = totalBudget - primaryBudget;
        
        if (primaryPool.Length > 0 && secondaryPool.Length > 0)
        {
            primaryBudget = Mathf.Max(primaryBudget, 1);
            secondaryBudget = Mathf.Max(secondaryBudget, 1);
            if (primaryBudget + secondaryBudget > totalBudget)
            {
                secondaryBudget = totalBudget - primaryBudget;
            }
        }
        
        if (primaryPool.Length > 0)
            AllocateStatsFromPool(stats, primaryPool, primaryBudget);
        if (secondaryPool.Length > 0)
            AllocateStatsFromPool(stats, secondaryPool, secondaryBudget);
        
        if (rarity == PartData.PartRarity.Epic)
        {
            Debug.Log($"[PartStatsGenerator] Epic {partType} - Primary budget: {primaryBudget}, Secondary budget: {secondaryBudget}");
            Debug.Log($"[PartStatsGenerator] Primary pool: [{string.Join(", ", primaryPool)}], Secondary pool: [{string.Join(", ", secondaryPool)}]");
        }
        
        EnsureStatCountForRarity(stats, rarity);
        
        if (CountNonZeroStats(stats) == 0)
        {
            Debug.LogError($"[PartStatsGenerator] Generated part with NO STATS! Theme: {theme}, Rarity: {rarity}, Type: {partType}");
            Debug.LogError($"[PartStatsGenerator] Budget: {totalBudget}, Primary: {primaryBudget}, Secondary: {secondaryBudget}");
            Debug.LogError($"[PartStatsGenerator] Primary pool: [{string.Join(", ", primaryPool)}], Secondary pool: [{string.Join(", ", secondaryPool)}]");
            
            int emergencyBudget = Mathf.Max(totalBudget, 4);
            stats.hp = emergencyBudget / 2;
            stats.attack = emergencyBudget - stats.hp;
            
            Debug.LogWarning($"[PartStatsGenerator] Emergency fallback applied: +{stats.hp} HP, +{stats.attack} ATK");
        }
        
        return stats;
    }
    
    public static PartData.PartStats GenerateGeneric(PartData.PartRarity rarity)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        int totalBudget = Mathf.RoundToInt(GetStatBudgetForRarity(rarity) * 1.25f);
        
        StatType[] allStats = { StatType.HP, StatType.Attack, StatType.Defense, 
                               StatType.CritChance, StatType.CritDamage, StatType.ArmorPen };
        
        AllocateStatsFromPool(stats, allStats, totalBudget);
        
        EnsureStatCountForRarity(stats, rarity);
        
        return stats;
    }
    
    private enum StatType
    {
        HP, Attack, Defense, CritChance, CritDamage, ArmorPen
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
                return new[] { StatType.CritChance, StatType.CritDamage, StatType.ArmorPen };
            case PartData.PartType.Torso:
                return new[] { StatType.HP, StatType.Defense, StatType.Attack };
            case PartData.PartType.Arms:
                return new[] { StatType.Attack, StatType.CritChance, StatType.ArmorPen };
            case PartData.PartType.Legs:
                return new[] { StatType.HP, StatType.Defense };
            default:
                return new[] { StatType.HP, StatType.Attack };
        }
    }
    
    private static StatType[] GetSecondaryStatPool(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return new[] { StatType.HP, StatType.Attack };
            case PartData.PartType.Torso:
                return new[] { StatType.CritChance, StatType.ArmorPen };
            case PartData.PartType.Arms:
                return new[] { StatType.HP, StatType.Defense, StatType.CritDamage };
            case PartData.PartType.Legs:
                return new[] { StatType.Attack, StatType.CritChance, StatType.CritDamage };
            default:
                return new[] { StatType.HP, StatType.Attack, StatType.Defense };
        }
    }
    
    private static void AllocateStatsFromPool(PartData.PartStats stats, StatType[] pool, int budget)
    {
        if (budget <= 0 || pool.Length == 0) 
        {
            if (budget > 0 && pool.Length == 0)
                Debug.LogWarning($"[PartStatsGenerator] Cannot allocate {budget} budget - empty stat pool!");
            return;
        }
        
        List<StatType> availableStats = pool.ToList();
        int remainingBudget = budget;
        int attempts = 0;
        const int maxAttempts = 50;
        
        while (remainingBudget > 0 && availableStats.Count > 0 && attempts < maxAttempts)
        {
            attempts++;
            
            StatType selectedStat = availableStats[Random.Range(0, availableStats.Count)];
            
            int statCost = GetStatCost(selectedStat);
            int maxAffordable = remainingBudget / statCost;
            
            if (maxAffordable > 0)
            {
                int statCap = GetStatCap(selectedStat);
                int currentValue = GetStatValue(stats, selectedStat);
                int maxInvestment = Mathf.Min(maxAffordable, statCap - currentValue, 8);
                
                if (maxInvestment > 0)
                {
                    int investment = Random.Range(1, maxInvestment + 1);
                    
                    ApplyStatInvestment(stats, selectedStat, investment);
                    remainingBudget -= investment * statCost;
                }
                else
                {
                    availableStats.Remove(selectedStat);
                }
            }
            else
            {
                availableStats.Remove(selectedStat);
            }
        }
        
        if (remainingBudget > 0 && attempts < maxAttempts && pool.Length > 0)
        {
            StatType cheapestStat = pool[0];
            int cheapestCost = GetStatCost(cheapestStat);
            
            foreach (StatType stat in pool)
            {
                int cost = GetStatCost(stat);
                if (cost <= remainingBudget && cost < cheapestCost)
                {
                    cheapestStat = stat;
                    cheapestCost = cost;
                }
            }
            
            if (cheapestCost <= remainingBudget)
            {
                int currentValue = GetStatValue(stats, cheapestStat);
                int statCap = GetStatCap(cheapestStat);
                if (currentValue < statCap)
                {
                    ApplyStatInvestment(stats, cheapestStat, 1);
                    remainingBudget -= cheapestCost;
                }
            }
        }
        
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"[PartStatsGenerator] AllocateStatsFromPool hit max attempts! Budget: {budget}, Remaining: {remainingBudget}");
        }
    }
    
    private static int GetStatCost(StatType statType)
    {
        switch (statType)
        {
            case StatType.HP: return HP_COST;
            case StatType.Attack: return ATTACK_COST;
            case StatType.Defense: return DEFENSE_COST;
            case StatType.CritChance: return CRIT_CHANCE_COST;
            case StatType.CritDamage: return CRIT_DAMAGE_COST;
            case StatType.ArmorPen: return ARMOR_PEN_COST;
            default: return 1;
        }
    }
    
    private static void ApplyStatInvestment(PartData.PartStats stats, StatType statType, int investment)
    {
        switch (statType)
        {
            case StatType.HP:
                stats.hp += investment; // 1 point = 1 HP
                break;
            case StatType.Attack:
                stats.attack += investment; // 1 point = 1 Attack
                break;
            case StatType.Defense:
                stats.defense += investment; // 1 point = 1 Defense
                break;
            case StatType.CritChance:
                stats.critChance += investment; // 1 point = 1% crit chance
                break;
            case StatType.CritDamage:
                stats.critDamage += investment; // 1 point = 1% crit damage
                break;
            case StatType.ArmorPen:
                stats.armorPen += investment; // 1 point = 1 armor pen
                break;
        }
    }
    
    private static void EnsureStatCountForRarity(PartData.PartStats stats, PartData.PartRarity rarity)
    {
        int currentStatCount = CountNonZeroStats(stats);
        int targetMinStats = GetMinStatsForRarity(rarity);
        int targetMaxStats = GetMaxStatsForRarity(rarity);
        
        while (currentStatCount < targetMinStats)
        {
            AddRandomBasicStat(stats);
            currentStatCount = CountNonZeroStats(stats);
        }
        
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
        if (stats.hp > 0) count++;
        if (stats.attack > 0) count++;
        if (stats.defense > 0) count++;
        if (stats.critChance > 0) count++;
        if (stats.critDamage > 0) count++;
        if (stats.armorPen > 0) count++;
        return count;
    }
    
    private static void AddRandomBasicStat(PartData.PartStats stats)
    {
        StatType[] basicStats = { StatType.HP, StatType.Attack, StatType.Defense };
        
        foreach (StatType stat in basicStats)
        {
            if (GetStatValue(stats, stat) == 0)
            {
                ApplyStatInvestment(stats, stat, Random.Range(1, 4));
                return;
            }
        }
        
        StatType randomBasic = basicStats[Random.Range(0, basicStats.Length)];
        ApplyStatInvestment(stats, randomBasic, 1);
    }
    
    private static void RemoveWeakestStat(PartData.PartStats stats)
    {
        StatType weakestStat = StatType.HP;
        int weakestValue = int.MaxValue;
        
        if (stats.hp > 0 && stats.hp < weakestValue) { weakestStat = StatType.HP; weakestValue = stats.hp; }
        if (stats.attack > 0 && stats.attack < weakestValue) { weakestStat = StatType.Attack; weakestValue = stats.attack; }
        if (stats.defense > 0 && stats.defense < weakestValue) { weakestStat = StatType.Defense; weakestValue = stats.defense; }
        if (stats.critChance > 0 && stats.critChance < weakestValue) { weakestStat = StatType.CritChance; weakestValue = stats.critChance; }
        if (stats.critDamage > 0 && stats.critDamage < weakestValue) { weakestStat = StatType.CritDamage; weakestValue = stats.critDamage; }
        if (stats.armorPen > 0 && stats.armorPen < weakestValue) { weakestStat = StatType.ArmorPen; weakestValue = stats.armorPen; }
        
        SetStatValue(stats, weakestStat, 0);
    }
    
    private static int GetStatValue(PartData.PartStats stats, StatType statType)
    {
        switch (statType)
        {
            case StatType.HP: return stats.hp;
            case StatType.Attack: return stats.attack;
            case StatType.Defense: return stats.defense;
            case StatType.CritChance: return stats.critChance;
            case StatType.CritDamage: return stats.critDamage;
            case StatType.ArmorPen: return stats.armorPen;
            default: return 0;
        }
    }
    
    private static void SetStatValue(PartData.PartStats stats, StatType statType, int value)
    {
        switch (statType)
        {
            case StatType.HP: stats.hp = value; break;
            case StatType.Attack: stats.attack = value; break;
            case StatType.Defense: stats.defense = value; break;
            case StatType.CritChance: stats.critChance = value; break;
            case StatType.CritDamage: stats.critDamage = value; break;
            case StatType.ArmorPen: stats.armorPen = value; break;
        }
    }
    
    private static int GetStatCap(StatType statType)
    {
        switch (statType)
        {
            case StatType.HP: return 25;
            case StatType.Attack: return 20;
            case StatType.Defense: return 8;
            case StatType.CritChance: return 25;
            case StatType.CritDamage: return 50;
            case StatType.ArmorPen: return 6;
            default: return 10;
        }
    }
    
    public static PartData.PartStats GenerateWithAbility(PartData.PartTheme theme, PartData.PartRarity rarity, 
        PartData.PartType partType, out PartData.SpecialAbility ability, out int abilityLevel)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        int totalBudget = GetStatBudgetForRarity(rarity);
        
        bool hasAbility = DetermineAbilityChance(rarity);
        
        if (hasAbility)
        {
            (ability, abilityLevel) = GenerateAbilityForPart(theme, partType, rarity);
            
            int abilityCost = GetAbilityCost(ability, abilityLevel);
            
            int minStatBudget = GetMinStatBudgetForAbility(ability, totalBudget);
            
            if (totalBudget >= abilityCost + minStatBudget)
            {
                AllocateRequiredStatsForAbility(stats, ability, minStatBudget);
                
                int remainingBudget = totalBudget - abilityCost - minStatBudget;
                if (remainingBudget > 0)
                {
                    StatType[] availableStats = GetAvailableStatsForPartType(partType);
                    AllocateStatsFromPool(stats, availableStats, remainingBudget);
                }
            }
            else
            {
                ability = PartData.SpecialAbility.None;
                abilityLevel = 0;
                AllocateStatsFromPool(stats, GetAvailableStatsForPartType(partType), totalBudget);
            }
        }
        else
        {
            ability = PartData.SpecialAbility.None;
            abilityLevel = 0;
            AllocateStatsFromPool(stats, GetAvailableStatsForPartType(partType), totalBudget);
        }
        
        if (CountNonZeroStats(stats) == 0)
        {
            Debug.LogWarning($"[PartStatsGenerator] Generated part with no stats! Adding safety stat. Theme: {theme}, Rarity: {rarity}, Type: {partType}"); 
            stats.hp += 1;
        }
        
        return stats;
    }
    
    public static int GetStatBudgetForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 8;
            case PartData.PartRarity.Uncommon: return 14;
            case PartData.PartRarity.Rare: return 22;
            case PartData.PartRarity.Epic: return 35;
            default: return 8;
        }
    }
    
    private static bool DetermineAbilityChance(PartData.PartRarity rarity)
    {
        float chance = rarity switch
        {
            PartData.PartRarity.Common => 0.3f,
            PartData.PartRarity.Uncommon => 0.5f,
            PartData.PartRarity.Rare => 0.8f,
            PartData.PartRarity.Epic => 1.0f,
            _ => 0.0f
        };
        return Random.value < chance;
    }
    
    private static (PartData.SpecialAbility, int) GenerateAbilityForPart(PartData.PartTheme theme, PartData.PartType partType, PartData.PartRarity rarity)
    {
        PartData.SpecialAbility[] availableAbilities = GetAvailableAbilitiesForPartType(partType);
        
        var weightedAbilities = availableAbilities
            .Select(ability => new { ability, weight = GetThemeAffinityWeight(theme, GetAbilityRole(ability)) })
            .Where(x => x.weight > 0)
            .ToArray();
        
        if (weightedAbilities.Length == 0)
        {
            return (PartData.SpecialAbility.None, 0);
        }
        
        float totalWeight = weightedAbilities.Sum(x => x.weight);
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0;
        
        PartData.SpecialAbility selectedAbility = PartData.SpecialAbility.None;
        foreach (var item in weightedAbilities)
        {
            currentWeight += item.weight;
            if (randomValue <= currentWeight)
            {
                selectedAbility = item.ability;
                break;
            }
        }
        
        int maxLevel = rarity switch
        {
            PartData.PartRarity.Common => 1,
            PartData.PartRarity.Uncommon => Random.value < 0.3f ? 2 : 1,
            PartData.PartRarity.Rare => Random.value < 0.6f ? 3 : (Random.value < 0.8f ? 2 : 1),
            PartData.PartRarity.Epic => Random.value < 0.7f ? 3 : (Random.value < 0.9f ? 2 : 1),
            _ => 1
        };
        
        return (selectedAbility, maxLevel);
    }
    
    private static StatType[] GetAvailableStatsForPartType(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return new[] { StatType.CritChance, StatType.CritDamage, StatType.ArmorPen };
            case PartData.PartType.Torso:
                return new[] { StatType.HP, StatType.Defense, StatType.Attack };
            case PartData.PartType.Arms:
                return new[] { StatType.Attack, StatType.CritChance, StatType.ArmorPen };
            case PartData.PartType.Legs:
                return new[] { StatType.HP, StatType.Defense };
            default:
                return new[] { StatType.HP, StatType.Attack, StatType.Defense };
        }
    }
    
    private static PartData.SpecialAbility[] GetAvailableAbilitiesForPartType(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                return new[] { PartData.SpecialAbility.Overwatch, PartData.SpecialAbility.Hunter, PartData.SpecialAbility.FocusFire, PartData.SpecialAbility.Confuse };
            case PartData.PartType.Torso:
                return new[] { PartData.SpecialAbility.Taunt, PartData.SpecialAbility.ShieldWall, PartData.SpecialAbility.DamageSharing, PartData.SpecialAbility.BattleCry };
            case PartData.PartType.Arms:
                return new[] { PartData.SpecialAbility.RangeAttack, PartData.SpecialAbility.Flanking, PartData.SpecialAbility.Momentum, PartData.SpecialAbility.Inspiration };
            case PartData.PartType.Legs:
                return new[] { PartData.SpecialAbility.Mobility, PartData.SpecialAbility.PhaseStep, PartData.SpecialAbility.Healing };
            default:
                return new PartData.SpecialAbility[0];
        }
    }
    
    private static PartData.AbilityRole GetAbilityRole(PartData.SpecialAbility ability)
    {
        return ability switch
        {
            PartData.SpecialAbility.Taunt or PartData.SpecialAbility.ShieldWall or PartData.SpecialAbility.DamageSharing => PartData.AbilityRole.Guardian,
            PartData.SpecialAbility.Flanking or PartData.SpecialAbility.FocusFire or PartData.SpecialAbility.Momentum => PartData.AbilityRole.Assault,
            PartData.SpecialAbility.RangeAttack or PartData.SpecialAbility.Overwatch or PartData.SpecialAbility.Hunter => PartData.AbilityRole.Marksman,
            PartData.SpecialAbility.Healing or PartData.SpecialAbility.Inspiration or PartData.SpecialAbility.BattleCry => PartData.AbilityRole.Support,
            PartData.SpecialAbility.Mobility or PartData.SpecialAbility.PhaseStep or PartData.SpecialAbility.Confuse => PartData.AbilityRole.Trickster,
            _ => PartData.AbilityRole.None
        };
    }
    
    private static float GetThemeAffinityWeight(PartData.PartTheme theme, PartData.AbilityRole role)
    {
        return theme switch
        {
            PartData.PartTheme.Bone => role switch
            {
                PartData.AbilityRole.Assault => 0.6f,
                PartData.AbilityRole.Marksman => 0.3f,
                PartData.AbilityRole.Guardian => 0.05f,
                PartData.AbilityRole.Support => 0.05f,
                PartData.AbilityRole.Trickster => 0.05f,
                _ => 0.0f
            },
            PartData.PartTheme.Flesh => role switch
            {
                PartData.AbilityRole.Guardian => 0.6f,
                PartData.AbilityRole.Support => 0.3f,
                PartData.AbilityRole.Assault => 0.05f,
                PartData.AbilityRole.Marksman => 0.05f,
                PartData.AbilityRole.Trickster => 0.05f,
                _ => 0.0f
            },
            PartData.PartTheme.Spirit => role switch
            {
                PartData.AbilityRole.Trickster => 0.4f,
                PartData.AbilityRole.Guardian => 0.15f,
                PartData.AbilityRole.Assault => 0.15f,
                PartData.AbilityRole.Marksman => 0.15f,
                PartData.AbilityRole.Support => 0.15f,
                _ => 0.0f
            },
            _ => 0.0f
        };
    }
    
    private static int GetAbilityCost(PartData.SpecialAbility ability, int level)
    {
        if (ability == PartData.SpecialAbility.None || level <= 0) return 0;
        
        int[] costs = ability switch
        {
            PartData.SpecialAbility.Taunt => new int[] { 4, 6, 8 },
            PartData.SpecialAbility.ShieldWall => new int[] { 6, 9, 12 },
            PartData.SpecialAbility.DamageSharing => new int[] { 5, 8, 11 },
            PartData.SpecialAbility.Flanking => new int[] { 4, 7, 10 },
            PartData.SpecialAbility.FocusFire => new int[] { 5, 8, 11 },
            PartData.SpecialAbility.Momentum => new int[] { 6, 9, 12 },
            PartData.SpecialAbility.RangeAttack => new int[] { 8, 12, 16 },
            PartData.SpecialAbility.Overwatch => new int[] { 7, 11, 15 },
            PartData.SpecialAbility.Hunter => new int[] { 5, 8, 11 },
            PartData.SpecialAbility.Healing => new int[] { 3, 5, 7 },
            PartData.SpecialAbility.Inspiration => new int[] { 4, 6, 8 },
            PartData.SpecialAbility.BattleCry => new int[] { 8, 12, 16 },
            PartData.SpecialAbility.Mobility => new int[] { 10, 15, 20 },
            PartData.SpecialAbility.PhaseStep => new int[] { 12, 18, 24 },
            PartData.SpecialAbility.Confuse => new int[] { 6, 9, 12 },
            _ => new int[] { 0, 0, 0 }
        };
        
        return costs[Mathf.Clamp(level - 1, 0, 2)];
    }
    
    private static int GetMinStatBudgetForAbility(PartData.SpecialAbility ability, int totalBudget)
    {
        return ability switch
        {
            PartData.SpecialAbility.Taunt or PartData.SpecialAbility.ShieldWall or PartData.SpecialAbility.DamageSharing 
                => Mathf.RoundToInt(totalBudget * 0.2f),
            
            PartData.SpecialAbility.Flanking or PartData.SpecialAbility.FocusFire or PartData.SpecialAbility.Momentum
                => Mathf.RoundToInt(totalBudget * 0.25f),
                
            PartData.SpecialAbility.RangeAttack or PartData.SpecialAbility.Overwatch or PartData.SpecialAbility.Hunter
                => Mathf.RoundToInt(totalBudget * 0.2f),
                
            _ => 0
        };
    }
    
    private static void AllocateRequiredStatsForAbility(PartData.PartStats stats, PartData.SpecialAbility ability, int budget)
    {
        if (budget <= 0) return;
        
        switch (ability)
        {
            case PartData.SpecialAbility.Taunt:
            case PartData.SpecialAbility.ShieldWall:
            case PartData.SpecialAbility.DamageSharing:
                if (budget >= 10)
                {
                    stats.hp += budget;
                }
                else
                {
                    stats.defense += budget / 2;
                }
                break;
                
            case PartData.SpecialAbility.Flanking:
            case PartData.SpecialAbility.FocusFire:
            case PartData.SpecialAbility.Momentum:
                int atkBudget = budget / 2;
                int critBudget = budget - atkBudget;
                stats.attack += atkBudget;
                stats.critChance += critBudget / 2;
                break;
                
            case PartData.SpecialAbility.RangeAttack:
            case PartData.SpecialAbility.Overwatch:
            case PartData.SpecialAbility.Hunter:
                stats.attack += budget;
                break;
        }
    }
} 
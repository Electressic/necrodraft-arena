using UnityEngine;

public static class PartStatsGenerator
{
    [System.Serializable]
    public class StatChances
    {
        [Header("Core Stats")]
        [Range(0, 1)] public float healthChance = 0.5f;
        [Range(0, 1)] public float attackChance = 0.5f;
        [Range(0, 1)] public float defenseChance = 0.5f;
        
        [Header("Combat Stats")]
        [Range(0, 1)] public float attackSpeedChance = 0.3f;
        [Range(0, 1)] public float critChanceChance = 0.3f;
        [Range(0, 1)] public float critDamageChance = 0.3f;
        
        [Header("Movement Stats")]
        [Range(0, 1)] public float moveSpeedChance = 0.3f;
        [Range(0, 1)] public float rangeChance = 0.3f;
    }
    
    [System.Serializable]
    public class StatRanges
    {
        [Header("Core Stats")]
        public int healthMin = 3, healthMax = 8;
        public int attackMin = 2, attackMax = 5;
        public int defenseMin = 1, defenseMax = 3;
        
        [Header("Combat Stats")]
        public float attackSpeedMin = 0.05f, attackSpeedMax = 0.25f;
        public float critChanceMin = 0.05f, critChanceMax = 0.20f;
        public float critDamageMin = 0.10f, critDamageMax = 0.30f;
        
        [Header("Movement Stats")]
        public float moveSpeedMin = 0.10f, moveSpeedMax = 0.30f;
        public float rangeMin = 0.10f, rangeMax = 0.25f;
    }
    
    /// <summary>
    /// Generate random stats for a part based on its theme, rarity, and type
    /// </summary>
    public static PartData.PartStats Generate(PartData.PartTheme theme, PartData.PartRarity rarity, PartData.PartType partType)
    {
        PartData.PartStats stats = new PartData.PartStats();
        
        // Get theme-specific stat chances
        StatChances chances = GetThemeStatChances(theme);
        
        // Get rarity-specific stat ranges
        StatRanges ranges = GetRarityStatRanges(rarity);
        
        // Roll stats based on chances and ranges
        RollCoreStats(stats, chances, ranges);
        RollCombatStats(stats, chances, ranges);
        RollMovementStats(stats, chances, ranges);
        
        // Ensure minimum stats based on rarity
        EnsureMinimumStats(stats, rarity);
        
        return stats;
    }
    
    private static void RollCoreStats(PartData.PartStats stats, StatChances chances, StatRanges ranges)
    {
        if (Random.value < chances.healthChance)
            stats.health = Random.Range(ranges.healthMin, ranges.healthMax + 1);
            
        if (Random.value < chances.attackChance)
            stats.attack = Random.Range(ranges.attackMin, ranges.attackMax + 1);
            
        if (Random.value < chances.defenseChance)
            stats.defense = Random.Range(ranges.defenseMin, ranges.defenseMax + 1);
    }
    
    private static void RollCombatStats(PartData.PartStats stats, StatChances chances, StatRanges ranges)
    {
        if (Random.value < chances.attackSpeedChance)
            stats.attackSpeed = Random.Range(ranges.attackSpeedMin, ranges.attackSpeedMax);
            
        if (Random.value < chances.critChanceChance)
            stats.critChance = Random.Range(ranges.critChanceMin, ranges.critChanceMax);
            
        if (Random.value < chances.critDamageChance)
            stats.critDamage = Random.Range(ranges.critDamageMin, ranges.critDamageMax);
    }
    
    private static void RollMovementStats(PartData.PartStats stats, StatChances chances, StatRanges ranges)
    {
        if (Random.value < chances.moveSpeedChance)
            stats.moveSpeed = Random.Range(ranges.moveSpeedMin, ranges.moveSpeedMax);
            
        if (Random.value < chances.rangeChance)
            stats.range = Random.Range(ranges.rangeMin, ranges.rangeMax);
    }
    
    private static void EnsureMinimumStats(PartData.PartStats stats, PartData.PartRarity rarity)
    {
        int statsCount = CountNonZeroStats(stats);
        int minStats = GetMinimumStatsForRarity(rarity);
        int maxStats = GetMaximumStatsForRarity(rarity);
        
        // If we have too many stats, we need to remove some
        while (statsCount > maxStats)
        {
            RemoveRandomStat(stats);
            statsCount = CountNonZeroStats(stats);
        }
        
        // If we don't have enough stats, add some basic ones
        while (statsCount < minStats)
        {
            AddRandomBasicStat(stats);
            statsCount = CountNonZeroStats(stats);
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
    
    private static int GetMinimumStatsForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 1;
            case PartData.PartRarity.Rare: return 2;
            case PartData.PartRarity.Epic: return 4;
            default: return 1;
        }
    }
    
    private static int GetMaximumStatsForRarity(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common: return 2;
            case PartData.PartRarity.Rare: return 3;
            case PartData.PartRarity.Epic: return 5;
            default: return 2;
        }
    }
    
    private static void RemoveRandomStat(PartData.PartStats stats)
    {
        // Find all non-zero stats and randomly remove one
        var nonZeroStats = new System.Collections.Generic.List<int>();
        
        if (stats.health > 0) nonZeroStats.Add(0);
        if (stats.attack > 0) nonZeroStats.Add(1);
        if (stats.defense > 0) nonZeroStats.Add(2);
        if (stats.attackSpeed > 0) nonZeroStats.Add(3);
        if (stats.critChance > 0) nonZeroStats.Add(4);
        if (stats.critDamage > 0) nonZeroStats.Add(5);
        if (stats.moveSpeed > 0) nonZeroStats.Add(6);
        if (stats.range > 0) nonZeroStats.Add(7);
        
        if (nonZeroStats.Count > 0)
        {
            int randomIndex = Random.Range(0, nonZeroStats.Count);
            int statToRemove = nonZeroStats[randomIndex];
            
            switch (statToRemove)
            {
                case 0: stats.health = 0; break;
                case 1: stats.attack = 0; break;
                case 2: stats.defense = 0; break;
                case 3: stats.attackSpeed = 0; break;
                case 4: stats.critChance = 0; break;
                case 5: stats.critDamage = 0; break;
                case 6: stats.moveSpeed = 0; break;
                case 7: stats.range = 0; break;
            }
        }
    }
    
    private static void AddRandomBasicStat(PartData.PartStats stats)
    {
        // Add a random basic stat if it's zero
        int attempts = 0;
        while (attempts < 10) // Prevent infinite loop
        {
            int statType = Random.Range(0, 3); // 0=health, 1=attack, 2=defense
            switch (statType)
            {
                case 0:
                    if (stats.health == 0) { stats.health = Random.Range(3, 8); return; }
                    break;
                case 1:
                    if (stats.attack == 0) { stats.attack = Random.Range(2, 5); return; }
                    break;
                case 2:
                    if (stats.defense == 0) { stats.defense = Random.Range(1, 3); return; }
                    break;
            }
            attempts++;
        }
    }
    
    /// <summary>
    /// Get stat chances based on part theme
    /// </summary>
    public static StatChances GetThemeStatChances(PartData.PartTheme theme)
    {
        switch (theme)
        {
            case PartData.PartTheme.Skeleton:
                return new StatChances
                {
                    // Skeleton: Fast, fragile, precise
                    healthChance = 0.2f,        // Low HP (fragile)
                    attackChance = 0.6f,        // Decent attack
                    defenseChance = 0.1f,       // No armor (brittle bones)
                    attackSpeedChance = 0.5f,   // Good attack speed
                    critChanceChance = 0.8f,    // High precision
                    critDamageChance = 0.6f,    // Good crit damage
                    moveSpeedChance = 0.9f,     // Very fast
                    rangeChance = 0.7f          // Good reach
                };
                
            case PartData.PartTheme.Zombie:
                return new StatChances
                {
                    // Zombie: Tanky, slow, sustaining
                    healthChance = 0.9f,        // High HP (tough)
                    attackChance = 0.5f,        // Average attack
                    defenseChance = 0.8f,       // Good armor (thick hide)
                    attackSpeedChance = 0.2f,   // Slow attacks
                    critChanceChance = 0.2f,    // Poor precision
                    critDamageChance = 0.4f,    // Average crit damage
                    moveSpeedChance = 0.2f,     // Very slow
                    rangeChance = 0.3f          // Short reach
                };
                
            case PartData.PartTheme.Ghost:
                return new StatChances
                {
                    // Ghost: Ethereal, magical, balanced
                    healthChance = 0.5f,        // Balanced HP
                    attackChance = 0.5f,        // Balanced attack
                    defenseChance = 0.4f,       // Light armor (ethereal)
                    attackSpeedChance = 0.6f,   // Good attack speed (supernatural)
                    critChanceChance = 0.5f,    // Balanced precision
                    critDamageChance = 0.7f,    // High crit damage (spectral power)
                    moveSpeedChance = 0.6f,     // Good speed (floating)
                    rangeChance = 0.5f          // Balanced range
                };
                
            default:
                return new StatChances(); // Default balanced chances
        }
    }
    
    /// <summary>
    /// Get stat value ranges based on rarity
    /// </summary>
    public static StatRanges GetRarityStatRanges(PartData.PartRarity rarity)
    {
        switch (rarity)
        {
            case PartData.PartRarity.Common:
                return new StatRanges
                {
                    healthMin = 3, healthMax = 8,
                    attackMin = 2, attackMax = 5,
                    defenseMin = 1, defenseMax = 2,
                    attackSpeedMin = 0.05f, attackSpeedMax = 0.15f,
                    critChanceMin = 0.05f, critChanceMax = 0.15f,
                    critDamageMin = 0.10f, critDamageMax = 0.20f,
                    moveSpeedMin = 0.05f, moveSpeedMax = 0.20f,
                    rangeMin = 0.05f, rangeMax = 0.15f
                };
                
            case PartData.PartRarity.Rare:
                return new StatRanges
                {
                    healthMin = 6, healthMax = 12,
                    attackMin = 4, attackMax = 8,
                    defenseMin = 2, defenseMax = 4,
                    attackSpeedMin = 0.10f, attackSpeedMax = 0.25f,
                    critChanceMin = 0.10f, critChanceMax = 0.25f,
                    critDamageMin = 0.15f, critDamageMax = 0.30f,
                    moveSpeedMin = 0.10f, moveSpeedMax = 0.30f,
                    rangeMin = 0.10f, rangeMax = 0.25f
                };
                
            case PartData.PartRarity.Epic:
                return new StatRanges
                {
                    healthMin = 10, healthMax = 18,
                    attackMin = 7, attackMax = 12,
                    defenseMin = 3, defenseMax = 6,
                    attackSpeedMin = 0.20f, attackSpeedMax = 0.40f,
                    critChanceMin = 0.20f, critChanceMax = 0.40f,
                    critDamageMin = 0.25f, critDamageMax = 0.50f,
                    moveSpeedMin = 0.20f, moveSpeedMax = 0.50f,
                    rangeMin = 0.20f, rangeMax = 0.40f
                };
                
            default:
                return new StatRanges(); // Default ranges
        }
    }
} 
using UnityEngine;

[CreateAssetMenu(fileName = "NewPart", menuName = "NecroDraft/Dynamic Part")]
public class PartData : ScriptableObject
{
    public enum PartType { Head, Torso, Arms, Legs }
    
    public enum PartRarity 
    { 
        Common,     // White - Basic parts, 1-2 stats, 12 budget
        Uncommon,   // Green - Solid parts, 2 stats, 20 budget
        Rare,       // Blue - Good parts, 2-3 stats, 32 budget
        Epic        // Purple - Powerful parts, 3-4 stats, 50 budget
    }
    
    public enum PartTheme
    {
        Bone,       // High affinity for Assault (60%) and Marksman (30%)
        Flesh,      // High affinity for Guardian (60%) and Support (30%)
        Spirit,     // High affinity for Trickster (40%) and balanced others (15% each)
        

    }
    
    // Role-based ability categories
    public enum AbilityRole
    {
        None,
        Guardian,   // Tank/protection abilities
        Assault,    // Direct damage abilities
        Marksman,   // Ranged/precision abilities
        Support,    // Team buff/healing abilities
        Trickster   // Utility/positioning abilities
    }
    
    public enum SpecialAbility 
    { 
        None,
        // Guardian Abilities
        Taunt,          // Forces enemy targeting
        ShieldWall,     // Blocks attacks to back row
        DamageSharing,  // Takes damage for adjacent allies
        // Assault Abilities
        Flanking,       // +damage from edge positions
        FocusFire,      // +damage vs low HP enemies
        Momentum,       // +ATK each turn (resets on damage)
        // Marksman Abilities
        RangeAttack,    // Can target back row from back row
        Overwatch,      // Chance to interrupt enemy attacks
        Hunter,         // +damage vs very low HP enemies
        // Support Abilities
        Healing,        // Heals wounded allies
        Inspiration,    // Adjacent allies gain +ATK
        BattleCry,      // All allies gain +ATK (once per combat)
        // Trickster Abilities
        Mobility,       // Can swap positions
        PhaseStep,      // Chance to avoid damage by swapping
        Confuse,        // Chance to make enemies attack randomly
        

    }

    [System.Serializable]
    public class PartStats
    {
        [Header("Core Combat Stats (Point Values)")]
        public int hp = 0;
        public int attack = 0;
        public int defense = 0;
        
        [Header("Advanced Combat Stats")]
        public int critChance = 0;
        public int critDamage = 0;
        public int armorPen = 0;
        

        
        public bool HasAnyStats()
        {
            return hp > 0 || attack > 0 || defense > 0 || 
                   critChance > 0 || critDamage > 0 || armorPen > 0;
        }
        
        public string GetStatsText()
        {
            var stats = new System.Collections.Generic.List<string>();
            
            // New point-based stats
            if (hp > 0) stats.Add($"+{hp} HP");
            if (attack > 0) stats.Add($"+{attack} ATK");
            if (defense > 0) stats.Add($"+{defense} DEF");
            if (critChance > 0) stats.Add($"+{critChance}% Crit Chance");
            if (critDamage > 0) stats.Add($"+{critDamage}% Crit DMG");
            if (armorPen > 0) stats.Add($"+{armorPen} Armor Pen");
            
            return stats.Count > 0 ? string.Join(", ", stats) : "No stat bonuses";
        }
    }

    [Header("Basic Info")]
    public string partName = "New Part";
    public Sprite icon;

    [Header("Part Type & Theme")]
    public PartType type;
    public PartTheme theme;
    public PartRarity rarity = PartRarity.Common;

    [Header("Dynamic Stats")]
    public PartStats stats = new PartStats();

    [Header("Special Ability")]
    public SpecialAbility specialAbility = SpecialAbility.None;
    public int abilityLevel = 1;
    public AbilityRole abilityRole = AbilityRole.None;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
    
    [Header("Budget System (Editor Only)")]
    [Tooltip("Total point budget used for this part")]
    public int totalBudget = 0;
    [Tooltip("Points spent on stats vs abilities")]
    public int statBudget = 0;
    public int abilityBudget = 0;
    
    [Header("Generation (Editor Only)")]
    [Tooltip("Call GenerateRandomStats() from code or create a custom editor script")]
    public bool needsGeneration = false;
    
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case PartRarity.Common: return Color.white;
            case PartRarity.Uncommon: return new Color(0.3f, 1f, 0.3f); // Light green
            case PartRarity.Rare: return new Color(0.3f, 0.7f, 1f); // Light blue
            case PartRarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
            default: return Color.white;
        }
    }
    
    public Color GetThemeColor()
    {
        switch (theme)
        {
            case PartTheme.Bone: return new Color(0.9f, 0.9f, 0.8f);     // Bone white
            case PartTheme.Flesh: return new Color(0.6f, 0.3f, 0.3f);    // Dark red
            case PartTheme.Spirit: return new Color(0.7f, 0.8f, 1f);     // Ethereal blue
            default: return Color.white;
        }
    }
    
    public string GetRarityText()
    {
        return rarity.ToString();
    }
    
    public string GetThemeText()
    {
        return theme.ToString();
    }
    
    public string GetAbilityDescription()
    {
        if (specialAbility == SpecialAbility.None) return "";
        
        string levelText = abilityLevel > 1 ? $" Level {abilityLevel}" : "";
        
        switch (specialAbility)
        {
            case SpecialAbility.Taunt: return $"Taunt{levelText}: Forces enemies to target this minion";
            case SpecialAbility.ShieldWall: return $"Shield Wall{levelText}: Blocks {abilityLevel} attacks to back row";
            case SpecialAbility.DamageSharing: return $"Damage Sharing{levelText}: Takes {25 * abilityLevel}% damage for adjacent allies";
            case SpecialAbility.Flanking: return $"Flanking{levelText}: +{25 * abilityLevel}% damage from edge positions";
            case SpecialAbility.FocusFire: return $"Focus Fire{levelText}: +{25 * abilityLevel}% damage vs enemies below 50% HP";
            case SpecialAbility.Momentum: return $"Momentum{levelText}: +{abilityLevel} ATK each turn (resets on damage)";
            case SpecialAbility.RangeAttack: return $"Range Attack{levelText}: Can target back row from back row";
            case SpecialAbility.Overwatch: return $"Overwatch{levelText}: {25 + 15 * (abilityLevel-1)}% chance to interrupt attacks";
            case SpecialAbility.Hunter: return $"Hunter{levelText}: +{50 * abilityLevel}% damage vs enemies below 25% HP";
            case SpecialAbility.Healing: return $"Healing{levelText}: Heals {5 * abilityLevel}% max HP to wounded ally each turn";
            case SpecialAbility.Inspiration: return $"Inspiration{levelText}: Adjacent allies gain +{10 * abilityLevel}% ATK";
            case SpecialAbility.BattleCry: return $"Battle Cry{levelText}: All allies gain +{15 * abilityLevel}% ATK for {abilityLevel + 1} turns";
            case SpecialAbility.Mobility: return $"Mobility{levelText}: Can swap positions {abilityLevel} times per combat";
            case SpecialAbility.PhaseStep: return $"Phase Step{levelText}: {25 + 15 * (abilityLevel-1)}% chance to avoid damage";
            case SpecialAbility.Confuse: return $"Confuse{levelText}: {25 + 15 * (abilityLevel-1)}% chance to confuse on hit";
            default: return "";
        }
    }
    
    public string GetFullDescription()
    {
        string desc = $"{partName} ({theme} {rarity})\n";
        desc += stats.GetStatsText() + "\n";
        
        if (specialAbility != SpecialAbility.None)
            desc += "Ability: " + GetAbilityDescription();
            
        return desc;
    }
    
    public void GenerateRandomStats()
    {
        stats = PartStatsGenerator.Generate(theme, rarity, type);
    }
    
    public void MigrateLegacyStats()
    {
        if (hpBonus > 0 && stats.hp == 0)
        {
            stats.hp = hpBonus;
        }
        if (attackBonus > 0 && stats.attack == 0)
        {
            stats.attack = attackBonus;
        }
        
        if (!stats.HasAnyStats() && hpBonus == 0 && attackBonus == 0)
        {
            GenerateRandomStats();
        }
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            MigrateLegacyStats();
        }
    }
    
    [Header("Legacy Stats (Old Format - Will be migrated)")]
    public int hpBonus = 0;
    public int attackBonus = 0;
    
    public int GetHPBonus() => stats.hp > 0 ? stats.hp : hpBonus;
    public int GetAttackBonus() => stats.attack > 0 ? stats.attack : attackBonus;
    
    public int GetTotalBudgetUsed()
    {
        int statCost = (stats.hp * 1) + (stats.attack * 1) + (stats.defense * 2) + 
                      (stats.critChance * 2) + (stats.critDamage * 1) + (stats.armorPen * 3);
        int abilityCost = GetAbilityCost();
        return statCost + abilityCost;
    }
    
    public int GetAbilityCost()
    {
        if (specialAbility == SpecialAbility.None) return 0;
        
        return specialAbility switch
        {
            SpecialAbility.Taunt => new int[] { 4, 6, 8 }[abilityLevel - 1],
            SpecialAbility.ShieldWall => new int[] { 6, 9, 12 }[abilityLevel - 1],
            SpecialAbility.DamageSharing => new int[] { 5, 8, 11 }[abilityLevel - 1],
            SpecialAbility.Flanking => new int[] { 4, 7, 10 }[abilityLevel - 1],
            SpecialAbility.FocusFire => new int[] { 5, 8, 11 }[abilityLevel - 1],
            SpecialAbility.Momentum => new int[] { 6, 9, 12 }[abilityLevel - 1],
            SpecialAbility.RangeAttack => new int[] { 8, 12, 16 }[abilityLevel - 1],
            SpecialAbility.Overwatch => new int[] { 7, 11, 15 }[abilityLevel - 1],
            SpecialAbility.Hunter => new int[] { 5, 8, 11 }[abilityLevel - 1],
            SpecialAbility.Healing => new int[] { 3, 5, 7 }[abilityLevel - 1],
            SpecialAbility.Inspiration => new int[] { 4, 6, 8 }[abilityLevel - 1],
            SpecialAbility.BattleCry => new int[] { 8, 12, 16 }[abilityLevel - 1],
            SpecialAbility.Mobility => new int[] { 10, 15, 20 }[abilityLevel - 1],
            SpecialAbility.PhaseStep => new int[] { 12, 18, 24 }[abilityLevel - 1],
            SpecialAbility.Confuse => new int[] { 6, 9, 12 }[abilityLevel - 1],
            _ => 0
        };
    }
    
    public AbilityRole GetAbilityRole()
    {
        return specialAbility switch
        {
            SpecialAbility.Taunt or SpecialAbility.ShieldWall or SpecialAbility.DamageSharing => AbilityRole.Guardian,
            SpecialAbility.Flanking or SpecialAbility.FocusFire or SpecialAbility.Momentum => AbilityRole.Assault,
            SpecialAbility.RangeAttack or SpecialAbility.Overwatch or SpecialAbility.Hunter => AbilityRole.Marksman,
            SpecialAbility.Healing or SpecialAbility.Inspiration or SpecialAbility.BattleCry => AbilityRole.Support,
            SpecialAbility.Mobility or SpecialAbility.PhaseStep or SpecialAbility.Confuse => AbilityRole.Trickster,
            _ => AbilityRole.None
        };
    }
}

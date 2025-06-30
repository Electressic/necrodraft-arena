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
        Skeleton,   // Fast, fragile, precise (speed, range, crit focused)
        Zombie,     // Tanky, slow, sustaining (health, defense, regen focused)
        Ghost       // Ethereal, magical, balanced (speed + magic, balanced stats)
    }
    
    public enum SpecialAbility 
    { 
        None,           // No special ability
        Berserker,      // +50% attack speed when below 50% HP
        Armored,        // Reduce incoming damage by 1 (min 1)
        Swift,          // +100% move speed, -25% HP
        Poison,         // Attacks deal 2 damage over 3 seconds
        Regeneration,   // Heal 1 HP every 3 seconds
        CriticalStrike, // 25% chance to deal double damage
        Thorns,         // Reflect 1 damage to attackers
        Vampiric        // Heal for 25% of damage dealt
    }

    [System.Serializable]
    public class PartStats
    {
        [Header("Core Stats (Percentage Multipliers)")]
        public float health = 0.0f;         // Health multiplier bonus (0.0 to 1.0+)
        public float attack = 0.0f;         // Attack multiplier bonus (0.0 to 1.0+)
        public float defense = 0.0f;        // Defense multiplier bonus (0.0 to 1.0+)
        
        [Header("Combat Stats")]
        public float attackSpeed = 0.0f;    // Attack speed multiplier bonus (0.0 to 1.0+)
        public float critChance = 0.0f;     // Critical hit chance (0.0 to 1.0)
        public float critDamage = 0.0f;     // Critical damage multiplier bonus
        
        [Header("Movement Stats")]
        public float moveSpeed = 0.0f;      // Move speed multiplier bonus
        public float range = 0.0f;          // Attack range multiplier bonus
        
        public bool HasAnyStats()
        {
            return health > 0 || attack > 0 || defense > 0 || 
                   attackSpeed > 0 || critChance > 0 || critDamage > 0 || 
                   moveSpeed > 0 || range > 0;
        }
        
        public string GetStatsText()
        {
            var stats = new System.Collections.Generic.List<string>();
            
            if (health > 0) stats.Add($"+{(health*100):F0}% HP");
            if (attack > 0) stats.Add($"+{(attack*100):F0}% ATK");
            if (defense > 0) stats.Add($"+{(defense*100):F0}% DEF");
            if (attackSpeed > 0) stats.Add($"+{(attackSpeed*100):F0}% ATK Speed");
            if (critChance > 0) stats.Add($"+{(critChance*100):F0}% Crit Chance");
            if (critDamage > 0) stats.Add($"+{(critDamage*100):F0}% Crit DMG");
            if (moveSpeed > 0) stats.Add($"+{(moveSpeed*100):F0}% Move Speed");
            if (range > 0) stats.Add($"+{(range*100):F0}% Range");
            
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

    [Header("Set Bonus Ability")]
    public SpecialAbility specialAbility = SpecialAbility.None;
    public float abilityValue = 0f; // Used for ability parameters

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
    
    [Header("Generation (Editor Only)")]
    [Tooltip("Call GenerateRandomStats() from code or create a custom editor script")]
    public bool needsGeneration = false;
    
    // Helper methods
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
            case PartTheme.Skeleton: return new Color(0.9f, 0.9f, 0.8f); // Bone white
            case PartTheme.Zombie: return new Color(0.4f, 0.6f, 0.3f);   // Sickly green
            case PartTheme.Ghost: return new Color(0.7f, 0.8f, 1f);      // Ethereal blue
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
        switch (specialAbility)
        {
            case SpecialAbility.None: return "";
            case SpecialAbility.Berserker: return "Berserker: +50% attack speed when below 50% HP";
            case SpecialAbility.Armored: return "Armored: Reduce incoming damage by 1";
            case SpecialAbility.Swift: return "Swift: +100% move speed, -25% HP";
            case SpecialAbility.Poison: return "Poison: Attacks deal 2 damage over 3 seconds";
            case SpecialAbility.Regeneration: return "Regeneration: Heal 1 HP every 3 seconds";
            case SpecialAbility.CriticalStrike: return "Critical Strike: 25% chance for double damage";
            case SpecialAbility.Thorns: return "Thorns: Reflect 1 damage to attackers";
            case SpecialAbility.Vampiric: return "Vampiric: Heal for 25% of damage dealt";
            default: return "";
        }
    }
    
    public string GetFullDescription()
    {
        string desc = $"{partName} ({theme} {rarity})\n";
        desc += stats.GetStatsText() + "\n";
        
        if (specialAbility != SpecialAbility.None)
            desc += "Set Bonus: " + GetAbilityDescription();
            
        return desc;
    }
    
    // Generate random stats based on theme and rarity (for editor use)
    public void GenerateRandomStats()
    {
        stats = PartStatsGenerator.Generate(theme, rarity, type);
    }
    
    // Migration helper - converts old format to new format
    public void MigrateLegacyStats()
    {
        if (hpBonus > 0 && stats.health == 0)
        {
            // Convert old flat HP bonus to percentage (assume base ~20 HP)
            stats.health = hpBonus / 20.0f; // Convert flat to ~percentage
        }
        if (attackBonus > 0 && stats.attack == 0)
        {
            // Convert old flat attack bonus to percentage (assume base ~5 ATK)  
            stats.attack = attackBonus / 5.0f; // Convert flat to ~percentage
        }
        
        // Ensure we have some basic stats if both old and new are empty
        if (!stats.HasAnyStats() && hpBonus == 0 && attackBonus == 0)
        {
            GenerateRandomStats();
        }
    }
    
    // Awake-like function to ensure data integrity
    void OnValidate()
    {
        // Auto-migrate legacy stats when asset is loaded/validated
        if (Application.isPlaying)
        {
            MigrateLegacyStats();
        }
    }
    
    // Legacy format support (for old part assets)
    [Header("Legacy Stats (Old Format - Will be migrated)")]
    public int hpBonus = 0;
    public int attackBonus = 0;
    
    // Backwards compatibility for existing systems
    public int GetHPBonus() => stats.health > 0 ? Mathf.RoundToInt(stats.health * 20) : hpBonus; // Convert percentage back to rough flat value
    public int GetAttackBonus() => stats.attack > 0 ? Mathf.RoundToInt(stats.attack * 5) : attackBonus; // Convert percentage back to rough flat value
}

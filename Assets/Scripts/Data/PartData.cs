using UnityEngine;

[CreateAssetMenu(fileName = "NewPart", menuName = "Scriptable Objects/PartData")]
public class PartData : ScriptableObject
{
    public enum PartType { Head, Torso, Arms, Legs }
    
    public enum PartRarity 
    { 
        Common,     // White - Basic parts
        Rare,       // Blue - Good abilities  
        Epic        // Purple - Powerful abilities
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

    [Header("Basic Info")]
    public string partName = "New Part";
    public Sprite icon;

    [Header("Part Type & Rarity")]
    public PartType type;
    public PartRarity rarity = PartRarity.Common;

    [Header("Stats")]
    public int hpBonus = 0;
    public int attackBonus = 0;

    [Header("Special Ability")]
    public SpecialAbility specialAbility = SpecialAbility.None;
    public float abilityValue = 0f; // Used for ability parameters (damage, healing, etc.)

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
    
    // Helper methods
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case PartRarity.Common: return Color.white;
            case PartRarity.Rare: return new Color(0.3f, 0.7f, 1f); // Light blue
            case PartRarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
            default: return Color.white;
        }
    }
    
    public string GetRarityText()
    {
        return rarity.ToString();
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
}

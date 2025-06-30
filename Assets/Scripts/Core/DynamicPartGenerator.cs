using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class DynamicPartGenerator
{
    // Part name templates by type and theme
    private static readonly Dictionary<PartData.PartType, Dictionary<PartData.PartTheme, string[]>> partNameTemplates = 
        new Dictionary<PartData.PartType, Dictionary<PartData.PartTheme, string[]>>()
        {
            {
                PartData.PartType.Head,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Skeleton, new[] { "Bone Skull", "Ancient Cranium", "Spectral Head", "Hollow Skull", "Brittle Crown" } },
                    { PartData.PartTheme.Zombie, new[] { "Rotting Head", "Decayed Brain", "Festering Skull", "Putrid Crown", "Bloated Head" } },
                    { PartData.PartTheme.Ghost, new[] { "Ethereal Visage", "Phantom Face", "Spectral Crown", "Wraith Head", "Spirit Essence" } }
                }
            },
            {
                PartData.PartType.Torso,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Skeleton, new[] { "Rib Cage", "Bone Frame", "Calcified Torso", "Ancient Chest", "Hollow Core" } },
                    { PartData.PartTheme.Zombie, new[] { "Rotting Torso", "Putrid Chest", "Decayed Body", "Festering Core", "Bloated Frame" } },
                    { PartData.PartTheme.Ghost, new[] { "Ethereal Body", "Phantom Torso", "Spectral Core", "Wraith Frame", "Spirit Vessel" } }
                }
            },
            {
                PartData.PartType.Arms,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Skeleton, new[] { "Bone Claws", "Skeletal Arms", "Ancient Limbs", "Brittle Grasp", "Calcified Reach" } },
                    { PartData.PartTheme.Zombie, new[] { "Rotting Arms", "Putrid Claws", "Decayed Grasp", "Festering Limbs", "Bloated Hands" } },
                    { PartData.PartTheme.Ghost, new[] { "Ethereal Arms", "Phantom Grasp", "Spectral Reach", "Wraith Claws", "Spirit Touch" } }
                }
            },
            {
                PartData.PartType.Legs,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Skeleton, new[] { "Bone Legs", "Swift Femurs", "Ancient Stride", "Brittle Steps", "Calcified Gait" } },
                    { PartData.PartTheme.Zombie, new[] { "Rotting Legs", "Putrid Stride", "Decayed Steps", "Festering Gait", "Bloated Limbs" } },
                    { PartData.PartTheme.Ghost, new[] { "Ethereal Legs", "Phantom Stride", "Spectral Steps", "Wraith Gait", "Spirit Float" } }
                }
            }
        };

    // Rarity probability tables by wave (Common, Uncommon, Rare, Epic)
    private static readonly Dictionary<int, float[]> waveRarityProbabilities = new Dictionary<int, float[]>()
    {
        // Act 1: Foundation (Waves 1-7)
        { 1, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } }, // Wave 1: 100% Common
        { 2, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } }, // Wave 2: 100% Common
        { 3, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } }, // Wave 3: 100% Common
        { 4, new float[] { 0.80f, 0.20f, 0.00f, 0.00f } }, // Wave 4: 80% Common, 20% Uncommon
        { 5, new float[] { 0.70f, 0.30f, 0.00f, 0.00f } }, // Wave 5: 70% Common, 30% Uncommon
        { 6, new float[] { 0.60f, 0.40f, 0.00f, 0.00f } }, // Wave 6: 60% Common, 40% Uncommon
        { 7, new float[] { 0.50f, 0.50f, 0.00f, 0.00f } }, // Wave 7: 50% Common, 50% Uncommon
        
        // Act 2: Mastery (Waves 8-14)
        { 8, new float[] { 0.30f, 0.60f, 0.10f, 0.00f } }, // Wave 8: 30% Common, 60% Uncommon, 10% Rare
        { 9, new float[] { 0.20f, 0.60f, 0.20f, 0.00f } }, // Wave 9: 20% Common, 60% Uncommon, 20% Rare
        { 10, new float[] { 0.10f, 0.60f, 0.30f, 0.00f } }, // Wave 10: Boss wave
        { 11, new float[] { 0.10f, 0.50f, 0.40f, 0.00f } }, // Wave 11: 10% Common, 50% Uncommon, 40% Rare
        { 12, new float[] { 0.00f, 1.00f, 0.00f, 0.00f } }, // Wave 12: 100% Uncommon
        { 13, new float[] { 0.00f, 0.40f, 0.60f, 0.00f } }, // Wave 13: 40% Uncommon, 60% Rare
        { 14, new float[] { 0.00f, 0.30f, 0.70f, 0.00f } }, // Wave 14: 30% Uncommon, 70% Rare
        
        // Act 3: Endgame (Waves 15-20)
        { 15, new float[] { 0.00f, 0.10f, 0.60f, 0.30f } }, // Wave 15: 10% Uncommon, 60% Rare, 30% Epic
        { 16, new float[] { 0.00f, 0.10f, 0.50f, 0.40f } }, // Wave 16: 10% Uncommon, 50% Rare, 40% Epic
        { 17, new float[] { 0.00f, 0.00f, 0.40f, 0.60f } }, // Wave 17: 40% Rare, 60% Epic
        { 18, new float[] { 0.00f, 0.00f, 0.30f, 0.70f } }, // Wave 18: 30% Rare, 70% Epic
        { 19, new float[] { 0.00f, 0.00f, 0.00f, 1.00f } }, // Wave 19: 100% Epic (Treasure Ghoul)
        { 20, new float[] { 0.00f, 0.00f, 0.00f, 0.00f } }  // Wave 20: Final Boss (no parts)
    };

    /// <summary>
    /// Generate a set of random parts for card selection
    /// </summary>
    public static List<PartData> GenerateCardSelection(int wave = 1, NecromancerClass playerClass = null, int cardCount = 3)
    {
        List<PartData> generatedParts = new List<PartData>();
        
        for (int i = 0; i < cardCount; i++)
        {
            PartData newPart = GenerateRandomPart(wave, playerClass);
            generatedParts.Add(newPart);
        }
        
        Debug.Log($"[DynamicPartGenerator] Generated {cardCount} parts for wave {wave}: {string.Join(", ", generatedParts.Select(p => $"{p.partName} ({p.rarity})"))}");
        return generatedParts;
    }

    /// <summary>
    /// Generate a single random part
    /// </summary>
    public static PartData GenerateRandomPart(int wave = 1, NecromancerClass playerClass = null)
    {
        // Create a new ScriptableObject instance
        PartData part = ScriptableObject.CreateInstance<PartData>();
        
        // Determine rarity based on wave
        PartData.PartRarity rarity = DetermineRarity(wave);
        
        // Random part type and theme
        PartData.PartType partType = GetRandomPartType();
        PartData.PartTheme theme = GetRandomTheme(playerClass);
        
        // Generate name
        string partName = GeneratePartName(partType, theme, rarity);
        
        // Generate stats
        PartData.PartStats stats = PartStatsGenerator.Generate(theme, rarity, partType);
        
        // Random special ability based on rarity
        PartData.SpecialAbility ability = GenerateSpecialAbility(rarity, theme);
        
        // Set up the part
        part.partName = partName;
        part.type = partType;
        part.theme = theme;
        part.rarity = rarity;
        part.stats = stats;
        part.specialAbility = ability;
        part.description = GenerateDescription(part);
        
        return part;
    }

    static PartData.PartRarity DetermineRarity(int wave)
    {
        // Get rarity probabilities for this wave (default to wave 19 for high waves)
        float[] probabilities = waveRarityProbabilities.ContainsKey(wave) 
            ? waveRarityProbabilities[wave] 
            : waveRarityProbabilities[19];
        
        float roll = Random.value;
        float cumulative = 0f;
        
        // Check Common (index 0)
        cumulative += probabilities[0];
        if (roll < cumulative)
            return PartData.PartRarity.Common;
        
        // Check Uncommon (index 1)
        cumulative += probabilities[1];
        if (roll < cumulative)
            return PartData.PartRarity.Uncommon;
        
        // Check Rare (index 2)
        cumulative += probabilities[2];
        if (roll < cumulative)
            return PartData.PartRarity.Rare;
        
        // Default to Epic (index 3)
        return PartData.PartRarity.Epic;
    }

    static PartData.PartType GetRandomPartType()
    {
        var values = System.Enum.GetValues(typeof(PartData.PartType));
        return (PartData.PartType)values.GetValue(Random.Range(0, values.Length));
    }

    static PartData.PartTheme GetRandomTheme(NecromancerClass playerClass)
    {
        // If player class provides theme preference, bias towards it
        if (playerClass != null)
        {
            // TODO: Add class theme preferences when NecromancerClass is implemented
            // For now, random
        }
        
        var values = System.Enum.GetValues(typeof(PartData.PartTheme));
        return (PartData.PartTheme)values.GetValue(Random.Range(0, values.Length));
    }

    static string GeneratePartName(PartData.PartType partType, PartData.PartTheme theme, PartData.PartRarity rarity)
    {
        if (!partNameTemplates.ContainsKey(partType) || !partNameTemplates[partType].ContainsKey(theme))
        {
            return $"{theme} {partType}";
        }
        
        string[] names = partNameTemplates[partType][theme];
        string baseName = names[Random.Range(0, names.Length)];
        
        // Add rarity prefix for higher rarities
        switch (rarity)
        {
            case PartData.PartRarity.Uncommon:
                return Random.value < 0.5f ? $"Sturdy {baseName}" : $"Refined {baseName}";
            case PartData.PartRarity.Rare:
                return Random.value < 0.5f ? $"Superior {baseName}" : $"Enhanced {baseName}";
            case PartData.PartRarity.Epic:
                return Random.value < 0.5f ? $"Legendary {baseName}" : $"Exalted {baseName}";
            default:
                return baseName;
        }
    }

    static PartData.SpecialAbility GenerateSpecialAbility(PartData.PartRarity rarity, PartData.PartTheme theme)
    {
        // Higher rarity = higher chance of special ability
        float abilityChance = rarity switch
        {
            PartData.PartRarity.Common => 0.1f,     // 10% chance
            PartData.PartRarity.Uncommon => 0.2f,   // 20% chance
            PartData.PartRarity.Rare => 0.4f,       // 40% chance
            PartData.PartRarity.Epic => 0.8f,       // 80% chance
            _ => 0.0f
        };
        
        if (Random.value > abilityChance)
            return PartData.SpecialAbility.None;
        
        // Theme-based ability preferences
        var themeAbilities = theme switch
        {
            PartData.PartTheme.Skeleton => new[] { 
                PartData.SpecialAbility.Swift, 
                PartData.SpecialAbility.CriticalStrike,
                PartData.SpecialAbility.None 
            },
            PartData.PartTheme.Zombie => new[] { 
                PartData.SpecialAbility.Regeneration, 
                PartData.SpecialAbility.Armored,
                PartData.SpecialAbility.Vampiric 
            },
            PartData.PartTheme.Ghost => new[] { 
                PartData.SpecialAbility.Swift, 
                PartData.SpecialAbility.Poison,
                PartData.SpecialAbility.Thorns 
            },
            _ => new[] { PartData.SpecialAbility.None }
        };
        
        return themeAbilities[Random.Range(0, themeAbilities.Length)];
    }

    static string GenerateDescription(PartData part)
    {
        string themeDesc = part.theme switch
        {
            PartData.PartTheme.Skeleton => "Made from ancient bones, light but brittle.",
            PartData.PartTheme.Zombie => "Rotting flesh that refuses to die.",
            PartData.PartTheme.Ghost => "Ethereal essence bound to physical form.",
            _ => "A mysterious undead component."
        };
        
        string abilityDesc = part.specialAbility != PartData.SpecialAbility.None 
            ? $" {part.GetAbilityDescription()}" 
            : "";
        
        return $"{themeDesc}{abilityDesc}";
    }
} 
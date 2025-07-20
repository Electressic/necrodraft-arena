using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class DynamicPartGenerator
{
    private static readonly Dictionary<PartData.PartType, Dictionary<PartData.PartTheme, string[]>> partNameTemplates = 
        new Dictionary<PartData.PartType, Dictionary<PartData.PartTheme, string[]>>()
        {
            {
                PartData.PartType.Head,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Bone, new[] { "Bone Skull", "Ancient Cranium", "Hollow Skull", "Brittle Crown", "Calcified Head" } },
                    { PartData.PartTheme.Flesh, new[] { "Rotting Head", "Decayed Brain", "Festering Skull", "Putrid Crown", "Bloated Head" } },
                    { PartData.PartTheme.Spirit, new[] { "Ethereal Visage", "Phantom Face", "Spectral Crown", "Wraith Head", "Spirit Essence" } }
                }
            },
            {
                PartData.PartType.Torso,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Bone, new[] { "Rib Cage", "Bone Frame", "Calcified Torso", "Ancient Chest", "Hollow Core" } },
                    { PartData.PartTheme.Flesh, new[] { "Rotting Torso", "Putrid Chest", "Decayed Body", "Festering Core", "Bloated Frame" } },
                    { PartData.PartTheme.Spirit, new[] { "Ethereal Body", "Phantom Torso", "Spectral Core", "Wraith Frame", "Spirit Vessel" } }
                }
            },
            {
                PartData.PartType.Arms,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Bone, new[] { "Bone Claws", "Skeletal Arms", "Ancient Limbs", "Brittle Grasp", "Calcified Reach" } },
                    { PartData.PartTheme.Flesh, new[] { "Rotting Arms", "Putrid Claws", "Decayed Grasp", "Festering Limbs", "Bloated Hands" } },
                    { PartData.PartTheme.Spirit, new[] { "Ethereal Arms", "Phantom Grasp", "Spectral Reach", "Wraith Claws", "Spirit Touch" } }
                }
            },
            {
                PartData.PartType.Legs,
                new Dictionary<PartData.PartTheme, string[]>()
                {
                    { PartData.PartTheme.Bone, new[] { "Bone Legs", "Swift Femurs", "Ancient Stride", "Brittle Steps", "Calcified Gait" } },
                    { PartData.PartTheme.Flesh, new[] { "Rotting Legs", "Putrid Stride", "Decayed Steps", "Festering Gait", "Bloated Limbs" } },
                    { PartData.PartTheme.Spirit, new[] { "Ethereal Legs", "Phantom Stride", "Spectral Steps", "Wraith Gait", "Spirit Float" } }
                }
            }
        };

    private static readonly Dictionary<int, float[]> waveRarityProbabilities = new Dictionary<int, float[]>()
    {
        { 1, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } },
        { 2, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } },
        { 3, new float[] { 1.00f, 0.00f, 0.00f, 0.00f } },
        { 4, new float[] { 0.80f, 0.20f, 0.00f, 0.00f } },
        { 5, new float[] { 0.70f, 0.30f, 0.00f, 0.00f } },
        { 6, new float[] { 0.60f, 0.40f, 0.00f, 0.00f } },
        { 7, new float[] { 0.50f, 0.50f, 0.00f, 0.00f } },
        { 8, new float[] { 0.30f, 0.60f, 0.10f, 0.00f } },
        { 9, new float[] { 0.20f, 0.60f, 0.20f, 0.00f } },
        { 10, new float[] { 0.10f, 0.60f, 0.30f, 0.00f } },
        { 11, new float[] { 0.10f, 0.50f, 0.40f, 0.00f } },
        { 12, new float[] { 0.00f, 1.00f, 0.00f, 0.00f } },
        { 13, new float[] { 0.00f, 0.40f, 0.60f, 0.00f } },
        { 14, new float[] { 0.00f, 0.30f, 0.70f, 0.00f } },
        { 15, new float[] { 0.00f, 0.10f, 0.60f, 0.30f } },
        { 16, new float[] { 0.00f, 0.10f, 0.50f, 0.40f } },
        { 17, new float[] { 0.00f, 0.00f, 0.40f, 0.60f } },
        { 18, new float[] { 0.00f, 0.00f, 0.30f, 0.70f } },
        { 19, new float[] { 0.00f, 0.00f, 0.00f, 1.00f } },
        { 20, new float[] { 0.00f, 0.00f, 0.00f, 0.00f } }
    };

    public static List<PartData> GenerateCardSelection(int wave = 1, NecromancerClass playerClass = null, int cardCount = 3)
    {
        List<PartData> generatedParts = new List<PartData>();
        for (int i = 0; i < cardCount; i++)
        {
            PartData newPart = GenerateRandomPart(wave, playerClass);
            generatedParts.Add(newPart);
        }
        return generatedParts;
    }

    public static PartData GenerateRandomPart(int wave = 1, NecromancerClass playerClass = null)
    {
        PartData part = ScriptableObject.CreateInstance<PartData>();
        PartData.PartRarity rarity = DetermineRarity(wave);
        PartData.PartType partType = GetRandomPartType();
        PartData.PartTheme theme = GetRandomTheme(playerClass);
        string partName = GeneratePartName(partType, theme, rarity);
        PartData.SpecialAbility ability;
        int abilityLevel;
        PartData.PartStats stats = PartStatsGenerator.GenerateWithAbility(theme, rarity, partType, out ability, out abilityLevel);
        part.partName = partName;
        part.type = partType;
        part.theme = theme;
        part.rarity = rarity;
        part.stats = stats;
        part.specialAbility = ability;
        part.abilityLevel = abilityLevel;
        part.abilityRole = part.GetAbilityRole();
        part.totalBudget = PartStatsGenerator.GetStatBudgetForRarity(rarity);
        part.statBudget = part.totalBudget - part.GetAbilityCost();
        part.abilityBudget = part.GetAbilityCost();
        part.description = GenerateDescription(part);
        return part;
    }

    static PartData.PartRarity DetermineRarity(int wave)
    {
        float[] probabilities = waveRarityProbabilities.ContainsKey(wave) 
            ? waveRarityProbabilities[wave] 
            : waveRarityProbabilities[19];
        float roll = Random.value;
        float cumulative = 0f;
        cumulative += probabilities[0];
        if (roll < cumulative)
            return PartData.PartRarity.Common;
        cumulative += probabilities[1];
        if (roll < cumulative)
            return PartData.PartRarity.Uncommon;
        cumulative += probabilities[2];
        if (roll < cumulative)
            return PartData.PartRarity.Rare;
        return PartData.PartRarity.Epic;
    }

    static PartData.PartType GetRandomPartType()
    {
        var values = System.Enum.GetValues(typeof(PartData.PartType));
        return (PartData.PartType)values.GetValue(Random.Range(0, values.Length));
    }

    static PartData.PartTheme GetRandomTheme(NecromancerClass playerClass)
    {
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

    static string GenerateDescription(PartData part)
    {
        string themeDesc = part.theme switch
        {
            PartData.PartTheme.Bone => "Crafted from ancient bone, lightweight yet durable.",
            PartData.PartTheme.Flesh => "Rotting flesh infused with dark magic.",
            PartData.PartTheme.Spirit => "Ethereal essence bound to spectral form.",
            _ => "A mysterious undead component."
        };
        string roleDesc = part.abilityRole switch
        {
            PartData.AbilityRole.Guardian => " Provides defensive capabilities.",
            PartData.AbilityRole.Assault => " Enhances offensive prowess.",
            PartData.AbilityRole.Marksman => " Improves ranged precision.",
            PartData.AbilityRole.Support => " Offers team support abilities.",
            PartData.AbilityRole.Trickster => " Grants tactical flexibility.",
            _ => ""
        };
        string abilityDesc = part.specialAbility != PartData.SpecialAbility.None 
            ? $" {part.GetAbilityDescription()}" 
            : "";
        return $"{themeDesc}{roleDesc}{abilityDesc}";
    }
} 
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class PartMigrationUtility : EditorWindow
{
    [MenuItem("NecroDraft/Migrate Parts to New Stats System")]
    public static void ShowWindow()
    {
        GetWindow<PartMigrationUtility>("Part Migration Utility");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Part Migration Utility", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will convert all existing parts from legacy stats (hpBonus/attackBonus) to the new dynamic stats system.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Migrate All Parts", GUILayout.Height(30)))
        {
            MigrateAllParts();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Random Stats for All Parts", GUILayout.Height(30)))
        {
            GenerateRandomStatsForAllParts();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Options:", EditorStyles.boldLabel);
        if (GUILayout.Button("Set All Parts to Common Rarity"))
        {
            SetAllPartsRarity(PartData.PartRarity.Common);
        }
        
        if (GUILayout.Button("Set Random Rarities (70% Common, 25% Rare, 5% Epic)"))
        {
            SetRandomRarities();
        }
    }
    
    void MigrateAllParts()
    {
        // Find all PartData assets
        string[] partGuids = AssetDatabase.FindAssets("t:PartData");
        List<PartData> allParts = new List<PartData>();
        
        foreach (string guid in partGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartData part = AssetDatabase.LoadAssetAtPath<PartData>(path);
            if (part != null)
            {
                allParts.Add(part);
            }
        }
        
        Debug.Log($"[PartMigration] Found {allParts.Count} parts to migrate");
        
        foreach (PartData part in allParts)
        {
            MigratePart(part);
        }
        
        // Save all changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[PartMigration] Successfully migrated {allParts.Count} parts!");
    }
    
    void MigratePart(PartData part)
    {
        bool changed = false;
        
        // Migrate legacy stats to new system if they exist
        if (part.hpBonus > 0 && part.stats.health == 0)
        {
            part.stats.health = part.hpBonus;
            part.hpBonus = 0; // Clear legacy value
            changed = true;
        }
        
        if (part.attackBonus > 0 && part.stats.attack == 0)
        {
            part.stats.attack = part.attackBonus;
            part.attackBonus = 0; // Clear legacy value
            changed = true;
        }
        
        // If part has no stats at all, generate some
        if (!part.stats.HasAnyStats())
        {
            part.GenerateRandomStats();
            changed = true;
        }
        
        if (changed)
        {
            EditorUtility.SetDirty(part);
            Debug.Log($"[PartMigration] Migrated: {part.partName} ({part.rarity}) - {part.stats.GetStatsText()}");
        }
    }
    
    void GenerateRandomStatsForAllParts()
    {
        string[] partGuids = AssetDatabase.FindAssets("t:PartData");
        List<PartData> allParts = new List<PartData>();
        
        foreach (string guid in partGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartData part = AssetDatabase.LoadAssetAtPath<PartData>(path);
            if (part != null)
            {
                allParts.Add(part);
            }
        }
        
        foreach (PartData part in allParts)
        {
            // Clear existing stats and generate new ones
            part.stats = new PartData.PartStats();
            part.GenerateRandomStats();
            EditorUtility.SetDirty(part);
            Debug.Log($"[PartMigration] Generated new stats for: {part.partName} ({part.rarity}) - {part.stats.GetStatsText()}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[PartMigration] Generated new stats for {allParts.Count} parts!");
    }
    
    void SetAllPartsRarity(PartData.PartRarity targetRarity)
    {
        string[] partGuids = AssetDatabase.FindAssets("t:PartData");
        int count = 0;
        
        foreach (string guid in partGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartData part = AssetDatabase.LoadAssetAtPath<PartData>(path);
            if (part != null)
            {
                part.rarity = targetRarity;
                EditorUtility.SetDirty(part);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[PartMigration] Set {count} parts to {targetRarity} rarity!");
    }
    
    void SetRandomRarities()
    {
        string[] partGuids = AssetDatabase.FindAssets("t:PartData");
        int commonCount = 0, rareCount = 0, epicCount = 0;
        
        foreach (string guid in partGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartData part = AssetDatabase.LoadAssetAtPath<PartData>(path);
            if (part != null)
            {
                float roll = Random.value;
                if (roll < 0.70f) // 70% Common
                {
                    part.rarity = PartData.PartRarity.Common;
                    commonCount++;
                }
                else if (roll < 0.95f) // 25% Rare
                {
                    part.rarity = PartData.PartRarity.Rare;
                    rareCount++;
                }
                else // 5% Epic
                {
                    part.rarity = PartData.PartRarity.Epic;
                    epicCount++;
                }
                
                EditorUtility.SetDirty(part);
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[PartMigration] Set random rarities - Common: {commonCount}, Rare: {rareCount}, Epic: {epicCount}");
    }
}
#endif 
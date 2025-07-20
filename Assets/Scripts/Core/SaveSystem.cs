using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SaveData
{
    public int currentWave = 1;
    public bool isFirstWave = true;
    public string selectedClassName = "";
    public DateTime saveDateTime;
    public List<PartSaveData> collectedParts = new List<PartSaveData>();
    public List<MinionSaveData> minionRoster = new List<MinionSaveData>();
    public int selectedMinionIndex = -1;
    public string saveVersion = "1.0";
    public bool isValid = false;
}

[System.Serializable]
public class PartSaveData
{
    public string partName;
    public string partType;
    public string partTheme;
    public string partRarity;
    public int hpBonus;
    public int attackBonus;
    public int defenseBonus;
    public string specialAbility;
    public int specialAbilityLevel;
    public string description;
    public PartSaveData(PartData part)
    {
        if (part == null) return;
        partName = part.partName;
        partType = part.type.ToString();
        partTheme = part.theme.ToString();
        partRarity = part.rarity.ToString();
        hpBonus = part.GetHPBonus();
        attackBonus = part.GetAttackBonus();
        defenseBonus = part.stats.defense;
        specialAbility = part.specialAbility.ToString();
        specialAbilityLevel = part.abilityLevel;
        description = part.description;
    }
    public PartData ToPartData()
    {
        PartData originalPart = FindOriginalPart(partName);
        if (originalPart != null)
        {
            return originalPart;
        }
        PartData part = ScriptableObject.CreateInstance<PartData>();
        part.partName = partName;
        if (System.Enum.TryParse(partType, out PartData.PartType type))
            part.type = type;
        if (System.Enum.TryParse(partTheme, out PartData.PartTheme theme))
            part.theme = theme;
        if (System.Enum.TryParse(partRarity, out PartData.PartRarity rarity))
            part.rarity = rarity;
        if (System.Enum.TryParse(specialAbility, out PartData.SpecialAbility ability))
            part.specialAbility = ability;
        part.stats.hp = hpBonus;
        part.stats.attack = attackBonus;
        part.stats.defense = defenseBonus;
        part.abilityLevel = specialAbilityLevel;
        part.description = description;
        return part;
    }
    private static PartData FindOriginalPart(string partName)
    {
        PartData[] allParts = Resources.LoadAll<PartData>("");
        foreach (PartData part in allParts)
        {
            if (part.partName == partName)
            {
                return part;
            }
        }
        allParts = Resources.LoadAll<PartData>("ScriptableObjects/Parts");
        foreach (PartData part in allParts)
        {
            if (part.partName == partName)
            {
                return part;
            }
        }
        return null;
    }
}

[System.Serializable]
public class MinionSaveData
{
    public string minionName;
    public string baseDataName;
    public int level;
    public int experience;
    public int experienceToNextLevel;
    public int totalExperienceSpent;
    public PartSaveData headPart;
    public PartSaveData torsoPart;
    public PartSaveData armsPart;
    public PartSaveData legsPart;
    public int totalHP;
    public int totalAttack;
    public int totalDefense;
    public int totalCritChance;
    public int totalCritDamage;
    public int totalArmorPen;
    public MinionSaveData(Minion minion)
    {
        if (minion == null) return;
        minionName = minion.minionName;
        baseDataName = minion.baseData?.minionName ?? "";
        level = minion.level;
        experience = minion.experience;
        experienceToNextLevel = minion.experienceToNextLevel;
        totalExperienceSpent = minion.totalExperienceSpent;
        headPart = minion.headPart != null ? new PartSaveData(minion.headPart) : null;
        torsoPart = minion.torsoPart != null ? new PartSaveData(minion.torsoPart) : null;
        armsPart = minion.armsPart != null ? new PartSaveData(minion.armsPart) : null;
        legsPart = minion.legsPart != null ? new PartSaveData(minion.legsPart) : null;
        totalHP = minion.totalHP;
        totalAttack = minion.totalAttack;
        totalDefense = minion.totalDefense;
        totalCritChance = minion.totalCritChance;
        totalCritDamage = minion.totalCritDamage;
        totalArmorPen = minion.totalArmorPen;
    }
    public Minion ToMinion()
    {
        MinionData baseData = null;
        if (!string.IsNullOrEmpty(baseDataName))
        {
            baseData = Resources.Load<MinionData>(baseDataName);
        }
        Minion minion = new Minion(baseData ?? ScriptableObject.CreateInstance<MinionData>());
        minion.minionName = minionName;
        minion.level = level;
        minion.experience = experience;
        minion.experienceToNextLevel = experienceToNextLevel;
        minion.totalExperienceSpent = totalExperienceSpent;
        if (headPart != null) minion.headPart = headPart.ToPartData();
        if (torsoPart != null) minion.torsoPart = torsoPart.ToPartData();
        if (armsPart != null) minion.armsPart = armsPart.ToPartData();
        if (legsPart != null) minion.legsPart = legsPart.ToPartData();
        minion.totalHP = totalHP;
        minion.totalAttack = totalAttack;
        minion.totalDefense = totalDefense;
        minion.totalCritChance = totalCritChance;
        minion.totalCritDamage = totalCritDamage;
        minion.totalArmorPen = totalArmorPen;
        return minion;
    }
}

public static class SaveSystem
{
    private const string SAVE_DATA_KEY = "NecroDraftArena_SaveData";
    private const string SAVE_VERSION = "1.0";
    public static System.Action OnGameSaved;
    public static System.Action OnGameLoaded;
    public static void AutoSave()
    {
        SaveData saveData = CreateSaveData();
        SaveToPlayerPrefs(saveData);
        OnGameSaved?.Invoke();
    }
    public static bool LoadGame()
    {
        SaveData saveData = LoadFromPlayerPrefs();
        if (saveData == null || !saveData.isValid)
        {
            return false;
        }
        RestoreGameState(saveData);
        OnGameLoaded?.Invoke();
        return true;
    }
    public static bool HasSaveData()
    {
        SaveData saveData = LoadFromPlayerPrefs();
        return saveData != null && saveData.isValid;
    }
    public static void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_DATA_KEY);
        PlayerPrefs.Save();
    }
    public static SaveInfo GetSaveInfo()
    {
        SaveData saveData = LoadFromPlayerPrefs();
        if (saveData == null || !saveData.isValid)
            return null;
        SaveInfo info = new SaveInfo
        {
            waveNumber = saveData.currentWave,
            className = saveData.selectedClassName,
            partCount = saveData.collectedParts.Count,
            minionCount = saveData.minionRoster.Count,
            saveDateTime = saveData.saveDateTime
        };
        return info;
    }
    private static SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();
        saveData.currentWave = GameData.GetCurrentWave();
        saveData.isFirstWave = GameData.IsFirstWave();
        saveData.selectedClassName = GameData.GetSelectedClass()?.className ?? "";
        saveData.saveDateTime = DateTime.Now;
        foreach (PartData part in PlayerInventory.GetAllParts())
        {
            saveData.collectedParts.Add(new PartSaveData(part));
        }
        foreach (Minion minion in MinionManager.GetMinionRoster())
        {
            saveData.minionRoster.Add(new MinionSaveData(minion));
        }
        saveData.selectedMinionIndex = MinionManager.GetSelectedMinionIndex();
        saveData.saveVersion = SAVE_VERSION;
        saveData.isValid = true;
        return saveData;
    }
    private static void RestoreGameState(SaveData saveData)
    {
        GameData.RestoreWaveProgress(saveData.currentWave, saveData.isFirstWave);
        PlayerInventory.ClearInventory();
        foreach (PartSaveData partSave in saveData.collectedParts)
        {
            PlayerInventory.AddPart(partSave.ToPartData());
        }
        MinionManager.ClearRoster();
        foreach (MinionSaveData minionSave in saveData.minionRoster)
        {
            Minion minion = minionSave.ToMinion();
            MinionManager.AddMinion(minion);
        }
        MinionManager.SetSelectedMinionIndex(saveData.selectedMinionIndex);
    }
    private static SaveData LoadFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey(SAVE_DATA_KEY))
            return null;
        string json = PlayerPrefs.GetString(SAVE_DATA_KEY);
        try
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null || string.IsNullOrEmpty(saveData.saveVersion))
                return null;
            if (saveData.saveVersion != SAVE_VERSION)
            {
                // TODO: Implement version migration here
            }
            return saveData;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
    private static void SaveToPlayerPrefs(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_DATA_KEY, json);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class SaveInfo
{
    public int waveNumber;
    public string className;
    public int partCount;
    public int minionCount;
    public DateTime saveDateTime;
    public string GetDisplayText()
    {
        return $"Wave {waveNumber} | Class: {className} | Parts: {partCount} | Minions: {minionCount}";
    }
    public string GetDateTimeText()
    {
        return saveDateTime.ToString("g");
    }
} 
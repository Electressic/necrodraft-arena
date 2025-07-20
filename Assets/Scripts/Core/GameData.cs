using UnityEngine;

public static class GameData
{
    private static NecromancerClass selectedClass;
    private static bool isFirstWave = true;
    private static int currentWave = 1;
    private static bool justCompletedCombat = false;
    
    public static void SetSelectedClass(NecromancerClass necroClass)
    {
        selectedClass = necroClass;
        isFirstWave = true;
        currentWave = 1;
        InitializeStartingResources();
    }
    
    public static NecromancerClass GetSelectedClass()
    {
        return selectedClass;
    }
    
    public static bool IsFirstWave()
    {
        return isFirstWave;
    }
    
    public static void CompleteFirstWave()
    {
        isFirstWave = false;
        currentWave++;
    }
    
    public static int GetCurrentWave()
    {
        return currentWave;
    }
    
    private static void InitializeStartingResources()
    {
        if (selectedClass == null) return;
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
    }
    
    public static void ResetGame()
    {
        selectedClass = null;
        isFirstWave = true;
        currentWave = 1;
        justCompletedCombat = false;
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
    }
    
    public static void RestoreWaveProgress(int wave, bool isFirst)
    {
        currentWave = wave;
        isFirstWave = isFirst;
        justCompletedCombat = false;
    }
    
    public static void CompleteWave()
    {
        if (isFirstWave)
        {
            CompleteFirstWave();
        }
        else
        {
            currentWave++;
        }
        justCompletedCombat = true;
        MinionManager.AwardBattleExperience();
        MinionManager.CheckForMinionUnlocks();
        NotifyWaveCompleted();
    }
    
    public static bool JustCompletedCombat()
    {
        return justCompletedCombat;
    }
    
    public static void ClearCombatCompletedFlag()
    {
        justCompletedCombat = false;
    }
    
    private static void NotifyWaveCompleted()
    {
        MinionAssemblyManager assemblyManager = UnityEngine.Object.FindAnyObjectByType<MinionAssemblyManager>();
    }
}
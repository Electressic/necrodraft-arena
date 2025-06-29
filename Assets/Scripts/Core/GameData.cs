using UnityEngine;

public static class GameData
{
    private static NecromancerClass selectedClass;
    private static bool isFirstWave = true;
    private static int currentWave = 1;
    
    public static void SetSelectedClass(NecromancerClass necroClass)
    {
        selectedClass = necroClass;
        isFirstWave = true;
        currentWave = 1;
        
        // Initialize player with starting resources
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
        
        // Clear existing data
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
        
        // Add starting parts to inventory
        int partsAdded = 0;
        foreach (PartData part in selectedClass.startingParts)
        {
            if (part != null)
            {
                PlayerInventory.AddPart(part);
                partsAdded++;
            }
        }
        
        Debug.Log($"[GameData] Initialized {selectedClass.className} with {partsAdded} starting parts");
        
        // Note: Starting minion will be created in MinionAssemblyManager
        // This allows the player to see the process and customize the name
    }
    
    public static void ResetGame()
    {
        selectedClass = null;
        isFirstWave = true;
        currentWave = 1;
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
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
        Debug.Log($"[GameData] Wave {currentWave - 1} completed. Now on wave {currentWave}");
        
        // Notify that a wave was completed (for card selection reset)
        NotifyWaveCompleted();
    }
    
    // Called to reset systems when a wave is completed
    private static void NotifyWaveCompleted()
    {
        // Find and notify MinionAssemblyManager if it exists in scene
        MinionAssemblyManager assemblyManager = UnityEngine.Object.FindAnyObjectByType<MinionAssemblyManager>();
        if (assemblyManager != null)
        {
            assemblyManager.ResetCardSelectionForNewWave();
        }
    }
}
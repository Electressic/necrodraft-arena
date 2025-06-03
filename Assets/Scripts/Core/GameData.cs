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
        // MinionManager.ClearRoster(); // If this method exists
        
        // Add starting parts to inventory
        foreach (PartData part in selectedClass.startingParts)
        {
            if (part != null)
                PlayerInventory.AddPart(part);
        }
        
        // Create starting minion
        if (selectedClass.startingMinionType != null)
        {
            // This will need to be implemented in MinionManager
            // MinionManager.CreateStartingMinion(selectedClass.startingMinionType);
        }
        
        Debug.Log($"[GameData] Initialized {selectedClass.className} with {selectedClass.startingParts.Length} starting parts");
    }
    
    public static void ResetGame()
    {
        selectedClass = null;
        isFirstWave = true;
        currentWave = 1;
        PlayerInventory.ClearInventory();
        // MinionManager.ClearRoster(); // If this method exists
    }
}
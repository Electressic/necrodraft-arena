using UnityEngine;
using System.Collections.Generic;

public static class MinionManager
{
    private static List<Minion> minionRoster = new List<Minion>();
    private static int selectedMinionIndex = -1;
    private static int maxMinions = 5; // Limit to prevent overwhelming the player
    
    public static System.Action OnRosterChanged;
    
    // Roster management
    public static List<Minion> GetMinionRoster()
    {
        return new List<Minion>(minionRoster);
    }
    
    public static bool AddMinion(Minion minion)
    {
        if (minionRoster.Count >= maxMinions)
        {
            Debug.LogWarning($"[MinionManager] Cannot add minion - roster full! ({maxMinions} max)");
            return false;
        }
        
        minionRoster.Add(minion);
        
        // If this is the first minion, select it
        if (selectedMinionIndex == -1)
            selectedMinionIndex = 0;
            
        OnRosterChanged?.Invoke();
        Debug.Log($"[MinionManager] Added {minion?.minionName} to roster. Total: {minionRoster.Count}");
        return true;
    }
    
    public static void RemoveMinion(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            string minionName = minionRoster[index].minionName;
            minionRoster.RemoveAt(index);
            
            // Adjust selected index if needed
            if (selectedMinionIndex >= minionRoster.Count)
                selectedMinionIndex = minionRoster.Count - 1;
            if (minionRoster.Count == 0)
                selectedMinionIndex = -1;
                
            OnRosterChanged?.Invoke();
            Debug.Log($"[MinionManager] Removed {minionName} from roster. Total: {minionRoster.Count}");
        }
    }
    
    // Selection management
    public static void SetSelectedMinionIndex(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            selectedMinionIndex = index;
            Debug.Log($"[MinionManager] Selected minion: {GetSelectedMinion()?.minionName}");
        }
        else if (index == -1)
        {
            selectedMinionIndex = -1;
            Debug.Log("[MinionManager] No minion selected");
        }
    }
    
    public static Minion GetSelectedMinion()
    {
        if (selectedMinionIndex >= 0 && selectedMinionIndex < minionRoster.Count)
            return minionRoster[selectedMinionIndex];
        return null;
    }
    
    public static int GetSelectedMinionIndex()
    {
        return selectedMinionIndex;
    }
    
    // Legacy compatibility
    public static void SetCurrentMinion(Minion minion)
    {
        // For backwards compatibility - add to roster if not present
        if (minion != null && !minionRoster.Contains(minion))
        {
            AddMinion(minion);
        }
        
        // Select this minion
        int index = minionRoster.IndexOf(minion);
        if (index >= 0)
            SetSelectedMinionIndex(index);
    }
    
    public static Minion GetCurrentMinion()
    {
        return GetSelectedMinion();
    }
    
    public static bool HasMinion()
    {
        return GetSelectedMinion() != null;
    }
    
    public static void ClearCurrentMinion()
    {
        selectedMinionIndex = -1;
        Debug.Log("[MinionManager] Current minion cleared");
    }
    
    // Utility
    public static int GetMinionCount()
    {
        return minionRoster.Count;
    }
    
    public static int GetMaxMinions()
    {
        return maxMinions;
    }
    
    public static bool CanAddMoreMinions()
    {
        return minionRoster.Count < maxMinions;
    }
    
    public static void ClearRoster()
    {
        minionRoster.Clear();
        selectedMinionIndex = -1;
        OnRosterChanged?.Invoke();
        Debug.Log("[MinionManager] Roster cleared");
    }
}
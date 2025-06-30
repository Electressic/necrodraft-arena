using UnityEngine;
using System.Collections.Generic;

public static class MinionManager
{
    private static List<Minion> minionRoster = new List<Minion>();
    private static int selectedMinionIndex = -1;
    
    public static System.Action OnRosterChanged;
    public static System.Action<int> OnMinionUnlocked; // Event when new minion slot unlocks
    public static System.Action<Minion, int> OnMinionGainedExperience; // Event when minion gains XP
    public static System.Action<Minion, int> OnMinionLeveledUp; // Event when minion levels up (with new level)
    
    /// <summary>
    /// Get current maximum minions allowed based on wave progression
    /// </summary>
    public static int GetCurrentMaxMinions()
    {
        int currentWave = GameData.GetCurrentWave();
        
        if (currentWave >= 17) return 5; // Act 3: All 5 minions
        if (currentWave >= 13) return 4; // Mid Act 2: 4 minions  
        if (currentWave >= 9) return 3;  // Start Act 2: 3 minions
        if (currentWave >= 5) return 2;  // End Act 1: 2 minions
        return 1;                        // Act 1: 1 minion only
    }
    
    /// <summary>
    /// Check if a new minion slot should be unlocked for the current wave
    /// </summary>
    public static void CheckForMinionUnlocks()
    {
        int currentWave = GameData.GetCurrentWave();
        int newMaxMinions = GetCurrentMaxMinions();
        
        // Check if we've unlocked a new minion slot
        bool shouldNotify = false;
        string unlockMessage = "";
        
        switch (currentWave)
        {
            case 5:
                shouldNotify = true;
                unlockMessage = "Minion #2 unlocked! You can now field 2 minions.";
                break;
            case 9:
                shouldNotify = true;
                unlockMessage = "Minion #3 unlocked! Build more complex formations with 3 minions.";
                break;
            case 13:
                shouldNotify = true;
                unlockMessage = "Minion #4 unlocked! Advanced 4-minion strategies available.";
                break;
            case 17:
                shouldNotify = true;
                unlockMessage = "Final Minion #5 unlocked! Command your full undead army.";
                break;
        }
        
        if (shouldNotify)
        {
            Debug.Log($"[MinionManager] {unlockMessage}");
            OnMinionUnlocked?.Invoke(newMaxMinions);
        }
    }
    
    // Roster management
    public static List<Minion> GetMinionRoster()
    {
        return new List<Minion>(minionRoster);
    }
    
    public static bool AddMinion(Minion minion)
    {
        int currentMaxMinions = GetCurrentMaxMinions();
        if (minionRoster.Count >= currentMaxMinions)
        {
            Debug.LogWarning($"[MinionManager] Cannot add minion - roster full! ({currentMaxMinions} max for wave {GameData.GetCurrentWave()})");
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
        return GetCurrentMaxMinions();
    }
    
    public static bool CanAddMoreMinions()
    {
        return minionRoster.Count < GetCurrentMaxMinions();
    }
    
    // ==================== EXPERIENCE SYSTEM ====================
    
    /// <summary>
    /// Award experience to all minions after winning a battle
    /// </summary>
    public static void AwardBattleExperience()
    {
        int currentWave = GameData.GetCurrentWave();
        int baseXP = CalculateBaseExperienceForWave(currentWave);
        
        foreach (Minion minion in minionRoster)
        {
            bool leveledUp = minion.AddExperience(baseXP);
            OnMinionGainedExperience?.Invoke(minion, baseXP);
            
            if (leveledUp)
            {
                OnMinionLeveledUp?.Invoke(minion, minion.level);
                Debug.Log($"[MinionManager] 🎉 {minion.minionName} leveled up to level {minion.level}!");
            }
        }
        
        if (minionRoster.Count > 0)
        {
            Debug.Log($"[MinionManager] All minions awarded {baseXP} XP for completing wave {currentWave}");
        }
    }
    
    /// <summary>
    /// Calculate base experience award based on wave difficulty
    /// </summary>
    public static int CalculateBaseExperienceForWave(int wave)
    {
        // Progressive XP rewards - later waves give more experience
        int baseXP = 50; // Starting XP for wave 1
        
        // Increase XP by 25% every 3 waves (rounded up)
        int waveGroup = (wave - 1) / 3;
        for (int i = 0; i < waveGroup; i++)
        {
            baseXP = Mathf.RoundToInt(baseXP * 1.25f);
        }
        
        return baseXP;
    }
    
    /// <summary>
    /// Award bonus experience to a specific minion (for MVP, survival, etc.)
    /// </summary>
    public static void AwardBonusExperience(Minion minion, int bonusXP, string reason = "")
    {
        if (minion == null || bonusXP <= 0) return;
        
        bool leveledUp = minion.AddExperience(bonusXP);
        OnMinionGainedExperience?.Invoke(minion, bonusXP);
        
        if (leveledUp)
        {
            OnMinionLeveledUp?.Invoke(minion, minion.level);
        }
        
        string logMessage = $"[MinionManager] {minion.minionName} earned {bonusXP} bonus XP";
        if (!string.IsNullOrEmpty(reason))
            logMessage += $" for {reason}";
        
        if (leveledUp)
            logMessage += $" and leveled up to level {minion.level}!";
        
        Debug.Log(logMessage);
    }
    
    public static void ClearRoster()
    {
        minionRoster.Clear();
        selectedMinionIndex = -1;
        OnRosterChanged?.Invoke();
        Debug.Log("[MinionManager] Roster cleared");
    }
}
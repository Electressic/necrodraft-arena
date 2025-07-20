using UnityEngine;
using System.Collections.Generic;

public static class MinionManager
{
    private static List<Minion> minionRoster = new List<Minion>();
    private static int selectedMinionIndex = -1;
    
    public static System.Action OnRosterChanged;
    public static System.Action<int> OnMinionUnlocked;
    public static System.Action<Minion, int> OnMinionGainedExperience;
    public static System.Action<Minion, int> OnMinionLeveledUp;

    public static int GetCurrentMaxMinions()
    {
        int currentWave = GameData.GetCurrentWave();
        if (currentWave >= 17) return 5;
        if (currentWave >= 13) return 4;
        if (currentWave >= 9) return 3;
        if (currentWave >= 5) return 2;
        return 1;
    }

    public static void CheckForMinionUnlocks()
    {
        int currentWave = GameData.GetCurrentWave();
        int newMaxMinions = GetCurrentMaxMinions();
        bool shouldNotify = false;
        switch (currentWave)
        {
            case 5:
            case 9:
            case 13:
            case 17:
                shouldNotify = true;
                break;
        }
        if (shouldNotify)
        {
            OnMinionUnlocked?.Invoke(newMaxMinions);
        }
    }

    public static List<Minion> GetMinionRoster()
    {
        return new List<Minion>(minionRoster);
    }

    public static bool AddMinion(Minion minion)
    {
        int currentMaxMinions = GetCurrentMaxMinions();
        if (minionRoster.Count >= currentMaxMinions)
        {
            return false;
        }
        minionRoster.Add(minion);
        if (selectedMinionIndex == -1)
            selectedMinionIndex = 0;
        OnRosterChanged?.Invoke();
        return true;
    }

    public static void RemoveMinion(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            minionRoster.RemoveAt(index);
            if (selectedMinionIndex >= minionRoster.Count)
                selectedMinionIndex = minionRoster.Count - 1;
            if (minionRoster.Count == 0)
                selectedMinionIndex = -1;
            OnRosterChanged?.Invoke();
        }
    }

    public static void SetSelectedMinionIndex(int index)
    {
        if (index >= 0 && index < minionRoster.Count)
        {
            selectedMinionIndex = index;
        }
        else if (index == -1)
        {
            selectedMinionIndex = -1;
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

    public static void SetCurrentMinion(Minion minion)
    {
        if (minion != null && !minionRoster.Contains(minion))
        {
            AddMinion(minion);
        }
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
    }

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
            }
        }
    }

    public static int CalculateBaseExperienceForWave(int wave)
    {
        int baseXP = 50;
        int waveGroup = (wave - 1) / 3;
        for (int i = 0; i < waveGroup; i++)
        {
            baseXP = Mathf.RoundToInt(baseXP * 1.25f);
        }
        return baseXP;
    }

    public static void AwardBonusExperience(Minion minion, int bonusXP, string reason = "")
    {
        if (minion == null || bonusXP <= 0) return;
        bool leveledUp = minion.AddExperience(bonusXP);
        OnMinionGainedExperience?.Invoke(minion, bonusXP);
        if (leveledUp)
        {
            OnMinionLeveledUp?.Invoke(minion, minion.level);
        }
    }

    public static void ClearRoster()
    {
        minionRoster.Clear();
        selectedMinionIndex = -1;
        OnRosterChanged?.Invoke();
    }
}
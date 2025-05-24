using UnityEngine;

public static class MinionManager
{
    private static Minion currentMinion;
    
    public static void SetCurrentMinion(Minion minion)
    {
        currentMinion = minion;
        Debug.Log($"[MinionManager] Current minion set: {minion?.minionName}");
    }
    
    public static Minion GetCurrentMinion()
    {
        return currentMinion;
    }
    
    public static bool HasMinion()
    {
        return currentMinion != null;
    }
    
    public static void ClearCurrentMinion()
    {
        currentMinion = null;
        Debug.Log("[MinionManager] Current minion cleared");
    }
}
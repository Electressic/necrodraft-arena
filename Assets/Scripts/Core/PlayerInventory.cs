using UnityEngine;
using System.Collections.Generic;

public static class PlayerInventory
{
    private static List<PartData> collectedParts = new List<PartData>();
    
    [Header("Debug Events")]
    public static System.Action<PartData> OnPartAdded;
    public static System.Action OnInventoryCleared;
    
    public static void AddPart(PartData part)
    {
        if (part == null)
        {
            Debug.LogWarning("[PlayerInventory] Attempted to add null part!");
            return;
        }
        
        collectedParts.Add(part);
        OnPartAdded?.Invoke(part);
        
        Debug.Log($"[PlayerInventory] Added {part.partName} to inventory. Total parts: {collectedParts.Count}");
    }
    
    public static List<PartData> GetAllParts()
    {
        return new List<PartData>(collectedParts);
    }
    
    public static List<PartData> GetPartsByType(PartData.PartType type)
    {
        List<PartData> partsOfType = new List<PartData>();
        foreach (PartData part in collectedParts)
        {
            if (part.type == type)
                partsOfType.Add(part);
        }
        return partsOfType;
    }
    
    public static void ClearInventory()
    {
        collectedParts.Clear();
        OnInventoryCleared?.Invoke();
        Debug.Log("[PlayerInventory] Inventory cleared");
    }
    
    public static int GetPartCount()
    {
        return collectedParts.Count;
    }
    
    public static int GetPartCountByType(PartData.PartType type)
    {
        int count = 0;
        foreach (PartData part in collectedParts)
        {
            if (part.type == type)
                count++;
        }
        return count;
    }
    
    public static bool HasPartsOfType(PartData.PartType type)
    {
        return GetPartCountByType(type) > 0;
    }
}
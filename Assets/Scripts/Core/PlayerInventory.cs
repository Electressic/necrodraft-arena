using UnityEngine;
using System.Collections.Generic;

public static class PlayerInventory
{
    private static List<PartData> collectedParts = new List<PartData>();
    private static HashSet<PartData> newParts = new HashSet<PartData>();

    public static System.Action<PartData> OnPartAdded;
    public static System.Action OnInventoryCleared;

    public static void AddPart(PartData part)
    {
        if (part == null)
        {
            return;
        }
        collectedParts.Add(part);
        newParts.Add(part);
        OnPartAdded?.Invoke(part);
    }

    public static void AddPartWithoutMarkingAsNew(PartData part)
    {
        if (part == null)
        {
            return;
        }
        collectedParts.Add(part);
        OnPartAdded?.Invoke(part);
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
        newParts.Clear();
        OnInventoryCleared?.Invoke();
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

    public static bool IsNewPart(PartData part)
    {
        return newParts.Contains(part);
    }

    public static void MarkPartAsSeen(PartData part)
    {
        newParts.Remove(part);
    }

    public static void ClearNewPartMarkers()
    {
        newParts.Clear();
    }

    public static void RemovePart(PartData part)
    {
        if (part == null) return;
        collectedParts.Remove(part);
        newParts.Remove(part);
    }
}
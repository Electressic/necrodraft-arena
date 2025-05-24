using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Minion
{
    [Header("Identity")]
    public string minionName;
    public MinionData baseData;
    
    [Header("Equipment Slots")]
    public PartData headPart;
    public PartData torsoPart;
    public PartData armsPart;
    public PartData legsPart;
    
    [Header("Calculated Stats")]
    public int totalHP;
    public int totalAttack;
    
    // Constructor
    public Minion(MinionData data)
    {
        baseData = data;
        minionName = data.minionName;
        CalculateStats();
    }
    
    public void EquipPart(PartData part)
    {
        if (part == null) return;
        
        switch (part.type)
        {
            case PartData.PartType.Head:
                headPart = part;
                break;
            case PartData.PartType.Torso:
                torsoPart = part;
                break;
            case PartData.PartType.Arms:
                armsPart = part;
                break;
            case PartData.PartType.Legs:
                legsPart = part;
                break;
        }
        
        CalculateStats();
    }
    
    public void UnequipPart(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head:
                headPart = null;
                break;
            case PartData.PartType.Torso:
                torsoPart = null;
                break;
            case PartData.PartType.Arms:
                armsPart = null;
                break;
            case PartData.PartType.Legs:
                legsPart = null;
                break;
        }
        
        CalculateStats();
    }
    
    public PartData GetEquippedPart(PartData.PartType partType)
    {
        switch (partType)
        {
            case PartData.PartType.Head: return headPart;
            case PartData.PartType.Torso: return torsoPart;
            case PartData.PartType.Arms: return armsPart;
            case PartData.PartType.Legs: return legsPart;
            default: return null;
        }
    }
    
    public void CalculateStats()
    {
        // Start with base stats
        totalHP = baseData.baseHP;
        totalAttack = baseData.baseAttack;
        
        // Add bonuses from equipped parts
        if (headPart != null)
        {
            totalHP += headPart.hpBonus;
            totalAttack += headPart.attackBonus;
        }
        
        if (torsoPart != null)
        {
            totalHP += torsoPart.hpBonus;
            totalAttack += torsoPart.attackBonus;
        }
        
        if (armsPart != null)
        {
            totalHP += armsPart.hpBonus;
            totalAttack += armsPart.attackBonus;
        }
        
        if (legsPart != null)
        {
            totalHP += legsPart.hpBonus;
            totalAttack += legsPart.attackBonus;
        }
    }
    
    public bool IsSlotEmpty(PartData.PartType partType)
    {
        return GetEquippedPart(partType) == null;
    }
    
    public int GetEquippedPartsCount()
    {
        int count = 0;
        if (headPart != null) count++;
        if (torsoPart != null) count++;
        if (armsPart != null) count++;
        if (legsPart != null) count++;
        return count;
    }
}
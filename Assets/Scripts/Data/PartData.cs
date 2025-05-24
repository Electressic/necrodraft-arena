using UnityEngine;

[CreateAssetMenu(fileName = "NewPart", menuName = "Scriptable Objects/PartData")]
public class PartData : ScriptableObject
{
    public enum PartType { Head, Torso, Arms, Legs }

    [Header("Basic Info")]
    public string partName = "New Part";
    public Sprite icon;

    [Header("Part Type")]
    public PartType type;

    [Header("Stats")]
    public int hpBonus = 0;
    public int attackBonus = 0;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
}

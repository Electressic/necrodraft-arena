using UnityEngine;

[CreateAssetMenu(fileName = "NewPart", menuName = "Scriptable Objects/PartData")]
public class PartData : ScriptableObject
{
    public enum PartType { Head, Torso, Arms, Legs }

    [Header("Basic Info")]
    public string partName = "New Part";
    public PartType type;
    public Sprite visual;

    [Header("Stats")]
    public int hpBonus = 0;
    public int attackBonus = 0;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
}

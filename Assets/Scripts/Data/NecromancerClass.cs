using UnityEngine;

[CreateAssetMenu(fileName = "NewNecromancerClass", menuName = "NecroDraft/NecromancerClass")]
public class NecromancerClass : ScriptableObject
{
    [Header("Class Identity")]
    public string className = "Bone Weaver";
    public Sprite classIcon;
    [TextArea(2, 3)]
    public string classDescription = "Masters of skeletal minions and bone-based parts.";
    
    [Header("Specialization")]
    public string specialtyKeyword = "Bone"; // Parts with this keyword get bonuses
    [TextArea(1, 2)]
    public string specialtyDescription = "Bone parts are 50% more effective";
    
    [Header("Starting Resources")]
    public MinionData startingMinionType;
    public PartData[] startingParts = new PartData[3]; // Extra parts to begin with
    
    [Header("Class Bonuses")]
    [Range(1.0f, 2.0f)]
    public float hpBonusMultiplier = 1.5f;    // For specialized parts
    [Range(1.0f, 2.0f)]
    public float attackBonusMultiplier = 1.5f; // For specialized parts
    
    [Header("Card Selection Bonuses")]
    public PartData[] guaranteedCardPool; // Cards this class always has access to
    [Range(0, 3)]
    public int bonusCardPicks = 1; // Extra cards to choose from
}
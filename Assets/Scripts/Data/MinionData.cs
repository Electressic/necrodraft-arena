using UnityEngine;

[CreateAssetMenu(fileName = "MinionData", menuName = "NecroDraft/MinionData")]
public class MinionData : ScriptableObject
{
    [Header("Basic Info")]
    public string minionName = "New Minion";
    public Sprite baseSprite;
    
    [Header("Base Stats")]
    public int baseHP = 20;
    public int baseAttack = 5;
    
    [Header("Description")]
    [TextArea(2, 3)]
    public string description = "A basic undead minion ready for enhancement.";
}

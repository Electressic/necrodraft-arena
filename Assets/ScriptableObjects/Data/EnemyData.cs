using UnityEngine;

// Targeting behavior types for tactical variety
public enum EnemyTargetingType
{
    Bruiser,    // Targets front row (leftmost) - current behavior
    Archer,     // Targets back row first (ignores front row)
    Assassin,   // Targets lowest HP unit anywhere
    Sniper,     // Targets highest ATK unit (threat priority)
    Bomber      // Targets center positions (area damage concept)
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "NecroDraft/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Basic Enemy";
    public Sprite enemySprite;
    
    [Header("Tactical Behavior")]
    public EnemyTargetingType targetingType = EnemyTargetingType.Bruiser;
    
    [Header("Stats")]
    public int maxHP = 15;
    public int attackPower = 3;
    public float attackSpeed = 2f; // Attacks per second
    public float moveSpeed = 1f;
    
    [Header("Behavior")]
    public float attackRange = 1.5f;
    public float detectionRange = 3f;
    
    [Header("Visual")]
    public Color enemyColor = Color.red;
    
    [Header("Description")]
    [TextArea(2, 3)]
    public string description = "A basic enemy unit.";
}
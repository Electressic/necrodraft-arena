using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "NecroDraft/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Basic Enemy";
    public Sprite enemySprite;
    
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
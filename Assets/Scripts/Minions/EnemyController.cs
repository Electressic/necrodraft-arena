using UnityEngine;

public class EnemyController : Unit
{
    [Header("Enemy Specific")]
    public EnemyData enemyData;
    private Unit currentTarget;
    private float lastAttackTime;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // Find target if we don't have one
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNearestMinion();
        }
        
        // Move towards and attack target
        if (currentTarget != null)
        {
            MoveTowardsTarget();
            TryAttack();
        }
    }
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        
        // Apply wave-based scaling for progressive difficulty
        int currentWave = GameData.GetCurrentWave();
        float scaleMultiplier = 1.0f + ((currentWave - 1) * 0.10f); // 10% increase per wave (reduced from 15%)
        
        int scaledHP = Mathf.RoundToInt(data.maxHP * scaleMultiplier);
        int scaledAttack = Mathf.RoundToInt(data.attackPower * scaleMultiplier);
        float scaledSpeed = data.moveSpeed * (1.0f + ((currentWave - 1) * 0.05f)); // 5% speed increase per wave
        
        // Use scaled stats
        Initialize(scaledHP, scaledAttack, scaledSpeed);
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed * (1.0f + ((currentWave - 1) * 0.08f)); // 8% attack speed increase
        
        // Update visual appearance
        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color = data.enemyColor;
        }
        
        // Add wave indicator to name for higher waves
        string displayName = data.enemyName;
        if (currentWave > 3)
        {
            displayName = $"{data.enemyName} (W{currentWave})";
        }
        gameObject.name = displayName;
        
        Debug.Log($"[EnemyController] Initialized {displayName} with {scaledHP} HP, {scaledAttack} ATK (x{scaleMultiplier:F2} scaling for wave {currentWave})");
    }
    
    void FindNearestMinion()
    {
        Unit[] minions = FindObjectsByType<MinionController>(FindObjectsSortMode.None);
        Unit nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Unit minion in minions)
        {
            if (!minion.isAlive) continue;
            
            float distance = Vector3.Distance(transform.position, minion.transform.position);
            if (distance < nearestDistance)
            {
                nearest = minion;
                nearestDistance = distance;
            }
        }
        
        currentTarget = nearest;
    }
    
    void MoveTowardsTarget()
    {
        if (currentTarget == null) return;
        
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (distance > attackRange)
        {
            // Move towards target
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    void TryAttack()
    {
        if (currentTarget == null) return;
        
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (distance <= attackRange && Time.time >= lastAttackTime + (1f / attackSpeed))
        {
            Attack(currentTarget);
            lastAttackTime = Time.time;
        }
    }
}
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
        
        // Use enemy data stats
        Initialize(data.maxHP, data.attackPower, data.moveSpeed);
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed;
        
        // Update visual appearance
        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color = data.enemyColor;
        }
        
        gameObject.name = data.enemyName;
        
        Debug.Log($"[EnemyController] Initialized {data.enemyName} with {data.maxHP} HP and {data.attackPower} ATK");
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
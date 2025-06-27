using UnityEngine;

public class MinionController : Unit
{
    [Header("Minion Specific")]
    public Minion minionData;
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
            FindNearestEnemy();
        }
        
        // Move towards and attack target
        if (currentTarget != null)
        {
            MoveTowardsTarget();
            TryAttack();
        }
    }
    
    public void Initialize(Minion minion)
    {
        minionData = minion;
        
        // Use minion's calculated stats
        Initialize(minion.totalHP, minion.totalAttack, moveSpeed);
        
        // Update visual appearance
        if (spriteRenderer != null && minion.baseData.baseSprite != null)
        {
            spriteRenderer.sprite = minion.baseData.baseSprite;
        }
        
        gameObject.name = minion.minionName;
        
        Debug.Log($"[MinionController] Initialized {minion.minionName} with {minion.totalHP} HP and {minion.totalAttack} ATK");
    }
    
    void FindNearestEnemy()
    {
        Unit[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        Unit nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Unit enemy in enemies)
        {
            if (!enemy.isAlive) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearest = enemy;
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
using UnityEngine;

public class EnemyController : Unit
{
    [Header("Enemy Specific")]
    public EnemyData enemyData;
    private Unit currentTarget;
    private float lastAttackTime;
    
    [Header("Visuals")]
    public Sprite backgroundSprite;
    private SpriteRenderer backgroundRenderer;
    
    protected override void Awake()
    {
        base.Awake();
        if (backgroundRenderer == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.transform.localPosition = Vector3.zero;
            backgroundRenderer = bgObj.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingOrder = 0;
            backgroundRenderer.sprite = backgroundSprite;
            backgroundRenderer.color = Color.white;
        }
    }
    
    [Header("Combat Control")]
    public bool enableAutonomousCombat = true;
    
    void Update()
    {
        if (!isAlive) return;
        
        if (!enableAutonomousCombat) return;
        
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNearestMinion();
        }
        
        if (currentTarget != null)
        {
            MoveTowardsTarget();
            TryAttack();
        }
    }
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        
        int currentWave = GameData.GetCurrentWave();
        float scaleMultiplier = 1.0f + ((currentWave - 1) * 0.10f);
        
        int scaledHP = Mathf.RoundToInt(data.maxHP * scaleMultiplier);
        int scaledAttack = Mathf.RoundToInt(data.attackPower * scaleMultiplier);
        float scaledSpeed = data.moveSpeed * (1.0f + ((currentWave - 1) * 0.05f));
        
        Initialize(scaledHP, scaledAttack, scaledSpeed);
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed * (1.0f + ((currentWave - 1) * 0.08f));
        
        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color = data.enemyColor;
        }
        if (backgroundRenderer != null && backgroundSprite != null)
        {
            backgroundRenderer.sprite = backgroundSprite;
            backgroundRenderer.color = Color.white;
        }
        
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
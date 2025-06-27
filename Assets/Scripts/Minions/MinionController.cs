using UnityEngine;
using System.Collections;

public class MinionController : Unit
{
    [Header("Minion Specific")]
    public Minion minionData;
    private Unit currentTarget;
    private float lastAttackTime;
    
    [Header("Special Abilities")]
    private float lastRegenTime;
    private bool berserkerActive = false;
    private float originalMoveSpeed;
    private float originalAttackSpeed;
    
    protected override void Awake()
    {
        base.Awake();
        originalMoveSpeed = moveSpeed;
        originalAttackSpeed = attackSpeed;
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // Process special abilities
        ProcessSpecialAbilities();
        
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
    
    void ProcessSpecialAbilities()
    {
        if (minionData == null) return;
        
        // Regeneration
        if (minionData.HasAbility(PartData.SpecialAbility.Regeneration))
        {
            if (Time.time >= lastRegenTime + 3f && currentHP < maxHP)
            {
                TakeDamage(-1); // Negative damage = healing
                lastRegenTime = Time.time;
                Debug.Log($"[{gameObject.name}] Regenerated 1 HP!");
            }
        }
        
        // Berserker ability
        if (minionData.HasAbility(PartData.SpecialAbility.Berserker))
        {
            bool shouldBeBerserker = (float)currentHP / maxHP < 0.5f;
            if (shouldBeBerserker && !berserkerActive)
            {
                berserkerActive = true;
                attackSpeed = originalAttackSpeed * 1.5f;
                Debug.Log($"[{gameObject.name}] Berserker mode activated!");
            }
            else if (!shouldBeBerserker && berserkerActive)
            {
                berserkerActive = false;
                attackSpeed = originalAttackSpeed;
                Debug.Log($"[{gameObject.name}] Berserker mode deactivated.");
            }
        }
    }
    
    public override void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        int modifiedDamage = damage;
        
        // Armored ability
        if (minionData != null && minionData.HasAbility(PartData.SpecialAbility.Armored))
        {
            modifiedDamage = Mathf.Max(1, damage - 1);
            if (modifiedDamage != damage)
            {
                Debug.Log($"[{gameObject.name}] Armor reduced damage from {damage} to {modifiedDamage}!");
            }
        }
        
        base.TakeDamage(modifiedDamage);
        
        // Thorns ability - reflect damage to attacker
        if (minionData != null && minionData.HasAbility(PartData.SpecialAbility.Thorns) && damage > 0)
        {
            if (currentTarget != null && currentTarget.isAlive)
            {
                currentTarget.TakeDamage(1);
                Debug.Log($"[{gameObject.name}] Thorns reflected 1 damage!");
            }
        }
    }
    
    public override void Attack(Unit target)
    {
        if (!isAlive || target == null || !target.isAlive) return;
        
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= attackRange)
        {
            CreateAttackEffect();
            
            int finalDamage = attackPower;
            
            // Critical Strike ability
            if (minionData != null && minionData.HasAbility(PartData.SpecialAbility.CriticalStrike))
            {
                if (Random.Range(0f, 1f) <= 0.25f) // 25% chance
                {
                    finalDamage *= 2;
                    Debug.Log($"[{gameObject.name}] CRITICAL HIT for {finalDamage} damage!");
                    CreateCriticalHitEffect();
                }
            }
            
            target.TakeDamage(finalDamage);
            
            // Vampiric ability
            if (minionData != null && minionData.HasAbility(PartData.SpecialAbility.Vampiric))
            {
                int healAmount = Mathf.RoundToInt(finalDamage * 0.25f);
                if (healAmount > 0 && currentHP < maxHP)
                {
                    TakeDamage(-healAmount); // Negative damage = healing
                    Debug.Log($"[{gameObject.name}] Vampiric healing for {healAmount} HP!");
                }
            }
            
            // Poison ability
            if (minionData != null && minionData.HasAbility(PartData.SpecialAbility.Poison))
            {
                StartCoroutine(ApplyPoison(target));
            }
            
            Debug.Log($"[Unit] {gameObject.name} attacked {target.gameObject.name} for {finalDamage} damage!");
        }
    }
    
    IEnumerator ApplyPoison(Unit target)
    {
        Debug.Log($"[{gameObject.name}] Applied poison to {target.gameObject.name}!");
        
        for (int i = 0; i < 3; i++) // 3 ticks over 3 seconds
        {
            yield return new WaitForSeconds(1f);
            
            if (target != null && target.isAlive)
            {
                target.TakeDamage(2);
                Debug.Log($"[{gameObject.name}] Poison damaged {target.gameObject.name} for 2!");
            }
            else
            {
                break; // Target died, stop poison
            }
        }
    }
    
    void CreateCriticalHitEffect()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(CriticalFlashEffect());
        }
    }
    
    IEnumerator CriticalFlashEffect()
    {
        Color critColor = Color.yellow;
        Color originalColor = spriteRenderer.color;
        
        // Flash multiple times for critical hit
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = critColor;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void Initialize(Minion minion)
    {
        minionData = minion;
        
        // Apply move speed from special abilities
        float finalMoveSpeed = moveSpeed * minion.totalMoveSpeedMultiplier;
        
        // Use minion's calculated stats
        Initialize(minion.totalHP, minion.totalAttack, finalMoveSpeed);
        
        // Update visual appearance
        if (spriteRenderer != null && minion.baseData.baseSprite != null)
        {
            spriteRenderer.sprite = minion.baseData.baseSprite;
        }
        
        gameObject.name = minion.minionName;
        
        Debug.Log($"[MinionController] Initialized {minion.minionName} with {minion.totalHP} HP, {minion.totalAttack} ATK, and abilities: {minion.GetAbilitiesSummary()}");
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
using UnityEngine;
using System.Collections;

public class MinionController : Unit
{
    [Header("Minion Specific")]
    public Minion minionData;
    public MinionVisual minionVisual;
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
        
        // Get or add visual component
        if (minionVisual == null)
            minionVisual = GetComponent<MinionVisual>();
        if (minionVisual == null)
            minionVisual = gameObject.AddComponent<MinionVisual>();
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
        if (minionData.HasSetBonus(PartData.SpecialAbility.Regeneration))
        {
            if (Time.time >= lastRegenTime + 3f && currentHP < maxHP)
            {
                TakeDamage(-1); // Negative damage = healing
                lastRegenTime = Time.time;
                Debug.Log($"[{gameObject.name}] Regenerated 1 HP!");
            }
        }
        
        // Berserker ability
        if (minionData.HasSetBonus(PartData.SpecialAbility.Berserker))
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
        
        // Apply level-enhanced defense first
        if (minionData != null && minionData.totalDefense > 0)
        {
            modifiedDamage = Mathf.Max(1, damage - minionData.totalDefense);
            if (modifiedDamage != damage)
            {
                Debug.Log($"[{gameObject.name}] Defense ({minionData.totalDefense}) reduced damage from {damage} to {modifiedDamage}!");
            }
        }
        
        // Armored set bonus (additional defense)
        if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Armored))
        {
            int armoredDamage = Mathf.Max(1, modifiedDamage - 1);
            if (armoredDamage != modifiedDamage)
            {
                Debug.Log($"[{gameObject.name}] Armored set bonus reduced damage from {modifiedDamage} to {armoredDamage}!");
                modifiedDamage = armoredDamage;
            }
        }
        
        base.TakeDamage(modifiedDamage);
        
        // Apply damage visual effect
        if (minionVisual != null && damage > 0)
        {
            minionVisual.ApplyVisualEffect("damage");
        }
        
        // Thorns ability - reflect damage to attacker
        if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Thorns) && damage > 0)
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
            
            // Critical Hit calculation using level-enhanced stats
            if (minionData != null && minionData.totalCritChance > 0)
            {
                float critRoll = Random.Range(0f, 100f);
                if (critRoll <= minionData.totalCritChance)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * (minionData.totalCritDamage / 100f));
                    Debug.Log($"[{gameObject.name}] CRITICAL HIT! ({minionData.totalCritChance:F1}% chance) for {finalDamage} damage! (x{minionData.totalCritDamage:F0}%)");
                    CreateCriticalHitEffect();
                }
            }
            
            // Additional Critical Strike set bonus (stacks with level-enhanced crit)
            else if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.CriticalStrike))
            {
                if (Random.Range(0f, 1f) <= 0.25f) // 25% chance from set bonus
                {
                    finalDamage *= 2;
                    Debug.Log($"[{gameObject.name}] CRITICAL STRIKE SET BONUS for {finalDamage} damage!");
                    CreateCriticalHitEffect();
                }
            }
            
            target.TakeDamage(finalDamage);
            
            // Vampiric ability
            if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Vampiric))
            {
                int healAmount = Mathf.RoundToInt(finalDamage * 0.25f);
                if (healAmount > 0 && currentHP < maxHP)
                {
                    TakeDamage(-healAmount); // Negative damage = healing
                    
                    // Show heal visual effect
                    if (minionVisual != null)
                        minionVisual.ApplyVisualEffect("heal");
                        
                    Debug.Log($"[{gameObject.name}] Vampiric healing for {healAmount} HP!");
                }
            }
            
            // Poison ability
            if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Poison))
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
        if (minionVisual != null)
        {
            minionVisual.ApplyVisualEffect("critical");
        }
        else if (spriteRenderer != null)
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
        
        // Use minion's level-enhanced calculated stats
        Initialize(minion.totalHP, minion.totalAttack, minion.totalMoveSpeed);
        
        // Apply additional level-enhanced stats not covered by base Initialize
        attackSpeed = minion.totalAttackSpeed;
        attackRange = minion.totalRange;
        
        // Store enhanced stats for combat calculations
        originalMoveSpeed = minion.totalMoveSpeed;
        originalAttackSpeed = minion.totalAttackSpeed;
        
        // Update visual appearance using the new modular system
        if (minionVisual != null)
        {
            minionVisual.UpdateVisuals(minion);
        }
        else if (spriteRenderer != null && minion.baseData.baseSprite != null)
        {
            // Fallback to old system if visual component not available
            spriteRenderer.sprite = minion.baseData.baseSprite;
        }
        
        gameObject.name = minion.minionName;
        
        Debug.Log($"[MinionController] Initialized {minion.minionName} with level {minion.level} stats: {minion.totalHP} HP, {minion.totalAttack} ATK, {minion.totalAttackSpeed:F1} AS, {minion.totalMoveSpeed:F1} MS, {minion.totalRange:F1} Range. Abilities: {minion.GetAbilitiesSummary()}");
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
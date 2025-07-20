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
    
    [Header("Combat Control")]
    public bool enableAutonomousCombat = true;
    
    protected override void Awake()
    {
        base.Awake();
        originalMoveSpeed = moveSpeed;
        originalAttackSpeed = attackSpeed;
        
        if (minionVisual == null)
            minionVisual = GetComponent<MinionVisual>();
        if (minionVisual == null)
            minionVisual = gameObject.AddComponent<MinionVisual>();
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        ProcessSpecialAbilities();
        
        if (!enableAutonomousCombat) return;
        
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNearestEnemy();
        }
        
        if (currentTarget != null)
        {
            MoveTowardsTarget();
            TryAttack();
        }
    }
    
    void ProcessSpecialAbilities()
    {
        if (minionData == null) return;
        
        if (minionData.HasSetBonus(PartData.SpecialAbility.Healing))
        {
            if (Time.time >= lastRegenTime + 3f && currentHP < maxHP)
            {
                TakeDamage(-1);
                lastRegenTime = Time.time;
            }
        }
        
        if (minionData.HasSetBonus(PartData.SpecialAbility.Momentum))
        {
            bool shouldBeBerserker = (float)currentHP / maxHP < 0.5f;
            if (shouldBeBerserker && !berserkerActive)
            {
                berserkerActive = true;
                attackSpeed = originalAttackSpeed * 1.5f;
            }
            else if (!shouldBeBerserker && berserkerActive)
            {
                berserkerActive = false;
                attackSpeed = originalAttackSpeed;
            }
        }
    }
    
    public override void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        int modifiedDamage = damage;
        
        if (minionData != null && minionData.totalDefense > 0)
        {
            modifiedDamage = Mathf.Max(1, damage - minionData.totalDefense);
        }
        
        if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.ShieldWall))
        {
            int armoredDamage = Mathf.Max(1, modifiedDamage - 1);
            if (armoredDamage != modifiedDamage)
            {
                modifiedDamage = armoredDamage;
            }
        }
        
        base.TakeDamage(modifiedDamage);
        
        if (minionVisual != null && damage > 0)
        {
            minionVisual.ApplyVisualEffect("damage");
        }
        
        if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.DamageSharing) && damage > 0)
        {
            if (currentTarget != null && currentTarget.isAlive)
            {
                currentTarget.TakeDamage(1);
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
            
            if (minionData != null && minionData.totalCritChance > 0)
            {
                float critRoll = Random.Range(0f, 100f);
                if (critRoll <= minionData.totalCritChance)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * (minionData.totalCritDamage / 100f));
                    CreateCriticalHitEffect();
                }
            }
            else if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.FocusFire))
            {
                if (Random.Range(0f, 1f) <= 0.25f)
                {
                    finalDamage *= 2;
                    CreateCriticalHitEffect();
                }
            }
            
            target.TakeDamage(finalDamage);
            
            if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Inspiration))
            {
                int healAmount = Mathf.RoundToInt(finalDamage * 0.25f);
                if (healAmount > 0 && currentHP < maxHP)
                {
                    TakeDamage(-healAmount);
                    
                    if (minionVisual != null)
                        minionVisual.ApplyVisualEffect("heal");
                }
            }
            
            if (minionData != null && minionData.HasSetBonus(PartData.SpecialAbility.Confuse))
            {
                StartCoroutine(ApplyPoison(target));
            }
        }
    }
    
    IEnumerator ApplyPoison(Unit target)
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            
            if (target != null && target.isAlive)
            {
                target.TakeDamage(2);
            }
            else
            {
                break;
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
        
        Initialize(minion.totalHP, minion.totalAttack, moveSpeed);
        
        originalMoveSpeed = moveSpeed;
        originalAttackSpeed = attackSpeed;
        
        if (minionVisual != null)
        {
            minionVisual.UpdateVisuals(minion);
        }
        else if (spriteRenderer != null && minion.baseData.baseSprite != null)
        {
            spriteRenderer.sprite = minion.baseData.baseSprite;
        }
        
        gameObject.name = minion.minionName;
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
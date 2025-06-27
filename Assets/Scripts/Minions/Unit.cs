using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Stats")]
    public int maxHP = 20;
    public int currentHP;
    public int attackPower = 5;
    public float attackSpeed = 1f;
    public float moveSpeed = 2f;
    
    [Header("Combat")]
    public float attackRange = 1.5f;
    public bool isAlive = true;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    
    // Events
    public System.Action<Unit> OnDeath;
    public System.Action<Unit, int> OnTakeDamage;
    
    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHP = maxHP;
    }
    
    public virtual void Initialize(int hp, int attack, float speed = 2f)
    {
        maxHP = hp;
        currentHP = hp;
        attackPower = attack;
        moveSpeed = speed;
        isAlive = true;
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        currentHP -= damage;
        OnTakeDamage?.Invoke(this, damage);
        
        Debug.Log($"[Unit] {gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        isAlive = false;
        OnDeath?.Invoke(this);
        
        Debug.Log($"[Unit] {gameObject.name} died!");
        
        // Simple death effect - you can expand this
        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;
        
        // Disable after a short delay
        Invoke(nameof(DestroyUnit), 1f);
    }
    
    void DestroyUnit()
    {
        Destroy(gameObject);
    }
    
    public virtual void Attack(Unit target)
    {
        if (!isAlive || target == null || !target.isAlive) return;
        
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= attackRange)
        {
            target.TakeDamage(attackPower);
            Debug.Log($"[Unit] {gameObject.name} attacked {target.gameObject.name} for {attackPower} damage!");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    
    [Header("Health Bar")]
    private GameObject healthBarCanvas;
    private Slider healthBarSlider;
    private Vector3 healthBarOffset = new Vector3(0, 0.8f, 0);
    
    [Header("Combat Effects")]
    public float hitFlashDuration = 0.1f;
    public float screenShakeIntensity = 0.2f;
    public float screenShakeDuration = 0.3f;
    
    private Color originalColor;
    private bool isFlashing = false;
    
    public System.Action<Unit> OnDeath;
    public System.Action<Unit, int> OnTakeDamage;
    
    private static Camera mainCamera;
    
    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        currentHP = maxHP;
        
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    void Start()
    {
        CreateHealthBar();
    }
    
    void Update()
    {
        UpdateHealthBarPosition();
    }
    
    void CreateHealthBar()
    {
        healthBarCanvas = new GameObject("HealthBar");
        healthBarCanvas.transform.SetParent(transform);
        
        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        
        healthBarCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        
        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.2f);
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarCanvas.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.black;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(1f, 0.2f); 
        bgRect.anchoredPosition = Vector2.zero;
        
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(healthBarCanvas.transform);
        healthBarSlider = sliderObj.AddComponent<Slider>();
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(0.9f, 0.15f);
        sliderRect.anchoredPosition = Vector2.zero;
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.sizeDelta = new Vector2(0, 0);
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(0, 0);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthBarSlider.fillRect = fillRect;
        healthBarSlider.value = 1f;
        healthBarSlider.minValue = 0f;
        healthBarSlider.maxValue = 1f;
        
        UpdateHealthBarPosition();
    }
    
    void UpdateHealthBarPosition()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.position = transform.position + healthBarOffset;
            
            if (Camera.main != null)
            {
                healthBarCanvas.transform.LookAt(Camera.main.transform);
                healthBarCanvas.transform.Rotate(0, 180, 0);
            }
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            float healthPercent = (float)currentHP / maxHP;
            healthBarSlider.value = healthPercent;
            
            Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (healthPercent > 0.6f)
                    fillImage.color = Color.green;
                else if (healthPercent > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }

    public virtual void Initialize(int hp, int attack, float speed = 2f)
    {
        maxHP = hp;
        currentHP = hp;
        attackPower = attack;
        moveSpeed = speed;
        isAlive = true;
        
        UpdateHealthBar();
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        currentHP -= damage;
        OnTakeDamage?.Invoke(this, damage);
        
        CreateHitFlash();
        
        UpdateHealthBar();
        
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
        
        CreateScreenShake();
        CreateDeathParticles();
        
        Debug.Log($"[Unit] {gameObject.name} died!");
        
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);
        
        if (spriteRenderer != null)
            spriteRenderer.color = new Color32(128, 128, 128, 255);
        
        Invoke(nameof(DestroyUnit), 1f);
    }
    
    void DestroyUnit()
    {
        if (healthBarCanvas != null)
            Destroy(healthBarCanvas);
        Destroy(gameObject);
    }
    
    public virtual void Attack(Unit target)
    {
        if (!isAlive || target == null || !target.isAlive) return;
        
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= attackRange)
        {
            CreateAttackEffect();
            target.TakeDamage(attackPower);
            Debug.Log($"[Unit] {gameObject.name} attacked {target.gameObject.name} for {attackPower} damage!");
        }
    }
    
    protected void CreateAttackEffect()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(AttackFlashEffect());
        }
    }
    
    IEnumerator AttackFlashEffect()
    {
        Color attackColor = Color.yellow;
        Color originalAttackColor = spriteRenderer.color;
        
        spriteRenderer.color = attackColor;
        yield return new WaitForSeconds(0.05f);
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalAttackColor;
    }


    
    void CreateHitFlash()
    {
        if (isFlashing || spriteRenderer == null) return;
        
        StartCoroutine(HitFlashEffect());
    }
    
    IEnumerator HitFlashEffect()
    {
        isFlashing = true;
        spriteRenderer.color = Color.white;
        
        yield return new WaitForSeconds(hitFlashDuration);
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        isFlashing = false;
    }
    
    void CreateScreenShake()
    {
        if (mainCamera != null)
            StartCoroutine(ScreenShakeEffect());
    }
    
    IEnumerator ScreenShakeEffect()
    {
        Vector3 originalPosition = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration)
        {
            elapsed += Time.deltaTime;
            
            float x = Random.Range(-1f, 1f) * screenShakeIntensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity;
            
            mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);
            
            yield return null;
        }
        
        mainCamera.transform.position = originalPosition;
    }
    
    void CreateDeathParticles()
    {
        int particleCount = 5;
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = new GameObject("DeathParticle");
            particle.transform.position = transform.position;
            
            SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();
            
            Texture2D texture = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int p = 0; p < pixels.Length; p++)
                pixels[p] = Color.red;
            texture.SetPixels(pixels);
            texture.Apply();
            
            Sprite particleSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            particleRenderer.sprite = particleSprite;
            particleRenderer.sortingOrder = 5;
            
            particle.transform.localScale = Vector3.one * 0.3f;
            
            Vector2 randomDirection = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f)
            ).normalized;
            
            float speed = Random.Range(2f, 4f);
            
            StartCoroutine(AnimateParticle(particle, randomDirection * speed));
        }
    }
    
    IEnumerator AnimateParticle(GameObject particle, Vector2 velocity)
    {
        float lifetime = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;
        SpriteRenderer renderer = particle.GetComponent<SpriteRenderer>();
        
        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            particle.transform.position = startPos + (Vector3)(velocity * elapsed) + Vector3.down * (elapsed * elapsed * 2f);
            
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 1f - progress;
                renderer.color = color;
            }
            
            yield return null;
        }
        
        if (particle != null)
            Destroy(particle);
    }
}
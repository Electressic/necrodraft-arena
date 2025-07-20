using UnityEngine;
using System.Collections;

public class CardCombatAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float attackDuration = 0.6f;
    public float attackDistance = 1.5f;
    public AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Effects")]
    public float anticipationDuration = 0.15f;
    public float impactPauseDuration = 0.1f;
    public Vector3 anticipationScale = new Vector3(1.1f, 0.9f, 1f);
    public Vector3 impactScale = new Vector3(0.9f, 1.2f, 1f);
    
    [Header("Screen Effects")]
    public bool enableScreenShake = true;
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.2f;
    
    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;
    
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isAnimating = false;
    
    private static Camera mainCamera;
    
    void Awake()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Store original transform values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        
        // Initialize camera reference for screen shake
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    public void PerformAttackAnimation(Transform target, System.Action onImpact = null, System.Action onComplete = null)
    {
        if (isAnimating)
        {
            Debug.LogWarning($"[CardCombatAnimator] {gameObject.name} is already animating!");
            return;
        }
        
        StartCoroutine(AttackSequence(target, onImpact, onComplete));
    }
    
    IEnumerator AttackSequence(Transform target, System.Action onImpact, System.Action onComplete)
    {
        isAnimating = true;
        
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        
        Vector3 attackDirection = (target.position - startPos).normalized;
        Vector3 attackPosition = target.position - (attackDirection * attackDistance);
        
        yield return StartCoroutine(AnticipationAnimation(startScale));
        
        PlaySound(attackSound);
        
        yield return StartCoroutine(AttackMovement(startPos, attackPosition));
        
        onImpact?.Invoke();
        PlaySound(impactSound);
        
        if (enableScreenShake)
            StartCoroutine(ScreenShake());
        
        yield return StartCoroutine(ImpactAnimation());
        
        yield return StartCoroutine(ReturnMovement(attackPosition, startPos, startScale));
        
        isAnimating = false;
        onComplete?.Invoke();
    }
    
    IEnumerator AnticipationAnimation(Vector3 originalScale)
    {
        float elapsed = 0f;
        Vector3 startScale = originalScale;
        
        Vector3 pullBackPos = originalPosition - transform.right * 0.3f;
        Vector3 startPos = transform.position;
        
        while (elapsed < anticipationDuration)
        {
            float t = elapsed / anticipationDuration;
            float curveValue = Mathf.Sin(t * Mathf.PI); 
            
            transform.localScale = Vector3.Lerp(startScale, 
                Vector3.Scale(originalScale, anticipationScale), curveValue);
            
            transform.position = Vector3.Lerp(startPos, pullBackPos, curveValue * 0.5f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator AttackMovement(Vector3 startPos, Vector3 targetPos)
    {
        float elapsed = 0f;
        float moveDuration = attackDuration * 0.4f; 
        
        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            float curveValue = attackCurve.Evaluate(t);
            
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPos;
    }
    
    IEnumerator ImpactAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < impactPauseDuration)
        {
            float t = elapsed / impactPauseDuration;
            
            if (t < 0.5f)
            {
                float scaleT = t * 2f;
                transform.localScale = Vector3.Lerp(startScale, 
                    Vector3.Scale(originalScale, impactScale), scaleT);
            }
            else
            {
                float scaleT = (t - 0.5f) * 2f;
                transform.localScale = Vector3.Lerp(
                    Vector3.Scale(originalScale, impactScale), startScale, scaleT);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator ReturnMovement(Vector3 startPos, Vector3 originalPos, Vector3 originalScale)
    {
        float elapsed = 0f;
        float returnDuration = attackDuration * 0.4f; 
        
        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            float curveValue = Mathf.Sqrt(t); 
            
            transform.position = Vector3.Lerp(startPos, originalPos, curveValue);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, curveValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPos;
        transform.localScale = originalScale;
    }
    
    IEnumerator ScreenShake()
    {
        if (mainCamera == null) yield break;
        
        Vector3 originalCamPos = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float intensity = shakeIntensity * (1f - t); 
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f) * intensity,
                Random.Range(-1f, 1f) * intensity,
                0f
            );
            
            mainCamera.transform.position = originalCamPos + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = originalCamPos;
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }
} 
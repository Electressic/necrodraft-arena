using UnityEngine;

public class PreviewIndicatorAnimator : MonoBehaviour
{
    private Renderer indicatorRenderer;
    private Color baseColor;
    private Vector3 originalScale;
    private float pulseSpeed = 2f;
    private float minAlpha = 0.3f;
    private float maxAlpha = 0.9f;
    
    void Start()
    {
        indicatorRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        
        if (indicatorRenderer != null)
        {
            baseColor = indicatorRenderer.material.color;
        }
    }
    
    public void SetColor(Color color)
    {
        baseColor = color;
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material.color = color;
        }
    }
    
    void Update()
    {
        if (indicatorRenderer != null)
        {
            // Create pulsing effect by oscillating alpha
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color currentColor = baseColor;
            currentColor.a = alpha;
            indicatorRenderer.material.color = currentColor;
            
            // Also add slight scale pulsing for extra visibility
            float scaleMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = originalScale * scaleMultiplier;
        }
    }
} 
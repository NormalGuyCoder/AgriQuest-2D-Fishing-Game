using System.Collections;
using UnityEngine;

public class FishHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int health = 1;
    public int maxHealth = 1;
    public bool isDead = false;

    [Header("Death Effects")]
    public float deathDelay = 0.5f;
    public Color deathColor = Color.red;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void dealDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"Fish took {damage} damage! Health remaining: {health}");
        
        // Flash effect when taking damage
        if (spriteRenderer != null)
        {
            spriteRenderer.color = deathColor;
            Invoke("ResetColor", 0.1f);
        }
        
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            Die();
        }
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log("Fish caught!");
        
        // Disable components
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;
        if (GetComponent<FishController>() != null)
            GetComponent<FishController>().enabled = false;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = deathColor;
            fadeCoroutine = StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Destroy(gameObject, deathDelay);
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(deathDelay, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            spriteRenderer.color = new Color(deathColor.r, deathColor.g, deathColor.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(deathColor.r, deathColor.g, deathColor.b, 0f);
        fadeCoroutine = null;
        Destroy(gameObject);
    }
} 
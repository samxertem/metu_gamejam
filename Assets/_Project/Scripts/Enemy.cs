using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;

    [Header("Feedback Settings")]
    public Color damageColor = Color.red;
    public float colorFlashDuration = 0.1f;
    
    [Header("Death")]
    public float deathDestroyDelay = 3f; // Ölüm animasyonundan sonra kaç saniye bekle

    private MeshRenderer meshRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private bool isDead = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        health -= amount;

        if (meshRenderer != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRed());
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = damageColor;
            yield return new WaitForSeconds(colorFlashDuration);
            if (meshRenderer != null)
                meshRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // EnemyAI'a ölüm animasyonu oynat
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.PlayDeathAnimation();
        }

        // Collider'ları kapat (ölü bedene hasar vermesin)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in childColliders)
            c.enabled = false;

        // Animasyon bittikten sonra yok et
        Destroy(gameObject, deathDestroyDelay);
    }
}

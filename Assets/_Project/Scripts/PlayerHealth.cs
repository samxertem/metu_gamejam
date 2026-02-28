using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Damage Feedback")]
    public Image damageOverlay;       // Inspector'dan atanırsa kullanır, yoksa kendisi oluşturur
    public float flashDuration = 0.3f;
    public float shakeIntensity = 0.04f;
    public float shakeDuration = 0.15f;
    
    // Vignette sistemi
    private Image vignetteOverlay;
    private float vignetteFlashAlpha = 0f;

    [Header("Stimpack (F Key)")]
    public int stimpacks = 3; // Kaç kere can basabilirsin?
    public float healAmount = 50f;
    
    // NOT: Hız artışı için kendi karakter kontrolcünün scriptini buraya bağlamalısın!
    // public scr_CharacterController playerController; 
    
    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;
    private GunController gunController;
    private PerfectPlayerController playerController;

    void Start()
    {
        initialSpawnPosition = transform.position;
        initialSpawnRotation = transform.rotation;
        
        gunController = GetComponent<GunController>();
        playerController = GetComponent<PerfectPlayerController>();
        currentHealth = maxHealth;
        if (damageOverlay != null) 
            damageOverlay.color = new Color(1, 0, 0, 0); // Başlangıçta görünmez yap
            
        // UIManager'i başlat
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitHealth(maxHealth);
        }
        gunController = GetComponentInChildren<GunController>();
        
        // Vignette overlay'ını oluştur
        CreateVignetteOverlay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && stimpacks > 0 && currentHealth < maxHealth)
        {
            UseStimpack();
        }

        // Vignette'i cana göre güncelle
        UpdateVignetteOverlay();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return; // Zaten ölü

        currentHealth -= amount;
        
        // UIManager'i güncelle
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth);
        
        if (damageOverlay != null) 
            StartCoroutine(FlashRed());
        
        // Ekran titremesi
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            StartCoroutine(ScreenShake(cam.transform));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Karakteri kilitliyoruz (Silah sıkma ve Hareket kapanıyor)
        if (gunController != null) gunController.enabled = false;
        if (playerController != null) playerController.enabled = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathScreen();
        }
    }

    public void RespawnPlayer()
    {
        // Karakter controllerını kapatıp konumu zorla
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        transform.position = initialSpawnPosition;
        transform.rotation = initialSpawnRotation;
        
        if (cc != null) cc.enabled = true;

        if (playerController != null) playerController.enabled = true;
        if (gunController != null) gunController.enabled = true;

        currentHealth = maxHealth;
        
        // UI'yı sıfırla
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitHealth(maxHealth);
        }

        // --- TÜM DÜŞMANLARI TEMİZLE VE YENİDEN BAŞLAT ---
        // gameFrozen flag'ını sıfırla
        EnemyAI.gameFrozen = false;

        // Sahnedeki tüm düşmanları yok et
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // Spawner'ı yeniden başlat (8 metre uzaklaşma koruması ile)
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in spawners)
        {
            spawner.enabled = true;
            spawner.RestartSpawning();
        }
    }

    private IEnumerator FlashRed()
    {
        // Vuruş anında vignette'e kısa bir parlama ekle
        vignetteFlashAlpha = 0.35f;
        yield return new WaitForSeconds(flashDuration);
        vignetteFlashAlpha = 0f;
    }

    /// <summary>
    /// Radial gradient vignette texture oluşturur — köşeler kırmızı, orta saydam.
    /// </summary>
    private void CreateVignetteOverlay()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Vignette paneli oluştur
        GameObject vigObj = new GameObject("DamageVignette");
        vigObj.transform.SetParent(canvas.transform, false);
        vigObj.transform.SetAsLastSibling();

        RectTransform rt = vigObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        vignetteOverlay = vigObj.AddComponent<Image>();
        vignetteOverlay.raycastTarget = false;

        // Radial gradient texture oluştur
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = center.magnitude; // Köşeye olan mesafe

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / maxDist); // 0=orta, 1=köşe
                // Köşelere doğru artan kırmızılık (power curve ile keskin geçiş)
                float alpha = Mathf.Pow(t, 2.2f);
                tex.SetPixel(x, y, new Color(0.8f, 0.05f, 0.05f, alpha));
            }
        }
        tex.Apply();

        Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        vignetteOverlay.sprite = spr;
        vignetteOverlay.type = Image.Type.Sliced;
        vignetteOverlay.color = new Color(1, 1, 1, 0); // Başlangıçta görünmez
    }

    /// <summary>
    /// Vignette'i cana göre günceller. Can azaldıkça köşelerdeki kırmızılık artar.
    /// </summary>
    private void UpdateVignetteOverlay()
    {
        if (vignetteOverlay == null) return;

        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        
        // Sağlık yüzdesi → vignette alpha
        // %100 can = 0 alpha (görünmez)
        // %50 can = 0.25 alpha (hafif köşeler)
        // %20 can = 0.6 alpha (belirgin)
        // %0 can = 0.85 alpha (neredeyse tam)
        float baseAlpha = Mathf.Lerp(0.85f, 0f, healthPercent);
        
        // Flash pulse ekle (vuruş anı parlama)
        float finalAlpha = Mathf.Clamp01(baseAlpha + vignetteFlashAlpha);
        
        vignetteOverlay.color = new Color(1, 1, 1, finalAlpha);
    }

    private void UseStimpack()
    {
        stimpacks--;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth); // Canı maxHealth'i geçmeyecek şekilde artır
        
        // UIManager'i güncelle
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth);
        
        // Hızlandırma fonksiyonunu çağır
        StartCoroutine(SpeedBoost());
    }

    private IEnumerator SpeedBoost()
    {
        // EĞER KENDİ KONTROLCÜNÜN HIZ DEĞİŞKENİ VARSA BURAYA YAZ:
        // playerController.walkSpeed += 5f;
        Debug.Log("ADRENALIN AKTIF! HIZLANDIN!");

        yield return new WaitForSeconds(3f); // 3 saniye sürsün

        // HIZI ESKİ HALİNE DÖNDÜR:
        // playerController.walkSpeed -= 5f;
        Debug.Log("Adrenalin bitti.");
    }

    private IEnumerator ScreenShake(Transform camTransform)
    {
        Vector3 originalLocalPos = camTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeIntensity, shakeIntensity);
            float y = Random.Range(-shakeIntensity, shakeIntensity);
            camTransform.localPosition = originalLocalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        camTransform.localPosition = originalLocalPos;
    }
}
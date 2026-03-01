using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // Singleton mapping

    [Header("Health UI (Box)")]
    public Image healthBox; // Changed from Slider to Image
    public float healthLerpSpeed = 5f;
    
    [Header("Health Colors")]
    public Color highHealthColor = new Color(0.2f, 0.8f, 0.2f); // Green
    public Color midHealthColor = new Color(0.8f, 0.8f, 0.2f);  // Yellow
    public Color lowHealthColor = new Color(0.8f, 0.2f, 0.2f);  // Red

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;
    public Color normalAmmoColor = Color.white;
    public Color lowAmmoColor = Color.red;
    public float pulseSpeed = 6f;

    [Header("Death Screen UI")]
    public GameObject deathScreenPanel;

    // Internal trackers
    private float maxHealthData;
    private float currentDisplayedHealth;
    private float targetHealth;
    private bool isLowAmmo = false;

    // Kontrol ipuçları
    private CanvasGroup controlHintsGroup;
    private GameObject controlHintsPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        FormatUIElements();
    }

    void Start()
    {
        ShowControlHints();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateAmmoUI();
    }

    private void FormatUIElements()
    {
        // 1. Ekran boyutuna göre otomatik ölçeklenen (Responsive) Canvas ayarları
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>(); // Fallback: UI_Manager root objedeyse
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // 2. Otomatik olarak UI elemanlarını kenarlara yapıştırır (Anchoring)
        if (healthBox == null && canvas != null)
        {
            // Kullanıcı silip atamayı unutmuşsa, biz otomatik olarak can barını yaratıp Canvas'a koyalım
            GameObject boxObj = new GameObject("HealthBox");
            boxObj.transform.SetParent(canvas.transform, false);
            healthBox = boxObj.AddComponent<Image>();
        }

        if (healthBox != null)
        {
            RectTransform rt = healthBox.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Sol Alt Köşe (Bottom-Left)
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                rt.anchoredPosition = new Vector2(50, 50); 
                rt.sizeDelta = new Vector2(200, 50); // Mükemmel bir kutu/bar boyutu
            }
            
            // Başlangıç rengi
            healthBox.color = highHealthColor;
        }

        if (ammoText != null)
        {
            RectTransform rt = ammoText.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Sağ Alt Köşe (Bottom-Right)
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                rt.anchoredPosition = new Vector2(-50, 50); 
                
                ammoText.fontSize = 42; // Daha büyük text
                ammoText.alignment = TextAlignmentOptions.BottomRight;
                ammoText.fontStyle = FontStyles.Bold;
            }
        }

        // 3. Ölüm Ekranı (Death Screen) Jenerasyonu
        if (deathScreenPanel == null && canvas != null)
        {
            deathScreenPanel = new GameObject("DeathScreenPanel");
            deathScreenPanel.transform.SetParent(canvas.transform, false);
            RectTransform drt = deathScreenPanel.AddComponent<RectTransform>();
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.offsetMin = Vector2.zero;
            drt.offsetMax = Vector2.zero;
            
            Image backImage = deathScreenPanel.AddComponent<Image>();
            backImage.color = new Color(0, 0, 0, 0.85f);

            // ÖLDÜN Yazısı
            GameObject textObj = new GameObject("DeathText");
            textObj.transform.SetParent(deathScreenPanel.transform, false);
            TextMeshProUGUI deathText = textObj.AddComponent<TextMeshProUGUI>();
            deathText.text = "ÖLDÜN";
            deathText.fontSize = 72;
            deathText.alignment = TextAlignmentOptions.Center;
            deathText.color = new Color(0.85f, 0.12f, 0.12f, 1f);
            deathText.fontStyle = FontStyles.Bold;
            RectTransform txtRt = deathText.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.5f, 0.5f);
            txtRt.anchorMax = new Vector2(0.5f, 0.5f);
            txtRt.pivot = new Vector2(0.5f, 0.5f);
            txtRt.sizeDelta = new Vector2(600, 100);
            txtRt.anchoredPosition = new Vector2(0, 80);

            // TEKRAR DENE Butonu
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(deathScreenPanel.transform, false);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(RestartGame);
            
            // Hover renk efekti
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.15f, 0.15f, 1f);
            cb.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
            btn.colors = cb;
            
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(320, 70);
            btnRt.anchoredPosition = new Vector2(0, -30);
            
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "TEKRAR DENE";
            btnText.fontSize = 36;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;
            RectTransform btnTxtRt = btnText.GetComponent<RectTransform>();
            btnTxtRt.anchorMin = Vector2.zero;
            btnTxtRt.anchorMax = Vector2.one;
            btnTxtRt.offsetMin = Vector2.zero;
            btnTxtRt.offsetMax = Vector2.zero;

            deathScreenPanel.SetActive(false);
        }
    }

    private void UpdateHealthUI()
    {
        // Smoothly lerp internal display health
        if (Mathf.Abs(currentDisplayedHealth - targetHealth) > 0.1f)
        {
            currentDisplayedHealth = Mathf.Lerp(currentDisplayedHealth, targetHealth, Time.deltaTime * healthLerpSpeed);
            
            // Calculate Box Size and Color based on percentage
            if (healthBox != null && maxHealthData > 0)
            {
                float healthPercent = currentDisplayedHealth / maxHealthData;
                
                // 1. Kutuyu fiziksel olarak Sola doğru küçült (Pivot 0 olduğu için soldan sağa daralır)
                RectTransform rt = healthBox.rectTransform;
                rt.sizeDelta = new Vector2(200f * healthPercent, rt.sizeDelta.y);
                
                
                // Color Lerping: Green -> Yellow -> Red
                if (healthPercent > 0.5f)
                {
                    // 50% to 100%: Lerp between Yellow and Green
                    float t = (healthPercent - 0.5f) * 2f; 
                    healthBox.color = Color.Lerp(midHealthColor, highHealthColor, t);
                }
                else
                {
                    // 0% to 50%: Lerp between Red and Yellow
                    float t = healthPercent * 2f;
                    healthBox.color = Color.Lerp(lowHealthColor, midHealthColor, t);
                }
            }
        }
    }

    private void UpdateAmmoUI()
    {
        // Pulse ammo text slightly when low
        if (isLowAmmo && ammoText != null)
        {
            float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            
            // Pulse Color
            ammoText.color = Color.Lerp(normalAmmoColor, lowAmmoColor, pingPong);
            
            // Pulse Scale
            float scale = Mathf.Lerp(1f, 1.25f, pingPong);
            ammoText.transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (ammoText != null)
        {
            ammoText.color = normalAmmoColor;
            ammoText.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Configures the health box upper boundaries.
    /// </summary>
    public void InitHealth(float maxHealth)
    {
        maxHealthData = maxHealth;
        targetHealth = maxHealth;
        currentDisplayedHealth = maxHealth;
        
        if (healthBox != null)
            healthBox.color = highHealthColor;
    }

    /// <summary>
    /// Feed this whenever taking damage or healing.
    /// </summary>
    public void UpdateHealth(float currentHealth)
    {
        targetHealth = currentHealth;
    }

    /// <summary>
    /// Update UI dynamically with ammo count integers.
    /// </summary>
    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = current + " / " + max;
        }

        // Trigger warning if <= 25% format bounds
        isLowAmmo = (max > 0 && current <= max * 0.25f);
    }

    /// <summary>
    /// Ölüm Ekranını açar ve fare imlecini görünür kılar.
    /// </summary>
    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Ölüm Ekranını saklar ve silah/fare kilitlenmesini geri yükler.
    /// </summary>
    public void HideDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// UI butonuna tıklandığında oyuncuyu diriltmek için çağrılır.
    /// </summary>
    public void RestartGame()
    {
        HideDeathScreen();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RespawnPlayer();
        }
        ShowControlHints();
    }

    /// <summary>
    /// Spawn olunca ekranda kontrol tuşlarını ve görev yazısını gösterir.
    /// </summary>
    public void ShowControlHints()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Eski paneli varsa yok et
        if (controlHintsPanel != null)
            Destroy(controlHintsPanel);

        // === SOL ÜST KÖŞE: Kontrol Tuşları ===
        controlHintsPanel = new GameObject("ControlHintsPanel");
        controlHintsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = controlHintsPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 1); // Sol üst
        panelRt.anchorMax = new Vector2(0, 1);
        panelRt.pivot = new Vector2(0, 1);
        panelRt.sizeDelta = new Vector2(420, 200);
        panelRt.anchoredPosition = new Vector2(20, -20);

        Image bg = controlHintsPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.4f);
        bg.raycastTarget = false;

        controlHintsGroup = controlHintsPanel.AddComponent<CanvasGroup>();
        controlHintsGroup.alpha = 1f;

        GameObject textObj = new GameObject("HintsText");
        textObj.transform.SetParent(controlHintsPanel.transform, false);
        TextMeshProUGUI hintsText = textObj.AddComponent<TextMeshProUGUI>();
        hintsText.text = 
            "Mermileri Yenilemek i\u00e7in  <color=#FFD700>[R]</color>\n" +
            "Can Doldurmak i\u00e7in  <color=#FFD700>[F]</color>\n" +
            "El Fenerini a\u00e7/kapa  <color=#FFD700>[G]</color>\n" +
            "Y\u00fcr\u00fcmek i\u00e7in  <color=#FFD700>[W A S D]</color>";
        hintsText.fontSize = 24;
        hintsText.alignment = TextAlignmentOptions.Left;
        hintsText.color = new Color(0.95f, 0.85f, 0.4f, 1f);
        hintsText.fontStyle = FontStyles.Bold;
        hintsText.lineSpacing = 8f;

        RectTransform txtRt = hintsText.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(15, 10);
        txtRt.offsetMax = new Vector2(-15, -10);

        // === ALT ORTA: G\u00f6rev Yaz\u0131s\u0131 ===
        GameObject missionObj = new GameObject("MissionText");
        missionObj.transform.SetParent(controlHintsPanel.transform.parent, false);
        RectTransform missionRt = missionObj.AddComponent<RectTransform>();
        missionRt.anchorMin = new Vector2(0.5f, 0);
        missionRt.anchorMax = new Vector2(0.5f, 0);
        missionRt.pivot = new Vector2(0.5f, 0);
        missionRt.sizeDelta = new Vector2(600, 50);
        missionRt.anchoredPosition = new Vector2(0, 230);

        TextMeshProUGUI missionText = missionObj.AddComponent<TextMeshProUGUI>();
        missionText.text = "Polislerden kurtulmak i\u00e7in buradan ka\u00e7!";
        missionText.fontSize = 32;
        missionText.alignment = TextAlignmentOptions.Center;
        missionText.color = new Color(1f, 0.9f, 0.3f, 1f);
        missionText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        missionText.outlineWidth = 0.3f;
        missionText.outlineColor = new Color32(0, 0, 0, 200);
        // Alt gölge (underlay) ekle — material üzerinden
        missionText.fontMaterial.EnableKeyword("UNDERLAY_ON");
        missionText.fontMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.8f));
        missionText.fontMaterial.SetFloat("_UnderlayOffsetX", 0.5f);
        missionText.fontMaterial.SetFloat("_UnderlayOffsetY", -0.5f);
        missionText.fontMaterial.SetFloat("_UnderlayDilate", 0.2f);

        CanvasGroup missionGroup = missionObj.AddComponent<CanvasGroup>();
        missionGroup.alpha = 1f;

        // MissionText ayrı fade yapacak ama controlHintsPanel silinirken onu da sil
        StartCoroutine(FadeOutHints());
        StartCoroutine(FadeOutAndDestroy(missionObj, missionGroup, 17f, 3f));
    }

    private System.Collections.IEnumerator FadeOutHints()
    {
        // 17 saniye tam g\u00f6r\u00fcns\u00fcn
        yield return new WaitForSeconds(17f);

        // 3 saniyede kaybolsun
        float duration = 3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (controlHintsGroup != null)
                controlHintsGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }

        if (controlHintsPanel != null)
            Destroy(controlHintsPanel);
    }

    private System.Collections.IEnumerator FadeOutAndDestroy(GameObject obj, CanvasGroup group, float showTime, float fadeDuration)
    {
        yield return new WaitForSeconds(showTime);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (group != null)
                group.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        if (obj != null)
            Destroy(obj);
    }
}

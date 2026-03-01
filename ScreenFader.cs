using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Ekranı siyaha fade eder/açar. Canvas + Image ile çalışır.
/// TunnelTeleporter tarafından kullanılır.
/// Singleton pattern — DontDestroyOnLoad.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    private Image fadeImage;
    private Canvas fadeCanvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Canvas oluştur (ScreenSpace - Overlay, en üstte)
        fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        // CanvasScaler ekle (UI ölçekleme)
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Fade Image oluştur (tam ekran siyah)
        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(transform, false);

        fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Başta şeffaf
        fadeImage.raycastTarget = false;

        // RectTransform → tam ekran kapla
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Ekranı karart (şeffaftan siyaha).
    /// </summary>
    public Coroutine FadeOut(float duration = 0.5f)
    {
        return StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    /// <summary>
    /// Ekranı aç (siyahtan şeffafa).
    /// </summary>
    public Coroutine FadeIn(float duration = 0.5f)
    {
        return StartCoroutine(FadeRoutine(1f, 0f, duration));
    }

    /// <summary>
    /// Ekranı anında siyah yap.
    /// </summary>
    public void SetBlack()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1);
    }

    /// <summary>
    /// Ekranı anında temiz yap.
    /// </summary>
    public void SetClear()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Yükleme sonrası ekran siyah kaldıysa otomatik olarak aç (fade in)
        if (fadeImage != null && fadeImage.color.a > 0.05f)
        {
            FadeIn(1.5f);
        }
    }

    IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        // Fade sırasında raycast'i blokla (karanlıkta tıklama engelle)
        fadeImage.raycastTarget = (toAlpha > 0.5f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // TimeScale'den bağımsız
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth ease
            t = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, toAlpha);

        // Fade tamamlandıktan sonra şeffafsa raycast kapat
        if (toAlpha < 0.01f)
            fadeImage.raycastTarget = false;
    }
}

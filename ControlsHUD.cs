using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Oyun kontrol tuşlarını sağ üst köşede gösterir.
/// Tab basılı tutunca görünür, bırakınca fade-out ile kaybolur.
/// Oyun başında 3 saniye otomatik gösterilir.
/// </summary>
public class ControlsHUD : MonoBehaviour
{
    public static ControlsHUD Instance { get; private set; }

    [Header("Ayarlar")]
    public float fadeSpeed = 4f;
    public float initialShowDuration = 3f;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private float targetAlpha = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateUI();
    }

    void Start()
    {
        // Oyun başında 3sn göster
        StartCoroutine(InitialShow());
    }

    void Update()
    {
        // Tab basılı tutunca göster
        if (Input.GetKey(KeyCode.Tab))
            targetAlpha = 1f;
        else
            targetAlpha = 0f;

        // Smooth fade
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    IEnumerator InitialShow()
    {
        targetAlpha = 1f;
        yield return new WaitForSeconds(initialShowDuration);
        // Tab basılı değilse gizle
        if (!Input.GetKey(KeyCode.Tab))
            targetAlpha = 0f;
    }

    void CreateUI()
    {
        // Canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        // CanvasGroup (fade kontrolü)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Panel (sağ üst köşe, yarı-saydam arka plan)
        GameObject panel = new GameObject("ControlsPanel");
        panel.transform.SetParent(transform, false);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.6f);
        panelBg.raycastTarget = false;

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 1f); // Sağ üst
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot = new Vector2(1f, 1f);
        panelRT.anchoredPosition = new Vector2(-20f, -20f);
        panelRT.sizeDelta = new Vector2(260f, 230f);

        // Başlık
        CreateLabel(panel.transform, "TitleText",
            "<b>KONTROLLER</b>  <size=14><color=#aaa>(Tab)</color></size>",
            new Vector2(0f, -10f), 18, TextAnchor.UpperCenter, new Color(1f, 0.85f, 0.4f));

        // Tuş listesi
        string[] controls = {
            "<color=#ffcc44>WASD</color>  —  Hareket",
            "<color=#ffcc44>Mouse</color>  —  Bakis",
            "<color=#ffcc44>E</color>  —  Etkilesim",
            "<color=#ffcc44>F</color>  —  Fener",
            "<color=#ffcc44>Ctrl</color>  —  Comel",
            "<color=#ffcc44>Shift</color>  —  Kos"
        };

        float yPos = -38f;
        for (int i = 0; i < controls.Length; i++)
        {
            CreateLabel(panel.transform, "Control_" + i,
                controls[i],
                new Vector2(0f, yPos), 15, TextAnchor.UpperCenter, Color.white);
            yPos -= 28f;
        }
    }

    void CreateLabel(Transform parent, string name, string content,
        Vector2 anchoredPos, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.supportRichText = true;
        text.raycastTarget = false;

        // Outline (okunabilirlik)
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(0f, 30f);
    }
}

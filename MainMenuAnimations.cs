using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuAnimations : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public Button playButton;
    public TextMeshProUGUI creditsText;
    public Image darkOverlay;

    [Header("Animation Settings")]
    public float fadeInDuration = 2f;
    public float titleGlowSpeed = 1.5f;
    public float titleGlowMin = 0.7f;
    public float titleGlowMax = 1f;
    public float buttonPulseSpeed = 2f;

    private Image playButtonImage;
    private Color playBtnBaseColor;

    private void Start()
    {
        if (playButton != null)
        {
            playButtonImage = playButton.GetComponent<Image>();
            if (playButtonImage != null)
                playBtnBaseColor = playButtonImage.color;
        }

        // Start everything invisible
        StartCoroutine(FadeInSequence());
    }

    private IEnumerator FadeInSequence()
    {
        // Set everything transparent
        if (titleText != null) SetAlpha(titleText, 0);
        if (subtitleText != null) SetAlpha(subtitleText, 0);
        if (playButton != null) SetGroupAlpha(playButton.gameObject, 0);
        if (creditsText != null) SetAlpha(creditsText, 0);

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        // Fade in title
        yield return StartCoroutine(FadeInText(titleText, 1.5f));

        // Fade in subtitle
        yield return StartCoroutine(FadeInText(subtitleText, 1f));
        
        yield return new WaitForSeconds(0.3f);

        // Fade in button
        if (playButton != null)
        {
            float elapsed = 0;
            CanvasGroup btnGroup = playButton.GetComponent<CanvasGroup>();
            if (btnGroup == null) btnGroup = playButton.gameObject.AddComponent<CanvasGroup>();
            
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                btnGroup.alpha = Mathf.Lerp(0, 1, elapsed / 1f);
                yield return null;
            }
            btnGroup.alpha = 1;
        }

        // Fade in credits
        yield return StartCoroutine(FadeInText(creditsText, 0.8f));
    }

    private void Update()
    {
        // Title glow pulsing
        if (titleText != null)
        {
            float glow = Mathf.Lerp(titleGlowMin, titleGlowMax,
                (Mathf.Sin(Time.time * titleGlowSpeed) + 1f) / 2f);
            Color c = titleText.color;
            c.a = glow;
            titleText.color = c;
        }

        // Play button subtle pulse
        if (playButtonImage != null)
        {
            float pulse = Mathf.Lerp(0.85f, 1f,
                (Mathf.Sin(Time.time * buttonPulseSpeed) + 1f) / 2f);
            Color c = playBtnBaseColor;
            c.a = pulse;
            playButtonImage.color = c;
        }
    }

    private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        if (text == null) yield break;

        Color targetColor = text.color;
        float targetAlpha = targetColor.a;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(0, targetAlpha, elapsed / duration);
            SetAlpha(text, a);
            yield return null;
        }
        SetAlpha(text, targetAlpha);
    }

    private void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    private void SetGroupAlpha(GameObject obj, float alpha)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }
}

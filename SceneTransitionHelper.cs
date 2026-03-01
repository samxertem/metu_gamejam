using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Static helper to show a "Part X: Name" text with audio during scene transitions.
/// Creates a DontDestroyOnLoad canvas with centered text and persistent audio.
/// </summary>
public static class SceneTransitionHelper
{
    /// <summary>
    /// Show transition text and play audio on a black screen.
    /// Call this AFTER the screen is already black.
    /// </summary>
    public static void ShowTransition(string partText, AudioClip voiceClip)
    {
        // Create persistent canvas
        GameObject canvasObj = new GameObject("TransitionOverlay_Persistent");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1500; // Above everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Black background
        Image bg = canvasObj.AddComponent<Image>();
        bg.color = Color.black;

        // Text
        GameObject textObj = new GameObject("TransitionText");
        textObj.transform.SetParent(canvasObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = partText;
        text.fontSize = 60;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.9f, 0.85f, 0.75f, 0f); // Start invisible
        text.enableWordWrapping = false;
        text.characterSpacing = 5;

        Object.DontDestroyOnLoad(canvasObj);

        // Start fade in coroutine via a runner
        GameObject runner = new GameObject("TransitionRunner");
        TransitionRunner tr = runner.AddComponent<TransitionRunner>();
        Object.DontDestroyOnLoad(runner);
        tr.StartCoroutine(tr.FadeInText(text, 1.5f));
        tr.StartCoroutine(tr.AutoDestroy(canvasObj, runner, 16f));

        // Play voice audio at boosted volume
        if (voiceClip != null)
        {
            GameObject audioObj = new GameObject("TransitionVoice_Persistent");
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = voiceClip;
            source.volume = 1.5f;
            source.Play();
            Object.DontDestroyOnLoad(audioObj);
            Object.Destroy(audioObj, voiceClip.length + 1f);
        }
    }
}

/// <summary>
/// MonoBehaviour runner for coroutines in static context.
/// </summary>
public class TransitionRunner : MonoBehaviour
{
    public IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        if (text == null) yield break;

        Color targetColor = text.color;
        float targetAlpha = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(elapsed / duration);
            if (text != null)
                text.color = new Color(targetColor.r, targetColor.g, targetColor.b, a * targetAlpha);
            yield return null;
        }

        if (text != null)
            text.color = new Color(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
    }

    public IEnumerator AutoDestroy(GameObject overlay, GameObject runner, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (overlay != null) Destroy(overlay);
        if (runner != null) Destroy(runner.gameObject);
    }
}
